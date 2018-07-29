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

namespace Musiccast.Service
{
    public class MusicCastService
    {
        public event EventHandler<MusicCastDevice> DeviceFound;

        private const string Info = "/YamahaExtendedControl/v1/system/getDeviceInfo";
        private const string Status = "/YamahaExtendedControl/v1/{0}/getStatus";
        private const string LocationInfo = "/YamahaExtendedControl/v1/system/getLocationInfo";
        private const string TogglePower = "/YamahaExtendedControl/v1/{0}/setPower?power=toggle";
        private const string TunerPlayInfo = "/YamahaExtendedControl/v1/tuner/getPlayInfo";
        private const string NetRadioPlayInfo = "/YamahaExtendedControl/v1/netusb/getPlayInfo";
        private const string SetVolume = "/YamahaExtendedControl/v1/{0}/setVolume?volume={1}";

        public async Task LoadRoomsAsync()
        {
            var service = new DLNADiscovery();
            service.DeviceFound += async (s, e) => { await DeviceFoundAsync(s, e); };
            await service.ScanNetworkAsync(CancellationToken.None);
        }

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
                var convertedModel = ConvertApiDeviceToDevice(device.X_device.X_URLBase, zone, device, status, info);

                if(status.input == Inputs.tuner)
                {
                    var tuner = await this.GetTunerPlayInfoAsync(baseUri);
                    convertedModel.NowPlayingInformation = tuner.NowPlayingSummary;
                }

                if (DeviceFound != null)
                    DeviceFound(this, convertedModel);
            }
        }

        

        public async Task<MusicCastDevice> RefreshDeviceAsync(string id, string zone)
        {
            try
            {
                var status = await this.GetDeviceStatusAsync(new Uri(id), zone);

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
                    var tuner = await this.GetTunerPlayInfoAsync(new Uri(id));
                    temp.NowPlayingInformation = tuner.NowPlayingSummary;
                }

                if (status.input == Inputs.net_radio)
                {
                    var tuner = await this.GetNetRadioInfoAsync(new Uri(id));
                    temp.NowPlayingInformation = tuner.NowPlayingSummary;
                }

                return temp;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private MusicCastDevice ConvertApiDeviceToDevice(string baseUri, string zone, DLNADescription device, GetStatusResponse status, GetDeviceInfoResponse info)
        {
            return new MusicCastDevice()
            {
                Id = baseUri,
                Zone = zone,
                ModelName = info.model_name,
                FriendlyName = device.device.friendlyName,
                Location = (device.Location),
                ImagePath = (device.device.iconList.Last().url),
                ImageSize = device.device.iconList.Last().width,
                Power = status.power,
                Input = (status.input)
            };
        }

        public async Task TogglePowerAsync(string id, string zoneName)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-AppName", "MusicCast/1.40(UWP)");
                client.DefaultRequestHeaders.Add("X-AppPort", "41100");
                var result = await client.GetStringAsync(new Uri(new Uri(id), string.Format(TogglePower, zoneName)));
                await Task.Delay(1000);
            }
        }

        private async Task<GetStatusResponse> GetDeviceStatusAsync(Uri baseUri, string zoneName)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-AppName", "MusicCast/1.40(UWP)");
                client.DefaultRequestHeaders.Add("X-AppPort", "41100");
                var result = await client.GetStringAsync(new Uri(baseUri, string.Format(Status, zoneName)));
                return JsonConvert.DeserializeObject<GetStatusResponse>(result);
            }
        }

        private async Task<GetDeviceInfoResponse> GetDeviceInfo(Uri baseUri)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-AppName", "MusicCast/1.40(UWP)");
                client.DefaultRequestHeaders.Add("X-AppPort", "41100");
                var result = await client.GetStringAsync(new Uri(baseUri, Info));
                return JsonConvert.DeserializeObject<GetDeviceInfoResponse>(result);
            }
        }

        private async Task<GetLocationInfoResponse> GetDeviceLocationInfoAsync(Uri baseUri)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-AppName", "MusicCast/1.40(UWP)");
                client.DefaultRequestHeaders.Add("X-AppPort", "41100");
                var result = await client.GetStringAsync(new Uri(baseUri, LocationInfo));
                return JsonConvert.DeserializeObject<GetLocationInfoResponse>(result);
            }
        }

        private async Task<GetTunerPlayInfoResponse> GetTunerPlayInfoAsync(Uri baseUri)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-AppName", "MusicCast/1.40(UWP)");
                client.DefaultRequestHeaders.Add("X-AppPort", "41100");
                var result = await client.GetStringAsync(new Uri(baseUri, TunerPlayInfo));
                return JsonConvert.DeserializeObject<GetTunerPlayInfoResponse>(result);
            }
        }

        private async Task<GetNetUsbPlayInfoResponse> GetNetRadioInfoAsync(Uri baseUri)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-AppName", "MusicCast/1.40(UWP)");
                client.DefaultRequestHeaders.Add("X-AppPort", "41100");
                var result = await client.GetStringAsync(new Uri(baseUri, NetRadioPlayInfo));
                return JsonConvert.DeserializeObject<GetNetUsbPlayInfoResponse>(result);
            }
        }
        public async Task AdjustDeviceVolume(string id, string zoneName, int volume)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-AppName", "MusicCast/1.40(UWP)");
                client.DefaultRequestHeaders.Add("X-AppPort", "41100");
                var result = await client.GetStringAsync(new Uri(new Uri(id), string.Format(SetVolume, zoneName, volume)));
            }
        }
    }
}
