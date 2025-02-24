using System.Windows;
using System.Windows.Controls;
using CommonPluginsStores;
using CommonPluginsStores.Steam;

namespace CommonPluginsControls.Stores.Steam
{
    /// <summary>
    /// Logique d'interaction pour Panel.xaml
    /// </summary>
    public partial class PanelView : UserControl
    {
        private PanelViewModel PanelViewModel { get; set; } = new PanelViewModel();
        private bool IsStarted { get; set; } = false;

        #region Properties
        public bool UseApi
        {
            get => (bool)GetValue(UseApiProperty);
            set => SetValue(UseApiProperty, value);
        }

        public static readonly DependencyProperty UseApiProperty = DependencyProperty.Register(
            nameof(UseApi),
            typeof(bool),
            typeof(PanelView),
            new FrameworkPropertyMetadata(true, UseApiPropertyChangedCallback));

        private static void UseApiPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is PanelView obj)
            {
                obj.PanelViewModel.UseApi = (bool)e.NewValue;
            }
        }


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
            get => (SteamApi)GetValue(storeApiProperty);
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

        private void PART_HasApiKey_Click(object sender, RoutedEventArgs e)
        {
            UseApi = PanelViewModel.UseApi;
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


    // http://blog.functionalfun.net/2008/06/wpf-passwordbox-and-data-binding.html
    public static class PasswordBoxAssistant
    {
        public static readonly DependencyProperty BoundPassword =
            DependencyProperty.RegisterAttached("BoundPassword", typeof(string), typeof(PasswordBoxAssistant), new PropertyMetadata(string.Empty, OnBoundPasswordChanged));

        public static readonly DependencyProperty BindPassword = DependencyProperty.RegisterAttached(
            "BindPassword", typeof(bool), typeof(PasswordBoxAssistant), new PropertyMetadata(false, OnBindPasswordChanged));

        private static readonly DependencyProperty UpdatingPassword =
            DependencyProperty.RegisterAttached("UpdatingPassword", typeof(bool), typeof(PasswordBoxAssistant), new PropertyMetadata(false));

        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PasswordBox box = d as PasswordBox;

            // only handle this event when the property is attached to a PasswordBox
            // and when the BindPassword attached property has been set to true
            if (d == null || !GetBindPassword(d))
            {
                return;
            }

            // avoid recursive updating by ignoring the box's changed event
            box.PasswordChanged -= HandlePasswordChanged;

            string newPassword = (string)e.NewValue;

            if (!GetUpdatingPassword(box))
            {
                box.Password = newPassword;
            }

            box.PasswordChanged += HandlePasswordChanged;
        }

        private static void OnBindPasswordChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            // when the BindPassword attached property is set on a PasswordBox,
            // start listening to its PasswordChanged event

            if (!(dp is PasswordBox box))
            {
                return;
            }

            bool wasBound = (bool)e.OldValue;
            bool needToBind = (bool)e.NewValue;

            if (wasBound)
            {
                box.PasswordChanged -= HandlePasswordChanged;
            }

            if (needToBind)
            {
                box.PasswordChanged += HandlePasswordChanged;
            }
        }

        private static void HandlePasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox box = sender as PasswordBox;

            // set a flag to indicate that we're updating the password
            SetUpdatingPassword(box, true);
            // push the new password into the BoundPassword property
            SetBoundPassword(box, box.Password);
            SetUpdatingPassword(box, false);
        }

        public static void SetBindPassword(DependencyObject dp, bool value)
        {
            dp.SetValue(BindPassword, value);
        }

        public static bool GetBindPassword(DependencyObject dp)
        {
            return (bool)dp.GetValue(BindPassword);
        }

        public static string GetBoundPassword(DependencyObject dp)
        {
            return (string)dp.GetValue(BoundPassword);
        }

        public static void SetBoundPassword(DependencyObject dp, string value)
        {
            dp.SetValue(BoundPassword, value);
        }

        private static bool GetUpdatingPassword(DependencyObject dp)
        {
            return (bool)dp.GetValue(UpdatingPassword);
        }

        private static void SetUpdatingPassword(DependencyObject dp, bool value)
        {
            dp.SetValue(UpdatingPassword, value);
        }
    }
}
