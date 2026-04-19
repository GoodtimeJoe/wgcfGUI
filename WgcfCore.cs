using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Chaos.NaCl;

namespace WgcfGUI
{
    public class WgcfCore
    {
        private static readonly HttpClient client;
        private const string ApiUrl = "https://api.cloudflareclient.com/v0a1922";
        
        public static string PrivateKeyBase64 { get; set; } = string.Empty;
        public static string DeviceId { get; set; } = string.Empty;
        public static string AccessToken { get; set; } = string.Empty;
        public static string LicenseKey { get; set; } = string.Empty;
        public static string AccountType { get; set; } = "Not Registered";

        public static string AddressV4 { get; set; } = string.Empty;
        public static string AddressV6 { get; set; } = string.Empty;
        public static string PeerPublicKey { get; set; } = string.Empty;
        public static string PeerEndpoint { get; set; } = string.Empty;

        private static string SettingsFilePath => System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WgcfGUI", "session.json");

        public static void SaveState()
        {
            try
            {
                var dir = System.IO.Path.GetDirectoryName(SettingsFilePath);
                if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);

                var state = new {
                    PrivateKeyBase64, DeviceId, AccessToken, LicenseKey, AccountType,
                    AddressV4, AddressV6, PeerPublicKey, PeerEndpoint
                };
                System.IO.File.WriteAllText(SettingsFilePath, JsonSerializer.Serialize(state));
            }
            catch {}
        }

        public static bool LoadState()
        {
            try
            {
                if (!System.IO.File.Exists(SettingsFilePath)) return false;
                var json = System.IO.File.ReadAllText(SettingsFilePath);
                var doc = JsonDocument.Parse(json).RootElement;
                
                PrivateKeyBase64 = doc.GetProperty("PrivateKeyBase64").GetString() ?? "";
                DeviceId = doc.GetProperty("DeviceId").GetString() ?? "";
                AccessToken = doc.GetProperty("AccessToken").GetString() ?? "";
                LicenseKey = doc.GetProperty("LicenseKey").GetString() ?? "";
                AccountType = doc.GetProperty("AccountType").GetString() ?? "";
                AddressV4 = doc.GetProperty("AddressV4").GetString() ?? "";
                AddressV6 = doc.GetProperty("AddressV6").GetString() ?? "";
                PeerPublicKey = doc.GetProperty("PeerPublicKey").GetString() ?? "";
                PeerEndpoint = doc.GetProperty("PeerEndpoint").GetString() ?? "";
                return !string.IsNullOrEmpty(DeviceId);
            }
            catch { return false; }
        }

        public static void ClearState()
        {
            try { if (System.IO.File.Exists(SettingsFilePath)) System.IO.File.Delete(SettingsFilePath); } catch {}
            PrivateKeyBase64 = ""; DeviceId = ""; AccessToken = ""; LicenseKey = ""; AccountType = "Not Registered";
            AddressV4 = ""; AddressV6 = ""; PeerPublicKey = ""; PeerEndpoint = "";
        }

