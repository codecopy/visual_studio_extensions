using System;
using System.ComponentModel.Design;
using System.IO;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace ExecomExtensions.Commands.OutputDestination
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class OutputDestination
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 4129;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("0ac451a1-191e-438e-822d-01e80de85c00");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputDestination"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private OutputDestination(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static OutputDestination Instance
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
            // Verify the current thread is the UI thread - the call to AddCommand in OutputDestination's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new OutputDestination(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private async void Execute(object sender, EventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var selectedProject = GetSelectedProject();

            if (selectedProject is null)
            {
                return;
            }

            var definedOutputPath = selectedProject.ConfigurationManager
                                        .ActiveConfiguration
                                        .Properties
                                        .Item("OutputPath").Value.ToString();

            var projectDir = Path.GetDirectoryName(selectedProject.FullName);

            if (string.IsNullOrWhiteSpace(projectDir))
            {
                return;
            }

            // Open file explorer
            System.Diagnostics.Process.Start(GetOutputDirectory(definedOutputPath, projectDir));
        }

        private static string GetOutputDirectory(string definedOutputPath, string projectDir)
        {
            // Check if output path is absolute
            var outputPath = Path.IsPathRooted(definedOutputPath) ? definedOutputPath : Path.Combine(projectDir, definedOutputPath);

            if (!Directory.Exists(outputPath))
            {
                // if directory does not exist set project directory to output
                outputPath = projectDir;
            }

            return outputPath;
        }

        /// <summary>
        /// Gets selected project from solution
        /// </summary>
        /// <returns>Selected project</returns>
        private Project GetSelectedProject()
        {
            Object selectedObject = null;

            IVsMonitorSelection monitorSelection =
                    (IVsMonitorSelection)Package.GetGlobalService(
                    typeof(SVsShellMonitorSelection));

            monitorSelection.GetCurrentSelection(out var hierarchyPointer,
                                                 out var projectItemId,
                                                 out _,
                                                 out _);


            if (Marshal.GetTypedObjectForIUnknown(
                hierarchyPointer,
                typeof(IVsHierarchy)) is IVsHierarchy selectedHierarchy)
            {
                ErrorHandler.ThrowOnFailure(selectedHierarchy.GetProperty(
                                                  projectItemId,
                                                  (int)__VSHPROPID.VSHPROPID_ExtObject,
                                                  out selectedObject));
            }

            return selectedObject as Project;
        }
    }
}
