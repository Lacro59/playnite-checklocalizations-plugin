using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonPluginsShared.Collections
{
    public abstract class PluginDataBaseGame<T> : PluginDataBaseGameBase
    {
        public abstract List<T> Items { get; set; }


        [DontSerialize]
        public override bool HasData => Items?.Count > 0;

        [DontSerialize]
        public override ulong Count => (ulong)(Items?.Count ?? 0);
    }
}
