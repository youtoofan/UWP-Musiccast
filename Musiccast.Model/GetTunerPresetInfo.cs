using System.Collections.Generic;

namespace Musiccast.Model
{
    public class GetTunerPresetInfo
    {
        public int response_code { get; set; }
        public List<TunerPresetInfo> preset_info { get; set; }
        public string[] func_list { get; set; }
    }

    public class TunerPresetInfo
    {
        public string band { get; set; }
        public int number { get; set; }
        public int hd_program { get; set; }
        public string text { get; set; }
    }

}
