using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoGameSystem.Models
{
    public class AppConfig
    {
        public int ScreenshotDelayMs { get; set; } = 300;
        public int ClickDelayMs { get; set; } = 200;
        public int JitterRangeMs { get; set; } = 50;
        public double ImageMatchThreshold { get; set; } = 0.85;
        public bool DebugMode { get; set; } = false;
        public string DebugScreenshotPath { get; set; } = "DebugScreenshots";
    }
}
