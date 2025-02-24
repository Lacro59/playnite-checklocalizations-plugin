using CommonPluginsShared.Interfaces;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonPluginsShared.Plugins
{
    public class PluginSettings : ObservableObject, IPluginSettings
    {
        public bool MenuInExtensions { get; set; } = true;

        public bool EnableTag { get; set; } = false;

        #region Automatic update when updating library
        public DateTime LastAutoLibUpdateAssetsDownload { get; set; } = DateTime.Now;
        public bool AutoImport { get; set; } = true;
        #endregion

        #region Automatic update when game is installed
        public bool AutoImportOnInstalled { get; set; } = false;
        #endregion

        #region Variables exposed for custom themes
        private bool _hasData = false;
        [DontSerialize]
        public bool HasData { get => _hasData; set => SetValue(ref _hasData, value); }
        #endregion

        [DontSerialize]
        public PluginState PluginState => new PluginState();
    }

    public class PluginState
    {
        public bool SteamIsEnabled => PlayniteTools.IsEnabledPlaynitePlugin(PlayniteTools.GetPluginId(PlayniteTools.ExternalPlugin.SteamLibrary));
        public bool EpicIsEnabled => PlayniteTools.IsEnabledPlaynitePlugin(PlayniteTools.GetPluginId(PlayniteTools.ExternalPlugin.EpicLibrary)) || PlayniteTools.IsEnabledPlaynitePlugin(PlayniteTools.GetPluginId(PlayniteTools.ExternalPlugin.LegendaryLibrary));
        public bool GogIsEnabled => PlayniteTools.IsEnabledPlaynitePlugin(PlayniteTools.GetPluginId(PlayniteTools.ExternalPlugin.GogLibrary)) || PlayniteTools.IsEnabledPlaynitePlugin(PlayniteTools.GetPluginId(PlayniteTools.ExternalPlugin.GogOssLibrary));
        public bool OriginIsEnabled => PlayniteTools.IsEnabledPlaynitePlugin(PlayniteTools.GetPluginId(PlayniteTools.ExternalPlugin.OriginLibrary));
        public bool XboxIsEnabled => PlayniteTools.IsEnabledPlaynitePlugin(PlayniteTools.GetPluginId(PlayniteTools.ExternalPlugin.XboxLibrary));
        public bool PsnIsEnabled => PlayniteTools.IsEnabledPlaynitePlugin(PlayniteTools.GetPluginId(PlayniteTools.ExternalPlugin.PSNLibrary));
        public bool BattleNetIsEnabled => PlayniteTools.IsEnabledPlaynitePlugin(PlayniteTools.GetPluginId(PlayniteTools.ExternalPlugin.BattleNetLibrary));
    }

    public class PluginUpdate
    {
        public bool OnStart { get; set; } = false;
        public bool EveryHours { get; set; } = false;
        public uint Hours { get; set; } = 3;
    }
}
