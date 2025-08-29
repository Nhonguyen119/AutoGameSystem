using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoGameSystem.Models;
using AutoGameSystem.Utilities;


namespace AutoGameSystem.Core
{
    public class TaskScheduler
    {
        private List<GameTask> _tasks;

        public TaskScheduler()
        {
            _tasks = new List<GameTask>();
        }

        public void LoadTasks()
        {
            _tasks = ConfigManager.LoadTasks();
        }

        public void SaveTasks()
        {
            ConfigManager.SaveTasks(_tasks);
        }

        public GameTask GetNextTask()
        {
            var now = DateTime.UtcNow;
            return _tasks
                .Where(t => t.IsEnabled && t.NextRun <= now)
                .OrderByDescending(t => t.Priority)
                .ThenBy(t => t.NextRun)
                .FirstOrDefault();
        }

        public void AddTask(GameTask task)
        {
            _tasks.Add(task);
            SaveTasks();
        }

        public void RemoveTask(string taskId)
        {
            _tasks.RemoveAll(t => t.Id == taskId);
            SaveTasks();
        }

        public void UpdateTask(GameTask updatedTask)
        {
            var existingTask = _tasks.FirstOrDefault(t => t.Id == updatedTask.Id);
            if (existingTask != null)
            {
                _tasks.Remove(existingTask);
                _tasks.Add(updatedTask);
                SaveTasks();
            }
        }

        public List<GameTask> GetAllTasks()
        {
            return _tasks.OrderBy(t => t.NextRun).ToList();
        }
    }
}
