using FzLib;
using Mapster;
using SimpleFFmpegGUI.Manager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SimpleFFmpegGUI.WPF.Model
{
    public class AllTasks : TaskCollectionBase
    {
        public AllTasks(TaskManager tm)
        {
            taskManager = tm;
            RefreshAsync();
        }

        private int page = 0;

        public int Page
        {
            get => page;
            set
            {
                this.SetValueAndNotify(ref page, value, nameof(Page));
                RefreshAsync();
            }
        }

        private int countPerPage = 20;

        public int CountPerPage
        {
            get => countPerPage;
            set
            {
                this.SetValueAndNotify(ref countPerPage, value, nameof(CountPerPage));
                RefreshAsync();
            }
        }

        private int pageCount;

        public int PageCount
        {
            get => pageCount;
            set => this.SetValueAndNotify(ref pageCount, value, nameof(PageCount));
        }

        private int count;
        private readonly TaskManager taskManager;

        public int Count
        {
            get => count;
            set => this.SetValueAndNotify(ref count, value, nameof(Count));
        }

        public override async Task RefreshAsync()
        {
            var tasks = await taskManager.GetTasksAsync(null, Page * CountPerPage, CountPerPage);
            Count = tasks.TotalCount;
            PageCount = (int)Math.Ceiling(1.0 * Count / CountPerPage);
            Tasks = new ObservableCollection<UITaskInfo>(tasks.List.Adapt<List<UITaskInfo>>());
        }
    }
}