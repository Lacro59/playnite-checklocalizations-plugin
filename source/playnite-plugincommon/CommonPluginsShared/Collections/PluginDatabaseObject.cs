using Playnite.SDK;
using Playnite.SDK.Models;
using CommonPluginsShared.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CommonPluginsControls.Controls;
using CommonPlayniteShared.Common;
using CommonPluginsShared.Interfaces;
using CommonPlayniteShared;
using Playnite.SDK.Data;
using System.Text;

namespace CommonPluginsShared.Collections
{
    public abstract class PluginDatabaseObject<TSettings, TDatabase, TItem, T> : ObservableObject, IPluginDatabase
        where TSettings : ISettings
        where TDatabase : PluginItemCollection<TItem>
        where TItem : PluginDataBaseGameBase
    {
        protected static ILogger Logger => LogManager.GetLogger();

        public TSettings PluginSettings { get; set; }

        public UI UI { get; set; } = new UI();

        public string PluginName { get; set; }
        public PluginPaths Paths { get; set; }
        public TDatabase Database { get; set; }
        public Game GameContext { get; set; }

        protected string TagBefore { get; set; } = string.Empty;

        protected IEnumerable<Tag> PluginTags => GetPluginTags();


        private bool _isLoaded = false;
        public bool IsLoaded { get => _isLoaded; set => SetValue(ref _isLoaded, value); }

        public bool IsViewOpen = false;

        public bool TagMissing { get; set; } = false;

        private IEnumerable<Guid> PreviousIds { get; set; } = new List<Guid>();


        protected PluginDatabaseObject(TSettings pluginSettings, string pluginName, string pluginUserDataPath)
        {
            PluginSettings = pluginSettings;
            PluginName = pluginName;

            Paths = new PluginPaths
            {
                PluginPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                PluginUserDataPath = pluginUserDataPath,
                PluginDatabasePath = Path.Combine(pluginUserDataPath, pluginName),
                PluginCachePath = Path.Combine(PlaynitePaths.DataCachePath, pluginName),
            };
            HttpFileCachePlugin.CacheDirectory = Paths.PluginCachePath;

            FileSystem.CreateDirectory(Paths.PluginDatabasePath);
            FileSystem.CreateDirectory(Paths.PluginCachePath);

            API.Instance.Database.Games.ItemUpdated += Games_ItemUpdated;
            API.Instance.Database.Games.ItemCollectionChanged += Games_ItemCollectionChanged;
        }


        #region Database
        public Task<bool> InitializeDatabase()
        {
            return Task.Run(() =>
            {
                if (IsLoaded)
                {
                    Logger.Info($"Database is already initialized");
                    return true;
                }

                IsLoaded = LoadDatabase();

                if (IsLoaded)
                {
                    Database.ItemCollectionChanged += Database_ItemCollectionChanged;
                    Database.ItemUpdated += Database_ItemUpdated;
                }

                return IsLoaded;
            });
        }


        private void Database_ItemUpdated(object sender, ItemUpdatedEventArgs<TItem> e)
        {
            if (GameContext == null)
            {
                return;
            }

            // Publish changes for the currently displayed game if updated
            ItemUpdateEvent<TItem> ActualItem = e.UpdatedItems.Find(x => x.NewData.Id == GameContext.Id);
            if (ActualItem?.NewData?.Id != null)
            {
                SetThemesResources(GameContext);
            }
        }

        private void Database_ItemCollectionChanged(object sender, ItemCollectionChangedEventArgs<TItem> e)
        {
            if (GameContext != null)
            {
                SetThemesResources(GameContext);
            }
        }


        protected abstract bool LoadDatabase();

        public virtual bool ClearDatabase()
        {
            bool IsOk = true;

            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions($"{PluginName} - {ResourceProvider.GetString("LOCCommonProcessing")}")
            {
                Cancelable = false,
                IsIndeterminate = false
            };

            _ = API.Instance.Dialogs.ActivateGlobalProgress((a) =>
            {
                a.ProgressMaxValue = Database.Items.Count();
                Database.Items.ForEach(x =>
                {
                    try
                    {
                        _ = Remove(x.Key);
                        a.CurrentProgressValue++;
                    }
                    catch (Exception ex)
                    {
                        Common.LogError(ex, false, $"Error on {x.Key} - {x.Value.Name}", false, PluginName);
                        IsOk = false;
                    }
                });

            }, globalProgressOptions);

            return IsOk;
        }

        public virtual void DeleteDataWithDeletedGame()
        {
            IEnumerable<KeyValuePair<Guid, TItem>> GamesDeleted = Database.Items.Where(x => API.Instance.Database.Games.Get(x.Key) == null).Select(x => x);
            GamesDeleted.ForEach(x =>
            {
                Logger.Info($"Delete data for missing game: {x.Value.Name} - {x.Key}");
                _ = Database.Remove(x.Key);
            });
        }


        public virtual void GetSelectData()
        {
            OptionsDownloadData View = new OptionsDownloadData(this);
            Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(PluginName + " - " + ResourceProvider.GetString("LOCCommonSelectData"), View);
            _ = windowExtension.ShowDialog();

            List<Game> playniteDb = View.GetFilteredGames();
            bool onlyMissing = View.GetOnlyMissing();

            if (playniteDb == null)
            {
                return;
            }

            if (onlyMissing)
            {
                playniteDb = playniteDb.FindAll(x => !Get(x.Id, true).HasData);
            }

            Refresh(playniteDb.Select(x => x.Id));
        }


        public virtual IEnumerable<Game> GetGamesList()
        {
            foreach (KeyValuePair<Guid, TItem> item in Database.Items)
            {
                Game game = API.Instance.Database.Games.Get(item.Key);
                if (game != null)
                {
                    yield return game;
                }
            }
        }

        public virtual IEnumerable<Game> GetGamesWithNoData()
        {
            IEnumerable<Game> gamesWithNoData = Database.Items.Where(x => !x.Value.HasData).Select(x => API.Instance.Database.Games.Get(x.Key)).Where(x => x != null);
            IEnumerable<Game> gamesNotInDb = API.Instance.Database.Games.Where(x => !Database.Items.Any(y => y.Key == x.Id));
            IEnumerable<Game> mergedList = gamesWithNoData.Union(gamesNotInDb).Distinct();
            return mergedList.Where(x => !x.Hidden);
        }

        public virtual IEnumerable<Game> GetGamesOldData(int months)
        {
            IEnumerable<Game> gamesOldData = Database.Items.Where(x => x.Value.DateLastRefresh <= DateTime.Now.AddMonths(-months)).Select(x => API.Instance.Database.Games.Get(x.Key)).Where(x => x != null);
            return gamesOldData;
        }


        public virtual IEnumerable<DataGame> GetDataGames()
        {
            return Database.Items.Select(x => new DataGame
            {
                Id = x.Value.Id,
                Icon = x.Value.Icon.IsNullOrEmpty() ? x.Value.Icon : API.Instance.Database.GetFullFilePath(x.Value.Icon),
                Name = x.Value.Name,
                IsDeleted = x.Value.IsDeleted,
                CountData = x.Value.Count
            }).Distinct();
        }


        public virtual IEnumerable<DataGame> GetIsolatedDataGames()
        {
            return Database.Items.Where(x => x.Value.IsDeleted).Select(x => new DataGame
            {
                Id = x.Value.Id,
                Icon = x.Value.Icon.IsNullOrEmpty() ? x.Value.Icon : API.Instance.Database.GetFullFilePath(x.Value.Icon),
                Name = x.Value.Name,
                IsDeleted = x.Value.IsDeleted,
                CountData = x.Value.Count
            }).Distinct();
        }
        #endregion


        #region Database item methods
        public virtual TItem GetDefault(Guid id)
        {
            Game game = API.Instance.Database.Games.Get(id);
            return game == null ? null : GetDefault(game);
        }

        public virtual TItem GetDefault(Game game)
        {
            TItem newItem = typeof(TItem).CrateInstance<TItem>();

            newItem.Id = game.Id;
            newItem.Name = game.Name;
            newItem.Game = game;
            newItem.IsSaved = false;

            return newItem;
        }


        public virtual void Add(TItem itemToAdd)
        {
            try
            {
                if (itemToAdd == null)
                {
                    Logger.Warn("itemToAdd is null in Add()");
                    return;
                }

                itemToAdd.IsSaved = true;
                Application.Current.Dispatcher?.Invoke(() => Database?.Add(itemToAdd), DispatcherPriority.Send);

                // If tag system
                object Settings = PluginSettings.GetType().GetProperty("Settings").GetValue(PluginSettings);
                PropertyInfo propertyInfo = Settings.GetType().GetProperty("EnableTag");

                if (propertyInfo != null && (bool)propertyInfo.GetValue(Settings))
                {
                    Common.LogDebug(true, $"RemoveTag & AddTag for {itemToAdd.Name} with {itemToAdd.Id}");
                    RemoveTag(itemToAdd.Id);
                    AddTag(itemToAdd.Id);
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, false, PluginName);
                API.Instance.Notifications.Add(new NotificationMessage(
                    $"{PluginName}-Error-Add",
                    $"{PluginName}" + Environment.NewLine + $"{ex.Message}",
                    NotificationType.Error,
                    () => PlayniteTools.CreateLogPackage(PluginName)
                ));
            }
        }

        public virtual void Update(TItem itemToUpdate)
        {
            try
            {
                if (itemToUpdate == null)
                {
                    Logger.Warn("itemToAdd is null in Update()");
                    return;
                }

                itemToUpdate.IsSaved = true;
                itemToUpdate.DateLastRefresh = DateTime.Now.ToUniversalTime();
                _ = Database?.Items.TryUpdate(itemToUpdate.Id, itemToUpdate, Get(itemToUpdate.Id, true));
                Application.Current.Dispatcher?.Invoke(() => Database?.Update(itemToUpdate), DispatcherPriority.Send);

                // If tag system
                object Settings = PluginSettings.GetType().GetProperty("Settings").GetValue(PluginSettings);
                PropertyInfo propertyInfo = Settings.GetType().GetProperty("EnableTag");

                if (propertyInfo != null && (bool)propertyInfo.GetValue(Settings))
                {
                    Common.LogDebug(true, $"RemoveTag & AddTag for {itemToUpdate.Name} with {itemToUpdate.Id}");
                    RemoveTag(itemToUpdate.Id);
                    AddTag(itemToUpdate.Id);
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, false, PluginName);
                API.Instance.Notifications.Add(new NotificationMessage(
                    $"{PluginName}-Error-Update",
                    $"{PluginName}" + Environment.NewLine + $"{ex.Message}",
                    NotificationType.Error,
                    () => PlayniteTools.CreateLogPackage(PluginName)
                ));
            }
        }

        public virtual void AddOrUpdate(TItem item)
        {
            if (item == null)
            {
                Logger.Warn("item is null in AddOrUpdate()");
                return;
            }

            TItem itemCached = GetOnlyCache(item.Id);
            if (itemCached == null)
            {
                Add(item);
            }
            else
            {
                Update(item);
            }
        }


        public void Refresh(Guid id)
        {
            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions($"{PluginName} - {ResourceProvider.GetString("LOCCommonProcessing")}")
            {
                Cancelable = false,
                IsIndeterminate = true
            };

            _ = API.Instance.Dialogs.ActivateGlobalProgress((activateGlobalProgress) =>
            {
                RefreshNoLoader(id);
            }, globalProgressOptions);
        }

        public void Refresh(IEnumerable<Guid> ids)
        {
            Logger.Info("Refresh() started");
            Refresh(ids, ResourceProvider.GetString("LOCCommonProcessing"));
        }

        public void Refresh(IEnumerable<Guid> ids, string message)
        {
            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions($"{PluginName} - {message}")
            {
                Cancelable = true,
                IsIndeterminate = ids.Count() == 1
            };

            _ = API.Instance.Dialogs.ActivateGlobalProgress((a) =>
            {
                API.Instance.Database.BeginBufferUpdate();
                Database.BeginBufferUpdate();

                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                a.ProgressMaxValue = ids.Count();

                foreach (Guid id in ids)
                {
                    Game game = API.Instance.Database.Games.Get(id);
                    a.Text = $"{PluginName} - {message}"
                        + (ids.Count() == 1 ? string.Empty : "\n\n" + $"{a.CurrentProgressValue}/{a.ProgressMaxValue}")
                        + "\n" + game?.Name + (game?.Source == null ? string.Empty : $" ({game?.Source.Name})");

                    if (a.CancelToken.IsCancellationRequested)
                    {
                        break;
                    }

                    try
                    {
                        Thread.Sleep(100);
                        RefreshNoLoader(id);
                    }
                    catch (Exception ex)
                    {
                        Common.LogError(ex, false, true, PluginName);
                    }

                    a.CurrentProgressValue++;
                }

                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;
                Logger.Info($"Task Refresh(){(a.CancelToken.IsCancellationRequested ? " canceled" : string.Empty)} - {string.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10)} for {a.CurrentProgressValue}/{ids.Count()} items");

                Database.EndBufferUpdate();
                API.Instance.Database.EndBufferUpdate();
            }, globalProgressOptions);
        }

        public virtual void RefreshNoLoader(Guid id)
        {
            Game game = API.Instance.Database.Games.Get(id);
            Logger.Info($"RefreshNoLoader({game?.Name} - {game?.Id})");

            TItem loadedItem = Get(id, true);
            TItem webItem = GetWeb(id);

            if (webItem != null && !ReferenceEquals(loadedItem, webItem))
            {
                Update(webItem);
            }
            else
            {
                webItem = loadedItem;
            }

            ActionAfterRefresh(webItem);
        }

        public virtual void RefreshWithNoData(IEnumerable<Guid> ids)
        {
            Refresh(ids);
        }

        public virtual void RefreshInstalled()
        {
            Logger.Info("RefreshInstalled() started");
            IEnumerable<Guid> ids = API.Instance.Database.Games.Where(x => x.IsInstalled && !x.Hidden).Select(x => x.Id);
            Logger.Info($"RefreshInstalled found {ids.Count()} game(s) that need updating");
            Refresh(ids, ResourceProvider.GetString("LOCCommonGettingInstalledDatas"));
        }

        public virtual void RefreshInstalled(IEnumerable<Guid> ids)
        {
            Logger.Info("RefreshInstalled() started");
            Refresh(ids, ResourceProvider.GetString("LOCCommonGettingInstalledDatas"));
        }

        public virtual void RefreshRecent()
        {
            Logger.Info("RefreshRecent() started");
            object Settings = PluginSettings.GetType().GetProperty("Settings").GetValue(PluginSettings);
            PropertyInfo propertyInfo = Settings.GetType().GetProperty("LastAutoLibUpdateAssetsDownload");
            DateTime LastAutoLibUpdateAssetsDownload;
            if (propertyInfo == null)
            {
                Logger.Warn($"No LastAutoLibUpdateAssetsDownload find");
                LastAutoLibUpdateAssetsDownload = DateTime.Now.AddMonths(-1);
            }
            else
            {
                LastAutoLibUpdateAssetsDownload = (DateTime)propertyInfo.GetValue(Settings);
            }

            IEnumerable<Guid> ids = API.Instance.Database.Games
                .Where(x => x.Added != null && x.Added > LastAutoLibUpdateAssetsDownload)
                .Select(x => x.Id);
            Logger.Info($"RefreshRecent found {ids.Count()} game(s) that need updating");
            Refresh(ids, ResourceProvider.GetString("LOCCommonGettingNewDatas"));
        }


        public virtual void ActionAfterRefresh(TItem item)
        {
        }

        public virtual PluginDataBaseGameBase MergeData(Guid fromId, Guid toId)
        {
            return null;
        }


        public virtual bool Remove(Game game)
        {
            return Remove(game.Id);
        }

        public virtual bool Remove(Guid id)
        {
            RemoveTag(id);
            return Database.Items.ContainsKey(id) && (bool)API.Instance.MainView.UIDispatcher?.Invoke(() => { return Database.Remove(id); });
        }

        public virtual bool Remove(IEnumerable<Guid> ids)
        {
            Logger.Info($"Remove() started");
            API.Instance.Database.Games.BeginBufferUpdate();
            Database.BeginBufferUpdate();

            foreach (Guid id in ids)
            {
                try
                {
                    _ = Remove(id);
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, true, PluginName);
                }
            }

            API.Instance.Database.Games.EndBufferUpdate();
            Database.EndBufferUpdate();

            return true;
        }


