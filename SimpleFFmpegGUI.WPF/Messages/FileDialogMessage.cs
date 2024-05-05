using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFFmpegGUI.WPF.Messages
{
    public class FileDialogMessage(CommonItemDialog dialog)
    {
        public CommonItemDialog Dialog { get; } = dialog;
        public bool? Result { get; set; }
    }
}