        static WgcfCore()
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            };
            client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("User-Agent", "okhttp/3.12.1");
            client.DefaultRequestHeaders.Add("CF-Client-Version", "a-6.3-1922");
        }

        public static async Task<bool> RegisterAsync()
        {
            try
            {
                var privKey = new byte[32];
                RandomNumberGenerator.Fill(privKey);
                privKey[0] &= 248;
                privKey[31] = (byte)((privKey[31] & 127) | 64);
                
                var pubKeyBytes = MontgomeryCurve25519.GetPublicKey(privKey);
                
                PrivateKeyBase64 = Convert.ToBase64String(privKey);
                string publicKeyBase64 = Convert.ToBase64String(pubKeyBytes);
                string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK"); // "2020-07-28T09:22:12.861+01:00" approx

                var regReq = new
                {
                    key = publicKeyBase64,
                    install_id = "",
                    fcm_token = "",
                    tos = timestamp,
                    model = "PC",
                    locale = "en_US",
                    type = "Android"
                };

                string jsonReq = JsonSerializer.Serialize(regReq);
                var content = new StringContent(jsonReq, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{ApiUrl}/reg", content);
                if (!response.IsSuccessStatusCode) return false;

                var resJson = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(resJson);
                var root = doc.RootElement;

                DeviceId = root.GetProperty("id").GetString() ?? "";
                AccessToken = root.GetProperty("token").GetString() ?? "";
                LicenseKey = root.GetProperty("account").GetProperty("license").GetString() ?? "";
                AccountType = root.GetProperty("account").GetProperty("account_type").GetString() ?? "";

                AddressV4 = root.GetProperty("config").GetProperty("interface").GetProperty("addresses").GetProperty("v4").GetString() ?? "";
                AddressV6 = root.GetProperty("config").GetProperty("interface").GetProperty("addresses").GetProperty("v6").GetString() ?? "";

                var peer = root.GetProperty("config").GetProperty("peers")[0];
                PeerPublicKey = peer.GetProperty("public_key").GetString() ?? "";
                PeerEndpoint = peer.GetProperty("endpoint").GetProperty("host").GetString() ?? "";

                SaveState();
                return true;
            }
            catch { return false; }
        }

        public static async Task<bool> UpdateLicenseAsync(string newLicense)
        {
            try
            {
                if (string.IsNullOrEmpty(DeviceId) || string.IsNullOrEmpty(AccessToken)) return false;

                var updateReq = new { license = newLicense };
                string jsonReq = JsonSerializer.Serialize(updateReq);
                var content = new StringContent(jsonReq, Encoding.UTF8, "application/json");

                using var req = new HttpRequestMessage(HttpMethod.Put, $"{ApiUrl}/reg/{DeviceId}")
                {
                    Content = content
                };
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

                var response = await client.SendAsync(req);
                if (!response.IsSuccessStatusCode)
                {
                    var msg = await response.Content.ReadAsStringAsync();
                    throw new Exception("HTTP Error: " + msg);
                }

                var resJson = await response.Content.ReadAsStringAsync();
                var root = JsonDocument.Parse(resJson).RootElement;
                LicenseKey = root.GetProperty("account").GetProperty("license").GetString() ?? newLicense;
                AccountType = root.GetProperty("account").GetProperty("account_type").GetString() ?? AccountType;

                SaveState();
                return true;
            }
            catch { return false; }
        }

        public static async Task<string> TraceAsync()
        {
            try
            {
                using var traceClient = new HttpClient();
                var res = await traceClient.GetAsync("https://1.1.1.1/cdn-cgi/trace");
                return await res.Content.ReadAsStringAsync();
            }
            catch (Exception ex) { return ex.Message; }
        }

        public static string GenerateProfileConf(string mtu, string excludedApps, string dnsString, string endpointMode, bool useKeepAlive, bool disableIpv6, string customV4, string customV6, bool enableKillSwitch)
        {
            var sb = new StringBuilder();
            sb.AppendLine("[Interface]");
            sb.AppendLine($"PrivateKey = {PrivateKeyBase64}");
            
            string v4 = string.IsNullOrWhiteSpace(customV4) ? AddressV4 : customV4.Trim();
            string v6 = string.IsNullOrWhiteSpace(customV6) ? AddressV6 : customV6.Trim();

            if (disableIpv6)
                sb.AppendLine($"Address = {v4}/32");
            else
                sb.AppendLine($"Address = {v4}/32, {v6}/128");

            if (enableKillSwitch)
            {
                sb.AppendLine("PreUp = powershell -command \"New-NetFirewallRule -DisplayName 'WireGuard Kill Switch Block All' -Direction Outbound -Action Block -Profile Any -InterfaceAlias '*'; New-NetFirewallRule -DisplayName 'WireGuard Kill Switch Allow WG' -Direction Outbound -Action Allow -Profile Any -InterfaceAlias '%WIREGUARD_TUNNEL_NAME%'\"");
                sb.AppendLine("PostDown = powershell -command \"Remove-NetFirewallRule -DisplayName 'WireGuard Kill Switch Block All' -ErrorAction SilentlyContinue; Remove-NetFirewallRule -DisplayName 'WireGuard Kill Switch Allow WG' -ErrorAction SilentlyContinue\"");
            }

            if (disableIpv6)
            {
                // Strip IPv6 addresses from DNS String if we disable IPv6
                var dnsParts = dnsString.Split(',');
                var v4Dns = new System.Collections.Generic.List<string>();
                foreach(var d in dnsParts) { if(d.Contains(".")) v4Dns.Add(d.Trim()); }
                dnsString = string.Join(", ", v4Dns);
            }
            sb.AppendLine($"DNS = {dnsString}");
            
            sb.AppendLine($"MTU = {mtu}");
            
            if (!string.IsNullOrWhiteSpace(excludedApps))
                sb.AppendLine($"ExcludedApplications = {excludedApps}");

            sb.AppendLine();
            sb.AppendLine("[Peer]");
            sb.AppendLine($"PublicKey = {PeerPublicKey}");
            
            if (disableIpv6)
                sb.AppendLine("AllowedIPs = 0.0.0.0/0");
            else
                sb.AppendLine("AllowedIPs = 0.0.0.0/0, ::/0");

            string epStr = PeerEndpoint;
            if (endpointMode == "IPv4 Direct") epStr = "162.159.192.1:2408";
            else if (endpointMode == "IPv6 Direct") epStr = "[2606:4700:d0::a29f:c001]:2408";
            else if (endpointMode == "Cloudflare Port 500") epStr = "engage.cloudflareclient.com:500";
            else if (endpointMode.StartsWith("Custom: ")) epStr = endpointMode.Replace("Custom: ", "").Trim();
            
            sb.AppendLine($"Endpoint = {epStr}");

            if (useKeepAlive)
                sb.AppendLine("PersistentKeepalive = 25");

            return sb.ToString();
        }
    }
}
