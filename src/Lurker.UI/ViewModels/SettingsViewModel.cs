﻿//-----------------------------------------------------------------------
// <copyright file="SettingsViewModel.cs" company="Wohs">
//     Missing Copyright information from a valid stylecop.json file.
// </copyright>
//-----------------------------------------------------------------------

namespace Lurker.UI.ViewModels
{
    using Caliburn.Micro;
    using Lurker.Helpers;
    using Lurker.Services;
    using Lurker.UI.Helpers;
    using MahApps.Metro.Controls;
    using System;
    using System.Diagnostics;
    using System.Security.Authentication;
    using System.Threading.Tasks;

    public class SettingsViewModel: ScreenBase
    {
        #region Fields

        private KeyboardHelper _keyboardHelper;
        private SettingsService _settingService;
        private UpdateManager _updateManager;
        private bool _needsUpdate;
        private bool _pledging;
        private int _alertVolume;
        private Patreon.PatreonService _currentPatreonService;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsViewModel"/> class.
        /// </summary>
        public SettingsViewModel(IWindowManager windowManager, KeyboardHelper keyboardHelper, SettingsService settingsService, UpdateManager updateManager)
            : base(windowManager)
        {
            this._keyboardHelper = keyboardHelper;
            this._settingService = settingsService;
            this._updateManager = updateManager;
            this.DisplayName = "Settings";

            this.PropertyChanged += this.SettingsViewModel_PropertyChanged;
        }

        #endregion

        #region Events

        /// <summary>
        /// Raises the Close event.
        /// </summary>
        public event EventHandler OnClose;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this <see cref="SettingsViewModel"/> is connected.
        /// </summary>
        public bool NotConnected => !new Patreon.TokenService().Connected;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="SettingsViewModel"/> is pledging.
        /// </summary>
        public bool Pledging
        {
            get
            {
                return this._pledging;
            }

            set
            {
                this._pledging = value;
                this.NotifyOfPropertyChange();
                this.NotifyOfPropertyChange("NotPledging");
            }
        }

        /// <summary>
        /// Gets a value indicating whether [not pledgin].
        /// </summary>
        public bool NotPledging => !this.Pledging && !this.NotConnected;

        /// <summary>
        /// Gets or sets a value indicating whether [needs update].
        /// </summary>
        public bool NeedsUpdate
        {
            get
            {
                return this._needsUpdate;
            }

            set
            {
                this._needsUpdate = value;
                this.NotifyOfPropertyChange();
            }
        }

        /// <summary>
        /// Gets or sets the busy message.
        /// </summary>
        public string BusyMessage
        {
            get
            {
                return this._settingService.BusyMessage;
            }

            set
            {
                this._settingService.BusyMessage = value;
                this.NotifyOfPropertyChange();
            }
        }

        /// <summary>
        /// Gets or sets the sold message.
        /// </summary>
        public string SoldMessage
        {
            get
            {
                return this._settingService.SoldMessage;
            }

            set
            {
                this._settingService.SoldMessage = value;
                this.NotifyOfPropertyChange();
            }
        }

        /// <summary>
        /// Gets or sets the thank you message.
        /// </summary>
        public string ThankYouMessage
        {
            get
            {
                return this._settingService.ThankYouMessage;
            }

            set
            {
                this._settingService.ThankYouMessage = value;
                this.NotifyOfPropertyChange();
            }
        }

