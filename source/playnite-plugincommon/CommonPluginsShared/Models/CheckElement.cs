using System;
using System.Collections.Generic;
using System.Text;

namespace CommonPluginsShared.Models
{
    public class CheckElement
    {
        public Guid Id { get; set; }
        public string Icon { get; set; }
        public string Name { get; set; }
        public string NameShort { get; set; }
        public bool IsCheck { get; set; }
        public bool IsVisible { get; set; }
    }
}
