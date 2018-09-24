using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace ExecomExtensions.Options
{
    [Guid("63D326CC-BAE1-4CC3-B447-F80CB8FEBC9C")]
    public class DefaultOptions : DialogPage
    {

        [DisplayName("Registry Sub Key")]
        [Description("Represents destination of registry sub key which holds information of VS logged user")]
        public string RegistrySubKey { get; set; }
            = @"Software\\Microsoft\\VSCommon\\ConnectedUser\\IdeUserV2\\Cache";
    }
}
