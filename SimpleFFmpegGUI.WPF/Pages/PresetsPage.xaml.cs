﻿using Enterwell.Clients.Wpf.Notifications;
using FzLib;
using FzLib.WPF;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAPICodePack.FzExtension;
using ModernWpf.FzExtension.CommonDialog;
using Newtonsoft.Json;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;
using SimpleFFmpegGUI.WPF.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SimpleFFmpegGUI.WPF.Pages
{
    public class PresetsPageViewModel : INotifyPropertyChanged
    {
        public PresetsPageViewModel()
        {
        }

        public IEnumerable TaskTypes => Enum.GetValues(typeof(TaskType));
        private TaskType type = TaskType.Code;

        public TaskType Type
        {
            get => type;
            set
            {
                this.SetValueAndNotify(ref type, value, nameof(Type));
                FillPresets();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<CodePreset> presets;

        public ObservableCollection<CodePreset> Presets
        {
            get => presets;
            set => this.SetValueAndNotify(ref presets, value, nameof(Presets));
        }

        public void FillPresets()
        {
            Presets = new ObservableCollection<CodePreset>(PresetManager.GetPresets(Type));
        }

        public void DeletePreset(CodePreset preset)
        {
            PresetManager.DeletePreset(preset.Id);
            Presets.Remove(preset);
        }
    }

    /// <summary>
    /// Interaction logic for PresetsPage.xaml
    /// </summary>
    public partial class PresetsPage : UserControl
    {
        public PresetsPageViewModel ViewModel { get; set; }

        public PresetsPage(PresetsPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = ViewModel;
            InitializeComponent();
            Loaded += (s, e) => ViewModel.FillPresets();
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var preset = (sender as Button).DataContext as CodePreset;
            if (await CommonDialog.ShowYesNoDialogAsync("删除预设", $"是否删除“{preset.Name}”？"))
            {
                ViewModel.DeletePreset(preset);
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var path = new FileFilterCollection().Add("配置文件", "json").CreateSaveFileDialog().SetParent(this.GetWindow()).SetDefault("FFmpeg工具箱 预设.json").GetFilePath();
            if (path != null)

            {
                var json = PresetManager.Export();
                File.WriteAllText(path, json, new UTF8Encoding());
                this.CreateMessage().QueueSuccess("导出成功");
            }
        }

        private async void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            var path = new FileFilterCollection().Add("配置文件", "json").CreateOpenFileDialog().SetParent(this.GetWindow()).GetFilePath();
            if (path != null)

            {
                try
                {
                    PresetManager.Import(File.ReadAllText(path, new UTF8Encoding()));
                    ViewModel.FillPresets();
                    this.CreateMessage().QueueSuccess("导入成功，同名预设已被更新");
                }
                catch (Exception ex)
                {
                    await CommonDialog.ShowErrorDialogAsync(ex, "导入失败");
                }
            }
        }

        private async void DeleteAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (await CommonDialog.ShowYesNoDialogAsync("删除预设", $"是否删除所有类型的所有预设？"))
            {
                IsEnabled = false;
                try
                {
                    PresetManager.DeletePresets();
                    ViewModel.FillPresets();
                }
                catch (Exception ex)
                {
                    await CommonDialog.ShowErrorDialogAsync(ex, "删除失败");
                }
                finally
                {
                    IsEnabled = true;
                }
            }
        }
    }
}