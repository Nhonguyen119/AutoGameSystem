using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoGameSystem.Models
{
    
        public enum TaskPriority
        {
            Low,
            Medium,
            High
        }

        public enum TaskCategory
        {
            Quest,
            Guild,
            Gift,
            Admin
        }

        public class GameTask
        {
            public string Id { get; set; } = Guid.NewGuid().ToString();
            public string Name { get; set; } = string.Empty;
            public TaskCategory Category { get; set; }
            public TaskPriority Priority { get; set; } = TaskPriority.Medium;
            public DateTime NextRun { get; set; } = DateTime.UtcNow;
            public DateTime LastRun { get; set; } = DateTime.MinValue;
            public bool IsEnabled { get; set; } = true;
            public int MaxRetries { get; set; } = 3;
            public int RetryCount { get; set; } = 0;
            public bool NeedsRecheck { get; set; } = false;
            public TimeSpan Cooldown { get; set; } = TimeSpan.Zero;
            public TimeSpan FallbackCooldown { get; set; } = TimeSpan.FromMinutes(10);
            public string[] TemplatePaths { get; set; } = Array.Empty<string>();
        }
    
}
