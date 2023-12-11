using Microsoft.Win32;
using System.Linq;
using System.Text;
using System.Windows;

namespace SimpleFFmpegGUI.WPF
{
    public static class FileDialogExtension
    {
        /// <summary>
        /// 满足条件是加入文件筛选器
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="b"></param>
        /// <param name="display"></param>
        /// <param name="extensions"></param>
        /// <returns></returns>
        public static T AddFilterIf<T>(this T dialog, bool b, string display, params string[] extensions) where T : FileDialog
        {
            if (b)
            {
                return AddFilter(dialog, display, extensions);
            }
            return dialog;
        }
        public static T AddFilter<T>(this T dialog, string display, params string[] extensions) where T : FileDialog
        {
            StringBuilder filter = new StringBuilder(dialog.Filter);
            if (filter.Length > 0)
            {
                filter.Append('|');
            }
            filter.Append(display);
            filter.Append('|');
            filter.Append(string.Join(';', extensions.Select(p => "*." + p)));
            dialog.Filter = filter.ToString();
            return dialog;
        }

        public static T AddAllFilesFilter<T>(this T dialog, string display = "所有文件") where T : FileDialog
        {
            StringBuilder filter = new StringBuilder(dialog.Filter);
            if (filter.Length > 0)
            {
                filter.Append('|');
            }
            filter.Append(display);
            filter.Append("|*.*");
            dialog.Filter = filter.ToString();
            return dialog;
        }

        public static string GetPath(this FileDialog dialog, Window owner)
        {
            if ((owner == null ? dialog.ShowDialog() : dialog.ShowDialog(owner)) == true)
            {
                return dialog.FileName;
            }
            return null;
        }

        public static string[] GetPaths(this OpenFileDialog dialog, Window owner)
        {
            dialog.Multiselect = true;
            if ((owner == null ? dialog.ShowDialog() : dialog.ShowDialog(owner)) == true)
            {
                return dialog.FileNames;
            }
            return null;
        }

        public static string GetPath(this OpenFolderDialog dialog, Window owner)
        {
            if ((owner == null ? dialog.ShowDialog() : dialog.ShowDialog(owner)) == true)
            {
                return dialog.FolderName;
            }
            return null;
        }
    }
}