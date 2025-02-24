using System;
using System.Collections.Generic;
using System.Text;

namespace CommonPluginsShared.Interfaces
{
    public interface IPluginSettings
    {
        bool MenuInExtensions { get; set; }

        bool EnableTag { get; set; }

        #region Automatic update when updating library
        DateTime LastAutoLibUpdateAssetsDownload { get; set; }
        bool AutoImport { get; set; }
        #endregion
    }
}
