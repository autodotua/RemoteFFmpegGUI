﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using SimpleFFmpegGUI.WPF.Messages;
using System;
using System.Windows;
using System.Windows.Controls;

namespace SimpleFFmpegGUI.WPF.ViewModels
{
    public partial class ViewModelBase : ObservableObject
    {
        protected void QueueSuccessMessage(string message)
        {
            WeakReferenceMessenger.Default.Send(new QueueMessagesMessage('S', message));
        }
        protected void QueueErrorMessage(string message, Exception ex = null)
        {
            WeakReferenceMessenger.Default.Send(new QueueMessagesMessage('E', message, ex));
        }

        protected ContentControl View { get; set; }
    }

    public static class ViewModelExtension
    {
        public static TVM SetDataContext<TVM>(this FrameworkElement element) where TVM : ViewModelBase
        {
            TVM viewModel = App.ServiceProvider.GetRequiredService<TVM>();
            element.DataContext = viewModel;
            return viewModel;
        }
    }
}