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

        public PagedListDto(IEnumerable<T> collection, int totalCount)
        {
            TotalCount = totalCount;
            List = new List<T>(collection);
        }

        public List<T> List { get; set; }

        [JsonProperty]
        public int TotalCount { get; set; }
    }
}