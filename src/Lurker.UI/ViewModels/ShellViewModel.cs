//-----------------------------------------------------------------------
// <copyright file="ShellViewModel.cs" company="Wohs">
//     Missing Copyright information from a valid stylecop.json file.
// </copyright>
//-----------------------------------------------------------------------

namespace Lurker.UI
{
    using Caliburn.Micro;
    using Lurker.Helpers;
    using Lurker.Models;
    using Lurker.Services;
    using Lurker.UI.Helpers;
    using Lurker.UI.Models;
    using Lurker.UI.ViewModels;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Text;
    using System.Threading.Tasks;

    public class ShellViewModel : Conductor<Screen>.Collection.AllActive , IViewAware, IHandle<Screen>
    {
        #region Fields

        private SimpleContainer _container;
        private ClientLurker _currentLurker;
        private DockingHelper _currentDockingHelper;
        private ClipboardLurker _clipboardLurker;
        private TradebarViewModel _incomingTradeBarOverlay;
        private OutgoingbarViewModel _outgoingTradeBarOverlay;
        private LifeBulbViewModel _lifeBulbOverlay;
        private ManaBulbViewModel _manaBulbOverlay;
        private SettingsService _settingsService;
        private ItemOverlayViewModel _itemOverlay;
        private UpdateManager _updateManager;
        private SettingsViewModel _settingsViewModel;
        private IEventAggregator _eventAggregator;
        private bool _startWithWindows;
        private bool _needUpdate;
        private bool _showInTaskBar;
        private bool _isItemOverlayOpen;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellViewModel"/> class.
        /// </summary>
        /// <param name="windowManager">The window manager.</param>
        /// <param name="container">The container.</param>
        public ShellViewModel(SimpleContainer container, SettingsService settingsService, UpdateManager updateManager, SettingsViewModel settingsViewModel, IEventAggregator eventAggregator)
        {
            this._eventAggregator = eventAggregator;
            this._settingsService = settingsService;
            this._container = container;
            this._updateManager = updateManager;
            this._settingsViewModel = settingsViewModel;

            this.WaitForPoe();
            this.StartWithWindows = File.Exists(this.ShortcutFilePath);
            this.ShowInTaskBar = true;

            if (settingsService.FirstLaunch)
            {
                settingsService.FirstLaunch = false;
                settingsService.Save();
                Process.Start("https://github.com/C1rdec/Poe-Lurker/releases/latest");
            }

            this._eventAggregator.Subscribe(this);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the item overlay.
        /// </summary>
        public ItemOverlayViewModel ItemOverlayViewModel
        {
            get
            {
                return this._itemOverlay;
            }

            set
            {
                this._itemOverlay = value;
                this.NotifyOfPropertyChange();
            }
        }

        /// <summary>
        /// Gets the command.
        /// </summary>
        public DoubleClickCommand ShowSettingsCommand => new DoubleClickCommand(this.ShowSettings);

        /// <summary>
        /// Gets or sets a value indicating whether [show in task bar].
        /// </summary>
        public bool ShowInTaskBar
        {
            get
            {
                return this._showInTaskBar;
            }

            set
            {
                this._showInTaskBar = value;
                this.NotifyOfPropertyChange();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is item open.
        /// </summary>
        public bool IsItemOverlayOpen
        {
            get
            {
                return this._isItemOverlayOpen;
            }

            set
            {
                this._isItemOverlayOpen = value;
                this.NotifyOfPropertyChange();
            }
        }

        /// <summary>
        /// Gets a value indicating whether [start with windows].
        /// </summary>
        public bool StartWithWindows
        {
            get
            {
                return this._startWithWindows;
            }

            set
            {
                this._startWithWindows = value;
                this.NotifyOfPropertyChange();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [need update].
        /// </summary>
        public bool NeedUpdate
        {
            get
            {
                return this._needUpdate;
            }

            set
            {
                this._needUpdate = value;
                this.NotifyOfPropertyChange();
            }
        }

        /// <summary>
        /// Gets the name of the shortcut.
        /// </summary>
        public string ShortcutName => "PoeLurker.lnk";

        /// <summary>
        /// Gets the application data folder path.
        /// </summary>
        public string ApplicationDataFolderPath => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        /// <summary>
        /// Gets the startup folder path.
        /// </summary>
        public string StartupFolderPath => Path.Combine(this.ApplicationDataFolderPath, @"Microsoft\Windows\Start Menu\Programs\Startup");

        /// <summary>
        /// Gets the shortcut file path.
        /// </summary>
        public string ShortcutFilePath => Path.Combine(this.StartupFolderPath, this.ShortcutName);

        /// <summary>
        /// Gets the version.
        /// </summary>
        public string Version => GetAssemblyVersion();

        #endregion

        #region Methods

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public void Close()
        {
            this._eventAggregator.Unsubscribe(this);
            this.CleanUp();
            this.TryClose();
        }

        /// <summary>
        /// Creates the short cut.
        /// </summary>
        public void CreateShortCut()
        {
            if (File.Exists(this.ShortcutFilePath))
            {
                File.Delete(this.ShortcutFilePath);
            }
            else
            {
                var link = (IShellLink)new ShellLink();
                link.SetDescription("PoeLurker");
                link.SetPath(System.Reflection.Assembly.GetExecutingAssembly().Location);
                var file = (IPersistFile)link;
                file.Save(this.ShortcutFilePath, false);
            }

            this.StartWithWindows = !this.StartWithWindows;
        }

        /// <summary>
        /// Gets the assembly version.
        /// </summary>
        /// <returns>The assembly version</returns>
        private static string GetAssemblyVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var information = FileVersionInfo.GetVersionInfo(assembly.Location);
            var version = information.FileVersion.Remove(information.FileVersion.Length - 2);
            return version;
        }

        /// <summary>
        /// Updates this instance.
        /// </summary>
        public async void Update()
        {
            this.ShowInTaskBar = false;
            await this._updateManager.Update();
        }

        /// <summary>
        /// Shows the settings.
        /// </summary>
        public void ShowSettings()
        {
            if (this._settingsViewModel.IsActive)
            {
                return;
            }

            this.ActivateItem(this._settingsViewModel);
        }

        /// <summary>
        /// Registers the instances.
        /// </summary>
        private void ShowOverlays(IntPtr windowHandle)
        {
            Execute.OnUIThread(() =>
            {
                var keyboarHelper = new PoeKeyboardHelper(windowHandle);
                this._currentDockingHelper = new DockingHelper(windowHandle);
                this._clipboardLurker = new ClipboardLurker(this._settingsService);
                this._clipboardLurker.Newitem += this.ClipboardLurker_Newitem;

                this._container.RegisterInstance(typeof(ClientLurker), null, this._currentLurker);
                this._container.RegisterInstance(typeof(ClipboardLurker), null, this._clipboardLurker);
                this._container.RegisterInstance(typeof(DockingHelper), null, this._currentDockingHelper);
                this._container.RegisterInstance(typeof(PoeKeyboardHelper), null, keyboarHelper);

                this._incomingTradeBarOverlay = this._container.GetInstance<TradebarViewModel>();
                this._outgoingTradeBarOverlay = this._container.GetInstance<OutgoingbarViewModel>();
                this._lifeBulbOverlay = this._container.GetInstance<LifeBulbViewModel>();
                //this._manaBulbOverlay = this._container.GetInstance<ManaBulbViewModel>();

                this.ActivateItem(this._incomingTradeBarOverlay);
                this.ActivateItem(this._outgoingTradeBarOverlay);
                this.ActivateItem(this._lifeBulbOverlay);
                //this.ActivateItem(this._manaBulbOverlay);
            });
        }

        /// <summary>
        /// Handles the PoeClosed event of the CurrentLurker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void CurrentLurker_PoeClosed(object sender, System.EventArgs e)
        {
            this.CleanUp();
            this.WaitForPoe();
        }

        /// <summary>
        /// Cleans up.
        /// </summary>
        private void CleanUp()
        {
            this._container.UnregisterHandler<ClientLurker>();
            this._container.UnregisterHandler<DockingHelper>();
            this._container.UnregisterHandler<PoeKeyboardHelper>();
            this._container.UnregisterHandler<ClipboardLurker>();

            if (this._clipboardLurker != null)
            {
                this._clipboardLurker.Newitem -= this.ClipboardLurker_Newitem;
                this._clipboardLurker.Dispose();
                this._clipboardLurker = null;
            }

            this._currentLurker.PoeClosed -= this.CurrentLurker_PoeClosed;
            this._currentLurker.Dispose();
            this._currentLurker = null;

            if (this._currentDockingHelper != null)
            {
                this._currentDockingHelper.Dispose();
                this._currentDockingHelper = null;
            }
        }

        /// <summary>
        /// Waits for poe.
        /// </summary>
        private async void WaitForPoe()
        {
            await AffixService.InitializeAsync();
            await this.CheckForUpdate();

            this._currentLurker = new ClientLurker();
            this._currentLurker.PoeClosed += CurrentLurker_PoeClosed;
            var windowHandle = await this._currentLurker.WaitForPoe();

            this.ShowOverlays(windowHandle);
        }

        /// <summary>
        /// Updates this instance.
        /// </summary>
        private async Task CheckForUpdate()
        {
            this.NeedUpdate = await this._updateManager.CheckForUpdate();
        }

        /// <summary>
        /// Clipboards the lurker newitem.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ClipboardLurker_Newitem(object sender, PoeItem e)
        {
            this.IsItemOverlayOpen = false;
            this.ItemOverlayViewModel = new ItemOverlayViewModel(e, () => { this.IsItemOverlayOpen = false; });
            this.IsItemOverlayOpen = true;
        }

        /// <summary>
        /// Handles the specified screen.
        /// </summary>
        /// <param name="screen">The screen.</param>
        public void Handle(Screen screen)
        {
            if (screen.IsActive)
            {
                return;
            }

            this.ActivateItem(screen);
        }

        #endregion
    }

    [ComImport]
    [Guid("00021401-0000-0000-C000-000000000046")]
    internal class ShellLink
    {
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214F9-0000-0000-C000-000000000046")]
    internal interface IShellLink
    {
        void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, int fFlags);
        void GetIDList(out IntPtr ppidl);
        void SetIDList(IntPtr pidl);
        void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
        void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
        void GetHotkey(out short pwHotkey);
        void SetHotkey(short wHotkey);
        void GetShowCmd(out int piShowCmd);
        void SetShowCmd(int iShowCmd);
        void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
        void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
        void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
        void Resolve(IntPtr hwnd, int fFlags);
        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }
}