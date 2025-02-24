using CommonPlayniteShared.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CommonPluginsControls.PlayniteControls
{
    public partial class SearchBox : UserControl
    {
        private int oldCarret;
        private bool ignoreTextCallback;
        internal IInputElement previousFocus;

        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }

            set
            {
                SetValue(TextProperty, value);
            }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(SearchBox), new PropertyMetadata(string.Empty, TextPropertyChangedCallback));

        public bool ShowImage
        {
            get
            {
                return (bool)GetValue(ShowImageProperty);
            }

            set
            {
                SetValue(ShowImageProperty, value);
            }
        }

        public static readonly DependencyProperty ShowImageProperty = DependencyProperty.Register(nameof(ShowImage), typeof(bool), typeof(SearchBox), new PropertyMetadata(true, ShowImagePropertyChangedCallback));

        public new bool IsFocused
        {
            get
            {
                return (bool)GetValue(IsFocusedProperty);
            }

            set
            {
                SetValue(IsFocusedProperty, value);
            }
        }

        public new static readonly DependencyProperty IsFocusedProperty = DependencyProperty.Register(nameof(IsFocused), typeof(bool), typeof(SearchBox), new PropertyMetadata(false, IsFocusedPropertyChangedCallback));

        public SearchBox()
        {
            InitializeComponent();

            if (PART_SeachIcon != null)
            {
            }

            if (PART_ClearTextIcon != null)
            {
                PART_ClearTextIcon.MouseUp += ClearImage_MouseUp;
            }

            if (PART_TextInpuText != null)
            {
                PART_TextInpuText.TextChanged += TextFilter_TextChanged;
                PART_TextInpuText.KeyUp += TextFilter_KeyUp;
                PART_TextInpuText.GotFocus += PART_TextInpuText_GotFocus;
                PART_TextInpuText.LostFocus += PART_TextInpuText_GotFocus;

                BindingTools.SetBinding(
                    PART_TextInpuText,
                    TextBox.TextProperty,
                    this,
                    nameof(Text),
                    mode: System.Windows.Data.BindingMode.OneWay,
                    trigger: System.Windows.Data.UpdateSourceTrigger.PropertyChanged);
            }

            UpdateIconStates();
        }

        private void PART_TextInpuText_GotFocus(object sender, RoutedEventArgs e)
        {
            UpdateIconStates();
        }

        private void UpdateIconStates()
        {
            if (PART_TextInpuText.IsFocused)
            {
                PART_SeachIcon.Visibility = Visibility.Collapsed;
            }

            if (Text.IsNullOrEmpty())
            {
                PART_ClearTextIcon.Visibility = Visibility.Collapsed;
                if (!PART_TextInpuText.IsFocused)
                {
                    PART_SeachIcon.Visibility = ShowImage ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            else
            {
                PART_ClearTextIcon.Visibility = Visibility.Visible;
                if (!PART_TextInpuText.IsFocused)
                {
                    PART_SeachIcon.Visibility = Visibility.Collapsed;
                }
            }
        }

        public void ClearFocus()
        {
            if (previousFocus != null)
            {
                Keyboard.Focus(previousFocus);
            }
            else
            {
                PART_TextInpuText.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }

            previousFocus = null;
            IsFocused = false;
        }

        private void TextFilter_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape || e.Key == Key.Enter)
            {
                ClearFocus();
            }
        }

        private void ClearImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            PART_TextInpuText.Clear();
        }

        private void TextFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ignoreTextCallback)
            {
                return;
            }

            ignoreTextCallback = true;
            Text = PART_TextInpuText.Text;
            ignoreTextCallback = false;
            UpdateIconStates();
        }

        private static void TextPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as SearchBox;
            if (obj.ignoreTextCallback)
            {
                return;
            }

            if (obj.PART_TextInpuText != null)
            {
                var currentCurret = obj.PART_TextInpuText.CaretIndex;
                if (currentCurret == 0 && obj.PART_TextInpuText.Text.Length > 0 && obj.oldCarret != obj.PART_TextInpuText.Text.Length)
                {
                    obj.PART_TextInpuText.CaretIndex = obj.oldCarret;
                }

                obj.oldCarret = obj.PART_TextInpuText.CaretIndex;
            }
        }

        private static void ShowImagePropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as SearchBox;
            obj.ShowImage = (bool)e.NewValue;
        }

        private static void IsFocusedPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as SearchBox;
            var shouldFocus = (bool)e.NewValue;

            if (!shouldFocus && !obj.PART_TextInpuText.IsFocused)
            {
                return;
            }

            if (shouldFocus == true)
            {
                obj.previousFocus = Keyboard.FocusedElement;
                obj.PART_TextInpuText.Focus();
            }
            else
            {
                obj.ClearFocus();
            }

            obj.UpdateIconStates();
        }


        public event TextChangedEventHandler TextChanged
        {
            add { PART_TextInpuText.TextChanged += value; }
            remove { PART_TextInpuText.TextChanged -= value; }
        }
    }
}
