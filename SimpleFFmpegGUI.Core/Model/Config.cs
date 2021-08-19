using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleFFmpegGUI.Model
{
    [Index(nameof(Key))]
    public class Config : ModelBase
    {
        public Config()
        {
        }

        public Config(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; set; }
        public string Value { get; set; }
    }
}