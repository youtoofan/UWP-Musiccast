using System;
using System.Linq;
using System.Threading.Tasks;
using Musiccast.Model;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http;
using Windows.Networking.Connectivity;

namespace Musiccast.Service
{
    /// <summary>
    /// 
    /// </summary>
    public class MusicCastService : IDisposable
    {
        private bool isDisposed;
        /// <summary>
        /// Occurs when [device found].
        /// </summary>
        public event EventHandler<MusicCastDevice> DeviceFound;

        /// <summary>
        /// The information
        /// </summary>
        private const string Info = "/YamahaExtendedControl/v1/system/getDeviceInfo";
        /// <summary>
        /// The features
        /// </summary>
        private const string Features = "/YamahaExtendedControl/v1/system/getFeatures";

        private const string NetworkStatus = "/YamahaExtendedControl/v1/system/getNetworkStatus";

        /// <summary>
        /// The status
        /// </summary>
        private const string Status = "/YamahaExtendedControl/v2/{0}/getStatus";

        private const string Distribution = "YamahaExtendedControl/v1/dist/getDistributionInfo";

        /// <summary>
        /// The names
        /// </summary>
        private const string Names = "/YamahaExtendedControl/v1/system/getNameText?id={0}";
        /// <summary>
        /// The location information
        /// </summary>
        private const string LocationInfo = "/YamahaExtendedControl/v1/system/getLocationInfo";
        /// <summary>
        /// The toggle power
        /// </summary>
        private const string TogglePower = "/YamahaExtendedControl/v1/{0}/setPower?power=toggle";
        /// <summary>
        /// The tuner play information
        /// </summary>
        private const string TunerPlayInfo = "/YamahaExtendedControl/v2/tuner/getPlayInfo";
        /// <summary>
        /// The tuner presets
        /// </summary>
        private const string TunerPresets = "/YamahaExtendedControl/v1/tuner/getPresetInfo?band={0}";
        /// <summary>
        /// The net usb presets
        /// </summary>
        private const string NetUsbPresets = "/YamahaExtendedControl/v1/netusb/getPresetInfo";
        /// <summary>
        /// The net radio play information
        /// </summary>
        private const string NetRadioPlayInfo = "/YamahaExtendedControl/v1/netusb/getPlayInfo";
        /// <summary>
        /// The set volume
        /// </summary>
        private const string SetVolume = "/YamahaExtendedControl/v1/{0}/setVolume?volume={1}";
        /// <summary>
        /// The recall tuner preset
        /// </summary>
        private const string RecallTunerPreset = "/YamahaExtendedControl/v1/tuner/recallPreset?zone={0}&band={1}&num={2}";
        /// <summary>
        /// The recall usb preset
        /// </summary>
        private const string RecallUsbPreset = "/YamahaExtendedControl/v1/netusb/recallPreset?zone={0}&num={1}";
        private readonly IHttpClientFactory _httpClientFactory;
        private DLNADiscovery service;

        /// <summary>
        /// Initializes a new instance of the <see cref="MusicCastService"/> class.
        /// </summary>
        /// <param name="httpClientFactory">The HTTP client factory.</param>
        public MusicCastService(IHttpClientFactory httpClientFactory)
        {
            this._httpClientFactory = httpClientFactory;
        }

        #region private methods

        /// <summary>
        /// Devices the found asynchronous.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="device">The device.</param>
        /// <returns></returns>
        private async void DeviceFoundAsync(object sender, DLNADescription device)
        {
            if (device == null || device.X_device == null)
                return;

            var baseUri = new Uri(device.X_device.X_URLBase);
            var info = await this.GetDeviceInfo(baseUri).ConfigureAwait(false);
            await this.GetNetworkStatus(baseUri).ConfigureAwait(false);
            var location = await this.GetDeviceLocationInfoAsync(baseUri).ConfigureAwait(false);
            var distributionInfo = await this.GetDistributionInfoAsync(baseUri).ConfigureAwait(false);

            foreach (var zone in location.zone_list.ValidZones)
            {
                var status = await this.GetDeviceStatusAsync(baseUri, zone).ConfigureAwait(false);
                var friendlyName = distributionInfo.client_list.Count() > 0 ? distributionInfo.group_name : string.Empty;

                if (string.IsNullOrEmpty(friendlyName))
                {
                    var temp = await this.GetDeviceZoneFriendlyNameAsync(baseUri, zone).ConfigureAwait(false);
                    friendlyName = temp.text;
                }

                var convertedModel = ConvertApiDeviceToDevice(baseUri, zone, friendlyName, device, status, info);

                if (status.input == Inputs.tuner)
                {
                    var tuner = await this.GetTunerPlayInfoAsync(baseUri).ConfigureAwait(false);
                    convertedModel.NowPlayingInformation = tuner.NowPlayingSummary;
                }

                if (DeviceFound != null)
                    DeviceFound(this, convertedModel);
            }
        }



