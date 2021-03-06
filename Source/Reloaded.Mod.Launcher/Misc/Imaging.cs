﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace Reloaded.Mod.Launcher.Misc
{
    public class Imaging
    {
        public static BitmapImage BitmapFromUri(Uri source)
        {
            using (var stream = new FileStream(source.LocalPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
        }

        public static Bitmap BitmapImageToBitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);

                Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destinationRectangle    = new Rectangle(0, 0, width, height);
            var destinationImage        = new Bitmap(width, height);

            destinationImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destinationImage))
            {
                graphics.CompositingMode    = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode  = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode      = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode    = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destinationRectangle, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destinationImage;
        }

    }
}
