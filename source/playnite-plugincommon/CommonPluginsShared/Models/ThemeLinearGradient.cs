using Playnite.SDK.Data;
using System;
using System.Windows;
using System.Windows.Media;

namespace CommonPluginsShared.Models
{
    public class ThemeLinearGradient
    {
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }

        public ThemeGradientColor GradientStop1 { get; set; }
        public ThemeGradientColor GradientStop2 { get; set; }

        [DontSerialize]
        public LinearGradientBrush ToLinearGradientBrush
        {
            get
            {
                LinearGradientBrush linearGradientBrush = new LinearGradientBrush();

                linearGradientBrush.StartPoint = StartPoint;
                linearGradientBrush.EndPoint = EndPoint;

                GradientStop gs1 = new GradientStop();
                GradientStop gs2 = new GradientStop();

                gs1.Offset = GradientStop1.ColorOffset;
                gs2.Offset = GradientStop2.ColorOffset;

                gs1.Color = (Color)ColorConverter.ConvertFromString(GradientStop1.ColorString);
                gs2.Color = (Color)ColorConverter.ConvertFromString(GradientStop2.ColorString);

                linearGradientBrush.GradientStops.Add(gs1);
                linearGradientBrush.GradientStops.Add(gs2);

                return linearGradientBrush;
            }
        }

        public static ThemeLinearGradient ToThemeLinearGradient(LinearGradientBrush linearGradientBrush)
        {
            return new ThemeLinearGradient
            {
                StartPoint = linearGradientBrush.StartPoint,
                EndPoint = linearGradientBrush.EndPoint,
                GradientStop1 = new ThemeGradientColor
                {
                    ColorString = linearGradientBrush.GradientStops[0].Color.ToString(),
                    ColorOffset = linearGradientBrush.GradientStops[0].Offset
                },
                GradientStop2 = new ThemeGradientColor
                {
                    ColorString = linearGradientBrush.GradientStops[1].Color.ToString(),
                    ColorOffset = linearGradientBrush.GradientStops[1].Offset
                }
            };
        }
    }

    public class ThemeGradientColor
    {
        public string ColorString { get; set; }
        public double ColorOffset { get; set; }
    }
}
