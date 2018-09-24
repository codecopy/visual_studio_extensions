using System;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using Task = System.Threading.Tasks.Task;

namespace ExecomExtensions.Commands.AddHeader
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class AddHeader
    {
        /// <summary>
        /// Special characters set at the beginning of line
        /// </summary>
        private const string SpecialCharacters = "//--$$%%";

        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 256;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("0ac451a1-191e-438e-822d-01e80de85c01");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddHeader"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private AddHeader(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.ExecuteAsync, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static AddHeader Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Verify the current thread is the UI thread - the call to AddCommand in AddHeader's constructor requires
            // the UI thread.
            // ThreadHelper.ThrowIfNotOnUIThread();

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new AddHeader(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private async void ExecuteAsync(object sender, EventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (!(this.package is ExecomExtensionsPackage execomPackage))
            {
                VsShellUtilities.ShowMessageBox(
                        this.package,
                        "Couldn't load ExecomExtensionsPackage",
                        "Error",
                        OLEMSGICON.OLEMSGICON_CRITICAL,
                        OLEMSGBUTTON.OLEMSGBUTTON_OK,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                return;
            }

            var template = execomPackage.AddHeaderOptions.TemplateHeader;
            if (string.IsNullOrWhiteSpace(template))
            {
                VsShellUtilities.ShowMessageBox(
                        this.package,
                        "Template for AddHeader command is empty!",
                        "Error",
                        OLEMSGICON.OLEMSGICON_CRITICAL,
                        OLEMSGBUTTON.OLEMSGBUTTON_OK,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                return;
            }

            EnvDTE80.DTE2 applicationObject = await ServiceProvider.GetServiceAsync(typeof(DTE)) as EnvDTE80.DTE2;

            EnvDTE.TextSelection textSelection = applicationObject?.ActiveDocument.Selection as TextSelection;

            if (textSelection == null)
            {
                return;
            }

            var information = ExtractDocumentInformation(textSelection);

            var userInformation = ExtractUserInformation(execomPackage.DefaultOptions.RegistrySubKey);

            template = ReplacePlaceholders(template, information, userInformation);

            InsertHeader(template, textSelection);

            applicationObject.ActiveDocument.Save();
        }

        /// <summary>
        /// Insert header in document
        /// </summary>
        /// <param name="template">Template to be added</param>
        /// <param name="textSelection">Text selection of document</param>
        private void InsertHeader(string template, TextSelection textSelection)
        {
            // Add header
            textSelection.StartOfDocument();
            textSelection.NewLine();
            textSelection.StartOfDocument();

            textSelection.Text = template;
            textSelection.NewLine();
            textSelection.NewLine();
            RemoveSpecialCharacters(textSelection, template);
        }

        /// <summary>
        /// Replaces placeholders with provided information in template
        /// </summary>
        /// <param name="template">Template</param>
        /// <param name="docInformation">Document information</param>
        /// <param name="userInformation">User information</param>
        /// <returns>Template with replaced values</returns>
        private string ReplacePlaceholders(string template, DocumentInformation docInformation, UserInformation userInformation)
        {
            // Change date place holder in template
            template = ChangeDatePlaceholder(template);

            StringBuilder builder = new StringBuilder(template);

            builder.Replace(Placeholders.NamespacePlaceholder, docInformation.Namespace);
            builder.Replace(Placeholders.ClassInterfacePlaceholder, docInformation.Class ?? docInformation.Interface);
            builder.Replace(Placeholders.AuthorPlaceholder, userInformation.Name);
            builder.Replace(Placeholders.EmailPlaceholder, userInformation.Email);

            builder.Insert(0, SpecialCharacters);
            builder.Replace("\n", $"\n{SpecialCharacters}");

            return builder.ToString();
        }

        /// <summary>
        /// Removes special characters after inserting header
        /// </summary>
        /// <param name="textSelection">Text selection</param>
        /// <param name="template">Template</param>
        private void RemoveSpecialCharacters(TextSelection textSelection, string template)
        {
            // Remove special characters from beginning of each line in template
            textSelection.StartOfDocument();
            for (int i = 0; i < template.Split('\n').Length; i++)
            {
                textSelection.StartOfLine();
                textSelection.CharRight(true, SpecialCharacters.Length);
                textSelection.Delete();
                textSelection.LineDown();
            }
        }

        /// <summary>
        /// Change date placeholder from a string
        /// </summary>
        /// <param name="text">Text</param>
        /// <returns>Changed text</returns>
        private string ChangeDatePlaceholder(string text)
        {
            // Set date in specified or default format
            var startIndex = text.IndexOf(Placeholders.DatePlaceholder);
            if (startIndex > -1)
            {
                var endIndex = text.IndexOf("}", startIndex);

                if (endIndex > -1)
                {
                    // Extract placeholder
                    var placeHolder = text.Substring(startIndex, endIndex - startIndex + 1);

                    string dateTimeFormat = string.Empty;

                    // Check if difference between start and end index is larger than 7.
                    // Meaning that in between there is date time format specified
                    if ((endIndex - startIndex) > Placeholders.DatePlaceholder.Length + 2)
                    {
                        // Extract format
                        dateTimeFormat = text.Substring(startIndex + Placeholders.DatePlaceholder.Length + 1, endIndex - startIndex - Placeholders.DatePlaceholder.Length - 1);
                    }

                    text = text.Replace(placeHolder, DateTime.Now.ToString(dateTimeFormat));
                }
            }

            return text;
        }

        /// <summary>
        /// Extracts logged user information
        /// </summary>
        /// <param name="subKey">Destination of SubKey registry</param>
        /// <returns>UserInformation</returns>
        private UserInformation ExtractUserInformation(string subKey)
        {
            var userInformation = new UserInformation();

            if (string.IsNullOrWhiteSpace(subKey))
            {
                return userInformation;
            }

            RegistryKey root = Registry.CurrentUser;
            RegistryKey sk = root.OpenSubKey(subKey);

            if (sk != null)
            {
                // Currently signed in user.
                userInformation.Name = (string)sk.GetValue("DisplayName");
                userInformation.Email = (string)sk.GetValue("EmailAddress");
            }

            return userInformation;
        }

        /// <summary>
        /// Extract document information. Such as namespace, class or interface.
        /// </summary>
        /// <param name="textSelection">TextSelection object of a document</param>
        /// <returns>Basic Information object</returns>
        private DocumentInformation ExtractDocumentInformation(TextSelection textSelection)
        {
            textSelection.StartOfDocument();

            string @namespace = string.Empty;
            string @class = string.Empty;
            string @interface = string.Empty;

            int previousLine = -1;
            while (previousLine != textSelection.CurrentLine)
            {
                if (textSelection.Text.Contains("//"))
                {
                    previousLine = textSelection.CurrentLine;
                    textSelection.SelectLine();
                    continue;
                }

                string[] split = textSelection.Text.Split(' ');

                if (@namespace == string.Empty)
                {
                    @namespace = FindName(split, "namespace");
                }

                if (@class == string.Empty)
                {
                    @class = FindName(split, "class");
                }

                if (@interface == string.Empty)
                {
                    @interface = FindName(split, "interface");
                }

                if (@namespace != string.Empty && (@class != string.Empty || @interface != string.Empty))
                {
                    break;
                }

                previousLine = textSelection.CurrentLine;
                textSelection.SelectLine();
            }

            return new DocumentInformation
            {
                Namespace = @namespace,
                Class = @class,
                Interface = @interface
            };
        }

        /// <summary>
        /// Finds name according to pattern
        /// </summary>
        /// <param name="splitLine"></param>
        /// <param name="pattern"></param>
        /// <returns>Name or empty string</returns>
        private string FindName(string[] splitLine, string pattern)
        {
            var word = splitLine.Select((x, i) => new { Value = x, Index = i })
                                   .FirstOrDefault(x => x.Value == pattern);

            if (word == null)
            {
                return string.Empty;
            }

            if (splitLine.Length <= (word.Index + 1))
            {
                return string.Empty;
            }

            try
            {
                // Clear all special characters 
                return Regex.Replace(splitLine[word.Index + 1], "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
            }
            catch
            {
            }

            return string.Empty;
        }
    }

    /// <summary>
    /// User information
    /// </summary>
    class UserInformation
    {
        /// <summary>
        /// Name of user if known
        /// </summary>
        public string Name { get; set; } = "(Unknown)";

        /// <summary>
        /// Email of user if known
        /// </summary>
        public string Email { get; set; } = "(Unknown)";
    }

    /// <summary>
    /// Document information. Namespace, class, etc.
    /// </summary>
    class DocumentInformation
    {
        /// <summary>
        /// Namespace of the document
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Class in document if any
        /// </summary>
        public string Class { get; set; }

        /// <summary>
        /// Interface n class if any
        /// </summary>
        public string Interface { get; set; }
    }
}
