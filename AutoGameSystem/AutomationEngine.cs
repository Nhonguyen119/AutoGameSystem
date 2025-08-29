using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoGameSystem.Models;
using AutoGameSystem.Services;
using AutoGameSystem.Utilities;
using System.Diagnostics;
namespace AutoGameSystem.Core
{
    public class AutomationEngine
    {
        private readonly AppConfig _config;
        private readonly TaskScheduler _taskScheduler;
        private readonly AccountManager _accountManager;
        private readonly ImageRecognitionService _imageRecognition;
        private readonly OcrService _ocrService;
        private readonly InputService _inputService;
        private readonly WindowManager _windowManager;
        private readonly PopupHandler _popupHandler;

        private bool _isRunning = false;
        private Thread _engineThread;

        public event Action<string> OnLogMessage;
        public event Action<string, bool> OnTaskCompleted;

        public AutomationEngine(AppConfig config)
        {
            _config = config;
            _taskScheduler = new TaskScheduler();
            _accountManager = new AccountManager();
            _imageRecognition = new ImageRecognitionService(config);
            _ocrService = new OcrService();
            _inputService = new InputService(config);
            _windowManager = new WindowManager();
            _popupHandler = new PopupHandler(_imageRecognition, _inputService, _windowManager, config);
        }

        public void Start()
        {
            if (_isRunning) return;

            _isRunning = true;
            _engineThread = new Thread(EngineLoop);
            _engineThread.IsBackground = true;
            _engineThread.Start();

            OnLogMessage?.Invoke("Automation engine started");
        }

        public void Stop()
        {
            _isRunning = false;
            _engineThread?.Join(1000);

            OnLogMessage?.Invoke("Automation engine stopped");
        }

        private void EngineLoop()
        {
            while (_isRunning)
            {
                try
                {
                    // Get next task to execute
                    var task = _taskScheduler.GetNextTask();
                    if (task == null)
                    {
                        Thread.Sleep(2000);
                        continue;
                    }

                    // Get account for this task
                    var account = _accountManager.GetNextAccount();
                    if (account == null)
                    {
                        Thread.Sleep(2000);
                        continue;
                    }

                    // Execute the task
                    bool success = ExecuteTask(task, account);

                    // Update task status
                    task.LastRun = DateTime.UtcNow;
                    if (success)
                    {
                        task.RetryCount = 0;
                        OnTaskCompleted?.Invoke($"{task.Name} completed successfully", true);
                    }
                    else
                    {
                        task.RetryCount++;
                        if (task.RetryCount >= task.MaxRetries)
                        {
                            task.IsEnabled = false;
                            OnTaskCompleted?.Invoke($"{task.Name} failed after {task.MaxRetries} attempts", false);
                        }
                    }

                    // Save updated tasks
                    _taskScheduler.SaveTasks();
                }
                catch (Exception ex)
                {
                    OnLogMessage?.Invoke($"Engine error: {ex.Message}");
                    Thread.Sleep(5000);
                }
            }
        }

        private bool ExecuteTask(GameTask task, Account account)
        {
            OnLogMessage?.Invoke($"Executing task: {task.Name} for account: {account.Name}");

            // Find emulator window
            IntPtr hWnd = _windowManager.FindEmulatorWindow(account.EmulatorWindowTitle);
            if (hWnd == IntPtr.Zero)
            {
                OnLogMessage?.Invoke($"Window not found: {account.EmulatorWindowTitle}");
                return false;
            }

            // Bring window to front
            if (!_windowManager.BringWindowToFront(hWnd))
            {
                OnLogMessage?.Invoke($"Failed to bring window to front: {account.EmulatorWindowTitle}");
                return false;
            }

            Thread.Sleep(_config.ScreenshotDelayMs);

            // Capture screenshot
            using (Bitmap screenshot = _windowManager.CaptureWindow(hWnd))
            {
                // Handle popups
                if (!_popupHandler.HandlePopups(hWnd, screenshot))
                {
                    OnLogMessage?.Invoke("Failed to handle popups");
                    if (_config.DebugMode) SaveDebugScreenshot(screenshot, "popup_fail");
                    return false;
                }

                // Execute task-specific logic
                bool success = false;
                switch (task.Category)
                {
                    case TaskCategory.Gift:
                        success = ExecuteOpenGiftTask(task, hWnd, screenshot);
                        break;
                    case TaskCategory.Admin:
                        success = ExecuteAdminQuestTask(task, hWnd, screenshot);
                        break;
                    case TaskCategory.Guild:
                        success = ExecuteGuildQuestTask(task, hWnd, screenshot);
                        break;
                    default:
                        OnLogMessage?.Invoke($"Unknown task category: {task.Category}");
                        break;
                }

                return success;
            }
        }

        private bool ExecuteOpenGiftTask(GameTask task, IntPtr hWnd, Bitmap screenshot)
        {
            try
            {
                // Implementation for opening gifts
                // This would use the template paths from the task to find and click on gift elements
                return true;
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke($"Error in OpenGiftTask: {ex.Message}");
                return false;
            }
        }

        private bool ExecuteAdminQuestTask(GameTask task, IntPtr hWnd, Bitmap screenshot)
        {
            try
            {
                // Implementation for admin quests
                return true;
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke($"Error in AdminQuestTask: {ex.Message}");
                return false;
            }
        }

        private bool ExecuteGuildQuestTask(GameTask task, IntPtr hWnd, Bitmap screenshot)
        {
            try
            {
                // Implementation for guild quests
                return true;
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke($"Error in GuildQuestTask: {ex.Message}");
                return false;
            }
        }

        private void SaveDebugScreenshot(Bitmap screenshot, string context)
        {
            try
            {
                string debugPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    _config.DebugScreenshotPath, DateTime.Now.ToString("yyyyMMdd"));

                if (!Directory.Exists(debugPath))
                    Directory.CreateDirectory(debugPath);

                string filename = $"{DateTime.Now:HHmmss}_{context}.png";
                screenshot.Save(Path.Combine(debugPath, filename));
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke($"Failed to save debug screenshot: {ex.Message}");
            }
        }

        public void LoadData()
        {
            _taskScheduler.LoadTasks();
            _accountManager.LoadAccounts();
            OnLogMessage?.Invoke("Data loaded successfully");
        }

        public void SaveData()
        {
            _taskScheduler.SaveTasks();
            _accountManager.SaveAccounts();
            OnLogMessage?.Invoke("Data saved successfully");
        }
    }
}
