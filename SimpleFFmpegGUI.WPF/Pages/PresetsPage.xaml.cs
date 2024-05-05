using Enterwell.Clients.Wpf.Notifications;
using FzLib;
using FzLib.WPF;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
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
using CommonDialog = ModernWpf.FzExtension.CommonDialog.CommonDialog;

namespace SimpleFFmpegGUI.WPF.Pages
{
    /// <summary>
    /// Interaction logic for PresetsPage.xaml
    /// </summary>
    public partial class PresetsPage : UserControl
    {
        private readonly PresetManager presetManager;

        public PresetsPage(PresetsPageViewModel viewModel, PresetManager presetManager)
        {
            ViewModel = viewModel;
            this.presetManager = presetManager;
            DataContext = ViewModel;
            InitializeComponent();
            Loaded += async (s, e) => await ViewModel.FillPresetsAsync();
        }

        public PresetsPageViewModel ViewModel { get; set; }
        private async void DeleteAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (await CommonDialog.ShowYesNoDialogAsync("删除预设", $"是否删除所有类型的所有预设？"))
            {
                IsEnabled = false;
                try
                {
                    await presetManager.DeletePresetsAsync();
                    await ViewModel.FillPresetsAsync();
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

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var preset = (sender as Button).DataContext as CodePreset;
            if (await CommonDialog.ShowYesNoDialogAsync("删除预设", $"是否删除“{preset.Name}”？"))
            {
                await presetManager.DeletePresetAsync(preset.Id);
                ViewModel.Presets.Remove(preset);
            }
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog().AddFilter("配置文件", "json");
            dialog.FileName = "FFmpeg工具箱 预设.json";
            string path = dialog.GetPath(this.GetWindow());
            if (path != null)
            {
                var json = await presetManager.ExportAsync();
                File.WriteAllText(path, json, new UTF8Encoding());
                this.CreateMessage().QueueSuccess("导出成功");
            }
        }

        private async void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog().AddFilter("配置文件", "json");
            string path = dialog.GetPath(this.GetWindow());
            if (path != null)
            {
                try
                {
                    await presetManager.ImportAsync(File.ReadAllText(path, new UTF8Encoding()));
                    await ViewModel.FillPresetsAsync();
                    this.CreateMessage().QueueSuccess("导入成功，同名预设已被更新");
                }
                catch (Exception ex)
                {
                    await CommonDialog.ShowErrorDialogAsync(ex, "导入失败");
                }
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.RemovedItems.Count > 0 && e.RemovedItems[0] is CodePreset oldPreset)
            {
            }
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is CodePreset preset)
            {

                grd.RowDefinitions[2].Height = new GridLength(48);
                lvw.IsHitTestVisible = false;
                lvw.ScrollIntoView(preset);
                grd.RowDefinitions[4].Height = new GridLength(1, GridUnitType.Star);
                argumentsPanel.Update(preset.Type, preset.Arguments);
            }
            else
            {
                grd.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);
                lvw.IsHitTestVisible = true;
                grd.RowDefinitions[4].Height = new GridLength(0);
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.Assert(ViewModel.SelectedPreset != null);
            try
            {
                ViewModel.SelectedPreset.Arguments = argumentsPanel.GetOutputArguments();
                await presetManager.UpdatePresetAsync(ViewModel.SelectedPreset);
                ViewModel.SelectedPreset = null;
            }
            catch (Exception ex)
            {
                this.CreateMessage().QueueError("更新预设失败", ex);
            }
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectedPreset = null;
        }

        private async void SetDefaultButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.Assert(ViewModel.SelectedPreset != null);
            ViewModel.Presets.ForEach(p => p.Default = false);
            ViewModel.SelectedPreset.Default = true;
            await presetManager.SetDefaultPresetAsync(ViewModel.SelectedPreset.Id);
        }
    }

    public class PresetsPageViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<CodePreset> presets;

        private CodePreset selectedPreset;

        private TaskType type = TaskType.Code;
        private readonly PresetManager presetManager;

        public PresetsPageViewModel(PresetManager presetManager)
        {
            this.presetManager = presetManager;
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<CodePreset> Presets
        {
            get => presets;
            set => this.SetValueAndNotify(ref presets, value, nameof(Presets));
        }

        public CodePreset SelectedPreset
        {
            get => selectedPreset;
            set => this.SetValueAndNotify(ref selectedPreset, value, nameof(SelectedPreset));
        }


        public IEnumerable TaskTypes => Enum.GetValues(typeof(TaskType));
        public TaskType Type
        {
            get => type;
            set
            {
                this.SetValueAndNotify(ref type, value, nameof(Type));
                FillPresetsAsync();
            }
        }

        public async Task FillPresetsAsync()
        {
            Presets = new ObservableCollection<CodePreset>(await presetManager.GetPresetsAsync(Type));
        }

    }
}