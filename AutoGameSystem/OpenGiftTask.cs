using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoGameSystem.Models;
using AutoGameSystem.Services;
using AutoGameSystem.Utilities;
using System.Drawing;

namespace AutoGameSystem.Tasks
{
    public class OpenGiftTask
    {
        private readonly ImageRecognitionService _imageRecognition;
        private readonly OcrService _ocrService;
        private readonly InputService _inputService;
        private readonly WindowManager _windowManager;
        private readonly AppConfig _config;

        public OpenGiftTask(ImageRecognitionService imageRecognition, OcrService ocrService,
                          InputService inputService, WindowManager windowManager, AppConfig config)
        {
            _imageRecognition = imageRecognition;
            _ocrService = ocrService;
            _inputService = inputService;
            _windowManager = windowManager;
            _config = config;
        }

        public bool Execute(IntPtr hWnd, GameTask task)
        {
            try
            {
                // Step 1: Capture screenshot
                using (Bitmap screenshot = _windowManager.CaptureWindow(hWnd))
                {
                    // Step 2: Find and click gift box
                    string giftBoxPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Gift", "giftbox.png");
                    var giftBoxPoint = _imageRecognition.FindImage(screenshot, giftBoxPath);
                    if (!giftBoxPoint.HasValue)
                    {
                        Logger.Error("Gift box not found");
                        return false;
                    }

                    _inputService.Click(hWnd, giftBoxPoint.Value);
                    Thread.Sleep(_config.ScreenshotDelayMs);

                    // Step 3: Find and click open button
                    using (Bitmap newScreenshot = _windowManager.CaptureWindow(hWnd))
                    {
                        string openButtonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Gift", "open_button.png");
                        var openButtonPoint = _imageRecognition.FindImage(newScreenshot, openButtonPath);
                        if (!openButtonPoint.HasValue)
                        {
                            Logger.Error("Open button not found");
                            return false;
                        }

                        _inputService.Click(hWnd, openButtonPoint.Value);
                        Thread.Sleep(_config.ScreenshotDelayMs);

                        // Step 4: Read cooldown if available
                        using (Bitmap finalScreenshot = _windowManager.CaptureWindow(hWnd))
                        {
                            string cooldownIconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Gift", "cooldown_icon.png");
                            var cooldownIconPoint = _imageRecognition.FindImage(finalScreenshot, cooldownIconPath);

                            if (cooldownIconPoint.HasValue)
                            {
                                Point offset = new Point(50, 0); // Adjust based on UI layout
                                Size size = new Size(100, 30); // Adjust based on UI layout

                                TimeSpan cooldown = _ocrService.ReadCooldown(finalScreenshot, cooldownIconPoint.Value, offset, size);
                                if (cooldown > TimeSpan.Zero)
                                {
                                    task.NextRun = DateTime.UtcNow + cooldown + TimeSpan.FromSeconds(new Random().Next(3, 10));
                                    task.Cooldown = cooldown;
                                }
                                else
                                {
                                    task.NextRun = DateTime.UtcNow + task.FallbackCooldown;
                                    task.NeedsRecheck = true;
                                }
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in OpenGiftTask: {ex.Message}");
                return false;
            }
        }
    }
}
