using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Musiccast.Model
{
    public class GetDeviceInfoResponse
    {
        public int response_code { get; set; }
        public string model_name { get; set; }
        public string destination { get; set; }
        public string device_id { get; set; }
        public string system_id { get; set; }
        public float system_version { get; set; }
        public float api_version { get; set; }
        public int netmodule_generation { get; set; }
        public string netmodule_version { get; set; }
        public string netmodule_checksum { get; set; }
        public string operation_mode { get; set; }
        public string update_error_code { get; set; }

    }
}
