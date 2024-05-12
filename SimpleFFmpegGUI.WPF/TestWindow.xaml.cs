﻿using FzLib;
using FzLib.WPF;
using FzLib.WPF.Converters;
using Microsoft.Win32;
using ModernWpf.Controls;
using ModernWpf.FzExtension.CommonDialog;
using SimpleFFmpegGUI.FFmpegLib;
using SimpleFFmpegGUI.Manager;
using SimpleFFmpegGUI.Model;
using SimpleFFmpegGUI.WPF.Converters;
using SimpleFFmpegGUI.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using static SimpleFFmpegGUI.WPF.ViewModels.PerformanceTestLine;
using static SimpleFFmpegGUI.WPF.ViewModels.TestWindowViewModel;
using static System.Net.Mime.MediaTypeNames;
using CommonDialog = ModernWpf.FzExtension.CommonDialog.CommonDialog;

namespace SimpleFFmpegGUI.WPF
{
    public partial class TestWindow : Window
    {
        public TestWindow()
        {
            ViewModel = this.SetDataContext<TestWindowViewModel>();
            InitializeComponent();
            CreateDataGrids();
            CreateTestItemsDataGrid();
        }

        public TestWindowViewModel ViewModel { get; set; }

        /// <summary>
        /// 动态创建表格
        /// </summary>
        private void CreateDataGrids()
        {
            grpSpeed.Content = GetDataGrid(nameof(PerformanceTestItem.FPS), "0.00");
            grpSSIM.Content = GetDataGrid(nameof(PerformanceTestItem.SSIM), "0.00%");
            grpPSNR.Content = GetDataGrid(nameof(PerformanceTestItem.PSNR), "0.00");
            grpVMAF.Content = GetDataGrid(nameof(PerformanceTestItem.VMAF), "0.00");
            grpCpuUsage.Content = GetDataGrid(nameof(PerformanceTestItem.CpuUsage), "0.00%");
            grpOutputSize.Content = GetDataGrid(nameof(PerformanceTestItem.OutputSize), "0.00");

            DataGrid GetDataGrid(string type, string format = null)
            {
                DataGrid dataGrid = new DataGrid()
                {
                    AutoGenerateColumns = false,
                    CanUserAddRows = false,
                    CanUserDeleteRows = false,
                    CanUserReorderColumns = false,
                    CanUserResizeColumns = false,
                    CanUserResizeRows = false,
                    CanUserSortColumns = false,
                    HeadersVisibility = DataGridHeadersVisibility.Column,
                    SelectionUnit = DataGridSelectionUnit.Cell,
                    IsReadOnly = true,
                    IsHitTestVisible = false,
                };
                dataGrid.SetBinding(DataGrid.IsEnabledProperty,
                    new Binding(nameof(TestWindowViewModel.IsTesting)) { Converter = new InverseBoolConverter() });
                dataGrid.SetBinding(DataGrid.ItemsSourceProperty,
                   new Binding(nameof(TestWindowViewModel.Tests)));
                DataGridTextColumn c = new DataGridTextColumn()
                {
                    Binding = new Binding(nameof(PerformanceTestLine.Header)),
                    IsReadOnly = true,
                };
                dataGrid.Columns.Add(c);
                for (int i = 0; i < CodecsCount; i++)
                {
                    c = new DataGridTextColumn()
                    {
                        Width = 108,
                        Header = ViewModel.Codecs[i].Name,
                        Binding = new Binding($"{nameof(PerformanceTestLine.Items)}[{i}].{type}")
                        {
                            Converter = new EmptyIfZeroConverter()
                        }
                    };
                    if (!string.IsNullOrEmpty(format))
                    {
                        c.Binding.StringFormat = format;
                    }
                    dataGrid.Columns.Add(c);
                }
                return dataGrid;
            }
        }

        private void CreateTestItemsDataGrid()
        {
            DataGrid dataGrid = new DataGrid()
            {
                AutoGenerateColumns = false,
                CanUserAddRows = false,
                CanUserDeleteRows = false,
                CanUserReorderColumns = false,
                CanUserResizeColumns = false,
                CanUserResizeRows = false,
                CanUserSortColumns = false,
                HeadersVisibility = DataGridHeadersVisibility.Column,
                SelectionUnit = DataGridSelectionUnit.Cell,
            };
            dataGrid.SetBinding(DataGrid.IsEnabledProperty,
                new Binding(nameof(TestWindowViewModel.IsTesting)) { Converter = new InverseBoolConverter() });
            dataGrid.SetBinding(DataGrid.ItemsSourceProperty,
               new Binding(nameof(TestWindowViewModel.Tests)));
            var c = new DataGridTextColumn()
            {
                Binding = new Binding(nameof(PerformanceTestLine.Header)),
                IsReadOnly = true,
            };
            dataGrid.Columns.Add(c);
            for (int i = 0; i < CodecsCount; i++)
            {
                var codec = VideoCodec.VideoCodecs[i];
                var tc = new DataGridTemplateColumn
                {
                    Width = 108,
                    Header = codec.Name,
                    CellTemplate = new DataTemplate()
                };
                FrameworkElementFactory factory = new FrameworkElementFactory(typeof(CheckBox));
                factory.SetValue(MarginProperty, new Thickness(16, 0, 0, 0));
                factory.SetBinding(CheckBox.IsCheckedProperty, new Binding($"{nameof(PerformanceTestLine.Items)}[{i}].{nameof(PerformanceTestItem.IsChecked)}")
                {
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });
                tc.CellTemplate.VisualTree = factory;
                dataGrid.Columns.Add(tc);
            }
            c = new DataGridTextColumn()
            {
                Width = 108,
                Binding = new Binding(nameof(PerformanceTestLine.MBitrate)),
                Header = "码率 (Mbps)"
            };
            dataGrid.Columns.Add(c);
            grpTestItems.Content = dataGrid;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ViewModel.SaveConfigs();

            if (ViewModel.IsTesting)
            {
                e.Cancel = true;
            }
        }
    }
}