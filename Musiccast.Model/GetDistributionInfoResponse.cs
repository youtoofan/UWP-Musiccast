using System.Collections.Generic;

namespace Musiccast.Model
{
    public class GetDistributionInfoResponse
    {
        public string response_code { get; set; }
        public string group_id { get; set; }
        public string group_name { get; set; }
        public string role { get; set; }
        public string server_zone { get; set; }
        public List<ClientList> client_list { get; set; }
    }

    public class ClientList
    {
        public string ip_address { get; set; }
        public string data_type { get; set; }
    }
}
