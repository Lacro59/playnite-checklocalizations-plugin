using System.Windows;
using System.Windows.Controls;

namespace CheckLocalizations.Views.Interfaces
{
    /// <summary>
    /// Logique d'interaction pour ClButton.xaml
    /// </summary>
    public partial class ClButton : Button
    {
        public ClButton(bool EnableIntegrationButtonJustIcon)
        {
            InitializeComponent();

            if (EnableIntegrationButtonJustIcon)
            {
                OnlyIcon.Visibility = Visibility.Visible;
                IndicatorSupportText.Visibility = Visibility.Collapsed;
            }
            else
            {
                OnlyIcon.Visibility = Visibility.Collapsed;
                IndicatorSupportText.Visibility = Visibility.Visible;
            }

        }
    }
}
