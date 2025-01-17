﻿//-----------------------------------------------------------------------
// <copyright file="TradeBarViewModel.cs" company="Wohs">
//     Missing Copyright information from a valid stylecop.json file.
// </copyright>
//-----------------------------------------------------------------------

namespace Lurker.UI.ViewModels
{
    using Caliburn.Micro;
    using Lurker.Events;
    using Lurker.Helpers;
    using Lurker.Services;
    using Lurker.UI.Helpers;
    using Lurker.UI.Models;
    using NAudio.Wave;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public class TradebarViewModel : PoeOverlayBase
    {
        #region Fields

        private static int DefaultOverlayHeight = 60;

        private ClientLurker _lurker;
        private PoeKeyboardHelper _keyboardHelper;
        private TradebarContext _context;
        private List<OfferViewModel> _activeOffers = new List<OfferViewModel>();
        private IEventAggregator _eventAggregator;
        private System.Action _removeActive;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TradeBarViewModal"/> class.
        /// </summary>
        /// <param name="lurker">The lurker.</param>
        /// <param name="dockingHelper">The docking helper.</param>
        /// <param name="keyboardHelper">The keyboard helper.</param>
        public TradebarViewModel(IEventAggregator eventAggregator, ClientLurker lurker, DockingHelper dockingHelper, PoeKeyboardHelper keyboardHelper, SettingsService settingsService, IWindowManager windowManager)
            : base (windowManager, dockingHelper, lurker, settingsService)
        {
            this._eventAggregator = eventAggregator;
            this._lurker = lurker;
            this._keyboardHelper = keyboardHelper;
            this._settingsService = settingsService;
            this.TradeOffers = new ObservableCollection<OfferViewModel>();

            this._lurker.IncomingOffer += this.Lurker_IncomingOffer;
            this._lurker.TradeAccepted += this.Lurker_TradeAccepted;
            this._lurker.PlayerJoined += this.Lurker_PlayerJoined;
            this._lurker.PlayerLeft += this.Lurker_PlayerLeft;

            this._context = new TradebarContext(this.RemoveOffer, this.AddActiveOffer, this.SetActiveOffer);
            this.DisplayName = "Poe Lurker";
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the trade offers.
        /// </summary>
        public ObservableCollection<OfferViewModel> TradeOffers { get; set; }

        /// <summary>
        /// Gets the active offer.
        /// </summary>
        private OfferViewModel ActiveOffer => this._activeOffers.FirstOrDefault();

        #endregion

        #region Methods

        /// <summary>
        /// Searches the item.
        /// </summary>
        public void SearchItem()
        {
            var activeOffer = this.ActiveOffer;
            if (activeOffer != null)
            {
                this._keyboardHelper.Search(activeOffer.BuildSearchItemName());
            }
        }

        /// <summary>
        /// Lurkers the new offer.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The trade event.</param>
        private void Lurker_IncomingOffer(object sender, Events.TradeEvent e)
        {
            if (this.TradeOffers.Any(o => o.Event.Equals(e)))
            {
                return;
            }

            if (this._settingsService.AlertEnabled)
            {
                this.PlayAlert();
            }

            Execute.OnUIThread(() => 
            {
                this.TradeOffers.Add(new OfferViewModel(e, this._keyboardHelper, this._context, this._settingsService));
            });
        }

        /// <summary>
        /// Plays the alert.
        /// </summary>
        private void PlayAlert()
        {
            try
            {
                var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Lurker.UI.Assets.TradeAlert.mp3");
                var waveOut = new WaveOutEvent();
                var mp3Reader = new Mp3FileReader(stream);
                waveOut.Init(mp3Reader);
                waveOut.Volume = this._settingsService.AlertVolume;
                waveOut.Play();

                EventHandler<StoppedEventArgs> handler = default;
                handler = (object s, StoppedEventArgs e) =>
                {
                    stream.Dispose();
                    mp3Reader.Dispose();
                    waveOut.Dispose();
                    waveOut.PlaybackStopped -= handler;
                };

                waveOut.PlaybackStopped += handler;
            }
            catch
            {
            }
        }

        /// <summary>
        /// Lurkers the trade accepted.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Lurker_TradeAccepted(object sender, Events.TradeAcceptedEvent e)
        {
            var offer = this.TradeOffers.Where(t => t.Status == OfferStatus.Traded).FirstOrDefault();
            if (offer != null)
            {
                if (!string.IsNullOrEmpty(this._settingsService.ThankYouMessage))
                {
                    this._keyboardHelper.Whisper(offer.PlayerName, this._settingsService.ThankYouMessage);
                }

                this._keyboardHelper.Kick(offer.PlayerName);
                this.RemoveOffer(offer);
            }
        }

        /// <summary>
        /// Lurkers the player joined.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The PLayerJoined Event.</param>
        private void Lurker_PlayerJoined(object sender, Events.PlayerJoinedEvent e)
        {
            foreach (var offer in this.TradeOffers.Where(o => o.PlayerName == e.PlayerName))
            {
                offer.BuyerInSameInstance = true;
            }
        }

        /// <summary>
        /// Lurkers the player left.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void Lurker_PlayerLeft(object sender, Events.PlayerLeftEvent e)
        {
            foreach (var offer in this.TradeOffers.Where(o => o.PlayerName == e.PlayerName))
            {
                offer.BuyerInSameInstance = false;
            }
        }

        /// <summary>
        /// Removes the offer.
        /// </summary>
        /// <param name="offer">The offer.</param>
        private void RemoveOffer(OfferViewModel offer)
        {
            if (offer != null)
            {
                Execute.OnUIThread(() => 
                { 
                    this.TradeOffers.Remove(offer);
                    this._activeOffers.Remove(offer);

                    var activeOffer = this.ActiveOffer;
                    if (this.ActiveOffer != null)
                    {
                        this.ActiveOffer.Active = true;
                        this.SendToLifeBulb(this.ActiveOffer.Event);
                    }
                    else
                    {
                        this._removeActive?.Invoke();
                    }

                    offer.Dispose();
                });
            }
        }

        /// <summary>
        /// Adds the active offer.
        /// </summary>
        /// <param name="offer">The offer.</param>
        private void AddActiveOffer(OfferViewModel offer)
        {
            this._activeOffers.Add(offer);
            this.ActiveOffer.Active = true;


            this.SendToLifeBulb(this.ActiveOffer.Event);
        }

        /// <summary>
        /// Sends to life bulb.
        /// </summary>
        /// <param name="tradeEvent">The trade event.</param>
        private void SendToLifeBulb(TradeEvent tradeEvent)
        {
            this._eventAggregator.PublishOnUIThread(new LifeBulbMessage()
            {
                View = new PositionViewModel(tradeEvent),
                OnShow = (a) => { this._removeActive = a; },
                Action = this.SearchItem
            });
        }

        /// <summary>
        /// Sets the active offer.
        /// </summary>
        /// <param name="offer">The offer.</param>
        private void SetActiveOffer(OfferViewModel offer)
        {
            var currentActiveOffer = this.ActiveOffer;
            if (currentActiveOffer == null)
            {
                this.AddActiveOffer(offer);
                return;
            }

            if (currentActiveOffer == offer)
            {
                return;
            }

            currentActiveOffer.Active = false;

            var index = this._activeOffers.IndexOf(offer);
            if (index != -1)
            {
                this._activeOffers.RemoveAt(index);
            }

            this._activeOffers.Insert(0, offer);
            this.ActiveOffer.Active = true;

            this.SendToLifeBulb(offer.Event);
        }

        /// <summary>
        /// Called when deactivating.
        /// </summary>
        /// <param name="close">Inidicates whether this instance will be closed.</param>
        protected override void OnDeactivate(bool close)
        {
            if (close)
            {
                this._lurker.IncomingOffer -= this.Lurker_IncomingOffer;
                this._lurker.TradeAccepted -= this.Lurker_TradeAccepted;
                this._lurker.PlayerJoined -= this.Lurker_PlayerJoined;
                this._lurker.PlayerLeft -= this.Lurker_PlayerLeft;
            }

            base.OnDeactivate(close);
        }

        /// <summary>
        /// Sets the window position.
        /// </summary>
        /// <param name="windowInformation"></param>
        protected override void SetWindowPosition(PoeWindowInformation windowInformation)
        {
            var overlayHeight = DefaultOverlayHeight * windowInformation.FlaskBarHeight / DefaultFlaskBarHeight;
            var overlayWidth = (windowInformation.Width - (windowInformation.FlaskBarWidth * 2)) / 2;

            Execute.OnUIThread(() =>
            {
                this._view.Height = overlayHeight;
                this._view.Width = overlayWidth;
                this._view.Left = windowInformation.Position.Left + windowInformation.FlaskBarWidth + Margin;
                this._view.Top = windowInformation.Position.Bottom - overlayHeight - windowInformation.ExpBarHeight - Margin;
            });
        }

        #endregion
    }
}
