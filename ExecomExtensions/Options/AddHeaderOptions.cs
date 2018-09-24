using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;

namespace ExecomExtensions.Options
{
    [Guid("91417BC2-E48C-43A4-9B2B-283BBF9B9908")]
    public class AddHeaderOptions : DialogPage
    {
        private AddHeaderOptionsUserControl _control;

        /// <summary>
        /// Template header
        /// </summary>
        public string TemplateHeader { get; set; } = string.Empty;

        protected override IWin32Window Window
        {
            get
            {
                _control = new AddHeaderOptionsUserControl();
                _control.TextBox = TemplateHeader;
                return _control;
            }
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            // Event occurs on save or apply
            // Set template
            TemplateHeader = _control.TextBox;
            base.OnApply(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            // Event occurs on cancel
            // Set back to default value
            _control.TextBox = TemplateHeader;
        }
    }
}
