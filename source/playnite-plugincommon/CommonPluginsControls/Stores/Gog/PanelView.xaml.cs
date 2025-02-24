using System.Windows;
using System.Windows.Controls;
using CommonPluginsStores;
using CommonPluginsStores.Gog;

namespace CommonPluginsControls.Stores.Gog
{
    /// <summary>
    /// Logique d'interaction pour Panel.xaml
    /// </summary>
    public partial class PanelView : UserControl
    {
        private PanelViewModel PanelViewModel { get; set; } = new PanelViewModel();
        private bool IsStarted { get; set; } = false;

        #region Properties        
        public bool UseAuth
        {
            get => (bool)GetValue(UseAuthProperty);
            set => SetValue(UseAuthProperty, value);
        }

        public static readonly DependencyProperty UseAuthProperty = DependencyProperty.Register(
            nameof(UseAuth),
            typeof(bool),
            typeof(PanelView),
            new FrameworkPropertyMetadata(true, UseAuthPropertyChangedCallback));

        private static void UseAuthPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is PanelView obj)
            {
                obj.PanelViewModel.UseAuth = (bool)e.NewValue;
            }
        }

        public bool ForceAuth
        {
            get => (bool)GetValue(ForceAuthProperty);
            set => SetValue(ForceAuthProperty, value);
        }

        public static readonly DependencyProperty ForceAuthProperty = DependencyProperty.Register(
            nameof(ForceAuth),
            typeof(bool),
            typeof(PanelView),
            new FrameworkPropertyMetadata(false, ForceAuthPropertyChangedCallback));

        private static void ForceAuthPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is PanelView obj)
            {
                obj.PanelViewModel.ForceAuth = (bool)e.NewValue;
            }
        }

        public IStoreApi StoreApi
        {
            get => (GogApi)GetValue(storeApiProperty);
            set => SetValue(storeApiProperty, value);
        }

        public static readonly DependencyProperty storeApiProperty = DependencyProperty.Register(
            nameof(StoreApi),
            typeof(IStoreApi),
            typeof(PanelView),
            new FrameworkPropertyMetadata(null, StoreApiPropertyChangedCallback));

        private static void StoreApiPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is PanelView obj)
            {
                obj.PanelViewModel.StoreApi = (IStoreApi)e.NewValue;
            }
        }
        #endregion

        public PanelView()
        {
            InitializeComponent();

            DataContext = PanelViewModel;
        }

        private void PART_IsPrivate_Click(object sender, RoutedEventArgs e)
        {
            UseAuth = PanelViewModel.UseAuth;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsStarted)
            {
                PanelViewModel.User.Link = null;
                PanelViewModel.User.Avatar = null;
            }
        }

        private void Expander_Loaded(object sender, RoutedEventArgs e)
        {
            IsStarted = true;
        }
    }
}
