using System.Windows;

namespace SimpleFFmpegGUI.WPF
{
    public static class WindowExtension
    {
        public static void BringToFront(this Window win)
        {
            if (!win.IsVisible)
            {
                win.Show();
            }

            if (win.WindowState == WindowState.Minimized)
            {
                win.WindowState = WindowState.Normal;
            }

            win.Activate();
            win.Topmost = true;  // important
            win.Topmost = false; // important
            win.Focus();
        }

        public static bool? ShowDialog(this Window win, Window owner, bool setCenterOwner = true)
        {
            win.Owner = owner;
            win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            return win.ShowDialog();
        }
    }
}