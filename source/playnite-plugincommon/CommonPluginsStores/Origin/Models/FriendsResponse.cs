using System;
using System.Collections.Generic;
using System.Text;

namespace CommonPluginsStores.Origin.Models
{
    public class FriendsResponse
    {
        public PagingInfo pagingInfo { get; set; }
        public List<Entry> entries { get; set; }
    }

    public class PagingInfo
    {
        public int totalSize { get; set; }
        public int size { get; set; }
        public int offset { get; set; }
    }

    public class Entry
    {
        public string displayName { get; set; }
        public long timestamp { get; set; }
        public string friendType { get; set; }
        public DateTime? dateTime { get; set; }
        public string userId { get; set; }
        public string personaId { get; set; }
        public bool favorite { get; set; }
        public string nickName { get; set; }
        public string userType { get; set; }
    }
}
