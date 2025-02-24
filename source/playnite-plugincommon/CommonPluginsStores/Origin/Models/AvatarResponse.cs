using System;
using System.Collections.Generic;
using System.Text;

namespace CommonPluginsStores.Origin.Models
{
    public class AvatarResponse
    {
        public List<User> users { get; set; }
    }

    public class Avatar
    {
        public int? avatarId { get; set; }
        public int? orderNumber { get; set; }
        public bool isRecent { get; set; }
        public string link { get; set; }
        public int? typeId { get; set; }
        public string typeName { get; set; }
        public int? statusId { get; set; }
        public string statusName { get; set; }
        public int? galleryId { get; set; }
        public string galleryName { get; set; }
    }

    public class User
    {
        public long userId { get; set; }
        public Avatar avatar { get; set; }
    }
}
