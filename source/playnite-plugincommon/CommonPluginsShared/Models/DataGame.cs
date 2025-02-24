using System;
using System.Collections.Generic;
using System.Text;

namespace CommonPluginsShared.Models
{
    public class DataGame
    {
        public Guid Id { get; set; }
        public string Icon { get; set; }
        public string Name { get; set; }

        public bool IsDeleted { get; set; }
        public ulong CountData { get; set; }
    }
}
