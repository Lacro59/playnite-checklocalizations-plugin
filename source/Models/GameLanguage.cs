namespace CheckLocalizations.Models
{
    public class GameLanguage
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool IsTag { get; set; }
        public bool IsNative { get; set; }
        public string SteamCode { get; set; }
    }
}