        /// <summary>
        /// Converts the API device to device.
        /// </summary>
        /// <param name="baseUri">The base URI.</param>
        /// <param name="zone">The zone.</param>
        /// <param name="device">The device.</param>
        /// <param name="status">The status.</param>
        /// <param name="info">The information.</param>
        /// <returns></returns>
        private MusicCastDevice ConvertApiDeviceToDevice(Uri baseUri, string zone, string friendlyName, DLNADescription device, GetStatusResponse status, GetDeviceInfoResponse info)
        {
            return new MusicCastDevice()
            {
                Id = info.device_id,
                BaseUri = baseUri,
                Zone = zone,
                ModelName = info.model_name,
                FriendlyName = friendlyName,
                Location = (device.Location),
                ImagePath = (device.device.iconList.Last().url),
                ImageSize = device.device.iconList.Last().width,
                Power = status.power,
                Input = (status.input)
            };
        }



        /// <summary>
        /// Gets the device status asynchronous.
        /// </summary>
        /// <param name="baseUri">The base URI.</param>
        /// <param name="zoneName">Name of the zone.</param>
        /// <returns></returns>
        private async Task<GetStatusResponse> GetDeviceStatusAsync(Uri baseUri, string zoneName)
        {
            var client = _httpClientFactory.CreateClient();
            {
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, string.Format(Status, zoneName))).ConfigureAwait(false);
                return JsonConvert.DeserializeObject<GetStatusResponse>(result);
            }
        }

        private async Task<GetDistributionInfoResponse> GetDistributionInfoAsync(Uri baseUri)
        {
            var client = _httpClientFactory.CreateClient();
            {
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, Distribution)).ConfigureAwait(false);
                return JsonConvert.DeserializeObject<GetDistributionInfoResponse>(result);
            }
        }

        /// <summary>
        /// Recalls the tuner preset asynchronous.
        /// </summary>
        /// <param name="baseUri">The base URI.</param>
        /// <param name="zoneName">Name of the zone.</param>
        /// <param name="band">The band.</param>
        /// <param name="number">The number.</param>
        /// <returns></returns>
        public async Task<bool> RecallTunerPresetAsync(Uri baseUri, string zoneName, string band, int number)
        {
            var client = _httpClientFactory.CreateClient();
            {
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, string.Format(RecallTunerPreset, zoneName, band, number))).ConfigureAwait(false);
                return result != null && JsonConvert.DeserializeObject<SetDevicePropertyResponse>(result).response_code == 0;
            }
        }

        /// <summary>
        /// Recalls the usb preset asynchronous.
        /// </summary>
        /// <param name="baseUri">The base URI.</param>
        /// <param name="zoneName">Name of the zone.</param>
        /// <param name="band">The band.</param>
        /// <param name="number">The number.</param>
        /// <returns></returns>
        public async Task<bool> RecallUsbPresetAsync(Uri baseUri, string zoneName, int number)
        {
            var client = _httpClientFactory.CreateClient();
            {
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, string.Format(RecallUsbPreset, zoneName, number))).ConfigureAwait(false);
                return result != null && JsonConvert.DeserializeObject<SetDevicePropertyResponse>(result).response_code == 0;
            }
        }

        /// <summary>
        /// Changes the device input asynchronous.
        /// </summary>
        /// <param name="baseUri">The base URI.</param>
        /// <param name="zoneName">Name of the zone.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public async Task<bool> ChangeDeviceInputAsync(Uri baseUri, string zoneName, string id)
        {
            var client = _httpClientFactory.CreateClient();
            {
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, string.Format(Status, zoneName))).ConfigureAwait(false);
                return result != null && JsonConvert.DeserializeObject<SetDevicePropertyResponse>(result).response_code == 0;
            }
        }

        /// <summary>
        /// Gets the device zone friendly name asynchronous.
        /// </summary>
        /// <param name="baseUri">The base URI.</param>
        /// <param name="zoneName">Name of the zone.</param>
        /// <returns></returns>
        private async Task<GetNameTextResponse> GetDeviceZoneFriendlyNameAsync(Uri baseUri, string zoneName)
        {
            var client = _httpClientFactory.CreateClient();
            {
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, string.Format(Names, zoneName))).ConfigureAwait(false);
                return JsonConvert.DeserializeObject<GetNameTextResponse>(result);
            }
        }

        /// <summary>
        /// Gets the device information.
        /// </summary>
        /// <param name="baseUri">The base URI.</param>
        /// <returns></returns>
        private async Task<GetDeviceInfoResponse> GetDeviceInfo(Uri baseUri)
        {
            var client = _httpClientFactory.CreateClient();
            {
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, Info)).ConfigureAwait(false);
                return JsonConvert.DeserializeObject<GetDeviceInfoResponse>(result);
            }
        }

        /// <summary>
        /// Gets the network status.
        /// </summary>
        /// <param name="baseUri">The base URI.</param>
        /// <returns></returns>
        private async Task GetNetworkStatus(Uri baseUri)
        {
            var client = _httpClientFactory.CreateClient();
            AddClientHeaders(client);
            var result = await client.GetStringAsync(new Uri(baseUri, NetworkStatus));
            Debug.WriteLine(result);
        }

        /// <summary>
        /// Gets the features.
        /// </summary>
        /// <param name="baseUri">The base URI.</param>
        /// <returns></returns>
        public async Task<GetFeaturesResponse> GetFeatures(Uri baseUri)
        {
            var client = _httpClientFactory.CreateClient();
            {
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, Features)).ConfigureAwait(false);
                return JsonConvert.DeserializeObject<GetFeaturesResponse>(result);
            }
        }

        /// <summary>
        /// Gets the tuner presets.
        /// </summary>
        /// <param name="baseUri">The base URI.</param>
        /// <param name="band">The band.</param>
        /// <returns></returns>
        public async Task<GetTunerPresetInfo> GetTunerPresets(Uri baseUri, string band)
        {
            var client = _httpClientFactory.CreateClient();
            {
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, string.Format(TunerPresets, band))).ConfigureAwait(false);
                return JsonConvert.DeserializeObject<GetTunerPresetInfo>(result);
            }
        }

        /// <summary>
        /// Gets the usb presets.
        /// </summary>
        /// <param name="baseUri">The base URI.</param>
        /// <returns></returns>
        public async Task<GetNetUsbPresetInfo> GetUsbPresets(Uri baseUri)
        {
            var client = _httpClientFactory.CreateClient();
            {
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, NetUsbPresets)).ConfigureAwait(false);
                return JsonConvert.DeserializeObject<GetNetUsbPresetInfo>(result);
            }
        }

        /// <summary>
        /// Gets the device location information asynchronous.
        /// </summary>
        /// <param name="baseUri">The base URI.</param>
        /// <returns></returns>
        private async Task<GetLocationInfoResponse> GetDeviceLocationInfoAsync(Uri baseUri)
        {
            var client = _httpClientFactory.CreateClient();
            {
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, LocationInfo)).ConfigureAwait(false);
                return JsonConvert.DeserializeObject<GetLocationInfoResponse>(result);
            }
        }

        /// <summary>
        /// Gets the tuner play information asynchronous.
        /// </summary>
        /// <param name="baseUri">The base URI.</param>
        /// <returns></returns>
        private async Task<GetTunerPlayInfoResponse> GetTunerPlayInfoAsync(Uri baseUri)
        {
            var client = _httpClientFactory.CreateClient();
            {
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, TunerPlayInfo)).ConfigureAwait(false);
                return JsonConvert.DeserializeObject<GetTunerPlayInfoResponse>(result);
            }
        }

        /// <summary>
        /// Gets the net radio information asynchronous.
        /// </summary>
        /// <param name="baseUri">The base URI.</param>
        /// <returns></returns>
        private async Task<GetNetUsbPlayInfoResponse> GetNetRadioInfoAsync(Uri baseUri)
        {
            var client = _httpClientFactory.CreateClient();
            {
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, NetRadioPlayInfo)).ConfigureAwait(false);
                return JsonConvert.DeserializeObject<GetNetUsbPlayInfoResponse>(result);
            }
        }

        /// <summary>
        /// Adds the client headers.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <returns></returns>
        private void AddClientHeaders(HttpClient client)
        {
            if (client == null)
                return;

            client.DefaultRequestHeaders.Add("X-AppName", Constants.UDP_ClientId);
            client.DefaultRequestHeaders.Add("X-AppPort", Constants.UDP_ListenPort.ToString());
        }

        #endregion

        #region public 

        /// <summary>
        /// Loads the rooms asynchronous.
        /// </summary>
        /// <returns></returns>
        public void LoadRooms()
        {
            this.service = new DLNADiscovery(_httpClientFactory, GetLocalIp());
            service.DeviceFound += DeviceFoundAsync;
            service.ScanNetwork();
        }

        private string GetLocalIp()
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();

            if (icp?.NetworkAdapter == null)
                return null;

            var hostnames = NetworkInformation.GetHostNames().Where(hn =>
                            hn.IPInformation?.NetworkAdapter != null &&
                            hn.IPInformation.NetworkAdapter.NetworkAdapterId == icp.NetworkAdapter.NetworkAdapterId);

            var hostname = hostnames.FirstOrDefault(t => t.Type == Windows.Networking.HostNameType.Ipv4);
            if (hostname == null)
                hostname = hostnames.FirstOrDefault();

            // the ip address
            return hostname?.CanonicalName;
        }

        /// <summary>
        /// Refreshes the device asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="baseUri">The base URI.</param>
        /// <param name="zone">The zone.</param>
        /// <returns></returns>
        public async Task<MusicCastDevice> RefreshDeviceAsync(string id, Uri baseUri, string zone)
        {
            try
            {
                var status = await this.GetDeviceStatusAsync(baseUri, zone).ConfigureAwait(false);
                var distributionInfo = await this.GetDistributionInfoAsync(baseUri).ConfigureAwait(false);

                var temp = new MusicCastDevice()
                {
                    Id = id,
                    Power = status.power,
                    Input = (status.input),
                    Volume = status.volume,
                    MaxVolume = status.max_volume
                };

                if (status.input == Inputs.tuner)
                {
                    var tuner = await this.GetTunerPlayInfoAsync(baseUri).ConfigureAwait(false);
                    temp.NowPlayingInformation = tuner.NowPlayingSummary;
                }

                if (status.input == Inputs.net_radio)
                {
                    var tuner = await this.GetNetRadioInfoAsync(baseUri).ConfigureAwait(false);
                    temp.NowPlayingInformation = tuner.NowPlayingSummary;
                }

                if (status.input == Inputs.mc_link)
                {
                    var tuner = await this.GetNetRadioInfoAsync(baseUri).ConfigureAwait(false);
                    temp.NowPlayingInformation = tuner.NowPlayingSummary;
                }

                if (distributionInfo.client_list.Count > 0 && !string.IsNullOrEmpty(distributionInfo.group_name))
                {
                    temp.FriendlyName = distributionInfo.group_name;
                }

                return temp;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Toggles the power asynchronous.
        /// </summary>
        /// <param name="baseUri">The base URI.</param>
        /// <param name="zoneName">Name of the zone.</param>
        /// <returns></returns>
        public async Task TogglePowerAsync(Uri baseUri, string zoneName)
        {
            var client = _httpClientFactory.CreateClient();
            {
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, string.Format(TogglePower, zoneName))).ConfigureAwait(false);
                await Task.Delay(1000);
            }
        }

        /// <summary>
        /// Adjusts the device volume.
        /// </summary>
        /// <param name="baseUri">The base URI.</param>
        /// <param name="zoneName">Name of the zone.</param>
        /// <param name="volume">The volume.</param>
        /// <returns></returns>
        public async Task AdjustDeviceVolume(Uri baseUri, string zoneName, int volume)
        {
            var client = _httpClientFactory.CreateClient();
            {
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, string.Format(SetVolume, zoneName, volume))).ConfigureAwait(false);
            }
        }

        #endregion

        // Dispose() calls Dispose(true)
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // The bulk of the clean-up code is implemented in Dispose(bool)
        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed) return;

            if (disposing)
            {
                if (service != null)
                    service.Dispose();
            }

            isDisposed = true;
        }
    }
}