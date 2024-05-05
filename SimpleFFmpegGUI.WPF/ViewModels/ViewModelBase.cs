using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows.Controls;

namespace SimpleFFmpegGUI.WPF.ViewModels
{
    public partial class ViewModelBase : ObservableObject
    {
        protected void QueueSuccessMessage(string message)
        {
            View.CreateMessage().QueueSuccess(message);
        }
        protected void QueueErrorMessage(string message, Exception ex)
        {
            View.CreateMessage().QueueError(message, ex);
        }

        protected ContentControl View { get; set; }

        public static TVM Bind<TVM>(ContentControl view) where TVM : ViewModelBase
        {
            TVM viewModel = App.ServiceProvider.GetRequiredService<TVM>();
            viewModel.View = view;
            return viewModel;
        }


    }
}