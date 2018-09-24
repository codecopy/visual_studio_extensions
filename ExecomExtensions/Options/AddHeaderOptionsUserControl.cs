using System.Windows.Forms;
using ExecomExtensions.Commands.AddHeader;

namespace ExecomExtensions.Options
{
    public partial class AddHeaderOptionsUserControl : UserControl
    {
        public AddHeaderOptionsUserControl()
        {
            InitializeComponent();

            patternsLabel.Text = "Placeholders:\n" +
                                 Placeholders.EmailPlaceholder + " - E-mail address of logged user\n" +
                                 Placeholders.AuthorPlaceholder + " - First and last name of logged user\n" +
                                 Placeholders.NamespacePlaceholder + " - Name of the Namespace\n" +
                                 Placeholders.ClassInterfacePlaceholder + " - Class or interface name\n" +
                                 Placeholders.DatePlaceholder + ":[DateFormat]} - Placeholder for current date in specified date format (optional)";
        }

        public string TextBox
        {
            get => headerTemplateTextBox.Text;
            set => headerTemplateTextBox.Text = value;
        }

        private void headerTemplateTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            // Handle new line in user control
            if (e.KeyData == Keys.Enter)
            {
                var startIndex = headerTemplateTextBox.SelectionStart;
                var endIndex = headerTemplateTextBox.SelectionStart + headerTemplateTextBox.SelectedText.Length;

                // Check if part of the text has been selected
                if (endIndex - startIndex != 0)
                {
                    headerTemplateTextBox.Text = headerTemplateTextBox.Text.Remove(startIndex, headerTemplateTextBox.SelectedText.Length);
                }

                headerTemplateTextBox.Text = headerTemplateTextBox.Text.Insert(startIndex, "\n");
                headerTemplateTextBox.SelectionStart = ++startIndex;
                headerTemplateTextBox.SelectionLength = 0;
            }
        }

        private void headerTemplateTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // For some reason, pressing escape hides the user control in options
            // window and it can not be visible again without turning off and on
            // the option window
            if (e.KeyData == Keys.Escape)
            {
                e.Handled = true;
            }
        }
    }
}
