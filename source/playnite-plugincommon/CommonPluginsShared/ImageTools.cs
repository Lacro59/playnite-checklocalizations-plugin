using CommonPlayniteShared.Common;
using Playnite.SDK;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CommonPluginsShared
{
    public enum ImageColor
    {
        None = 0,
        Gray = 1,
        Black = 2
    }


    public class ImageProperty
    {
        public int Width { get; set; } 
        public int Height { get; set; } 
    }


    public class ImageTools
    {
        #region ImageProperty
        public static ImageProperty GetImapeProperty(string srcPath)
        {
            if (!File.Exists(srcPath))
            {
                return null;
            }

            try
            {
                using (Image image = Image.FromFile(srcPath))
                {
                    return new ImageProperty
                    {
                        Width = image.Width,
                        Height = image.Height,
                    };
                };
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, $"Error on GetImapeProperty({srcPath})");
                return null;
            }
        }

        public static ImageProperty GetImapeProperty(Stream imgStream)
        {
            try
            {
                using (Image image = Image.FromStream(imgStream))
                {
                    return new ImageProperty
                    {
                        Width = image.Width,
                        Height = image.Height,
                    };
                };
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false);
                return null;
            }
        }

        public static ImageProperty GetImapeProperty(Image image)
        {
            try
            {
                return new ImageProperty
                {
                    Width = image.Width,
                    Height = image.Height,
                };
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false);
                return null;
            }
        }
        #endregion

        #region Resize
        public static string Resize(string srcPath, int width, int height)
        {
            if (!File.Exists(srcPath))
            {
                return string.Empty;
            }

            try
            {
                Image image = Image.FromFile(srcPath);
                Bitmap resultImage = Resize(image, width, height);
                string newPath = srcPath.Replace(".png", "_" + width + "x" + height + ".png");
                resultImage.Save(newPath);

                image.Dispose();
                resultImage.Dispose();

                return newPath;
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, $"Error on Resize({srcPath})");
                return string.Empty;
            }
        }

        public static bool Resize(string srcPath, int max, string path)
        {
            if (!File.Exists(srcPath))
            {
                return false;
            }

            try
            {
                Image image = Image.FromFile(srcPath);

                int width = image.Width;
                int height = image.Height;
                if (width > height)
                {
                    width = max;
                    height = height * max / image.Width;
                }
                else
                {
                    height = max;
                    width = width * max / image.Height;
                }

                Bitmap resultImage = Resize(image, width, height);
                resultImage.Save(path);

                image.Dispose();
                resultImage.Dispose();

                return true;
            }
            catch (Exception ex)
            {
                Common.LogError(ex, true, $"Error on Resize({srcPath})");
                return false;
            }
        }


        public static bool Resize(string srcPath, int width, int height, string path)
        {
            if (!File.Exists(srcPath))
            {
                return false;
            }

            try
            {
                Image image = Image.FromFile(srcPath);
                Bitmap resultImage = null;
                if (image.Width > width || image.Height > height)
                {
                    resultImage = Resize(image, width, height);
                    resultImage.Save(path);
                    resultImage.Dispose();
                }
                else
                {
                    FileSystem.CopyFile(srcPath, path);
                }

                image.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                Common.LogError(ex, true, $"Error on Resize({srcPath})");
                return false;
            }
        }

        public static bool Resize(Stream imgStream, int width, int height, string path)
        {
            try
            {
                Image image = Image.FromStream(imgStream);
                Bitmap resultImage = Resize(image, width, height);
                resultImage.Save(path);

                image.Dispose();
                resultImage.Dispose();

                return true;
            }
            catch (Exception ex)
            {
                Common.LogError(ex, true);
                return false;
            }
        }

        public static Bitmap Resize(Image image, int width, int height)
        {
            try
            { 
                Rectangle destRect = new Rectangle(0, 0, width, height);
                Bitmap destImage = new Bitmap(width, height);

                destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

                using (Graphics graphics = Graphics.FromImage(destImage))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    using (ImageAttributes wrapMode = new ImageAttributes())
                    {
                        wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                        graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                    }
                }

                return destImage;
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false);
                return null;
            }
        }
        #endregion

        #region Convert
        public static FormatConvertedBitmap ConvertBitmapImage(BitmapImage IconImage, ImageColor imageColor = ImageColor.None)
        {
            if (IconImage is null)
            {
                return null;
            }


            FormatConvertedBitmap ConvertBitmapSource = new FormatConvertedBitmap();

            try
            {
                ConvertBitmapSource.BeginInit();
                ConvertBitmapSource.Source = IconImage;

                switch (imageColor)
                {
                    case ImageColor.Gray:
                        ConvertBitmapSource.DestinationFormat = PixelFormats.Gray32Float;
                        break;

                    case ImageColor.Black:
                        ConvertBitmapSource.Source = IconImage;
                        break;

                    case ImageColor.None:
                        break;

                    default:
                        break;
                }

                ConvertBitmapSource.EndInit();
                ConvertBitmapSource.Freeze();
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false);
            }

            return ConvertBitmapSource;
        }

        public static BitmapSource ConvertBitmapToBitmapSource(Bitmap bitmap)
        {
            try
            {
                BitmapData bitmapData = bitmap.LockBits(
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadOnly, bitmap.PixelFormat);

                BitmapSource bitmapSource = BitmapSource.Create(
                    bitmapData.Width, bitmapData.Height,
                    bitmap.HorizontalResolution, bitmap.VerticalResolution,
                    PixelFormats.Bgr24, null,
                    bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

                bitmap.UnlockBits(bitmapData);

                return bitmapSource;
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false);
                return null;
            }
        }

        public static BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            try
            {
                using (MemoryStream memory = new MemoryStream())
                {
                    bitmap.Save(memory, ImageFormat.Png);
                    memory.Position = 0;

                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();

                    return bitmapImage;
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false);
                return null;
            }
        }

        public static BitmapImage ConvertImageToBitmapImage(Image image)
        {
            try
            {
                using (MemoryStream memory = new MemoryStream())
                {
                    image.Save(memory, ImageFormat.Png);
                    memory.Position = 0;

                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();

                    return bitmapImage;
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false);
                return null;
            }
        }


        public static string ConvertToJpg(string srcPath, long quality = 98L)
        {
            try
            {
                if (File.Exists(srcPath) && Path.GetExtension(srcPath).ToLower() != ".jpg" && Path.GetExtension(srcPath).ToLower() != ".jpeg")
                {
                    using (Image image = Image.FromFile(srcPath))
                    {
                        ImageCodecInfo codecInfo = GetEncoderInfo(ImageFormat.Jpeg);

                        //  Set the quality
                        EncoderParameters parameters = new EncoderParameters(1);
                        parameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);

                        string destPath = srcPath.Replace(Path.GetExtension(srcPath), ".jpg");
                        if (!File.Exists(destPath))
                        {
                            image.Save(destPath, codecInfo, parameters);
                            return destPath;
                        }
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false);
            }

            return null;
        }
        public static ImageCodecInfo GetEncoderInfo(ImageFormat format)
        {
            return ImageCodecInfo.GetImageEncoders().ToList().Find(delegate (ImageCodecInfo codec)
            {
                return codec.FormatID == format.Guid;
            });
        }
        #endregion
    }
}
