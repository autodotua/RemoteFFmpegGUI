﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FzLib;
using Microsoft.Win32;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;
using SimpleFFmpegGUI.WPF.Messages;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ModernWpf.FzExtension.CommonDialog;
using CommonDialog = ModernWpf.FzExtension.CommonDialog.CommonDialog;

namespace SimpleFFmpegGUI.WPF.ViewModels
{
    public partial class PresetsPageViewModel : ViewModelBase
    {
        private readonly PresetManager presetManager;

        [ObservableProperty]
        private ObservableCollection<CodePreset> presets;

        [ObservableProperty]
        private CodePreset selectedPreset;

        [ObservableProperty]
        private TaskType type = TaskType.Code;
        public PresetsPageViewModel(PresetManager presetManager)
        {
            this.presetManager = presetManager;
        }

        public CodeArgumentsPanelViewModel CodeArgumentsViewModel { get; set; }
        public IEnumerable TaskTypes => Enum.GetValues(typeof(TaskType));

        public async Task FillPresetsAsync()
        {
            Presets = new ObservableCollection<CodePreset>(await presetManager.GetPresetsAsync(Type));
        }

        protected override async void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(Type))
            {
                await FillPresetsAsync();
            }
        }
        [RelayCommand]
        private async Task DeleteAllAsync()
        {
            if (await CommonDialog.ShowYesNoDialogAsync("删除预设", $"是否删除所有类型的所有预设？"))
            {
                try
                {
                    await presetManager.DeletePresetsAsync();
                    await FillPresetsAsync();
                }
                catch (Exception ex)
                {
                    await CommonDialog.ShowErrorDialogAsync(ex, "删除失败");
                }
            }
        }

        [RelayCommand]
        private async Task DeleteAsync(CodePreset preset)
        {
            if (await CommonDialog.ShowYesNoDialogAsync("删除预设", $"是否删除“{preset.Name}”？"))
            {
                await presetManager.DeletePresetAsync(preset.Id);
                Presets.Remove(preset);
            }
        }

        [RelayCommand]
        private async Task ExportAsync()
        {
            var dialog = new SaveFileDialog().AddFilter("配置文件", "json");
            dialog.FileName = "FFmpeg工具箱 预设.json";
            SendMessage(new FileDialogMessage(dialog));
            string path = dialog.FileName;
            if (!string.IsNullOrEmpty(path))
            {
                var json = await presetManager.ExportAsync();
                File.WriteAllText(path, json, new UTF8Encoding());
                QueueSuccessMessage("导出成功");
            }
        }

        [RelayCommand]
        private async Task ImportAsync()
        {
            var dialog = new OpenFileDialog().AddFilter("配置文件", "json");
            SendMessage(new FileDialogMessage(dialog));
            string path = dialog.FileName;
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    await presetManager.ImportAsync(File.ReadAllText(path, new UTF8Encoding()));
                    await FillPresetsAsync();
                    QueueSuccessMessage("导入成功，同名预设已被更新");
                }
                catch (Exception ex)
                {
                    await CommonDialog.ShowErrorDialogAsync(ex, "导入失败");
                }
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            Debug.Assert(SelectedPreset != null);
            try
            {
                SelectedPreset.Arguments = CodeArgumentsViewModel.GetArguments();
                await presetManager.UpdatePresetAsync(SelectedPreset);
                SelectedPreset = null;
            }
            catch (Exception ex)
            {
                QueueErrorMessage("更新预设失败", ex);
            }
        }

        [RelayCommand]
        private async Task SetDefaultAsync()
        {
            Debug.Assert(SelectedPreset != null);
            Presets.ForEach(p => p.Default = false);
            SelectedPreset.Default = true;
            await presetManager.SetDefaultPresetAsync(SelectedPreset.Id);
        }

        [RelayCommand]
        private void Unselect()
        {
            SelectedPreset = null;
        }
    }
}