        public virtual TItem GetOnlyCache(Guid id)
        {
            return Database?.Get(id);
        }

        public virtual TItem GetOnlyCache(Game game)
        {
            return Database?.Get(game.Id);
        }


        public virtual TItem GetClone(Guid id)
        {
            return Serialization.GetClone(Get(id, true, false));
        }

        public virtual TItem GetClone(Game game)
        {
            return Serialization.GetClone(Get(game, true, false));
        }


#pragma warning disable CS1066 // La valeur par défaut spécifiée pour le paramètre n'aura aucun effet, car elle s'applique à un membre utilisé dans des contextes qui n'autorisent pas les arguments facultatifs
        PluginDataBaseGameBase IPluginDatabase.Get(Game game, bool onlyCache, bool force = false)
#pragma warning restore CS1066 // La valeur par défaut spécifiée pour le paramètre n'aura aucun effet, car elle s'applique à un membre utilisé dans des contextes qui n'autorisent pas les arguments facultatifs
        {
            return Get(game, onlyCache, force);
        }

#pragma warning disable CS1066 // La valeur par défaut spécifiée pour le paramètre n'aura aucun effet, car elle s'applique à un membre utilisé dans des contextes qui n'autorisent pas les arguments facultatifs
        PluginDataBaseGameBase IPluginDatabase.Get(Guid id, bool onlyCache, bool force = false)
#pragma warning restore CS1066 // La valeur par défaut spécifiée pour le paramètre n'aura aucun effet, car elle s'applique à un membre utilisé dans des contextes qui n'autorisent pas les arguments facultatifs
        {
            return Get(id, onlyCache, force);
        }


