using Playnite.SDK.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using CommonPlayniteShared.Controls;
using Playnite.SDK.Plugins;
using static CommonPluginsShared.PlayniteTools;
using CommonPlayniteShared;

namespace CommonPluginsShared.Plugins
{
    public class GameMenu : ContextMenu
    {
        public Game Game { get; set; }


        // TODO Used?
        public GameMenu()
        {
            Opened += GameMenu_Opened;
            Closed += GameMenu_Closed;
            DataContextChanged += GameMenu_DataContextChanged;
        }


        private void GameMenu_Closed(object sender, RoutedEventArgs e)
        {
            Deinitialize();
        }

        private void GameMenu_Opened(object sender, RoutedEventArgs e)
        {
            InitializeItems();
        }

        private void GameMenu_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is Game)
            {
                Game = DataContext as Game;
            }
            else
            {
                Game = null;
            }
        }


        public void Deinitialize()
        {
            Items.Clear();
        }

        public void InitializeItems()
        {
            Items.Clear();

            GetGameMenuItemsArgs args = new GetGameMenuItemsArgs();
            if (Game != null)
            {
                PluginMenu pm = new PluginMenu(new List<ExternalPlugin> { ExternalPlugin.SuccessStory });
                IEnumerable<GameMenuItem> items = pm.GetGameMenuItems(new List<Game> { Game });
                if (items.Count() > 0)
                {
                    Items.Add(new Separator());
                    Dictionary<string, MenuItem> menuItems = new Dictionary<string, MenuItem>();
                    foreach (GameMenuItem item in items)
                    {
                        object newItem = null;
                        if (item.Description == "-")
                        {
                            newItem = new Separator();
                        }
                        else
                        {
                            newItem = new MenuItem()
                            {
                                Header = item.Description,
                                Icon = MenuHelpers.GetIcon(item.Icon)
                            };

                            if (item.Action != null)
                            {
                                ((MenuItem)newItem).Click += (_, __) =>
                                {
                                    try
                                    {
                                        item.Action(new GameMenuItemActionArgs
                                        {
                                            Games = args.Games,
                                            SourceItem = item
                                        });
                                    }
                                    catch (Exception ex) 
                                    {
                                        Common.LogError(ex, false);
                                    }
                                };
                            }
                        }

                        if (item.MenuSection.IsNullOrEmpty())
                        {
                            Items.Add(newItem);
                        }
                        else
                        {
                            MenuItem parent = MenuHelpers.GenerateMenuParents(menuItems, item.MenuSection, Items);
                            parent?.Items.Add(newItem);
                        }
                    }
                }
            }

            if (Items.Count > 0) 
            {
                object item = Items.GetItemAt(0);
                if (item is Separator)
                {
                    Items.RemoveAt(0);
                }
            }
        }
    }
}
