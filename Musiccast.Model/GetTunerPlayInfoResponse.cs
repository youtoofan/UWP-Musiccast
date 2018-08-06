using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Musiccast.Model
{
    public class GetTunerPlayInfoResponse
    {
        public int response_code { get; set; }
        public string band { get; set; }
        public bool auto_scan { get; set; }
        public Fm fm { get; set; }
        public Rds rds { get; set; }
        public Dab dab { get; set; }

        public string NowPlayingSummary
        {
            get
            {
                if(band.Equals("FM", StringComparison.OrdinalIgnoreCase))
                {
                    return fm.freq.ToString(CultureInfo.InvariantCulture);
                }

                if (band.Equals("RDS", StringComparison.OrdinalIgnoreCase))
                {
                    return rds.program_service;
                }

                if (band.Equals("DAB", StringComparison.OrdinalIgnoreCase))
                {
                    return dab.dls;
                }

                return string.Empty;
            }
        }
    }

    public class Fm
    {
        public int preset { get; set; }
        public int freq { get; set; }
        public bool tuned { get; set; }
        public string audio_mode { get; set; }
    }

    public class Rds
    {
        public string program_type { get; set; }
        public string program_service { get; set; }
        public string radio_text_a { get; set; }
        public string radio_text_b { get; set; }
    }

    public class Dab
    {
        public int preset { get; set; }
        public int id { get; set; }
        public string status { get; set; }
        public int freq { get; set; }
        public string category { get; set; }
        public string audio_mode { get; set; }
        public int bit_rate { get; set; }
        public int quality { get; set; }
        public int tune_aid { get; set; }
        public bool off_air { get; set; }
        public bool dab_plus { get; set; }
        public string program_type { get; set; }
        public string ch_label { get; set; }
        public string service_label { get; set; }
        public string dls { get; set; }
        public string ensemble_label { get; set; }
    }

}
