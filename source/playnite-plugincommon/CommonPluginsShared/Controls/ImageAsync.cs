using CommonPlayniteShared;
using CommonPluginsShared.Converters;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace CommonPluginsShared.Controls
{
    public class ImageAsync : Image
    {
        internal object CurrentImage { get; set; }


        #region Properties
        public static new readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            nameof(Source),
            typeof(string),
            typeof(ImageAsync),
            new FrameworkPropertyMetadata(string.Empty, SourceChanged)
        );
        public new string Source
        {
            get => (string)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }
        private static void SourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            try
            {
                ImageAsync control = (ImageAsync)obj;
                control.LoadNewSource(args.NewValue, args.OldValue);
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, "ImageAsync");
            }
        }

        public static readonly DependencyProperty ParameterProperty = DependencyProperty.Register(
            nameof(Parameter),
            typeof(string),
            typeof(ImageAsync),
            new FrameworkPropertyMetadata(string.Empty, SourceChanged)
        );
        public string Parameter
        {
            get => (string)GetValue(ParameterProperty);
            set => SetValue(ParameterProperty, value);
        }

        public static readonly DependencyProperty DecodePixelHeightProperty = DependencyProperty.Register(
            nameof(DecodePixelHeight),
            typeof(double),
            typeof(ImageAsync),
            new FrameworkPropertyMetadata(200.0, SourceChanged)
        );
        public double DecodePixelHeight
        {
            get => (double)GetValue(DecodePixelHeightProperty);
            set => SetValue(DecodePixelHeightProperty, value);
        }
        #endregion


        public ImageAsync()
        {
        }


        private async void LoadNewSource(object newSource, object oldSource)
        {
            if (newSource?.Equals(CurrentImage) == true)
            {
                return;
            }

            CurrentImage = newSource;
            dynamic image = null;
            string parameter = Parameter;
            object[] values = new object[] { newSource, DecodePixelHeight };

            if (newSource != null)
            {
                image = await Task.Factory.StartNew(() =>
                {
                    if (newSource is string str)
                    {
                        object tmpImage = new ImageConverter().Convert(values, null, parameter, null);
                        if (tmpImage is BitmapImage)
                        {
                            ((BitmapImage)tmpImage).Freeze();
                        }
                        else
                        {
                            tmpImage = ImageSourceManagerPlugin.GetImage(str, false);
                        }

                        return tmpImage;
                    }
                    else
                    {
                        return null;
                    }
                });
            }

            base.Source = image;
        }
    }
}
