using Newtonsoft.Json;
using System.Collections.Generic;

namespace SimpleFFmpegGUI.Dto
{
    public class PagedListDto<T>
    {
        public PagedListDto()
        {
            List = new List<T>();
        }

        public PagedListDto(IList<T> list, int totalCount)
        {
            TotalCount = totalCount;
            List = list;
        }

        public IList<T> List { get; set; }

        [JsonProperty]
        public int TotalCount { get; set; }
    }
}