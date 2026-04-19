using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace WgcfGUI
{
    public partial class AndroidAppWindow : Wpf.Ui.Controls.FluentWindow
    {
        public ObservableCollection<AndroidAppInfo> Apps { get; set; } = new ObservableCollection<AndroidAppInfo>();
        private ICollectionView _appCollectionView;

        public string ResultExcludedApps { get; private set; } = string.Empty;

        public AndroidAppWindow()
        {
            InitializeComponent();
            LoadApps();
            _appCollectionView = CollectionViewSource.GetDefaultView(Apps);
            _appCollectionView.Filter = FilterApps;
            AppListView.ItemsSource = _appCollectionView;
            UpdatePreview();
        }
        // Just a random list of apps I use that I thought would be good to have in the app. Change as needed.
        private void LoadApps()
        {
            var rawList = new System.Collections.Generic.List<dynamic>();

            // --- Video Streaming (TV + Mobile + Beta variants grouped by service) ---
            rawList.Add(new { pkg = "com.netflix.ninja, com.netflix.mediaclient", name = "Netflix", cat = "Streaming" });
            rawList.Add(new { pkg = "com.amazon.amazonvideo.livingroom, com.amazon.avod.thirdpartyclient, com.amazon.avod", name = "Prime Video", cat = "Streaming" });
            rawList.Add(new { pkg = "com.disney.disneyplus, com.disney.disneyplus.tv", name = "Disney+", cat = "Streaming" });
            rawList.Add(new { pkg = "com.wbd.stream, com.hbo.hbonow, com.hbo.max", name = "Max", cat = "Streaming" });
            rawList.Add(new { pkg = "com.hulu.livingroomplus, com.hulu.plus", name = "Hulu", cat = "Streaming" });
            rawList.Add(new { pkg = "com.apple.atve.androidtv.appletv, com.apple.atve.sony.appletv", name = "Apple TV", cat = "Streaming" });
            rawList.Add(new { pkg = "com.google.android.youtube.tv, com.google.android.youtube", name = "YouTube", cat = "Streaming" });
            rawList.Add(new { pkg = "com.google.android.youtube.tvunplugged, com.google.android.apps.youtube.unplugged", name = "YouTube TV", cat = "Streaming" });
            rawList.Add(new { pkg = "com.google.android.youtube.tvkids, com.google.android.apps.youtube.kids", name = "YouTube Kids", cat = "Streaming" });
            rawList.Add(new { pkg = "com.google.android.apps.youtube.music, com.google.android.youtube.tvmusic", name = "YouTube Music", cat = "Streaming" });
            rawList.Add(new { pkg = "tv.twitch.android.app, tv.twitch.android.viewer", name = "Twitch", cat = "Streaming" });
            rawList.Add(new { pkg = "com.crunchyroll.crunchyroid, com.crunchyroll.titan", name = "Crunchyroll", cat = "Streaming" });
            rawList.Add(new { pkg = "com.peacocktv.peacockandroid", name = "Peacock", cat = "Streaming" });
            rawList.Add(new { pkg = "com.cbs.app, com.cbs.ott", name = "Paramount+", cat = "Streaming" });
            rawList.Add(new { pkg = "com.tubitv, com.tubitv.ott", name = "Tubi", cat = "Streaming" });
            rawList.Add(new { pkg = "tv.pluto.android", name = "Pluto TV", cat = "Streaming" });
            rawList.Add(new { pkg = "com.plexapp.android, com.plexapp.mediaserver.smb", name = "Plex", cat = "Streaming" });
            rawList.Add(new { pkg = "com.mubi", name = "MUBI", cat = "Streaming" });
            rawList.Add(new { pkg = "com.stremio.one, com.stremio.beta", name = "Stremio", cat = "Streaming" });
            rawList.Add(new { pkg = "org.xbmc.kodi", name = "Kodi", cat = "Streaming" });
            rawList.Add(new { pkg = "com.sling, com.sling.slingtv.android", name = "Sling TV", cat = "Streaming" });
            rawList.Add(new { pkg = "com.att.tv", name = "DirecTV Stream", cat = "Streaming" });
            rawList.Add(new { pkg = "tv.fubo.mobile, tv.fubo.android", name = "fuboTV", cat = "Streaming" });
            rawList.Add(new { pkg = "com.philo.philo.google", name = "Philo", cat = "Streaming" });
            rawList.Add(new { pkg = "com.vypv.starz", name = "STARZ", cat = "Streaming" });
            rawList.Add(new { pkg = "com.showtime.standalone", name = "Showtime", cat = "Streaming" });
            rawList.Add(new { pkg = "com.vudu.android", name = "Vudu", cat = "Streaming" });
            rawList.Add(new { pkg = "com.discovery.dplay", name = "Discovery+", cat = "Streaming" });
            rawList.Add(new { pkg = "com.viacom.vidcon", name = "VidCon", cat = "Streaming" });
            
            // --- Sports & News ---
            rawList.Add(new { pkg = "com.espn.score_center, com.espn.score_center.android.tv", name = "ESPN", cat = "Streaming" });
            rawList.Add(new { pkg = "com.foxsports.android, com.foxsports.videogo", name = "FOX Sports", cat = "Streaming" });
            rawList.Add(new { pkg = "com.turner.sports.nba.app", name = "NBA App", cat = "Streaming" });
            rawList.Add(new { pkg = "com.nhl.gc1112.free", name = "NHL", cat = "Streaming" });
            rawList.Add(new { pkg = "com.mlb.com.android", name = "MLB", cat = "Streaming" });
            rawList.Add(new { pkg = "com.nfl.mobile", name = "NFL", cat = "Streaming" });
            rawList.Add(new { pkg = "com.cnn.mobile.android.phone, com.cnn.mobile.android.tv", name = "CNN News", cat = "Streaming" });
            rawList.Add(new { pkg = "com.foxnews.android, com.foxnews.android.tv", name = "Fox News", cat = "Streaming" });
            rawList.Add(new { pkg = "bbc.mobile.news.ww", name = "BBC News", cat = "Streaming" });
            rawList.Add(new { pkg = "com.nbcuni.nbc.android.news", name = "NBC News", cat = "Streaming" });
            rawList.Add(new { pkg = "com.cbsnews.ott", name = "CBS News", cat = "Streaming" });
            rawList.Add(new { pkg = "com.abcnews.android.abcnews", name = "ABC News", cat = "Streaming" });

            // Gaming
            rawList.Add(new { pkg = "com.redphx.betterxc", name = "BetterXCloud", cat = "Gaming" });
            rawList.Add(new { pkg = "com.valvesoftware.steamlink", name = "Steam Link", cat = "Gaming" });
            rawList.Add(new { pkg = "com.microsoft.xcloud", name = "Xbox Cloud Gaming", cat = "Gaming" });
            rawList.Add(new { pkg = "com.nvidia.tegrazone3", name = "NVIDIA Games", cat = "Gaming" });
            rawList.Add(new { pkg = "com.moonlight_stream.moonlight", name = "Moonlight", cat = "Gaming" });
            rawList.Add(new { pkg = "tv.ouya.console", name = "Discover", cat = "Gaming" });

            // --- Music ---
            rawList.Add(new { pkg = "com.spotify.music, com.spotify.tv.android", name = "Spotify", cat = "Music" });
            rawList.Add(new { pkg = "com.pandora.android, com.pandora.android.atv", name = "Pandora", cat = "Music" });
            rawList.Add(new { pkg = "com.aspiro.tidal", name = "TIDAL", cat = "Music" });
            rawList.Add(new { pkg = "deezer.android.tv, com.deezer.android", name = "Deezer", cat = "Music" });
            rawList.Add(new { pkg = "com.apple.android.music", name = "Apple Music", cat = "Music" });
            rawList.Add(new { pkg = "com.amazon.mp3", name = "Amazon Music", cat = "Music" });
            rawList.Add(new { pkg = "com.siriusxm, com.siriusxm.tv", name = "SiriusXM", cat = "Music" });
            rawList.Add(new { pkg = "com.clearchannel.iheartradio.controller, com.clearchannel.iheartradio.tv", name = "iHeartRadio", cat = "Music" });
            rawList.Add(new { pkg = "com.tunein.player, com.tunein.player.tv", name = "TuneIn Radio", cat = "Music" });
            rawList.Add(new { pkg = "com.shazam.android", name = "Shazam", cat = "Music" });
            rawList.Add(new { pkg = "com.google.android.apps.musicsms", name = "Messages (Audio)", cat = "Music" });

            // Socials
            rawList.Add(new { pkg = "com.zhiliaoapp.musically", name = "TikTok", cat = "Social" });
            rawList.Add(new { pkg = "com.instagram.android", name = "Instagram", cat = "Social" });
            rawList.Add(new { pkg = "com.twitter.android", name = "X (Twitter)", cat = "Social" });
            rawList.Add(new { pkg = "com.snapchat.android", name = "Snapchat", cat = "Social" });
            rawList.Add(new { pkg = "com.reddit.frontpage", name = "Reddit", cat = "Social" });
            rawList.Add(new { pkg = "com.discord", name = "Discord", cat = "Social" });
            rawList.Add(new { pkg = "com.whatsapp", name = "WhatsApp", cat = "Social" });
            rawList.Add(new { pkg = "com.facebook.lite", name = "Facebook Lite", cat = "Social" });
            rawList.Add(new { pkg = "com.facebook.katana", name = "Facebook", cat = "Social" });

            // Banking/Finance
            rawList.Add(new { pkg = "com.chase.sig.android", name = "Chase Mobile", cat = "Banking" });
            rawList.Add(new { pkg = "com.infonow.bofa", name = "Bank of America", cat = "Banking" });
            rawList.Add(new { pkg = "com.wf.wellsfargomobile", name = "Wells Fargo", cat = "Banking" });
            rawList.Add(new { pkg = "com.citi.citimobile", name = "Citi Mobile", cat = "Banking" });
            rawList.Add(new { pkg = "com.konylabs.capitalone", name = "Capital One", cat = "Banking" });
            rawList.Add(new { pkg = "com.discoverfinancial.mobile", name = "Discover Mobile", cat = "Banking" });
            rawList.Add(new { pkg = "com.americanexpress.android.acctsvcs.us", name = "Amex", cat = "Banking" });
            rawList.Add(new { pkg = "com.paypal.android.p2pmobile", name = "PayPal", cat = "Banking" });
            rawList.Add(new { pkg = "com.venmo", name = "Venmo", cat = "Banking" });
            rawList.Add(new { pkg = "com.squareup.cash", name = "Cash App", cat = "Banking" });
            rawList.Add(new { pkg = "com.coinbase.android", name = "Coinbase", cat = "Banking" });
            rawList.Add(new { pkg = "com.robinhood.android", name = "Robinhood", cat = "Banking" });
            rawList.Add(new { pkg = "com.sofi.mobile", name = "SoFi", cat = "Banking" });
            rawList.Add(new { pkg = "com.usaa.mobile.android.usaa", name = "USAA Mobile", cat = "Banking" });
            rawList.Add(new { pkg = "com.navyfederal.android", name = "Navy Federal", cat = "Banking" });

            // --- Browsers ---
            rawList.Add(new { pkg = "com.android.chrome", name = "Google Chrome", cat = "Browsers" });
            rawList.Add(new { pkg = "org.mozilla.firefox, org.mozilla.tv.firefox", name = "Firefox", cat = "Browsers" });
            rawList.Add(new { pkg = "com.amazon.cloud9", name = "Amazon Silk", cat = "Browsers" });
            rawList.Add(new { pkg = "com.opera.browser, com.opera.sdk.example", name = "Opera", cat = "Browsers" });
            rawList.Add(new { pkg = "com.microsoft.emmx", name = "Microsoft Edge", cat = "Browsers" });
            rawList.Add(new { pkg = "com.tvbro.browser", name = "TV Bro", cat = "Browsers" });
            rawList.Add(new { pkg = "com.puffinsvr.itv", name = "Puffin TV", cat = "Browsers" });
            rawList.Add(new { pkg = "com.jio.web.tv", name = "Jio Pages TV", cat = "Browsers" });
            rawList.Add(new { pkg = "com.seraphic.openbrowser", name = "Open Browser", cat = "Browsers" });

            // --- Utility & Downloader Apps ---
            rawList.Add(new { pkg = "com.browser.downloader", name = "Downloader (AFTVnews)", cat = "Utility / Downloads" });
            rawList.Add(new { pkg = "com.ionitech.aio", name = "All-In-One Toolbox", cat = "Utility / Downloads" });
            rawList.Add(new { pkg = "com.estrongs.android.pop", name = "ES File Explorer", cat = "Utility / Downloads" });
            rawList.Add(new { pkg = "com.lonelycatgames.Xplore", name = "X-plore File Manager", cat = "Utility / Downloads" });
            rawList.Add(new { pkg = "com.speed.software.explorer", name = "Explorer", cat = "Utility / Downloads" });
            rawList.Add(new { pkg = "com.sentinel.vlc, org.videolan.vlc", name = "VLC Player", cat = "Utility / Downloads" });
            rawList.Add(new { pkg = "com.mxtech.videoplayer.ad, com.mxtech.videoplayer.pro", name = "MX Player", cat = "Utility / Downloads" });
            rawList.Add(new { pkg = "com.nova.videoplayer", name = "Nova Video Player", cat = "Utility / Downloads" });
            rawList.Add(new { pkg = "com.speedtest.android", name = "Speedtest by Ookla", cat = "Utility / Downloads" });
            rawList.Add(new { pkg = "com.analiti.fastest.android", name = "Analiti Speed Test", cat = "Utility / Downloads" });
            rawList.Add(new { pkg = "com.sand.airdroid", name = "AirDroid", cat = "Utility / Downloads" });
            rawList.Add(new { pkg = "com.cxinventor.file.explorer", name = "Cx File Explorer", cat = "Utility / Downloads" });
            rawList.Add(new { pkg = "pl.solidexplorer2", name = "Solid Explorer", cat = "Utility / Downloads" });

            // --- Cable / IPTV Providers ---
            rawList.Add(new { pkg = "com.xfinity.cloudtv", name = "Xfinity Stream", cat = "Streaming" });
            rawList.Add(new { pkg = "com.spectrum.fanzone", name = "Spectrum TV", cat = "Streaming" });
            rawList.Add(new { pkg = "com.cox.res.contour", name = "Cox Contour", cat = "Streaming" });
            rawList.Add(new { pkg = "com.optimum.alticeone", name = "Optimum TV", cat = "Streaming" });
            rawList.Add(new { pkg = "com.dish.anywhere", name = "DISH Anywhere", cat = "Streaming" });
            rawList.Add(new { pkg = "com.directv.dvrscheduler", name = "DirecTV", cat = "Streaming" });
            rawList.Add(new { pkg = "com.roku.na, com.roku.remote", name = "The Roku Channel", cat = "Streaming" });
            rawList.Add(new { pkg = "com.tivicompany.tivimate, com.tivicompany.tivimate.pro", name = "TiviMate", cat = "Streaming" });
            rawList.Add(new { pkg = "ru.iptvremote.android.iptv", name = "OTT Navigator", cat = "Streaming" });
            rawList.Add(new { pkg = "com.gsetech.gsesmartiptv", name = "GSE Smart IPTV", cat = "Streaming" });
            rawList.Add(new { pkg = "com.ipboot.perfectplayer", name = "Perfect Player", cat = "Streaming" });
            rawList.Add(new { pkg = "com.progdvb.android", name = "ProgTV", cat = "Streaming" });
            rawList.Add(new { pkg = "com.google.android.videos", name = "Google TV (Movies & TV)", cat = "Streaming" });
            rawList.Add(new { pkg = "com.samsung.android.tvplus", name = "Samsung TV Plus", cat = "Streaming" });

            // Utility/VPN Apps
            rawList.Add(new { pkg = "com.cloudflare.onedotonedotonedotone", name = "1.1.1.1 + WARP", cat = "Utility / Downloads" });
            rawList.Add(new { pkg = "com.wireguard.android", name = "WireGuard", cat = "Utility / Downloads" });
            rawList.Add(new { pkg = "com.tailscale.ipn", name = "Tailscale", cat = "Utility / Downloads" });
            rawList.Add(new { pkg = "com.nytimes.android", name = "NYTimes", cat = "Utility / Downloads" });

            // Basic System/Casting Apps
            rawList.Add(new { pkg = "com.google.android.katniss", name = "Google Search", cat = "System" });
            rawList.Add(new { pkg = "com.google.android.apps.mediashell", name = "Chromecast built-in", cat = "System" });
            rawList.Add(new { pkg = "com.google.android.tv", name = "Android TV Core Services", cat = "System" });
            rawList.Add(new { pkg = "com.android.hotwordenrollment.okgoogle", name = "Ok Google Enrollment", cat = "System" });
            rawList.Add(new { pkg = "com.google.android.tvlauncher", name = "Android TV Home", cat = "System" });
            rawList.Add(new { pkg = "com.google.android.tvrecommendations", name = "Android TV Core", cat = "System" });
            rawList.Add(new { pkg = "com.google.android.leanbacklauncher", name = "Leanback Launcher", cat = "System" });
            rawList.Add(new { pkg = "com.google.android.apps.tv.settings", name = "TV Settings", cat = "System" });
            rawList.Add(new { pkg = "com.google.android.tts", name = "Text-to-speech", cat = "System" });

            // Amazon & Apple & Google & Third Party Cast
            rawList.Add(new { pkg = "com.amazon.tv.launcher", name = "Fire TV Home", cat = "System" });
            rawList.Add(new { pkg = "com.amazon.firehomestarter", name = "Home Starter", cat = "System" });
            rawList.Add(new { pkg = "com.amazon.vizzini", name = "Fire TV Settings", cat = "System" });
            rawList.Add(new { pkg = "com.amazon.tv.nimh", name = "Network Services", cat = "System" });
            rawList.Add(new { pkg = "com.amazon.whitedome", name = "Update Service", cat = "System" });
            rawList.Add(new { pkg = "com.amazon.device.software.ota", name = "OTA Updates", cat = "System" });
            rawList.Add(new { pkg = "com.amazon.bueller.music", name = "Amazon Alexa", cat = "System" });
            rawList.Add(new { pkg = "com.amazon.ssm", name = "Screen Saver", cat = "System" });
            rawList.Add(new { pkg = "com.amazon.avod", name = "Prime Video Core", cat = "System" });
            rawList.Add(new { pkg = "com.amazon.storm.lightning.client.aosp", name = "Amazon Casting", cat = "System" });
            rawList.Add(new { pkg = "com.amazon.tv.routing", name = "Fire TV Routing", cat = "System" });
            rawList.Add(new { pkg = "com.amazon.dial.service", name = "DIAL Discovery Server", cat = "System" });
            rawList.Add(new { pkg = "com.amazon.whisperplay.contracts", name = "Whisperplay Casting", cat = "System" });
            rawList.Add(new { pkg = "com.apple.airplay.receiver", name = "Apple TV AirPlay", cat = "System" });
            rawList.Add(new { pkg = "com.waxrain.airplayreceiver", name = "AirScreen (Casting)", cat = "System" });

            // Specific Manufacturer Systems
            rawList.Add(new { pkg = "com.nvidia.shield.remote.server", name = "Shield Remote Server", cat = "System" });
            rawList.Add(new { pkg = "com.nvidia.shield.ask", name = "Shield TV Setup", cat = "System" });
            rawList.Add(new { pkg = "com.nvidia.blaketi", name = "Shield Controller", cat = "System" });
            rawList.Add(new { pkg = "com.nvidia.osc", name = "Shield Overlay", cat = "System" });
            rawList.Add(new { pkg = "com.nvidia.ota", name = "Shield System Update", cat = "System" });
            rawList.Add(new { pkg = "com.droidlogic.otaupgrade", name = "Onn / Amlogic OTA", cat = "System" });
            rawList.Add(new { pkg = "com.droidlogic.overlay", name = "Onn Device Overlays", cat = "System" });
            rawList.Add(new { pkg = "com.tcl.tvinfo", name = "TCL TV Info", cat = "System" });
            rawList.Add(new { pkg = "com.tcl.airplay", name = "TCL AirPlay Receiver", cat = "System" });
            rawList.Add(new { pkg = "com.tcl.tv.settings", name = "TCL TV Settings", cat = "System" });
            rawList.Add(new { pkg = "com.tcl.tv.input", name = "TCL Input Manager", cat = "System" });
            rawList.Add(new { pkg = "com.hisense.tv.settings", name = "Hisense TV Settings", cat = "System" });
            rawList.Add(new { pkg = "com.hisense.vidaa.launcher", name = "Vidaa Home", cat = "System" });
            rawList.Add(new { pkg = "com.sony.dtv.settings", name = "Sony TV Settings", cat = "System" });
            rawList.Add(new { pkg = "com.sony.dtv.network.settings", name = "Sony Network Setup", cat = "System" });
            rawList.Add(new { pkg = "com.samsung.tv.launcher", name = "Samsung Hub (Android)", cat = "System" });
            rawList.Add(new { pkg = "com.google.android.apps.assistant", name = "Google Assistant", cat = "System" });
            rawList.Add(new { pkg = "com.google.android.gms", name = "Google Play Services", cat = "System" });
            rawList.Add(new { pkg = "com.google.android.apps.nexuslauncher", name = "Pixel Launcher", cat = "System" });
            rawList.Add(new { pkg = "com.android.vending", name = "Google Play Store", cat = "System" });
            rawList.Add(new { pkg = "com.android.settings", name = "Android Settings", cat = "System" });
            rawList.Add(new { pkg = "com.android.systemui", name = "System UI", cat = "System" });
            rawList.Add(new { pkg = "com.android.bluetooth", name = "Bluetooth Stack", cat = "System" });
            rawList.Add(new { pkg = "com.android.vpndialogs", name = "VPN Confirmation", cat = "System" });
            rawList.Add(new { pkg = "com.android.providers.downloads", name = "Download Manager", cat = "System" });
            rawList.Add(new { pkg = "com.android.shell", name = "System Shell", cat = "System" });
            rawList.Add(new { pkg = "com.android.pacprocessor", name = "Proxy Processor", cat = "System" });
            rawList.Add(new { pkg = "com.android.proxyhandler", name = "Proxy Handler", cat = "System" });
            rawList.Add(new { pkg = "com.hisense.tv.launcher", name = "Hisense TV Launcher", cat = "System" });
            rawList.Add(new { pkg = "com.hisense.vidaa.launcher", name = "Vidaa Launcher", cat = "System" });
            rawList.Add(new { pkg = "com.hisense.tv.airplay", name = "Hisense AirPlay", cat = "System" });

            foreach (var a in rawList)
            {
                Apps.Add(new AndroidAppInfo { PackageName = a.pkg, AppName = a.name, Category = a.cat });
            }
        }

        private bool FilterApps(object item)
        {
            var app = item as AndroidAppInfo;
            if (app == null) return false;

            // Category filter
            if (CategoryCombo != null && CategoryCombo.SelectedItem != null)
            {
                string catFilter = ((System.Windows.Controls.ComboBoxItem)CategoryCombo.SelectedItem).Content.ToString();
                if (catFilter != "All Categories")
                {
                    string targetCat = catFilter;
                    if (catFilter == "Utility / Downloads") targetCat = "Utility / Downloads";
                    else if (catFilter == "Browsers") targetCat = "Browsers";
                    else if (catFilter == "System (Android / Fire TV)") targetCat = "System";

                    if (app.Category != targetCat)
                        return false;
                }
            }

            if (string.IsNullOrWhiteSpace(SearchBox.Text))
                return true;

            var lowerQuery = SearchBox.Text.ToLowerInvariant();
            return (app.AppName?.ToLowerInvariant().Contains(lowerQuery) == true) ||
                   (app.PackageName?.ToLowerInvariant().Contains(lowerQuery) == true) ||
                   (app.Category?.ToLowerInvariant().Contains(lowerQuery) == true);
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _appCollectionView?.Refresh();
        }

        private void CategoryCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _appCollectionView?.Refresh();
        }

        private string _lastSortHeader = "";
        private ListSortDirection _lastSortDirection = ListSortDirection.Ascending;

        private void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            var headerClicked = e.OriginalSource as GridViewColumnHeader;
            if (headerClicked != null && headerClicked.Column != null)
            {
                string sortBy = headerClicked.Column.Header as string;
                if (string.IsNullOrEmpty(sortBy)) return;

                string propertyName = sortBy == "App Name" ? "AppName" :
                                      sortBy == "Category" ? "Category" : "PackageName";

                ListSortDirection direction = ListSortDirection.Ascending;
                if (_lastSortHeader == propertyName && _lastSortDirection == ListSortDirection.Ascending)
                {
                    direction = ListSortDirection.Descending;
                }

                _appCollectionView.SortDescriptions.Clear();
                _appCollectionView.SortDescriptions.Add(new SortDescription(propertyName, direction));
                
                _lastSortHeader = propertyName;
                _lastSortDirection = direction;
            }
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var app in _appCollectionView.Cast<AndroidAppInfo>())
                app.IsSelected = true;
            UpdatePreview();
        }

        private void DeselectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var app in _appCollectionView.Cast<AndroidAppInfo>())
                app.IsSelected = false;
            UpdatePreview();
        }

        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            UpdatePreview();
        }

        private void AppListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (AppListView.SelectedItem is AndroidAppInfo app)
            {
                app.IsSelected = !app.IsSelected;
                UpdatePreview();
            }
        }

        private void UpdatePreview()
        {
            var selectedPkgs = Apps.Where(a => a.IsSelected).Select(a => a.PackageName);
            PreviewTextBox.Text = string.Join(", ", selectedPkgs);
        }

        private void ExportList_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PreviewTextBox.Text))
            {
                MessageBox.Show("Select some apps first before trying to back them up!", "Empty List", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var sfd = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "excluded_apps_backup.txt",
                Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Backup your Exclusion List"
            };

            if (sfd.ShowDialog() == true)
            {
                File.WriteAllText(sfd.FileName, PreviewTextBox.Text);
                MessageBox.Show("Exclusion list backed up successfully!", "Backup Done", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ImportList_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Restore your Exclusion List"
            };

            if (ofd.ShowDialog() == true)
            {
                try
                {
                    string content = File.ReadAllText(ofd.FileName);
                    if (string.IsNullOrWhiteSpace(content)) return;

                    var pkgs = content.Split(',').Select(p => p.Trim()).ToList();
                    
                    // Reset all
                    foreach (var app in Apps) app.IsSelected = false;

                    // Match
                    foreach (var pkg in pkgs)
                    {
                        var match = Apps.FirstOrDefault(a => a.PackageName.Contains(pkg));
                        if (match != null) match.IsSelected = true;
                    }

                    UpdatePreview();
                    MessageBox.Show($"Successfully restored selection from backup!", "Import Done", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Failed to import list: {ex.Message}", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            ResultExcludedApps = PreviewTextBox.Text;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
