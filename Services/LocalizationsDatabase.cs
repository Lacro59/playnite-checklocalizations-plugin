using CheckLocalizations.Models;
using Newtonsoft.Json;
using Playnite.SDK;
using Playnite.SDK.Models;
using CommonPluginsShared.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommonPluginsShared;
using System.Windows;
using System.Windows.Threading;

namespace CheckLocalizations.Services
{
    public class LocalizationsDatabase : PluginDatabaseObject<CheckLocalizationsSettingsViewModel, GameLocalizationsCollection, GameLocalizations>
    {
        private LocalizationsApi localizationsApi;
        private bool IsGetWeb = false;


        public LocalizationsDatabase(IPlayniteAPI PlayniteApi, CheckLocalizationsSettingsViewModel PluginSettings, string PluginUserDataPath) : base(PlayniteApi, PluginSettings, "CheckLocalizations", PluginUserDataPath)
        {

        }
        

        protected override bool LoadDatabase()
        {
            Database = new GameLocalizationsCollection(Paths.PluginDatabasePath);
            Database.SetGameInfo<Models.Localization>(PlayniteApi);

            GetPluginTags();

            return true;
        }


        public override GameLocalizations Get(Guid Id, bool OnlyCache = false, bool Force = false)
        {
            GameLocalizations gameLocalizations = GetOnlyCache(Id);

            if ((gameLocalizations == null && !OnlyCache) || Force)
            {
                gameLocalizations = GetWeb(Id);
                AddOrUpdate(gameLocalizations);
            }
            else if (gameLocalizations != null && !OnlyCache
                && gameLocalizations.Items.Where(x => x.IsManual == true).Count() != 0
                && gameLocalizations.Items.Where(x => x.IsManual == false).Count() == 0)
            {
                if (!gameLocalizations.HasChecked)
                {
                    var dataWeb = GetWeb(Id);

                    gameLocalizations.Items = gameLocalizations.Items.Concat(dataWeb.Items).ToList();
                    gameLocalizations.HasChecked = true;

                    Update(gameLocalizations);
                }
            }
            else if (gameLocalizations == null)
            {
                Game game = PlayniteApi.Database.Games.Get(Id);
                gameLocalizations = GetDefault(game);
                Add(gameLocalizations);
            }

            gameLocalizations.Items.Sort((x, y) => x.DisplayName.CompareTo(y.DisplayName));

            return gameLocalizations;
        }

        public override GameLocalizations GetWeb(Guid Id)
        {
            IsGetWeb = true;

            if (localizationsApi == null)
            {
                localizationsApi = new LocalizationsApi(PlayniteApi, Paths.PluginUserDataPath);
            }

            var data = localizationsApi.GetLocalizations(Id);

            Task.Run(() =>
            {
                Thread.Sleep(2000);

                if (!IsGetWeb)
                {
                    localizationsApi.Dispose();
                    localizationsApi = null;
                    GC.Collect();
                }
            });

            IsGetWeb = false;
            
            return data;
        }


        public override void Refresh(Guid Id)
        {
            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                $"{PluginName} - {resources.GetString("LOCCommonProcessing")}",
                false
            );
            globalProgressOptions.IsIndeterminate = true;

            PlayniteApi.Dialogs.ActivateGlobalProgress((activateGlobalProgress) =>
            {
                var loadedItem = Get(Id, true);
                var webItem = GetWeb(Id);

                if (webItem != null)
                {
                    // Add manual items
                    foreach (var item in loadedItem.Items.FindAll(x => x.IsManual))
                    {
                        webItem.Items.Add(item);
                    }

                    if (!ReferenceEquals(loadedItem, webItem))
                    {
                        Update(webItem);
                    }
                }
            }, globalProgressOptions);
        }


