using CommonPlayniteShared.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace CommonPluginsControls.PlayniteControls
{
    /// <summary>
    /// Logique d'interaction pour DdItemListSelectionBox.xaml
    /// </summary>
    public partial class DdItemListSelectionBox : UserControl
    {
        internal ItemsControl ItemsPanel;
        internal Button ButtonClearFilter;
        internal TextBlock TextFilterString;

        internal bool IgnoreChanges { get; set; }

        public bool IsThreeState
        {
            get => (bool)GetValue(IsThreeStateProperty);
            set => SetValue(IsThreeStateProperty, value);
        }

        public static readonly DependencyProperty IsThreeStateProperty = DependencyProperty.Register(
            nameof(IsThreeState),
            typeof(bool),
            typeof(DdItemListSelectionBox));


        internal ToggleButton ToggleSelectedOnly;
        internal SearchBox TextSearchBox;
        internal FrameworkElement ElemSearchHost;

        public bool ShowSearchBox
        {
            get => (bool)GetValue(ShowSearchBoxProperty);
            set => SetValue(ShowSearchBoxProperty, value);
        }

        public static readonly DependencyProperty ShowSearchBoxProperty = DependencyProperty.Register(
            nameof(ShowSearchBox),
            typeof(bool),
            typeof(DdItemListSelectionBox),
            new PropertyMetadata(false));

        public SelectableDbItemList ItemsList
        {
            get
            {
                return (SelectableDbItemList)GetValue(ItemsListProperty);
            }

            set
            {
                SetValue(ItemsListProperty, value);
            }
        }

        public static readonly DependencyProperty ItemsListProperty = DependencyProperty.Register(
            nameof(ItemsList),
            typeof(SelectableDbItemList),
            typeof(DdItemListSelectionBox),
            new PropertyMetadata(null, ItemsListPropertyChangedCallback));

        private static void ItemsListPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as DdItemListSelectionBox;
            var oldVal = (SelectableDbItemList)e.NewValue;
            if (oldVal != null)
            {
                oldVal.SelectionChanged -= obj.List_SelectionChanged;
            }

            var list = (SelectableDbItemList)e.NewValue;
            list.SelectionChanged += obj.List_SelectionChanged;
            obj.UpdateTextStatus();
        }

        private void List_SelectionChanged(object sender, EventArgs e)
        {
            if (!IgnoreChanges)
            {
                IgnoreChanges = true;
                BoundIds = ItemsList?.GetSelectedIds();
                IgnoreChanges = false;
                UpdateTextStatus();
            }
        }

        public object BoundIds
        {
            get
            {
                return GetValue(BoundIdsProperty);
            }

            set
            {
                SetValue(BoundIdsProperty, value);
            }
        }

        public static readonly DependencyProperty BoundIdsProperty = DependencyProperty.Register(
            nameof(BoundIds),
            typeof(object),
            typeof(DdItemListSelectionBox),
            new PropertyMetadata(null, BoundIdsPropertyChangedCallback));

        private static void BoundIdsPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as DdItemListSelectionBox;
            if (obj.IgnoreChanges)
            {
                return;
            }

            obj.IgnoreChanges = true;
            obj.ItemsList?.SetSelection(obj.BoundIds as IEnumerable<Guid>);
            obj.IgnoreChanges = false;
            obj.UpdateTextStatus();
        }

        static DdItemListSelectionBox()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(DdItemListSelectionBox), new FrameworkPropertyMetadata(typeof(DdItemListSelectionBox)));
        }

        public DdItemListSelectionBox()
        {
            InitializeComponent();


            ButtonClearFilter = PART_ButtonClearFilter;
            TextFilterString = PART_TextFilterString;
            ItemsPanel = PART_ItemsPanel;

            if (ButtonClearFilter != null)
            {
                ButtonClearFilter.Click += (_, e) => ClearButtonAction(e);
            }

            if (ItemsPanel != null)
            {
                BindingTools.SetBinding(
                    ItemsPanel,
                    ItemsControl.ItemsSourceProperty,
                    this,
                    "ItemsList");

                XNamespace pns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

                ItemsPanel.ItemsPanel = Xaml.FromString<ItemsPanelTemplate>(new XDocument(
                    new XElement(pns + nameof(ItemsPanelTemplate),
                        new XElement(pns + nameof(VirtualizingStackPanel)))
                ).ToString());

                ItemsPanel.Template = Xaml.FromString<ControlTemplate>(new XDocument(
                     new XElement(pns + nameof(ControlTemplate),
                        new XElement(pns + nameof(ScrollViewer),
                            new XAttribute(nameof(ScrollViewer.Focusable), false),
                            new XElement(pns + nameof(ItemsPresenter))))
                ).ToString());

                ItemsPanel.ItemTemplate = Xaml.FromString<DataTemplate>(new XDocument(
                    new XElement(pns + nameof(DataTemplate),
                        new XElement(pns + nameof(CheckBox),
                            new XAttribute(nameof(CheckBox.IsChecked), "{Binding Selected}"),
                            new XAttribute(nameof(CheckBox.Content), "{Binding Item}"),
                            new XAttribute(nameof(CheckBox.Style), $"{{DynamicResource ComboBoxListItemStyle}}")))
                ).ToString());

                ScrollViewer.SetCanContentScroll(ItemsPanel, true);
                KeyboardNavigation.SetDirectionalNavigation(ItemsPanel, KeyboardNavigationMode.Contained);
                VirtualizingPanel.SetIsVirtualizing(ItemsPanel, true);
                VirtualizingPanel.SetVirtualizationMode(ItemsPanel, VirtualizationMode.Recycling);
            }


            if (ItemsPanel != null)
            {
                BindingTools.ClearBinding(ItemsPanel, ItemsControl.ItemsSourceProperty);
                BindingTools.SetBinding(
                    ItemsPanel,
                    ItemsControl.ItemsSourceProperty,
                    this,
                    "ItemsList.CollectionView");

                XNamespace pns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
                ItemsPanel.ItemTemplate = Xaml.FromString<DataTemplate>(new XDocument(
                    new XElement(pns + nameof(DataTemplate),
                        new XElement(pns + nameof(CheckBox),
                            new XAttribute(nameof(CheckBox.IsChecked), "{Binding Selected}"),
                            new XAttribute(nameof(CheckBox.Content), "{Binding Item.Name}"),
                            new XAttribute(nameof(CheckBox.IsThreeState), "{Binding IsThreeState, Mode=OneWay, RelativeSource={RelativeSource AncestorType=DdItemListSelectionBox}}"),
                            new XAttribute(nameof(CheckBox.Style), $"{{DynamicResource ComboBoxListItemStyle}}")))
                ).ToString());
            }

            ToggleSelectedOnly = PART_ToggleSelectedOnly;
            if (ToggleSelectedOnly != null)
            {
                BindingTools.SetBinding(
                   ToggleSelectedOnly,
                   ToggleButton.IsCheckedProperty,
                   this,
                   nameof(ItemsList) + "." + nameof(ItemsList.ShowSelectedOnly),
                   BindingMode.TwoWay);
            }

            ElemSearchHost = PART_ElemSearchHost;
            if (ElemSearchHost != null)
            {
                BindingTools.SetBinding(
                    ElemSearchHost,
                    FrameworkElement.VisibilityProperty,
                    this,
                    nameof(ShowSearchBox),
                    converter: new CommonPlayniteShared.Converters.BooleanToVisibilityConverter());
            }

            TextSearchBox = PART_SearchBox;
            if (TextSearchBox != null)
            {
                BindingTools.SetBinding(
                    TextSearchBox,
                    SearchBox.TextProperty,
                    this,
                    nameof(ItemsList) + "." + nameof(ItemsList.SearchText),
                    BindingMode.TwoWay);
            }

            UpdateTextStatus();

            if (Popup != null)
            {
                Popup.Opened += (_, __) =>
                {
                    if (ShowSearchBox && TextSearchBox != null)
                    {
                        TextSearchBox.IsFocused = true;
                    }
                };

                Popup.Closed += (_, __) =>
                {
                    if (ShowSearchBox && TextSearchBox != null)
                    {
                        TextSearchBox.IsFocused = false;
                        TextSearchBox.Text = string.Empty;
                    }
                };

                Popup.PreviewKeyUp += (_, keyArgs) =>
                {
                    if (keyArgs.Key == Key.Escape)
                    {
                        Popup.IsOpen = false;
                    }
                };
            }
        }

        public void ClearButtonAction(RoutedEventArgs e)
        {
            ItemsList.SetSelection(null);
            BoundIds = null;
        }

        private void UpdateTextStatus()
        {
            if (TextFilterString != null)
            {
                TextFilterString.Text = ItemsList?.AsString;
            }
        }
    }
}
