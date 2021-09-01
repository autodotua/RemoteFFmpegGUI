using System.ComponentModel;

namespace SimpleFFmpegGUI.WPF.Model
{
    public interface ITempArguments : INotifyPropertyChanged
    {
        /// <summary>
        /// 将原始数据更新到UI数据
        /// </summary>
        public void Update();

        /// <summary>
        /// 从UI数据应用到原始数据
        /// </summary>
        public void Apply();
    }
}