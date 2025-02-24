using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommonPluginsStores;
using CommonPluginsStores.Models;
using CommonPluginsStores.Steam;
using CommonPluginsStores.Steam.Models;
using Playnite.SDK;

namespace CommonPluginsControls.Stores.Epic
{
    public class PanelViewModel : ObservableObject
    {
        internal IStoreApi StoreApi { get; set; }
        public AccountInfos User => StoreApi?.CurrentAccountInfos;

        private bool useAuth = true;
        public bool UseAuth { get => useAuth; set => SetValue(ref useAuth, value); }

        private bool forceAuth = false;
        public bool ForceAuth { get => forceAuth; set => SetValue(ref forceAuth, value); }

        public AuthStatus AuthStatus => StoreApi == null ? AuthStatus.Failed : StoreApi.IsUserLoggedIn ? AuthStatus.Ok : AuthStatus.AuthRequired;

        public RelayCommand<object> LoginCommand => new RelayCommand<object>((a) =>
        {
            StoreApi.Login();
            OnPropertyChanged(nameof(AuthStatus));
            OnPropertyChanged(nameof(User));
        });
    }
}
