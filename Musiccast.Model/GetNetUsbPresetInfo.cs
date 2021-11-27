using System.Collections.Generic;

namespace Musiccast.Model
{
    public class GetNetUsbPresetInfo
    {
        public int response_code { get; set; }
        public List<NetUsbPresetInfo> preset_info { get; set; }
        public string[] func_list { get; set; }
    }

    public class NetUsbPresetInfo
    {
        public string input { get; set; }
        public string text { get; set; }
        public int attribute { get; set; }
    }

}
