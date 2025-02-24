using CommonPlayniteShared;
using CommonPlayniteShared.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Resources;

namespace CommonPluginsShared
{
    public class ImageSourceManagerPlugin
    {
        private static ILogger Logger => LogManager.GetLogger();

        public static MemoryCache Cache = new MemoryCache(Units.MegaBytesToBytes(100));
        private const string btmpPropsFld = "bitmappros";

        public static string GetImagePath(string source, int resize = 0)
        {
            if (source.IsNullOrEmpty())
            {
                return null;
            }

            if (source.StartsWith("resources:") || source.StartsWith("pack://"))
            {
                try
                {
                    var imagePath = source;
                    if (source.StartsWith("resources:"))
                    {
                        imagePath = source.Replace("resources:", "pack://application:,,,");
                    }

                    return imagePath;
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Failed to create bitmap from resources " + source);
                    return null;
                }
            }

            if (StringExtensions.IsHttpUrl(source))
            {
                try
                {
                    var cachedFile = HttpFileCachePlugin.GetWebFile(source, resize);
                    if (string.IsNullOrEmpty(cachedFile))
                    {
                        return HttpFileCache.GetWebFile(source);
                    }

                    return cachedFile;
                }
                catch (Exception exc)
                {
                    Common.LogError(exc, true, $"Failed to create bitmap from {source} file.");
                    return null;
                }
            }

            if (File.Exists(source))
            {
                return source;
            }
            return null;
        }

        public static BitmapImage GetResourceImage(string resourceKey, bool cached, BitmapLoadProperties loadProperties = null)
        {
            if (cached && Cache.TryGet(resourceKey, out var image))
            {
                BitmapLoadProperties existingMetadata = null;
                if (image.Metadata.TryGetValue(btmpPropsFld, out object metaValue))
                {
                    existingMetadata = (BitmapLoadProperties)metaValue;
                }

                if (existingMetadata?.MaxDecodePixelWidth == loadProperties?.MaxDecodePixelWidth)
                {
                    return image.CacheObject as BitmapImage;
                }
                else
                {
                    Cache.TryRemove(resourceKey);
                }
            }

            BitmapImage resource = ResourceProvider.GetResource(resourceKey) as BitmapImage;
            if (loadProperties?.MaxDecodePixelWidth > 0 && resource?.PixelWidth > loadProperties?.MaxDecodePixelWidth)
            {
                resource = resource.GetClone(loadProperties);
            }

            if (cached && resource != null)
            {
                long imageSize = 0;
                try
                {
                    imageSize = resource.GetSizeInMemory();
                }
                catch (Exception e)
                {
                    Logger.Error(e, $"Failed to get image memory size: {resourceKey}");
                }

                if (imageSize > 0)
                {
                    Cache.TryAdd(resourceKey, resource, imageSize, new Dictionary<string, object>
                    {
                        { btmpPropsFld, loadProperties }
                    });
                }
            }

            return resource;
        }

        public static BitmapImage GetImage(string source, bool cached, BitmapLoadProperties loadProperties = null)
        {
            if (DesignerTools.IsInDesignMode)
            {
                cached = false;
            }

            if (source.IsNullOrEmpty())
            {
                return null;
            }

            if (cached && Cache.TryGet(source, out var image))
            {
                BitmapLoadProperties existingMetadata = null;
                if (image.Metadata.TryGetValue(btmpPropsFld, out object metaValue))
                {
                    existingMetadata = (BitmapLoadProperties)metaValue;
                }

                if (existingMetadata == loadProperties)
                {
                    return image.CacheObject as BitmapImage;
                }
                else
                {
                    Cache.TryRemove(source);
                }
            }

            if (source.StartsWith("resources:") || source.StartsWith("pack://"))
            {
                try
                {
                    string imagePath = source;
                    if (source.StartsWith("resources:"))
                    {
                        imagePath = source.Replace("resources:", "pack://application:,,,");
                    }

                    StreamResourceInfo streamInfo = Application.GetResourceStream(new Uri(imagePath));
                    using (Stream stream = streamInfo.Stream)
                    {
                        BitmapImage imageData = BitmapExtensions.BitmapFromStream(stream, loadProperties);
                        if (imageData != null)
                        {
                            if (cached)
                            {
                                _ = Cache.TryAdd(source, imageData, imageData.GetSizeInMemory(),
                                    new Dictionary<string, object>
                                    {
                                        { btmpPropsFld, loadProperties }
                                    });
                            }

                            return imageData;
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Failed to create bitmap from resources " + source);
                    return null;
                }
            }

            if (StringExtensions.IsHttpUrl(source))
            {
                try
                {
                    string cachedFile = HttpFileCachePlugin.GetWebFile(source);
                    if (string.IsNullOrEmpty(cachedFile))
                    {
                        cachedFile = HttpFileCache.GetWebFile(source);
                        if (string.IsNullOrEmpty(cachedFile))
                        {
                            return null;
                        }
                    }

                    return BitmapExtensions.BitmapFromFile(cachedFile, loadProperties);
                }
                catch (Exception exc)
                {
                    Logger.Error(exc, $"Failed to create bitmap from {source} file.");
                    return null;
                }
            }

            if (File.Exists(source))
            {
                try
                {
                    var imageData = BitmapExtensions.BitmapFromFile(source, loadProperties);
                    if (imageData != null)
                    {
                        if (cached)
                        {
                            Cache.TryAdd(source, imageData, imageData.GetSizeInMemory(),
                                new Dictionary<string, object>
                                {
                                    { btmpPropsFld, loadProperties }
                                });
                        }

                        return imageData;
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Failed to create bitmap from " + source);
                    return null;
                }
            }
            return null;
        }
    }
}
