using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoGameSystem.Models;
using AutoGameSystem.Services;
using AutoGameSystem.Utilities;
using System.Drawing;

namespace AutoGameSystem.Core
{
    public class PopupHandler
    {
        private readonly ImageRecognitionService _imageRecognition;
        private readonly InputService _inputService;
        private readonly WindowManager _windowManager;
        private readonly AppConfig _config;

        private readonly List<string> _popupTemplates = new List<string>
        {
            "Popup/CloseX.png",
            "Popup/Ok.png",
            "Popup/Cancel.png",
            "Popup/Claim.png"
        };

        public PopupHandler(ImageRecognitionService imageRecognition, InputService inputService,
                          WindowManager windowManager, AppConfig config)
        {
            _imageRecognition = imageRecognition;
            _inputService = inputService;
            _windowManager = windowManager;
            _config = config;
        }

        public bool HandlePopups(IntPtr hWnd, Bitmap screenshot)
        {
            int maxAttempts = 5;
            int attempt = 0;
            string lastHash = "";

            while (attempt < maxAttempts)
            {
                bool foundPopup = false;

                foreach (var template in _popupTemplates)
                {
                    string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", template);
                    if (!File.Exists(templatePath))
                        continue;

                    var point = _imageRecognition.FindImage(screenshot, templatePath);
                    if (point.HasValue)
                    {
                        foundPopup = true;
                        _inputService.Click(hWnd, point.Value);
                        Thread.Sleep(300 + attempt * 50); // Increasing delay with each attempt

                        // Take new screenshot to check if popup is gone
                        using (var newScreenshot = _windowManager.CaptureWindow(hWnd))
                        {
                            screenshot = new Bitmap(newScreenshot);
                            string currentHash = GetImageHash(screenshot);

                            // Check if we're stuck on the same popup
                            if (currentHash == lastHash)
                            {
                                attempt++;
                            }
                            else
                            {
                                lastHash = currentHash;
                            }
                        }
                        break;
                    }
                }

                if (!foundPopup)
                    return true;

                attempt++;
            }

            Logger.Error("Failed to handle popups after multiple attempts");
            return false;
        }

        private string GetImageHash(Bitmap image)
        {
            // Simple hash based on image dimensions and a few pixel values
            // For a real implementation, consider a proper image hashing algorithm
            int hash = image.Width.GetHashCode() ^ image.Height.GetHashCode();

            // Sample a few pixels
            for (int i = 0; i < 10; i++)
            {
                int x = i * image.Width / 10;
                int y = i * image.Height / 10;
                if (x < image.Width && y < image.Height)
                {
                    Color pixel = image.GetPixel(x, y);
                    hash ^= pixel.GetHashCode();
                }
            }

            return hash.ToString();
        }
    }
}
