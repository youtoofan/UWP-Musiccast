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
        public event EventHandler<MusicCastDevice> DeviceFound;

        private const string Info = "/YamahaExtendedControl/v1/system/getDeviceInfo";
        private const string Features = "/YamahaExtendedControl/v1/system/getFeatures";
        private const string NetworkStatus = "/YamahaExtendedControl/v1/system/getNetworkStatus";

        private const string Status = "/YamahaExtendedControl/v2/{0}/getStatus";
        private const string Distribution = "YamahaExtendedControl/v1/dist/getDistributionInfo";

        private const string Names = "/YamahaExtendedControl/v1/system/getNameText?id={0}";
        private const string LocationInfo = "/YamahaExtendedControl/v1/system/getLocationInfo";
        private const string TogglePower = "/YamahaExtendedControl/v1/{0}/setPower?power=toggle";
        private const string TunerPlayInfo = "/YamahaExtendedControl/v2/tuner/getPlayInfo";
        private const string TunerPresets = "/YamahaExtendedControl/v1/tuner/getPresetInfo?band={0}";
        private const string NetUsbPresets = "/YamahaExtendedControl/v1/netusb/getPresetInfo";
        private const string NetRadioPlayInfo = "/YamahaExtendedControl/v1/netusb/getPlayInfo";
        private const string SetVolume = "/YamahaExtendedControl/v1/{0}/setVolume?volume={1}";

        private const string SetInput = "/YamahaExtendedControl/v1/{0}/setInput?input={1}";
        private const string RecallTunerPreset = "/YamahaExtendedControl/v1/tuner/recallPreset?zone={0}&band={1}&num={2}";
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
            this.service = new DLNADiscovery(_httpClientFactory, GetLocalIp());
            service.DeviceFound += DeviceFoundAsync;
        }

        #region private methods

        private async void DeviceFoundAsync(object sender, DLNADescription device)
        {
            if (device == null || device.X_device == null)
                return;

            try
            {
                var baseUri = new Uri(device.X_device.X_URLBase);
                var info = await this.GetDeviceInfo(baseUri).ConfigureAwait(false);
                await this.GetNetworkStatus(baseUri).ConfigureAwait(false);
                var location = await this.GetDeviceLocationInfoAsync(baseUri).ConfigureAwait(false);
                var distributionInfo = await this.GetDistributionInfoAsync(baseUri).ConfigureAwait(false);

                foreach (var zone in location.zone_list.ValidZones)
                {
                    var status = await this.GetDeviceStatusAsync(baseUri, zone).ConfigureAwait(false);
                    var friendlyName = await this.GetDeviceZoneFriendlyNameAsync(baseUri, zone).ConfigureAwait(false);
                    var convertedModel = ConvertApiDeviceToDevice(baseUri, zone, friendlyName.text, device, status, info);

                    if (status.input == Inputs.tuner)
                    {
                        var tuner = await this.GetTunerPlayInfoAsync(baseUri).ConfigureAwait(false);
                        convertedModel.NowPlayingInformation = tuner.NowPlayingSummary;
                    }

                    if (DeviceFound != null)
                        DeviceFound(this, convertedModel);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

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

        private async Task<GetNameTextResponse> GetDeviceZoneFriendlyNameAsync(Uri baseUri, string zoneName)
        {
            var client = _httpClientFactory.CreateClient();
            {
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, string.Format(Names, zoneName))).ConfigureAwait(false);
                return JsonConvert.DeserializeObject<GetNameTextResponse>(result);
            }
        }

        private async Task<GetDeviceInfoResponse> GetDeviceInfo(Uri baseUri)
        {
            var client = _httpClientFactory.CreateClient();
            {
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, Info)).ConfigureAwait(false);
                return JsonConvert.DeserializeObject<GetDeviceInfoResponse>(result);
            }
        }

        private async Task GetNetworkStatus(Uri baseUri)
        {
            var client = _httpClientFactory.CreateClient();
            AddClientHeaders(client);
            var result = await client.GetStringAsync(new Uri(baseUri, NetworkStatus));
            Debug.WriteLine(result);
        }

        private async Task<GetLocationInfoResponse> GetDeviceLocationInfoAsync(Uri baseUri)
        {
            var client = _httpClientFactory.CreateClient();
            {
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, LocationInfo)).ConfigureAwait(false);
                return JsonConvert.DeserializeObject<GetLocationInfoResponse>(result);
            }
        }

        private async Task<GetTunerPlayInfoResponse> GetTunerPlayInfoAsync(Uri baseUri)
        {
            var client = _httpClientFactory.CreateClient();
            {
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, TunerPlayInfo)).ConfigureAwait(false);
                return JsonConvert.DeserializeObject<GetTunerPlayInfoResponse>(result);
            }
        }

        private async Task<GetNetUsbPlayInfoResponse> GetNetRadioInfoAsync(Uri baseUri)
        {
            var client = _httpClientFactory.CreateClient();
            {
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, NetRadioPlayInfo)).ConfigureAwait(false);
                return JsonConvert.DeserializeObject<GetNetUsbPlayInfoResponse>(result);
            }
        }

        private void AddClientHeaders(HttpClient client)
        {
            if (client == null)
                return;

            client.DefaultRequestHeaders.Add("X-AppName", Constants.UDP_ClientId);
            client.DefaultRequestHeaders.Add("X-AppPort", Constants.UDP_ListenPort.ToString());
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

        #endregion

        #region public 

        public async Task LoadRoomsAsync()
        {
            await service.ScanNetworkAsync();
        }

        public async Task<GetFeaturesResponse> GetFeatures(Uri baseUri)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, Features)).ConfigureAwait(false);
                return JsonConvert.DeserializeObject<GetFeaturesResponse>(result);
            }
            catch (Exception)
            {
                return new GetFeaturesResponse() { system = new Model.System() { input_list = Enumerable.Empty<Input_List>().ToArray() } };
            }
        }

        public async Task<GetTunerPresetInfo> GetTunerPresets(Uri baseUri, string band)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, string.Format(TunerPresets, band))).ConfigureAwait(false);
                return JsonConvert.DeserializeObject<GetTunerPresetInfo>(result);
            }
            catch (Exception)
            {
                return new GetTunerPresetInfo() { preset_info = Enumerable.Empty<TunerPresetInfo>().ToList() };
            }
        }

        public async Task<GetNetUsbPresetInfo> GetUsbPresets(Uri baseUri)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, NetUsbPresets)).ConfigureAwait(false);
                return JsonConvert.DeserializeObject<GetNetUsbPresetInfo>(result);
            }
            catch (Exception)
            {
                return new GetNetUsbPresetInfo() { preset_info = Enumerable.Empty<NetUsbPresetInfo>().ToList() };
            }
        }

        public async Task<bool> RecallTunerPresetAsync(Uri baseUri, string zoneName, string band, int number)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, string.Format(RecallTunerPreset, zoneName, band, number))).ConfigureAwait(false);
                return result != null && JsonConvert.DeserializeObject<SetDevicePropertyResponse>(result).response_code == 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> RecallUsbPresetAsync(Uri baseUri, string zoneName, int number)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, string.Format(RecallUsbPreset, zoneName, number))).ConfigureAwait(false);
                return result != null && JsonConvert.DeserializeObject<SetDevicePropertyResponse>(result).response_code == 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ChangeDeviceInputAsync(Uri baseUri, string zoneName, string id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, string.Format(SetInput, zoneName, id))).ConfigureAwait(false);
                return result != null && JsonConvert.DeserializeObject<SetDevicePropertyResponse>(result).response_code == 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

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
                else
                {
                    var tuner = await this.GetNetRadioInfoAsync(baseUri).ConfigureAwait(false);
                    temp.NowPlayingInformation = tuner.NowPlayingSummary;
                }

                var friendlyName = await this.GetDeviceZoneFriendlyNameAsync(baseUri, zone).ConfigureAwait(false);
                temp.FriendlyName = friendlyName.text;

                return temp;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        public async Task TogglePowerAsync(Uri baseUri, string zoneName)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                AddClientHeaders(client);
                _ = await client.GetStringAsync(new Uri(baseUri, string.Format(TogglePower, zoneName))).ConfigureAwait(false);
                await Task.Delay(1000);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        public async Task AdjustDeviceVolume(Uri baseUri, string zoneName, int volume)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                AddClientHeaders(client);
                _ = await client.GetStringAsync(new Uri(baseUri, string.Format(SetVolume, zoneName, volume))).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
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