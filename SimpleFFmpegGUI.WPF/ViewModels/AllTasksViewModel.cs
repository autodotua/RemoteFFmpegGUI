﻿using CommunityToolkit.Mvvm.ComponentModel;
using FzLib;
using Mapster;
using SimpleFFmpegGUI.Manager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SimpleFFmpegGUI.WPF.ViewModels
{
    public partial class AllTasksViewModel : TaskCollectionViewModelBase
    {
        private readonly TaskManager taskManager;

        [ObservableProperty]
        private int count;

        [ObservableProperty]
        private int countPerPage = 20;

        [ObservableProperty]
        private int page = 0;

        [ObservableProperty]
        private int pageCount;

        public AllTasksViewModel(TaskManager tm)
        {
            taskManager = tm;
            RefreshAsync();
        }
        public override async Task RefreshAsync()
        {
            var tasks = await taskManager.GetTasksAsync(null, Page * CountPerPage, CountPerPage);
            Count = tasks.TotalCount;
            PageCount = (int)Math.Ceiling(1.0 * Count / CountPerPage);
            Tasks = new ObservableCollection<TaskInfoViewModel>(tasks.List.Adapt<List<TaskInfoViewModel>>());
        }

        protected override async void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName is nameof(Page) or nameof(CountPerPage))
            {
                await
                RefreshAsync();
            }
        }
    }
}