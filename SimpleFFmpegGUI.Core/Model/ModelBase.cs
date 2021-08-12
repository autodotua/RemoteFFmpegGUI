using System.ComponentModel.DataAnnotations;

namespace SimpleFFmpegGUI.Model
{
    public abstract class ModelBase
    {
        [Key]
        public int ID { get; set; }

        public bool IsDeleted { get; set; }
    }
}