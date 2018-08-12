using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Musiccast.Model
{
    public class GetFeaturesResponse
    {
        public int response_code { get; set; }
        public System system { get; set; }
        public Zone[] zone { get; set; }
        public Tuner tuner { get; set; }
        public Netusb netusb { get; set; }
        public Distribution distribution { get; set; }
        public Ccs ccs { get; set; }
    }

    public class System
    {
        public string[] func_list { get; set; }
        public int zone_num { get; set; }
        public Input_List[] input_list { get; set; }
    }

    public class Input_List
    {
        public string id { get; set; }
        public bool distribution_enable { get; set; }
        public bool rename_enable { get; set; }
        public bool account_enable { get; set; }
        public string play_info_type { get; set; }
    }

    public class Tuner
    {
        public string[] func_list { get; set; }
        public Range_Step[] range_step { get; set; }
        public Preset preset { get; set; }
    }

    public class Preset
    {
        public string type { get; set; }
        public int num { get; set; }
    }

    public class Range_Step
    {
        public string id { get; set; }
        public int min { get; set; }
        public int max { get; set; }
        public int step { get; set; }
    }

    public class Netusb
    {
        public string[] func_list { get; set; }
        public Preset1 preset { get; set; }
        public Recent_Info recent_info { get; set; }
        public Play_Queue play_queue { get; set; }
        public Mc_Playlist mc_playlist { get; set; }
        public string net_radio_type { get; set; }
        public Pandora pandora { get; set; }
    }

    public class Preset1
    {
        public int num { get; set; }
    }

    public class Recent_Info
    {
        public int num { get; set; }
    }

    public class Play_Queue
    {
        public int size { get; set; }
    }

    public class Mc_Playlist
    {
        public int size { get; set; }
        public int num { get; set; }
    }

    public class Pandora
    {
        public string[] sort_option_list { get; set; }
    }

    public class Distribution
    {
        public float version { get; set; }
        public int[] compatible_client { get; set; }
        public int client_max { get; set; }
        public string[] server_zone_list { get; set; }
    }

    public class Ccs
    {
        public bool supported { get; set; }
    }

    public class Zone
    {
        public string id { get; set; }
        public string[] func_list { get; set; }
        public string[] input_list { get; set; }
        public string[] sound_program_list { get; set; }
        public string[] surr_decoder_type_list { get; set; }
        public string[] tone_control_mode_list { get; set; }
        public string[] link_control_list { get; set; }
        public string[] link_audio_delay_list { get; set; }
        public string[] link_audio_quality_list { get; set; }
        public Range_Step1[] range_step { get; set; }
        public int scene_num { get; set; }
        public string[] cursor_list { get; set; }
        public string[] menu_list { get; set; }
        public string[] actual_volume_mode_list { get; set; }
        public bool zone_b { get; set; }
    }

    public class Range_Step1
    {
        public string id { get; set; }
        public float min { get; set; }
        public float max { get; set; }
        public float step { get; set; }
    }

}
