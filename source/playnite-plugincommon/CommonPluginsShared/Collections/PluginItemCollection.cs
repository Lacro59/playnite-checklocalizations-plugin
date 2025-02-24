using Playnite.SDK;
using Playnite.SDK.Models;
using CommonPlayniteShared.Database;
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace CommonPluginsShared.Collections
{
    public class PluginItemCollection<TItem> : ItemCollection<TItem> where TItem : PluginDataBaseGameBase
    {
        public PluginItemCollection(string path, GameDatabaseCollection type = GameDatabaseCollection.Uknown) : base(path, type)
        {
        }

        public void SetGameInfo<T>(Guid id)
        {
            try
            {
                _ = Items.TryGetValue(id, out TItem item);
                Game game = API.Instance.Database.Games.Get(id);

                if (game != null && item is PluginDataBaseGame<T>)
                {
                    item.Name = game.Name;
                    item.Game = game;
                    item.IsSaved = true;
                }
                else
                {
                    if (item != null)
                    {
                        item.IsDeleted = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false);
            }
        }

        public void SetGameInfo<T>()
        {
            _ = System.Threading.SpinWait.SpinUntil(() => API.Instance.Database.IsOpen, -1);
            foreach (KeyValuePair<Guid, TItem> item in Items)
            {
                SetGameInfo<T>(item.Key);
            }
        }

        public void SetGameInfoDetails<T, Y>( Guid id)
        {
            try
            {
                _ = Items.TryGetValue(id, out TItem item);
                Game game = API.Instance.Database.Games.Get(id);

                if (game != null && item is PluginDataBaseGameDetails<T, Y>)
                {
                    item.Name = game.Name;
                    item.Game = game;
                    item.IsSaved = true;
                }
                else
                {
                    if (item != null)
                    {
                        item.IsDeleted = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false);
            }
        }

        public void SetGameInfoDetails<T, Y>()
        {
            _ = System.Threading.SpinWait.SpinUntil(() => API.Instance.Database.IsOpen, -1);
            foreach (KeyValuePair<Guid, TItem> item in Items)
            {
                SetGameInfoDetails<T, Y>(item.Key);
            }
        }
    }
}
