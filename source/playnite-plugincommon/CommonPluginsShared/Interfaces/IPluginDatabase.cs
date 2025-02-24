using CommonPluginsShared.Collections;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace CommonPluginsShared.Interfaces
{
    public interface IPluginDatabase
    {
        string PluginName { get; set; }

        bool IsLoaded { get; set; }

        Task<bool> InitializeDatabase();


        PluginDataBaseGameBase Get(Game game, bool onlyCache = false, bool force = false);
        PluginDataBaseGameBase Get(Guid id, bool onlyCache = false, bool force = false);

        PluginDataBaseGameBase GetClone(Guid id);
        PluginDataBaseGameBase GetClone(Game game);

        PluginDataBaseGameBase MergeData(Guid fromId, Guid toId);


        bool Remove(Game game);
        bool Remove(Guid id);

        void AddOrUpdate(PluginDataBaseGameBase item);


        void Refresh(Guid id);
        void Refresh(IEnumerable<Guid> ids);

        [Obsolete("Used Refresh(ids)")]
        void RefreshWithNoData(IEnumerable<Guid> ids);

        IEnumerable<Game> GetGamesWithNoData();

        IEnumerable<Game> GetGamesOldData(int months);
    }
}
