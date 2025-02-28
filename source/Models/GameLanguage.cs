namespace CheckLocalizations.Models
{
    public class GameLanguage
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool IsTag { get; set; }
        public bool IsNative { get; set; }
        public string Alpha2 { get; set; }
        public string Alpha3 { get; set; }
        public string ISO_639_1 { get; set; }
        public string ISO_639_2 { get; set; }
    }
}
