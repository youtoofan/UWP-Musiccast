using Musiccast.Model;

namespace Musiccast.Service
{
    public class GetStatusResponse
    {
        public int response_code { get; set; }
        public string power { get; set; }
        public int volume { get; set; }
        public bool mute { get; set; }
        public int max_volume { get; set; }
        public Inputs input { get; set; }
        public bool distribution_enable { get; set; }
        public string sound_program { get; set; }
        public bool clear_voice { get; set; }
        public int subwoofer_volume { get; set; }
        public bool bass_extension { get; set; }
        public string link_control { get; set; }
        public string link_audio_delay { get; set; }
        public string link_audio_quality { get; set; }
        public int disable_flags { get; set; }

    }
}
