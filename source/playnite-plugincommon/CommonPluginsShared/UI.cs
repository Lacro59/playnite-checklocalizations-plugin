using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Data;

namespace CommonPluginsShared
{
    public class UI
    {
        private static ILogger Logger => LogManager.GetLogger();


        public bool AddResources(List<ResourcesList> ResourcesList)
        {
            Common.LogDebug(true, $"AddResources() - {Serialization.ToJson(ResourcesList)}");

            string ItemKey = string.Empty;

            foreach (ResourcesList item in ResourcesList)
            {
                ItemKey = item.Key;

                try
                {
                    try
                    {
                        Application.Current.Resources.Add(item.Key, item.Value);
                    }
                    catch
                    {
                        Type TypeActual = Application.Current.Resources[ItemKey].GetType();
                        Type TypeNew = item.Value.GetType();

                        if (TypeActual != TypeNew)
                        {
                            if ((TypeActual.Name == "SolidColorBrush" || TypeActual.Name == "LinearGradientBrush")
                                && (TypeNew.Name == "SolidColorBrush" || TypeNew.Name == "LinearGradientBrush"))
                            {
                            }
                            else
                            {
                                Logger.Warn($"Different type for {ItemKey}");
                                continue;
                            }
                        }

                        Application.Current.Resources[ItemKey] = item.Value;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error in AddResources({ItemKey})");
                    Common.LogError(ex, true, $"Error in AddResources({ItemKey})");
                }
            }
            return true;
        }
        

        /// <summary>
        /// Gel all controls in depObj
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="depObj"></param>
        /// <returns></returns>
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        /// <summary>
        /// Get control's parent by type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="child"></param>
        /// <remarks>https://www.infragistics.com/community/blogs/b/blagunas/posts/find-the-parent-control-of-a-specific-type-in-wpf-and-silverlight</remarks>
        /// <returns></returns>
        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            if (parentObject is T parent)
            {
                return parent;
            }
            else
            {
                return FindParent<T>(parentObject);
            }
        }

        [Obsolete("Use UI.FindParent<T>", true)]
        public static T GetAncestorOfType<T>(FrameworkElement child) where T : FrameworkElement
        {
            var parent = VisualTreeHelper.GetParent(child);
            if (parent != null && !(parent is T))
            {
                return (T)GetAncestorOfType<T>((FrameworkElement)parent);
            }
            return (T)parent;
        }


