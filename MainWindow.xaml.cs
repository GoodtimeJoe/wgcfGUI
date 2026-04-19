using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace WgcfGUI
{
    public partial class MainWindow : Wpf.Ui.Controls.FluentWindow
    {
        private string savedExcludedApps = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (WgcfCore.LoadState())
            {
                UpdateStatusLog("Ready. Loaded persistent Cloudflare session cache.");
            }
            else
            {
                UpdateStatusLog("Ready. WireGuard cryptography routines loaded.");
            }
            RefreshAccountState();
            RefreshPublicIp();
            
            // Set theme to follow system OS
            Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.Unknown);

            // Force Cloudflare default initialize
            if (DnsProviderCombo.SelectedIndex == -1) DnsProviderCombo.SelectedIndex = 0;
            DnsProviderCombo_SelectionChanged(null, null);

            UpdateLivePreview();
        }

        private void ClearCache_Click(object sender, RoutedEventArgs e)
        {
            WgcfCore.ClearState();
            UpdateStatusLog("Cleared persistent account cache layer.");
            RefreshAccountState();
        }

        private void RefreshAccount_Click(object sender, RoutedEventArgs e)
        {
            UpdateStatusLog("Refreshing UI against memory state...");
            RefreshAccountState();
        }

        private void UpdateStatusLog(string message)
        {
            GlobalStatusText.Text = message;
            LogConsoleBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
            LogConsoleBox.ScrollToEnd();
        }

        private void RefreshAccountState()
        {
            if (!string.IsNullOrEmpty(WgcfCore.DeviceId))
            {
                AccountStatusText.Text = $"Status: {WgcfCore.AccountType}";
                AccountStatusText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 204, 106));
                
                if (string.IsNullOrWhiteSpace(LicenseBox.Text) && !string.IsNullOrEmpty(WgcfCore.LicenseKey))
                {
                    LicenseBox.Text = WgcfCore.LicenseKey;
                }
            }
            else
            {
                AccountStatusText.Text = "Not Registered. Click 'Register New Account' to begin.";
                AccountStatusText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 185, 0));
            }
        }

        private async void RegisterAccount_Click(object sender, RoutedEventArgs e)
        {
            UpdateStatusLog("Registering device on Cloudflare Warp API...");
            var success = await WgcfCore.RegisterAsync();
            
            if (success)
            {
                UpdateStatusLog("Device successfully registered on Warp Edge.");
                RefreshAccountState();
                MessageBox.Show("New Warp account generated and bound to this runtime successfully!", "Registration Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                UpdateStatusLog("Registration request failed.");
                MessageBox.Show("Failed to register account against Cloudflare API.\nPlease verify your internet connection.", "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ApplyLicense_Click(object sender, RoutedEventArgs e)
        {
            string newKey = LicenseBox.Text.Trim();
            if (string.IsNullOrEmpty(newKey))
            {
                MessageBox.Show("Please enter a valid Warp+ License Key.", "Missing Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(WgcfCore.DeviceId))
            {
                MessageBox.Show("You must register a Free account first before applying a Warp+ key.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            UpdateStatusLog("Authenticating license key...");
            
            var success = await WgcfCore.UpdateLicenseAsync(newKey);
            
            if (success)
            {
                UpdateStatusLog("License applied and verified.");
                RefreshAccountState();
                MessageBox.Show("Account successfully upgraded/updated via License.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                UpdateStatusLog("Failed to apply license.");
                MessageBox.Show("Error updating account with new license. The key might be invalid or expired.", "Verification Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeviceMode_Changed(object sender, RoutedEventArgs e)
        {
            if (AndroidOptionsPanel != null)
            {
                if (RdoDeviceAndroid.IsChecked == true)
                    AndroidOptionsPanel.Visibility = Visibility.Visible;
                else
                    AndroidOptionsPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void MtuPresetCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (MtuBox == null || MtuPresetCombo == null || MtuPresetCombo.SelectedItem == null) return;
            string sel = ((System.Windows.Controls.ComboBoxItem)MtuPresetCombo.SelectedItem).Content.ToString();
            
            if (sel.Contains("1280")) MtuBox.Text = "1280";
            else if (sel.Contains("1420")) MtuBox.Text = "1420";
            else if (sel.Contains("1212")) MtuBox.Text = "1212";
            UpdateLivePreview();
        }

        private void EndpointCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (CustomEndpointBox == null || EndpointCombo == null) return;
            string sel = ((System.Windows.Controls.ComboBoxItem)EndpointCombo.SelectedItem).Content.ToString();
            CustomEndpointBox.Visibility = sel == "Custom Endpoint" ? Visibility.Visible : Visibility.Collapsed;
            UpdateLivePreview();
        }

        private void DnsProviderCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (DnsProviderCombo == null || DnsFilterCombo == null || CustomDnsBox == null) return;
            string prov = ((System.Windows.Controls.ComboBoxItem)DnsProviderCombo.SelectedItem).Content.ToString();
            
            DnsFilterCombo.Items.Clear();
            CustomDnsBox.Visibility = Visibility.Collapsed;
            
            if (prov == "Cloudflare")
            {
                DnsFilterCombo.Items.Add(new System.Windows.Controls.ComboBoxItem { Content = "Standard Default" });
                DnsFilterCombo.Items.Add(new System.Windows.Controls.ComboBoxItem { Content = "Malware Blocking" });
                DnsFilterCombo.Items.Add(new System.Windows.Controls.ComboBoxItem { Content = "Family (Malware + Adult)" });
                DnsFilterCombo.SelectedIndex = 0;
            }
            else if (prov == "Google")
            {
                DnsFilterCombo.Items.Add(new System.Windows.Controls.ComboBoxItem { Content = "Standard Default" });
                DnsFilterCombo.SelectedIndex = 0;
            }
            else if (prov == "Quad9")
            {
                DnsFilterCombo.Items.Add(new System.Windows.Controls.ComboBoxItem { Content = "Malware Blocking" });
                DnsFilterCombo.Items.Add(new System.Windows.Controls.ComboBoxItem { Content = "Unfiltered" });
                DnsFilterCombo.SelectedIndex = 0;
            }
            else if (prov == "AdGuard")
            {
                DnsFilterCombo.Items.Add(new System.Windows.Controls.ComboBoxItem { Content = "Standard (Ad & Tracker Block)" });
                DnsFilterCombo.Items.Add(new System.Windows.Controls.ComboBoxItem { Content = "Family (Ad + Adult Block)" });
                DnsFilterCombo.Items.Add(new System.Windows.Controls.ComboBoxItem { Content = "Unfiltered" });
                DnsFilterCombo.SelectedIndex = 0;
            }
            else if (prov == "Custom Server")
            {
                CustomDnsBox.Visibility = Visibility.Visible;
                DnsFilterCombo.Items.Add(new System.Windows.Controls.ComboBoxItem { Content = "Custom Config" });
                DnsFilterCombo.SelectedIndex = 0;
            }
            UpdateLivePreview();
        }

        private void DnsFilterCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (DnsProviderCombo == null || DnsFilterCombo == null || DnsFilterCombo.SelectedItem == null || DnsResultText == null || DnsWarningText == null) return;
            string prov = ((System.Windows.Controls.ComboBoxItem)DnsProviderCombo.SelectedItem).Content.ToString();
            string filter = ((System.Windows.Controls.ComboBoxItem)DnsFilterCombo.SelectedItem).Content.ToString();

            string dnsStr = "";
            bool hasWarning = false;

            if (prov == "Cloudflare")
            {
                if (filter == "Standard Default") dnsStr = "1.1.1.1, 1.0.0.1, 2606:4700:4700::1111, 2606:4700:4700::1001";
                else if (filter == "Malware Blocking") { dnsStr = "1.1.1.2, 1.0.0.2, 2606:4700:4700::1112, 2606:4700:4700::1002"; hasWarning = true; }
                else if (filter == "Family (Malware + Adult)") { dnsStr = "1.1.1.3, 1.0.0.3, 2606:4700:4700::1113, 2606:4700:4700::1003"; hasWarning = true; }
            }
            else if (prov == "Google")
            {
                dnsStr = "8.8.8.8, 8.8.4.4, 2001:4860:4860::8888, 2001:4860:4860::8844";
            }
            else if (prov == "Quad9")
            {
                if (filter == "Malware Blocking") { dnsStr = "9.9.9.9, 149.112.112.112, 2620:fe::fe, 2620:fe::9"; hasWarning = true; }
                else if (filter == "Unfiltered") dnsStr = "9.9.9.10, 149.112.112.10, 2620:fe::10, 2620:fe::fe:10";
            }
            else if (prov == "AdGuard")
            {
                if (filter == "Standard (Ad & Tracker Block)") { dnsStr = "94.140.14.14, 94.140.15.15, 2a10:50c0::ad1, 2a10:50c0::ad2"; hasWarning = true; }
                else if (filter == "Family (Ad + Adult Block)") { dnsStr = "94.140.14.15, 94.140.15.16, 2a10:50c0::bad1, 2a10:50c0::bad2"; hasWarning = true; }
                else if (filter == "Unfiltered") dnsStr = "94.140.14.140, 94.140.14.141, 2a10:50c0::1, 2a10:50c0::2";
            }
            else if (prov == "Custom Server")
            {
                dnsStr = string.IsNullOrWhiteSpace(CustomDnsBox.Text) ? "8.8.8.8" : CustomDnsBox.Text.Trim();
            }

            DnsResultText.Text = dnsStr;
            DnsWarningText.Visibility = hasWarning ? Visibility.Visible : Visibility.Collapsed;
            UpdateLivePreview();
        }

        private void ConfigureExclusions_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AndroidAppWindow();
            dialog.Owner = this;
            if (dialog.ShowDialog() == true)
            {
                savedExcludedApps = dialog.ResultExcludedApps;
                int count = string.IsNullOrWhiteSpace(savedExcludedApps) ? 0 : savedExcludedApps.Split(',').Length;
                ExcludeCountText.Text = $"{count} app(s) selected to bypass VPN.";
                UpdateStatusLog("Updated android app exclusions caching.");
            }
        }

        private string GenerateInternalConf()
        {
            string finalMtu = string.IsNullOrWhiteSpace(MtuBox.Text) ? "1280" : MtuBox.Text.Trim();
            string appsToExclude = RdoDeviceAndroid.IsChecked == true ? savedExcludedApps : string.Empty;

            string dnsStr = DnsResultText.Text;
            if (((System.Windows.Controls.ComboBoxItem)DnsProviderCombo.SelectedItem).Content.ToString() == "Custom Server")
                dnsStr = CustomDnsBox.Text.Trim();

            string endpointMode = ((System.Windows.Controls.ComboBoxItem)EndpointCombo.SelectedItem).Content.ToString();
            if (endpointMode == "Custom Endpoint")
                endpointMode = "Custom: " + CustomEndpointBox.Text.Trim();

            bool useKeepAlive = KeepAliveCheck.IsChecked == true;
            bool disableIpv6 = RemoveIpv6Check.IsChecked == true;
            string customV4 = CustomV4Box.Text;
            string customV6 = CustomV6Box.Text;
            bool enableKillSwitch = KillSwitchCheck.IsChecked == true;

            return WgcfCore.GenerateProfileConf(finalMtu, appsToExclude, dnsStr, endpointMode, useKeepAlive, disableIpv6, customV4, customV6, enableKillSwitch);
        }

        private void GenerateProfile_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(WgcfCore.DeviceId))
            {
                MessageBox.Show("Looks like you haven't registered an account yet. Hit \"Register New Account\" first!", "Hold up", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            UpdateStatusLog("Putting together your config...");
            string confData = GenerateInternalConf();

            var sfd = new SaveFileDialog
            {
                FileName = "wgcf-profile.conf",
                Filter = "WireGuard Config Files (*.conf)|*.conf|All Files (*.*)|*.*",
                Title = "Save your WireGuard Profile"
            };

            if (sfd.ShowDialog() == true)
            {
                File.WriteAllText(sfd.FileName, confData);
                ExportStatusText.Text = $"✔ Got it! Saved to:\n{sfd.FileName}";
                ExportStatusText.Visibility = Visibility.Visible;
                UpdateStatusLog("Config saved to your PC.");
                
                if (OpenFolderCheck.IsChecked == true)
                {
                    try { System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{sfd.FileName}\""); } catch { }
                }

                if (AutoCopyCheck.IsChecked == true)
                {
                    Clipboard.SetText(confData);
                    UpdateStatusLog("Config saved and copied to clipboard.");
                }
            }
            else
            {
                UpdateStatusLog("Export canceled.");
            }
        }

        private void CopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(WgcfCore.DeviceId))
            {
                MessageBox.Show("Register an account before copying a profile.", "Hold up", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string confData = GenerateInternalConf();
            Clipboard.SetText(confData);
            UpdateStatusLog("Copied your config to clipboard.");
            ExportStatusText.Text = "✔ Copied to clipboard!";
            ExportStatusText.Visibility = Visibility.Visible;
        }

        private void GenerateQr_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(WgcfCore.DeviceId))
            {
                MessageBox.Show("Need an account first before doing QR codes.", "Wait a sec", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string confData = GenerateInternalConf();
            var qr = new QrWindow(confData);
            qr.Owner = this;
            qr.ShowDialog();
            UpdateStatusLog("Opened up the QR code window.");
        }

        private async void TraceConnection_Click(object sender, RoutedEventArgs e)
        {
            UpdateStatusLog("Pinging Cloudflare gateway...");
            var output = await WgcfCore.TraceAsync();

            // Match warp=on, warp=plus, or warp=off from the trace
            string statusInfo = System.Text.RegularExpressions.Regex.Match(output, @"warp=(on|plus|off)").Value;
            string ipInfo = System.Text.RegularExpressions.Regex.Match(output, @"ip=([\d\.]+)").Value;
            if (string.IsNullOrEmpty(statusInfo)) statusInfo = "warp=off (or connection timed out)";

            string shortOutput = output.Length > 350 ? output.Substring(0, 350) + "..." : output;
            var result = MessageBox.Show($"Cloudflare Trace Result:\n\n{statusInfo}\n{ipInfo}\n\nFull Details:\n{shortOutput}\n\nWould you like to copy the full trace to your clipboard?", 
                "Trace Connection", MessageBoxButton.YesNo, MessageBoxImage.Information);
            
            if (result == MessageBoxResult.Yes)
            {
                Clipboard.SetText(output);
                UpdateStatusLog("Trace details copied to clipboard.");
            }
            
            UpdateStatusLog($"Ping finished. Status: {statusInfo} | {ipInfo}");
        }

        private void ApplyBestSettings_Click(object sender, RoutedEventArgs e) { }

        private async void OneClickFlow_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(WgcfCore.DeviceId))
            {
                UpdateStatusLog("One-Click: Registering new account first...");
                var regSuccess = await WgcfCore.RegisterAsync();
                if (!regSuccess)
                {
                    MessageBox.Show("Failed to register account for One-Click flow.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                RefreshAccountState();
            }

            // Set MTU to 1280 for best compatibility
            MtuBox.Text = "1280";
            MtuPresetCombo.SelectedIndex = 1; // Compatibility

            // Set DNS to Cloudflare Standard
            DnsProviderCombo.SelectedIndex = 0; // Cloudflare
            DnsFilterCombo.SelectedIndex = 0; // Standard

            // Set Endpoint to engage.cloudflareclient.com
            EndpointCombo.SelectedIndex = 0;

            // Enable common QoL settings
            KeepAliveCheck.IsChecked = true;
            RemoveIpv6Check.IsChecked = true;
            KillSwitchCheck.IsChecked = true;
            AutoCopyCheck.IsChecked = true;

            UpdateStatusLog("One-Click settings applied.");
            UpdateLivePreview();
            GenerateProfile_Click(null, null);
            UpdateStatusLog("One-Click Flow Complete.");
        }

        private void BenchmarkUnfiltered_Click(object sender, RoutedEventArgs e)
        {
            var targets = new System.Collections.Generic.Dictionary<string, string>
            {
                { "Cloudflare (Standard)", "1.1.1.1" },
                { "Google (Standard)", "8.8.8.8" },
                { "Quad9 (Standard)", "9.9.9.9" },
                { "AdGuard (Unfiltered)", "94.140.14.140" },
                { "Cisco OpenDNS", "208.67.222.222" }
            };
            RunBenchmark("Unfiltered / Standard", targets);
        }

        private void BenchmarkMalware_Click(object sender, RoutedEventArgs e)
        {
            var targets = new System.Collections.Generic.Dictionary<string, string>
            {
                { "Cloudflare (Security)", "1.1.1.2" },
                { "AdGuard (Malware)", "94.140.14.14" },
                { "Quad9 (No-EDNS)", "9.9.9.10" },
                { "Comodo Secure", "8.26.56.26" }
            };
            RunBenchmark("Malware Filtered", targets);
        }

        private void BenchmarkFamily_Click(object sender, RoutedEventArgs e)
        {
            var targets = new System.Collections.Generic.Dictionary<string, string>
            {
                { "Cloudflare (Family)", "1.1.1.3" },
                { "AdGuard (Family)", "94.140.14.15" },
                { "CleanBrowsing (Family)", "185.228.168.168" },
                { "OpenDNS (Family)", "208.67.222.123" }
            };
            RunBenchmark("Family (Protected)", targets);
        }

        private async void RunBenchmark(string category, System.Collections.Generic.Dictionary<string, string> targets)
        {
            UpdateStatusLog($"Benchmarking {category} latencies...");
            string results = $"DNS Latency Results ({category}):\n\n";
            
            foreach (var target in targets)
            {
                try
                {
                    using var ping = new System.Net.NetworkInformation.Ping();
                    var reply = await ping.SendPingAsync(target.Value, 1200);
                    if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                    {
                        results += $"{target.Key}: {reply.RoundtripTime}ms\n";
                    }
                    else
                    {
                        results += $"{target.Key}: Timed Out\n";
                    }
                }
                catch
                {
                    results += $"{target.Key}: Error\n";
                }
            }

            MessageBox.Show(results, $"{category} Benchmark", MessageBoxButton.OK, MessageBoxImage.Information);
            UpdateStatusLog($"{category} benchmark complete.");
        }

        private void PublicIpMirror_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            RefreshPublicIp();
        }

        private async void RefreshPublicIp()
        {
            PublicIpMirror.Text = "My IP: Fetching...";
            try
            {
                using var http = new System.Net.Http.HttpClient();
                // User Agent to avoid being blocked by mirror services
                http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 WgcfGUI/" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
                string ip = await http.GetStringAsync("https://api.ipify.org");
                PublicIpMirror.Text = $"My IP: {ip.Trim()}";
            }
            catch
            {
                PublicIpMirror.Text = "My IP: [Error]";
            }
        }

        private async void LeakTest_Click(object sender, RoutedEventArgs e)
        {
            UpdateStatusLog("Running DNS Leak Test...");
            try
            {
                using var http = new System.Net.Http.HttpClient();
                string json = await http.GetStringAsync("https://edns.ip-api.com/json");
                var doc = System.Text.Json.JsonDocument.Parse(json);
                string dnsIp = doc.RootElement.GetProperty("dns").GetProperty("ip").GetString() ?? "Unknown";
                string dnsGeo = doc.RootElement.GetProperty("dns").GetProperty("geo").GetString() ?? "Unknown";
                
                string message = $"External DNS Detected:\nIP: {dnsIp}\nLocation: {dnsGeo}\n\n";
                if (dnsIp.Contains("108.162.") || dnsIp.Contains("172.64.") || dnsIp.Contains("162.158.") || dnsGeo.Contains("Cloudflare"))
                {
                    message += "✅ Result: Secure (Using Cloudflare Edge)";
                }
                else
                {
                    message += "⚠️ Result: Potential Leak! (Not using Cloudflare)";
                }

                MessageBox.Show(message, "DNS Leak Test", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {
                MessageBox.Show("Failed to run leak test. Check your internet connection.", "Test Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            UpdateStatusLog("Leak test finished.");
        }

        private void Control_Changed(object sender, EventArgs e)
        {
            UpdateLivePreview();
        }

        private void UpdateLivePreview()
        {
            if (LivePreviewBox == null) return;
            if (string.IsNullOrEmpty(WgcfCore.DeviceId))
            {
                LivePreviewBox.Text = "# Configure your account to see real-time profile data.";
                return;
            }

            try
            {
                LivePreviewBox.Text = GenerateInternalConf();
            }
            catch
            {
                LivePreviewBox.Text = "# Updating preview...";
            }
        }
    }
}