using System.Collections.Generic;

namespace SimpleFFmpegGUI.Dto
{
    public class PagedListDto<T> : List<T>
    {
        public PagedListDto()
        {
        }

        public PagedListDto(IEnumerable<T> collection, int totalCount) : base(collection)
        {
            TotalCount = totalCount;
        }

        public int TotalCount { get; set; }
    }
}