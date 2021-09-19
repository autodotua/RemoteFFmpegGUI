using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace SimpleFFmpegGUI.Model
{
    public class EFJsonConverter<T> : ValueConverter<T, string>
    {
        public EFJsonConverter() : base(p => JsonConvert.SerializeObject(p), p => JsonConvert.DeserializeObject<T>(p))
        {
        }
    }
}