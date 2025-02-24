using System;
using System.Collections.Generic;
using System.Text;

namespace CommonPluginsShared.Models
{
    public class Folder
    {
        public string FolderPath { get; set; }

        // Specific SteamEmulator
        public string GameId { get; set; }
        public bool HasGame { get; set; }
    }
}
