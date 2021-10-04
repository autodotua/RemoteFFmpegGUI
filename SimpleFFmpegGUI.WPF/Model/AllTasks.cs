using FzLib;
using Mapster;
using SimpleFFmpegGUI.Manager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SimpleFFmpegGUI.WPF.Model
{
    public class AllTasks : TaskCollectionBase
    {
        public AllTasks()
        {
            Refresh();
        }

        private int page = 0;

        public int Page
        {
            get => page;
            set
            {
                this.SetValueAndNotify(ref page, value, nameof(Page));
                Refresh();
            }
        }

        private int countPerPage = 20;

        public int CountPerPage
        {
            get => countPerPage;
            set
            {
                this.SetValueAndNotify(ref countPerPage, value, nameof(CountPerPage));
                Refresh();
            }
        }

        private int pageCount;

        public int PageCount
        {
            get => pageCount;
            set => this.SetValueAndNotify(ref pageCount, value, nameof(PageCount));
        }

        private int count;

        public int Count
        {
            get => count;
            set => this.SetValueAndNotify(ref count, value, nameof(Count));
        }

        public override void Refresh()
        {
            var tasks = TaskManager.GetTasks(null, Page * CountPerPage, CountPerPage);
            Count = tasks.TotalCount;
            PageCount = (int)Math.Ceiling(1.0 * Count / CountPerPage);
            Tasks = new ObservableCollection<UITaskInfo>(tasks.List.Adapt<List<UITaskInfo>>());
        }
    }
}