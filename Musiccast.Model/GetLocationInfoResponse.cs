using System.Collections.Generic;

namespace Musiccast.Model
{
    public class GetLocationInfoResponse
    {
        public int response_code { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public Zone_List zone_list { get; set; }
    }

    public class Zone_List
    {
        public bool main { get; set; }
        public bool zone2 { get; set; }
        public bool zone3 { get; set; }
        public bool zone4 { get; set; }

        public IEnumerable<string> ValidZones
        {
            get
            {
                if (main)
                    yield return "main";

                if (zone2)
                    yield return "zone2";

                if (zone3)
                    yield return "zone3";

                if (zone4)
                    yield return "zone4";
            }
            
        }
    }
}