        public bool RemoveWithManual(Guid Id)
        {
            try
            {
                GameLocalizations gameLocalizations = GetOnlyCache(Id);

                if (gameLocalizations.Items == null)
                {
                    gameLocalizations.Items = new List<Models.Localization>();
                }


                if (gameLocalizations.Items.Where(x => x.IsManual).Count() == 0)
                {
                    return Remove(Id);
                }
                else
                {
                    var ItemsManual = gameLocalizations.Items.Where(x => x.IsManual).ToList();
                    gameLocalizations.Items = null;
                    gameLocalizations.Items = ItemsManual;
                    gameLocalizations.HasChecked = false;

                    Common.LogDebug(true, $"RemoveWithoutManual({Id.ToString()}) - gameLocalizations: {JsonConvert.SerializeObject(gameLocalizations)}");

                    Update(gameLocalizations);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false);
            }

            return false;
        }


        protected override void GetPluginTags()
        {
            try
            {
                // Get tags in playnite database
                PluginTags = new List<Tag>();
                foreach (Tag tag in PlayniteApi.Database.Tags)
                {
                    if (tag.Name.IndexOf("[CL] ") > -1)
                    {
                        PluginTags.Add(tag);
                    }
                }

                // Add missing tags
                if (PluginTags.Count < PluginSettings.Settings.GameLanguages.Count)
                {
                    foreach (GameLanguage gameLanguage in PluginSettings.Settings.GameLanguages)
                    {
                        if (PluginTags.Find(x => x.Name == $"[CL] {gameLanguage.DisplayName}") == null)
                        {
                            PlayniteApi.Database.Tags.Add(new Tag { Name = $"[CL] {gameLanguage.DisplayName}" });
                        }
                    }

                    foreach (Tag tag in PlayniteApi.Database.Tags)
                    {
                        if (tag.Name.IndexOf("[CL] ") > -1)
                        {
                            PluginTags.Add(tag);
                        }
                    }
                }

                Common.LogDebug(true, $"PluginTags: {JsonConvert.SerializeObject(PluginTags)}");
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false);
            }
        }

        public override void AddTag(Game game, bool noUpdate = false)
        {
            GameLocalizations gameLocalizations = Get(game, true);

            if (gameLocalizations.HasData)
            {
                try
                {
                    foreach (GameLanguage gameLanguage in PluginSettings.Settings.GameLanguages.FindAll(x => x.IsTag && gameLocalizations.Items.Any(y => x.Name.ToLower() == y.Language.ToLower())))
                    {
                        Guid? TagId = FindGoodPluginTags($"[CL] { gameLanguage.DisplayName}");
                        if (TagId != null)
                        {
                            if (game.TagIds != null)
                            {
                                if (!game.TagIds.Contains((Guid)TagId))
                                {
                                    game.TagIds.Add((Guid)TagId);
                                }
                            }
                            else
                            {
                                game.TagIds = new List<Guid> { (Guid)TagId };
                            }
                        }
                    }

                    if (!noUpdate)
                    {
                        Application.Current.Dispatcher?.Invoke(() => 
                        {
                            PlayniteApi.Database.Games.Update(game);
                            game.OnPropertyChanged();
                        }, DispatcherPriority.Send);
                    }
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, true);
                    logger.Error($"Tag insert error with {game.Name}");

                    PlayniteApi.Notifications.Add(new NotificationMessage(
                        $"{PluginName}-Tag-Errors",
                        $"{PluginName}\r\n" + resources.GetString("LOCCommonNotificationTagError"),
                        NotificationType.Error
                    ));
                }
            }
        }


        public override void SetThemesResources(Game game)
        {
            GameLocalizations gameLocalizations = Get(game, true);

            PluginSettings.Settings.HasData = gameLocalizations.HasData;
            PluginSettings.Settings.HasNativeSupport = gameLocalizations.HasNativeSupport();
            PluginSettings.Settings.ListNativeSupport = gameLocalizations.Items;
        }

        public override void Games_ItemUpdated(object sender, ItemUpdatedEventArgs<Game> e)
        {
            foreach (var GameUpdated in e.UpdatedItems)
            {
                Database.SetGameInfo<Models.Localization>(PlayniteApi, GameUpdated.NewData.Id);
            }
        }
    }
}