        /// <summary>
        /// Gets or sets the still interested message.
        /// </summary>
        public string StillInterestedMessage
        {
            get
            {
                return this._settingService.StillInterestedMessage;
            }

            set
            {
                this._settingService.StillInterestedMessage = value;
                this.NotifyOfPropertyChange();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [alert enabled].
        /// </summary>
        public bool AlertEnabled
        {
            get
            {
                return this._settingService.AlertEnabled;
            }

            set
            {
                this._settingService.AlertEnabled = value;
                this.NotifyOfPropertyChange();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [alert enabled].
        /// </summary>
        public bool SearchEnabled
        {
            get
            {
                return this._settingService.SearchEnabled;
            }

            set
            {
                this._settingService.SearchEnabled = value;
                this.NotifyOfPropertyChange();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [clipboard enabled].
        /// </summary>
        public bool ClipboardEnabled
        {
            get
            {
                return this._settingService.ClipboardEnabled;
            }

            set
            {
                this._settingService.ClipboardEnabled = value;
                this.NotifyOfPropertyChange();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [debug enabled].
        /// </summary>
        public bool DebugEnabled
        {
            get
            {
                return this._settingService.DebugEnabled;
            }

            set
            {
                this._settingService.DebugEnabled = value;
                this.NotifyOfPropertyChange();
            }
        }

        /// <summary>
        /// Gets or sets the alert volume.
        /// </summary>
        public int AlertVolume
        {
            get
            {
                return this._alertVolume;
            }

            set
            {
                this._alertVolume = value;
                this.NotifyOfPropertyChange();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [tool tip enabled].
        /// </summary>
        public bool ToolTipEnabled
        {
            get
            {
                return this._settingService.ToolTipEnabled;
            }

            set
            {
                this._settingService.ToolTipEnabled = value;
                this.NotifyOfPropertyChange();
            }
        }

        /// <summary>
        /// Gets or sets the tool tip delay.
        /// </summary>
        public int ToolTipDelay
        {
            get
            {
                return this._settingService.ToolTipDelay;
            }

            set
            {
                this._settingService.ToolTipDelay = value;
                this.NotifyOfPropertyChange();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Inserts the buyer name token.
        /// </summary>
        public void InsertBuyerNameToken()
        {
            this._keyboardHelper.Write(TokenHelper.BuyerName);
        }

        /// <summary>
        /// Inserts the price token.
        /// </summary>
        public void InsertPriceToken()
        {
            this._keyboardHelper.Write(TokenHelper.Price);
        }

        /// <summary>
        /// Inserts the item name token.
        /// </summary>
        public void InsertItemNameToken()
        {
            this._keyboardHelper.Write(TokenHelper.ItemName);
        }

        /// <summary>
        /// Updates this instance.
        /// </summary>
        public async void Update()
        {
            await this._updateManager.Update();
        }

        /// <summary>
        /// Logins to patreon.
        /// </summary>
        public async void LoginToPatreon()
        {
            if (this._currentPatreonService != null)
            {
                this._currentPatreonService.Cancel();
                await Task.Delay(600);
            }

            try
            {
                using (this._currentPatreonService = new Patreon.PatreonService())
                {
                    this.Pledging = await this._currentPatreonService.IsPledging();
                    this.NotifyOfPropertyChange("NotConnected");
                }
            }
            catch (AuthenticationException)
            { 
            }
        }

        /// <summary>
        /// Pledges this instance.
        /// </summary>
        public void Pledge()
        {
            Process.Start("https://www.patreon.com/poelurker");
        }

        /// <summary>
        /// Called when an attached view's Loaded event fires.
        /// </summary>
        /// <param name="view"></param>
        protected override void OnViewLoaded(object view)
        {
            var window = view as MetroWindow;
            window.Activate();
            base.OnViewLoaded(view);
        }

        /// <summary>
        /// Called when deactivating.
        /// </summary>
        /// <param name="close">Inidicates whether this instance will be closed.</param>
        protected override void OnDeactivate(bool close)
        {
            if (close)
            {
                this._settingService.Save();
                this.OnClose?.Invoke(this, EventArgs.Empty);
            }

            base.OnDeactivate(close);
        }

        /// <summary>
        /// Called when activating.
        /// </summary>
        protected async override void OnActivate()
        {
            if (!this.NotConnected)
            {
                using (var service = new Patreon.PatreonService())
                {
                    this.Pledging = await service.IsPledging();
                    if (!this.Pledging)
                    {
                        this._settingService.SearchEnabled = false;
                    }
                }
            }
            else
            {
                this.Pledging = false;
            }

            this.AlertVolume = (int)(this._settingService.AlertVolume * 100);
            this.CheckForUpdate();
            base.OnActivate();
        }

        /// <summary>
        /// Checks for update.
        /// </summary>
        private async void CheckForUpdate()
        {
            this.NeedsUpdate = await this._updateManager.CheckForUpdate();
        }

        /// <summary>
        /// Handles the PropertyChanged event of the SettingsViewModel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void SettingsViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.AlertVolume))
            {
                this._settingService.AlertVolume = (float)this.AlertVolume / 100;
            }
        }

        #endregion
    }
}