        PluginDataBaseGameBase IPluginDatabase.GetClone(Game game)
        {
            return GetClone(game);
        }

        PluginDataBaseGameBase IPluginDatabase.GetClone(Guid id)
        {
            return GetClone(id);
        }


        void IPluginDatabase.AddOrUpdate(PluginDataBaseGameBase item)
        {
            AddOrUpdate((TItem)item);
        }


        public abstract TItem Get(Guid id, bool onlyCache = false, bool force = false);

        public virtual TItem Get(Game game, bool onlyCache = false, bool force = false)
        {
            return Get(game.Id, onlyCache, force);
        }


        public virtual TItem GetWeb(Guid dd)
        {
            return null;
        }

        public virtual TItem GetWeb(Game game)
        {
            return GetWeb(game.Id);
        }
        #endregion


        #region Tag system
        private IEnumerable<Tag> GetPluginTags()
        {
            IEnumerable<Tag> PluginTags = new List<Tag>();
            if (!TagBefore.IsNullOrEmpty())
            {
                PluginTags = API.Instance.Database.Tags?.Where(x => (bool)x.Name?.StartsWith(TagBefore))?.ToList() ?? new List<Tag>();
            }
            return PluginTags;
        }

        internal Guid? CheckTagExist(string tagName)
        {
            string completTagName = TagBefore.IsNullOrEmpty() ? tagName : TagBefore + " " + tagName;
            Guid? findGoodPluginTags = PluginTags.FirstOrDefault(x => x.Name == completTagName)?.Id;
            if (findGoodPluginTags == null)
            {
                API.Instance.Database.Tags.Add(new Tag { Name = completTagName });
                findGoodPluginTags = PluginTags.FirstOrDefault(x => x.Name == completTagName).Id;
            }
            return findGoodPluginTags;
        }


