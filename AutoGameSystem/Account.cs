using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoGameSystem.Models
{
    public class Account
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string EmulatorWindowTitle { get; set; } = string.Empty;
        public double ScaleFactor { get; set; } = 1.0;
        public bool IsActive { get; set; } = true;
        public DateTime LastRun { get; set; } = DateTime.MinValue;
    }
}
