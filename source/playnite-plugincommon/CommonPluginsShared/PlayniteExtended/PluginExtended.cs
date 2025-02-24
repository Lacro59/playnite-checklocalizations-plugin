using CommonPlayniteShared.Common;
using CommonPluginsShared.Interfaces;
using Playnite.SDK;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace CommonPluginsShared.PlayniteExtended
{
    public abstract class PluginExtended<ISettings> : PlaynitePlugin<ISettings>
    {
        public PluginExtended(IPlayniteAPI playniteAPI) : base(playniteAPI)
        {
        }
    }


    public abstract class PluginExtended<ISettings, TPluginDatabase> : PlaynitePlugin<ISettings> where TPluginDatabase : IPluginDatabase
    {
        public static TPluginDatabase PluginDatabase { get; set; }


        public PluginExtended(IPlayniteAPI playniteAPI) : base(playniteAPI)
        {
            TransfertOldDatabase();
            CleanOldDatabase();

            // Get plugin's database if used
            PluginDatabase = typeof(TPluginDatabase).CrateInstance<TPluginDatabase>(PluginSettings, this.GetPluginUserDataPath());
            _ = PluginDatabase.InitializeDatabase();
        }


        // TODO Temp; must be deleted
        #region Transfert database directory
        private void TransfertOldDatabase()
        {
            string OldDirectory = Path.Combine(GetPluginUserDataPath(), "Activity");
            string NewDirectory = Path.Combine(GetPluginUserDataPath(), "GameActivity");
            if (Directory.Exists(OldDirectory))
            {
                if (Directory.Exists(NewDirectory))
                {
                    Logger.Warn($"{NewDirectory} already exists");
                }
                else
                {
                    Directory.Move(OldDirectory, NewDirectory);
                }
            }

            OldDirectory = Path.Combine(this.GetPluginUserDataPath(), "Achievements");
            NewDirectory = Path.Combine(this.GetPluginUserDataPath(), "SuccessStory");
            if (Directory.Exists(OldDirectory))
            {
                if (Directory.Exists(NewDirectory))
                {
                    Logger.Warn($"{NewDirectory} already exists");
                }
                else
                {
                    Directory.Move(OldDirectory, NewDirectory);
                }
            }

            OldDirectory = Path.Combine(this.GetPluginUserDataPath(), "Requierements");
            NewDirectory = Path.Combine(this.GetPluginUserDataPath(), "SystemChecker");
            if (Directory.Exists(OldDirectory))
            {
                if (Directory.Exists(NewDirectory))
                {
                    Logger.Warn($"{NewDirectory} already exists");
                }
                else
                {
                    Directory.Move(OldDirectory, NewDirectory);
                }
            }
        }

        // TODO Temp; must be deleted
        private void CleanOldDatabase()
        {
            try
            {
                // Clean old database
                string OldDirectory = Path.Combine(this.GetPluginUserDataPath(), "activity_old");
                FileSystem.DeleteDirectory(OldDirectory);

                OldDirectory = Path.Combine(this.GetPluginUserDataPath(), "activityDetails_old");
                FileSystem.DeleteDirectory(OldDirectory);

                OldDirectory = Path.Combine(this.GetPluginUserDataPath(), "cache");
                FileSystem.DeleteDirectory(OldDirectory);
            }
            catch { }
        }
        #endregion
    }


    public abstract class PlaynitePlugin<ISettings> : GenericPlugin
    {
        internal static ILogger Logger => LogManager.GetLogger();

        public ISettings PluginSettings { get; set; }

        public string PluginFolder { get; set; }


        protected PlaynitePlugin(IPlayniteAPI playniteAPI) : base(playniteAPI)
        {
            Properties = new GenericPluginProperties { HasSettings = true };

            // Get plugin's settings 
            PluginSettings = typeof(ISettings).CrateInstance<ISettings>(this);

            // Get plugin's location 
            PluginFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            LoadCommon();
        }

        protected void LoadCommon()
        {
            // Set the common resourses & event
            Common.Load(PluginFolder, PlayniteApi.ApplicationSettings.Language);
            Common.SetEvent();
        }
    }
}
