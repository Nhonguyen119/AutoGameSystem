using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoGameSystem.Models;
using AutoGameSystem.Utilities;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace AutoGameSystem.Services
{
    public class ImageRecognitionService
    {
        private readonly AppConfig _config;

        public ImageRecognitionService(AppConfig config)
        {
            _config = config;
        }

        public Point? FindImage(Bitmap screenshot, string templatePath, double threshold = -1)
        {
            if (threshold < 0) threshold = _config.ImageMatchThreshold;

            try
            {
                //using (Image<Bgr, byte> sourceImage = new Image<Bgr, byte>(screenshot))
                using (Image<Bgr, byte> sourceImage = screenshot.ToImage<Bgr, byte>())
                using (Image<Bgr, byte> templateImage = new Image<Bgr, byte>(templatePath))
                {
                    using (Image<Gray, float> result = sourceImage.MatchTemplate(templateImage, TemplateMatchingType.CcoeffNormed))
                    {
                        double[] minValues, maxValues;
                        Point[] minLocations, maxLocations;
                        result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                        if (maxValues[0] >= threshold)
                        {
                            Point matchLocation = maxLocations[0];
                            Rectangle matchRect = new Rectangle(
                                matchLocation.X,
                                matchLocation.Y,
                                templateImage.Width,
                                templateImage.Height
                            );

                            // Return center point of the matched area
                            return new Point(
                                matchRect.X + matchRect.Width / 2,
                                matchRect.Y + matchRect.Height / 2
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in FindImage: {ex.Message}");
            }

            return null;
        }

        public Point? FindImage(Bitmap screenshot, string templatePath, Rectangle roi, double threshold = -1)
        {
            if (threshold < 0) threshold = _config.ImageMatchThreshold;

            try
            {
                // Crop the screenshot to ROI
                using (Bitmap croppedScreenshot = new Bitmap(roi.Width, roi.Height))
                using (Graphics g = Graphics.FromImage(croppedScreenshot))
                {
                    g.DrawImage(screenshot, new Rectangle(0, 0, roi.Width, roi.Height),
                               roi, GraphicsUnit.Pixel);

                    return FindImage(croppedScreenshot, templatePath, threshold);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in FindImage with ROI: {ex.Message}");
            }

            return null;
        }

        public List<Point> FindAllImages(Bitmap screenshot, string templatePath, double threshold = -1)
        {
            if (threshold < 0) threshold = _config.ImageMatchThreshold;
            List<Point> matches = new List<Point>();

            try
            {
                // using (Image<Bgr, byte> sourceImage = new Image<Bgr, byte>(screenshot))
                using (Image<Bgr, byte> sourceImage = screenshot.ToImage<Bgr, byte>())
                using (Image<Bgr, byte> templateImage = new Image<Bgr, byte>(templatePath))
                {
                    using (Image<Gray, float> result = sourceImage.MatchTemplate(templateImage, TemplateMatchingType.CcoeffNormed))
                    {
                        // Find all matches above threshold
                        for (int y = 0; y < result.Height; y++)
                        {
                            for (int x = 0; x < result.Width; x++)
                            {
                                float matchValue = result.Data[y, x, 0];
                                if (matchValue >= threshold)
                                {
                                    matches.Add(new Point(
                                        x + templateImage.Width / 2,
                                        y + templateImage.Height / 2
                                    ));

                                    // Skip nearby pixels to avoid duplicate matches
                                    x += templateImage.Width / 2;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in FindAllImages: {ex.Message}");
            }

            return matches;
        }
    }
}
