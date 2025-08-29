using AutoGameSystem.Utilities;
using System.Drawing;
using System.Text.RegularExpressions;
using Tesseract;

namespace AutoGameSystem.Services
{
    public class OcrService
    {
        private readonly string _tessdataPath;

        public OcrService()
        {
            _tessdataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");
        }

        public string ExtractText(Bitmap image, Rectangle? roi = null)
        {
            try
            {
                using (var engine = new TesseractEngine(_tessdataPath, "eng", EngineMode.Default))
                {
                    engine.SetVariable("tessedit_char_whitelist", "0123456789:dhms");
                    engine.SetVariable("tessedit_pageseg_mode", "7"); // PSM_SINGLE_LINE

                    using (var pix = PixConverter.ToPix(image))
                    {
                        if (roi.HasValue)
                        {
                            // FIX: Sử dụng phương thức khác để crop
                            // Tạo bitmap mới từ ROI và chuyển sang Pix
                            using (Bitmap croppedBitmap = new Bitmap(roi.Value.Width, roi.Value.Height))
                            using (Graphics g = Graphics.FromImage(croppedBitmap))
                            {
                                g.DrawImage(image, new Rectangle(0, 0, roi.Value.Width, roi.Value.Height),
                                           roi.Value, GraphicsUnit.Pixel);

                                using (var croppedPix = PixConverter.ToPix(croppedBitmap))
                                using (var page = engine.Process(croppedPix))
                                {
                                    return page.GetText().Trim();
                                }
                            }
                        }
                        else
                        {
                            using (var page = engine.Process(pix))
                            {
                                return page.GetText().Trim();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"OCR Error: {ex.Message}");
                return string.Empty;
            }
        }

        // Các phương thức khác giữ nguyên...
        public TimeSpan ParseCooldown(string text)
        {
            // Giữ nguyên implementation cũ
            if (string.IsNullOrEmpty(text))
                return TimeSpan.Zero;

            try
            {
                // Patterns: HH:MM:SS, 1d2h, 45m, 30s, etc.
                var patterns = new[]
                {
                    new Regex(@"(\d+):(\d+):(\d+)"), // HH:MM:SS
                    new Regex(@"(\d+)d(\d+)h(\d+)m(\d+)s"), // 1d2h3m4s
                    new Regex(@"(\d+)d(\d+)h(\d+)m"), // 1d2h3m
                    new Regex(@"(\d+)d(\d+)h"), // 1d2h
                    new Regex(@"(\d+)d"), // 1d
                    new Regex(@"(\d+)h(\d+)m(\d+)s"), // 2h3m4s
                    new Regex(@"(\d+)h(\d+)m"), // 2h3m
                    new Regex(@"(\d+)h"), // 2h
                    new Regex(@"(\d+)m(\d+)s"), // 3m4s
                    new Regex(@"(\d+)m"), // 3m
                    new Regex(@"(\d+)s") // 4s
                };

                foreach (var pattern in patterns)
                {
                    var match = pattern.Match(text);
                    if (match.Success)
                    {
                        int days = 0, hours = 0, minutes = 0, seconds = 0;

                        if (pattern.ToString().Contains(@":") && match.Groups.Count >= 4)
                        {
                            // HH:MM:SS format
                            hours = int.Parse(match.Groups[1].Value);
                            minutes = int.Parse(match.Groups[2].Value);
                            seconds = int.Parse(match.Groups[3].Value);
                        }
                        else
                        {
                            // Component-based format (d, h, m, s)
                            for (int i = 1; i < match.Groups.Count; i++)
                            {
                                string value = match.Groups[i].Value;
                                if (value.EndsWith("d")) days = int.Parse(value.TrimEnd('d'));
                                else if (value.EndsWith("h")) hours = int.Parse(value.TrimEnd('h'));
                                else if (value.EndsWith("m")) minutes = int.Parse(value.TrimEnd('m'));
                                else if (value.EndsWith("s")) seconds = int.Parse(value.TrimEnd('s'));
                                else if (i == 1) seconds = int.Parse(value); // Fallback for single numbers
                            }
                        }

                        return new TimeSpan(days, hours, minutes, seconds);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error parsing cooldown '{text}': {ex.Message}");
            }

            return TimeSpan.Zero;
        }

        public TimeSpan ReadCooldown(Bitmap screenshot, Point anchorPoint, Point offset, Size size)
        {
            Rectangle roi = new Rectangle(
                anchorPoint.X + offset.X,
                anchorPoint.Y + offset.Y,
                size.Width,
                size.Height
            );

            // Preprocess image for better OCR
            using (Bitmap processedImage = PreprocessImage(screenshot, roi))
            {
                string text = ExtractText(processedImage);
                return ParseCooldown(text);
            }
        }

        private Bitmap PreprocessImage(Bitmap image, Rectangle roi)
        {
            // Crop to ROI
            using (Bitmap cropped = new Bitmap(roi.Width, roi.Height))
            using (Graphics g = Graphics.FromImage(cropped))
            {
                g.DrawImage(image, new Rectangle(0, 0, roi.Width, roi.Height), roi, GraphicsUnit.Pixel);

                // Convert to grayscale
                using (Bitmap grayscale = new Bitmap(cropped.Width, cropped.Height))
                {
                    for (int y = 0; y < cropped.Height; y++)
                    {
                        for (int x = 0; x < cropped.Width; x++)
                        {
                            Color pixel = cropped.GetPixel(x, y);
                            int grayValue = (int)(pixel.R * 0.3 + pixel.G * 0.59 + pixel.B * 0.11);
                            grayscale.SetPixel(x, y, Color.FromArgb(grayValue, grayValue, grayValue));
                        }
                    }

                    // Increase contrast
                    using (Bitmap highContrast = new Bitmap(grayscale.Width, grayscale.Height))
                    {
                        for (int y = 0; y < grayscale.Height; y++)
                        {
                            for (int x = 0; x < grayscale.Width; x++)
                            {
                                Color pixel = grayscale.GetPixel(x, y);
                                int newValue = pixel.R < 128 ? 0 : 255;
                                highContrast.SetPixel(x, y, Color.FromArgb(newValue, newValue, newValue));
                            }
                        }

                        return new Bitmap(highContrast);
                    }
                }
            }
        }
    }
}