        public virtual void AddTag(Game game)
        {
            TItem item = Get(game, true);
            if (item.HasData)
            {
                try
                {
                    Guid? tagId = FindGoodPluginTags(string.Empty);
                    if (tagId != null)
                    {
                        if (game.TagIds != null)
                        {
                            game.TagIds.Add((Guid)tagId);
                        }
                        else
                        {
                            game.TagIds = new List<Guid> { (Guid)tagId };
                        }
                    }
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, $"Tag insert error with {game.Name}", true, PluginName, string.Format(ResourceProvider.GetString("LOCCommonNotificationTagError"), game.Name));
                    return;
                }
            }
            else if (TagMissing)
            {
                if (game.TagIds != null)
                {
                    game.TagIds.Add((Guid)AddNoDataTag());
                }
                else
                {
                    game.TagIds = new List<Guid> { (Guid)AddNoDataTag() };
                }
            }

            API.Instance.MainView.UIDispatcher?.Invoke(() =>
            {
                API.Instance.Database.Games.Update(game);
                game.OnPropertyChanged();
            });
        }

        public void AddTag(Guid id)
        {
            Game game = API.Instance.Database.Games.Get(id);
            if (game != null)
            {
                AddTag(game);
            }
        }

        public void AddTag(IEnumerable<Guid> ids, string message)
        {
            GlobalProgressOptions options = new GlobalProgressOptions(message)
            {
                Cancelable = true,
                IsIndeterminate = ids.Count() == 1
            };

            _ = API.Instance.Dialogs.ActivateGlobalProgress((a) =>
            {
                try
                {
                    API.Instance.Database.BeginBufferUpdate();
                    API.Instance.Database.Games.BeginBufferUpdate();

                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    a.ProgressMaxValue = ids.Count();

                    foreach (Guid id in ids)
                    {
                        Game game = API.Instance.Database.Games.Get(id);
                        if (game == null)
                        {
                            continue;
                        }

                        a.Text = message
                            + (ids.Count() == 1 ? string.Empty : "\n\n" + $"{a.CurrentProgressValue}/{a.ProgressMaxValue}")
                            + "\n" + game?.Name + (game?.Source == null ? string.Empty : $" ({game?.Source.Name})");

                        if (a.CancelToken.IsCancellationRequested)
                        {
                            break;
                        }

                        Thread.Sleep(10);

                        try
                        {
                            RemoveTag(game);
                            AddTag(game);
                        }
                        catch (Exception ex)
                        {
                            Common.LogError(ex, false, false, PluginName);
                        }

                        a.CurrentProgressValue++;
                    }

                    stopWatch.Stop();
                    TimeSpan ts = stopWatch.Elapsed;
                    Logger.Info($"AddTag(){(a.CancelToken.IsCancellationRequested ? " canceled" : string.Empty)} - {string.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10)} for {a.CurrentProgressValue}/{(double)ids.Count()} items");
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, false, PluginName);
                }
                finally
                {
                    API.Instance.Database.Games.EndBufferUpdate();
                    API.Instance.Database.EndBufferUpdate();
                }
            }, options);
        }

        public void AddTagAllGame()
        {
            Logger.Info($"AddTagAllGame() started");
            IEnumerable<Guid> ids = API.Instance.Database.Games.Where(x => x.Hidden == false).Select(x => x.Id);
            AddTag(ids, $"{PluginName} - {ResourceProvider.GetString("LOCCommonAddingAllTag")}");
        }

        public void AddTagSelectData()
        {
            Logger.Info($"AddTagSelectData() started");

            OptionsDownloadData View = new OptionsDownloadData(this, true);
            Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(PluginName + " - " + ResourceProvider.GetString("LOCCommonSelectGames"), View);
            _ = windowExtension.ShowDialog();

            List<Game> playniteDb = View.GetFilteredGames();
            TagMissing = View.GetTagMissing();

            if (playniteDb == null)
            {
                TagMissing = false;
                return;
            }

            IEnumerable<Guid> ids = playniteDb.Select(x => x.Id);
            AddTag(ids, $"{PluginName} - {ResourceProvider.GetString("LOCCommonAddingAllTag")}");
            TagMissing = false;
        }


        public void RemoveTag(Game game)
        {
            if (game?.TagIds != null)
            {
                game.TagIds = game.TagIds.Where(x => !PluginTags.Any(y => x == y.Id)).ToList();
                API.Instance.MainView.UIDispatcher?.Invoke(() =>
                {
                    API.Instance.Database.Games.Update(game);
                    game.OnPropertyChanged();
                });
            }
        }

        public void RemoveTag(Guid id)
        {
            Game game = API.Instance.Database.Games.Get(id);
            if (game != null)
            {
                RemoveTag(game);
            }
        }


        public void RemoveTagAllGame(bool fromClearDatabase = false)
        {
            Common.LogDebug(true, "RemoveTagAllGame()");

            string message = fromClearDatabase
                ? $"{PluginName} - {ResourceProvider.GetString("LOCCommonClearingAllTag")}"
                : $"{PluginName} - {ResourceProvider.GetString("LOCCommonRemovingAllTag")}";

            GlobalProgressOptions options = new GlobalProgressOptions(message)
            {
                Cancelable = true,
                IsIndeterminate = false
            };

            _ = API.Instance.Dialogs.ActivateGlobalProgress((a) =>
            {
                try
                {
                    Logger.Info($"RemoveTagAllGame() started");
                    API.Instance.Database.BeginBufferUpdate();
                    API.Instance.Database.Games.BeginBufferUpdate();

                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    IEnumerable<Game> playniteDb = API.Instance.Database.Games.Where(x => x.Hidden == false);
                    a.ProgressMaxValue = playniteDb.Count();

                    foreach (Game game in playniteDb)
                    {
                        a.Text = message
                            + "\n\n" + $"{a.CurrentProgressValue}/{a.ProgressMaxValue}"
                            + "\n" + game?.Name + (game?.Source == null ? string.Empty : $" ({game?.Source.Name})");

                        if (a.CancelToken.IsCancellationRequested)
                        {
                            break;
                        }

                        try
                        {
                            RemoveTag(game);
                        }
                        catch (Exception ex)
                        {
                            Common.LogError(ex, false, false, PluginName);
                        }

                        a.CurrentProgressValue++;
                    }

                    stopWatch.Stop();
                    TimeSpan ts = stopWatch.Elapsed;
                    Logger.Info($"RemoveTagAllGame(){(a.CancelToken.IsCancellationRequested ? " canceled" : string.Empty)} - {string.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10)} for {a.CurrentProgressValue}/{(double)playniteDb.Count()} items");
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, false, PluginName);
                }
                finally
                {
                    API.Instance.Database.Games.EndBufferUpdate();
                    API.Instance.Database.EndBufferUpdate();
                }
            }, options);
        }


        internal virtual Guid? FindGoodPluginTags(string tagName)
        {
            return CheckTagExist(tagName);
        }
        #endregion


        public void ClearCache()
        {
            string PathDirectory = Path.Combine(PlaynitePaths.DataCachePath, PluginName);

            GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions($"{PluginName} - {ResourceProvider.GetString("LOCCommonProcessing")}")
            {
                Cancelable = false,
                IsIndeterminate = true
            };

            _ = API.Instance.Dialogs.ActivateGlobalProgress((activateGlobalProgress) =>
            {
                try
                {
                    Thread.Sleep(2000);
                    FileSystem.DeleteDirectory(PathDirectory, true);
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, false, PluginName);
                    _ = API.Instance.Dialogs.ShowErrorMessage(
                        string.Format(ResourceProvider.GetString("LOCCommonErrorDeleteCache"), PathDirectory),
                        PluginName
                    );
                }
            }, globalProgressOptions);
        }


        public virtual void Games_ItemUpdated(object sender, ItemUpdatedEventArgs<Game> e)
        {
            try
            {
                if (e?.UpdatedItems?.Count > 0 && Database != null)
                {
                    e?.UpdatedItems?.ForEach(x =>
                    {
                        if (x.NewData?.Id != null)
                        {
                            Database.SetGameInfo<T>(x.NewData.Id);
                        }
                    });

                    // If auto update for installed
                    object settings = PluginSettings.GetType().GetProperty("Settings").GetValue(PluginSettings);
                    PropertyInfo propertyInfo = settings.GetType().GetProperty("AutoImportOnInstalled");
                    if (propertyInfo != null && (bool)propertyInfo.GetValue(settings))
                    {
                        // Only for a new installation
                        List<Guid> ids = e.UpdatedItems.Where(x => !x.OldData.IsInstalled & x.NewData.IsInstalled && !PreviousIds.Contains(x.NewData.Id)).Select(x => x.NewData.Id).ToList();
                        PreviousIds = ids;
                        if (ids?.Count() > 0)
                        {
                            RefreshInstalled(ids);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, false, PluginName);
            }
        }

        private void Games_ItemCollectionChanged(object sender, ItemCollectionChangedEventArgs<Game> e)
        {
            try
            {
                e?.RemovedItems?.ForEach(x =>
                {
                    _ = Remove(x);
                });
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, false, PluginName);
            }
        }

        public Guid? AddNoDataTag()
        {
            return CheckTagExist($"{ResourceProvider.GetString("LOCNoData")}");
        }

        public virtual void SetThemesResources(Game game)
        {
        }


        #region Extract data
        public bool ExtractToCsv(string path, bool minimum)
        {
            bool isOK = false;
            try
            {
                Logger.Info($"ExtractToCsv({minimum}) started");
                GlobalProgressOptions options = new GlobalProgressOptions($"{PluginName} - {ResourceProvider.GetString("LOCCommonExtracting")}")
                {
                    Cancelable = true,
                    IsIndeterminate = false
                };

                _ = API.Instance.Dialogs.ActivateGlobalProgress((a) =>
                {
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    try
                    {
                        path = Path.Combine(path, $"{PluginName}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv");
                        path = CommonPlayniteShared.Common.Paths.FixPathLength(path);
                        FileSystem.PrepareSaveFile(path);
                        string csvData = GetCsvData(a, minimum);
                        if (!csvData.IsNullOrEmpty())
                        {
                            File.WriteAllText(path, csvData, Encoding.UTF8);
                            isOK = true;
                        }
                        else
                        {
                            Logger.Warn($"No csv data for {PluginName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Common.LogError(ex, false, true, PluginName);
                    }

                    stopWatch.Stop();
                    TimeSpan ts = stopWatch.Elapsed;
                    Logger.Info($"ExtractToCsv({minimum}){(a.CancelToken.IsCancellationRequested ? " canceled" : string.Empty)} - {string.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10)} for {Database.Items?.Count()} items");
                }, options);
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginName);
            }

            return isOK;
        }

        internal virtual string GetCsvData(GlobalProgressActionArgs a, bool minimum)
        {
            return null;
        }
        #endregion
    }
}