        private static FrameworkElement SearchElementByNameInExtander(object control, string ElementName)
        {
            if (control is FrameworkElement)
            {
                if (((FrameworkElement)control).Name == ElementName)
                {
                    return (FrameworkElement)control;
                }


                var children = LogicalTreeHelper.GetChildren((FrameworkElement)control);
                foreach (object child in children)
                {
                    if (child is FrameworkElement)
                    {
                        if (((FrameworkElement)child).Name == ElementName)
                        {
                            return (FrameworkElement)child;
                        }

                        var subItems = LogicalTreeHelper.GetChildren((FrameworkElement)child);
                        foreach (object subItem in subItems)
                        {
                            if (subItem.ToString().ToLower().Contains("expander") || subItem.ToString().ToLower().Contains("tabitem"))
                            {
                                FrameworkElement tmp = null;

                                if (subItem.ToString().ToLower().Contains("expander"))
                                {
                                    tmp = SearchElementByNameInExtander(((Expander)subItem).Content, ElementName);
                                }

                                if (subItem.ToString().ToLower().Contains("tabitem"))
                                {
                                    tmp = SearchElementByNameInExtander(((TabItem)subItem).Content, ElementName);
                                }

                                if (tmp != null)
                                {
                                    return tmp;
                                }
                            }
                            else
                            {
                                var tmp = SearchElementByNameInExtander(child, ElementName);
                                if (tmp != null)
                                {
                                    return tmp;
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        public static FrameworkElement SearchElementByName(string ElementName, bool MustVisible = false, bool ParentMustVisible = false, int counter = 1)
        {
            return SearchElementByName(ElementName, Application.Current.MainWindow, MustVisible, ParentMustVisible, counter);
        }

        public static FrameworkElement SearchElementByName(string ElementName, DependencyObject dpObj, bool MustVisible = false, bool ParentMustVisible = false, int counter = 1)
        {
            FrameworkElement ElementFound = null;

            int count = 0;

            if (ElementFound == null)
            {
                foreach (FrameworkElement el in UI.FindVisualChildren<FrameworkElement>(dpObj))
                {
                    if (el.ToString().ToLower().Contains("expander") || el.ToString().ToLower().Contains("tabitem"))
                    {
                        FrameworkElement tmpEl = null;

                        if (el.ToString().ToLower().Contains("expander"))
                        {
                            tmpEl = SearchElementByNameInExtander(((Expander)el).Content, ElementName);
                        }

                        if (el.ToString().ToLower().Contains("tabitem"))
                        {
                            tmpEl = SearchElementByNameInExtander(((TabItem)el).Content, ElementName);
                        }

                        if (tmpEl != null)
                        {
                            if (tmpEl.Name == ElementName)
                            {
                                if (!MustVisible)
                                {
                                    if (!ParentMustVisible)
                                    {
                                        ElementFound = tmpEl;
                                        break;
                                    }
                                    else if (((FrameworkElement)el.Parent).IsVisible)
                                    {
                                        ElementFound = tmpEl;
                                        break;
                                    }
                                }
                                else if (tmpEl.IsVisible)
                                {
                                    if (!ParentMustVisible)
                                    {
                                        ElementFound = tmpEl;
                                        break;
                                    }
                                    else if (((FrameworkElement)el.Parent).IsVisible)
                                    {
                                        ElementFound = tmpEl;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else if (el.Name == ElementName)
                    {
                        count++;

                        if (!MustVisible)
                        {
                            if (!ParentMustVisible)
                            {
                                if (count == counter)
                                {
                                    ElementFound = el;
                                    break;
                                }
                            }
                            else if (((FrameworkElement)((FrameworkElement)((FrameworkElement)((FrameworkElement)((FrameworkElement)el.Parent).Parent).Parent).Parent).Parent).IsVisible)
                            {
                                if (count == counter)
                                {
                                    ElementFound = el;
                                    break;
                                }
                            }
                        }
                        else if (el.IsVisible)
                        {
                            if (!ParentMustVisible)
                            {
                                if (count == counter)
                                {
                                    ElementFound = el;
                                    break;
                                }
                            }
                            else if (((FrameworkElement)((FrameworkElement)((FrameworkElement)((FrameworkElement)((FrameworkElement)el.Parent).Parent).Parent).Parent).Parent).IsVisible)
                            {
                                if (count == counter)
                                {
                                    ElementFound = el;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return ElementFound;
        }


        public static void SetControlSize(FrameworkElement ControlElement)
        {
            SetControlSize(ControlElement, 0, 0);
        }

        public static void SetControlSize(FrameworkElement ControlElement, double DefaultHeight, double DefaultWidth)
        {
            try
            {
                UserControl ControlParent = UI.FindParent<UserControl>(ControlElement);
                FrameworkElement ControlContener = (FrameworkElement)ControlParent.Parent;

                Common.LogDebug(true, $"SetControlSize({ControlElement.Name}) - parent.name: {ControlContener.Name} - parent.Height: {ControlContener.Height} - parent.Width: {ControlContener.Width} - parent.MaxHeight: {ControlContener.MaxHeight} - parent.MaxWidth: {ControlContener.MaxWidth}");

                // Set Height
                if (!double.IsNaN(ControlContener.Height))
                {
                    ControlElement.Height = ControlContener.Height;
                }
                else if (DefaultHeight != 0)
                {
                    ControlElement.Height = DefaultHeight;
                }
                // Control with MaxHeight
                if (!double.IsNaN(ControlContener.MaxHeight))
                {
                    if (ControlElement.Height > ControlContener.MaxHeight)
                    {
                        ControlElement.Height = ControlContener.MaxHeight;
                    }
                }


                // Set Width
                if (!double.IsNaN(ControlContener.Width))
                {
                    ControlElement.Width = ControlContener.Width;
                }
                else if (DefaultWidth != 0)
                {
                    ControlElement.Width = DefaultWidth;
                }
                // Control with MaxWidth
                if (!double.IsNaN(ControlContener.MaxWidth))
                {
                    if (ControlElement.Width > ControlContener.MaxWidth)
                    {
                        ControlElement.Width = ControlContener.MaxWidth;
                    }
                }
            }
            catch
            {

            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>https://stackoverflow.com/questions/1585462/bubbling-scroll-events-from-a-listview-to-its-parent</remarks>
        public static void HandlePreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                var scrollViewer = UI.FindVisualChildren<ScrollViewer>((FrameworkElement)sender).FirstOrDefault();

                if (scrollViewer == null)
                {
                    return;
                }

                var scrollPos = scrollViewer.ContentVerticalOffset;
                if ((scrollPos == scrollViewer.ScrollableHeight && e.Delta < 0) || (scrollPos == 0 && e.Delta > 0))
                {
                    e.Handled = true;
                    var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                    eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                    eventArg.Source = sender;
                    var parent = ((Control)sender).Parent as UIElement;
                    parent.RaiseEvent(eventArg);
                }
            }
        }
    }


    public class ResourcesList
    {
        public string Key { get; set; }
        public object Value { get; set; }
    }
}
