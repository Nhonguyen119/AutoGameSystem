using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using AutoGameSystem.Models;
using Newtonsoft.Json;
using System.IO;
using Formatting = Newtonsoft.Json.Formatting;

namespace AutoGameSystem.Utilities
{
    public static class ConfigManager
    {
        private static readonly string BasePath = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string DataPath = Path.Combine(BasePath, "Data");
        private static readonly string ConfigPath = Path.Combine(DataPath, "appsettings.json");
        private static readonly string TasksPath = Path.Combine(DataPath, "tasks.json");
        private static readonly string AccountsPath = Path.Combine(DataPath, "accounts.json");

        static ConfigManager()
        {
            if (!Directory.Exists(DataPath))
                Directory.CreateDirectory(DataPath);
        }

        public static AppConfig LoadAppConfig()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    var json = File.ReadAllText(ConfigPath);
                    return JsonConvert.DeserializeObject<AppConfig>(json) ?? new AppConfig();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error loading app config: {ex.Message}");
            }

            return new AppConfig();
        }

        public static void SaveAppConfig(AppConfig config)
        {
            try
            {
                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error saving app config: {ex.Message}");
            }
        }

        public static List<GameTask> LoadTasks()
        {
            try
            {
                if (File.Exists(TasksPath))
                {
                    var json = File.ReadAllText(TasksPath);
                    return JsonConvert.DeserializeObject<List<GameTask>>(json) ?? new List<GameTask>();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error loading tasks: {ex.Message}");
            }

            return new List<GameTask>();
        }

        public static void SaveTasks(List<GameTask> tasks)
        {
            try
            {
                var json = JsonConvert.SerializeObject(tasks, Formatting.Indented);
                File.WriteAllText(TasksPath, json);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error saving tasks: {ex.Message}");
            }
        }

        public static List<Account> LoadAccounts()
        {
            try
            {
                if (File.Exists(AccountsPath))
                {
                    var json = File.ReadAllText(AccountsPath);
                    return JsonConvert.DeserializeObject<List<Account>>(json) ?? new List<Account>();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error loading accounts: {ex.Message}");
            }

            return new List<Account>();
        }

        public static void SaveAccounts(List<Account> accounts)
        {
            try
            {
                var json = JsonConvert.SerializeObject(accounts, Formatting.Indented);
                File.WriteAllText(AccountsPath, json);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error saving accounts: {ex.Message}");
            }
        }
    }
}
