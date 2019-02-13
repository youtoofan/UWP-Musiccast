using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Musiccast.Model;
using Newtonsoft.Json;
using Windows.System.Threading;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace Musiccast.Service
{
    /// <summary>
    /// 
    /// </summary>
    public class MusicCastService
    {
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
        /// <summary>
        /// The status
        /// </summary>
        private const string Status = "/YamahaExtendedControl/v2/{0}/getStatus";
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

        #region private methods

        /// <summary>
        /// Devices the found asynchronous.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="device">The device.</param>
        /// <returns></returns>
        private async Task DeviceFoundAsync(object sender, DLNADescription device)
        {
            if (device == null || device.X_device == null)
                return;

            var baseUri = new Uri(device.X_device.X_URLBase);
            var info = await this.GetDeviceInfo(baseUri);

            var location = await this.GetDeviceLocationInfoAsync(baseUri);

            foreach (var zone in location.zone_list.ValidZones)
            {
                var status = await this.GetDeviceStatusAsync(baseUri, zone);
                var friendlyName = await this.GetDeviceZoneFriendlyNameAsync(baseUri, zone);
                var convertedModel = ConvertApiDeviceToDevice(device.X_device.X_URLBase, zone, friendlyName.text, device, status, info);

                if(status.input == Inputs.tuner)
                {
                    var tuner = await this.GetTunerPlayInfoAsync(baseUri);
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
        private MusicCastDevice ConvertApiDeviceToDevice(string baseUri, string zone, string friendlyName, DLNADescription device, GetStatusResponse status, GetDeviceInfoResponse info)
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
            using (HttpClient client = new HttpClient())
            {
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, string.Format(Status, zoneName)));
                return JsonConvert.DeserializeObject<GetStatusResponse>(result);
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
            using (var client = new HttpClient())
            {
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, string.Format(RecallTunerPreset, zoneName, band, number)));
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
            using (var client = new HttpClient())
            {
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, string.Format(RecallUsbPreset, zoneName, number)));
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
            using (var client = new HttpClient())
            {
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, string.Format(Status, zoneName)));
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
            using (HttpClient client = new HttpClient())
            {
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, string.Format(Names, zoneName)));
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
            using (HttpClient client = new HttpClient())
            {
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, Info));
                return JsonConvert.DeserializeObject<GetDeviceInfoResponse>(result);
            }
        }

        /// <summary>
        /// Gets the features.
        /// </summary>
        /// <param name="baseUri">The base URI.</param>
        /// <returns></returns>
        public async Task<GetFeaturesResponse> GetFeatures(Uri baseUri)
        {
            using (HttpClient client = new HttpClient())
            {
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, Features));
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
            using (HttpClient client = new HttpClient())
            {
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, string.Format(TunerPresets, band)));
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
            using (HttpClient client = new HttpClient())
            {
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, NetUsbPresets));
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
            using (HttpClient client = new HttpClient())
            {
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, LocationInfo));
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
            using (HttpClient client = new HttpClient())
            {
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, TunerPlayInfo));
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
            using (HttpClient client = new HttpClient())
            {
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, NetRadioPlayInfo));
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

            client.DefaultRequestHeaders.Add("X-AppName", "MusicCast/1.41(UWP)");
            client.DefaultRequestHeaders.Add("X-AppPort", "41100");
        }

        #endregion

        #region public 

        /// <summary>
        /// Loads the rooms asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task LoadRoomsAsync()
        {
            var service = new DLNADiscovery();
            service.DeviceFound += async (s, e) => { await DeviceFoundAsync(s, e); };
            await service.ScanNetworkAsync(CancellationToken.None);
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
                var status = await this.GetDeviceStatusAsync(baseUri, zone);

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
                    var tuner = await this.GetTunerPlayInfoAsync(baseUri);
                    temp.NowPlayingInformation = tuner.NowPlayingSummary;
                }

                if (status.input == Inputs.net_radio)
                {
                    var tuner = await this.GetNetRadioInfoAsync(baseUri);
                    temp.NowPlayingInformation = tuner.NowPlayingSummary;
                }

                if(status.input == Inputs.mc_link)
                {
                    var tuner = await this.GetNetRadioInfoAsync(baseUri);
                    temp.NowPlayingInformation = tuner.NowPlayingSummary;
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
            using (HttpClient client = new HttpClient())
            {
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, string.Format(TogglePower, zoneName)));
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
            using (HttpClient client = new HttpClient())
            {
                AddClientHeaders(client);
                var result = await client.GetStringAsync(new Uri(baseUri, string.Format(SetVolume, zoneName, volume)));
            }
        }

        

        #endregion
    }
}
