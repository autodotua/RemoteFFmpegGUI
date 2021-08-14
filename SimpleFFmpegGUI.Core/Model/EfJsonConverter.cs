using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace SimpleFFmpegGUI.Model
{
    public class EfJsonConverter<T> : ValueConverter<T, string>
    {
        public EfJsonConverter() : base(p => JsonConvert.SerializeObject(p), p => JsonConvert.DeserializeObject<T>(p))
        {
        }
    }
}