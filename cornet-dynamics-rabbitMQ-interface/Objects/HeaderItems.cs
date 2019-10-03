using Newtonsoft.Json;
using System;

namespace pssg_rabbitmq_interface.Objects
{
    public class HeaderItems
    {
        [JsonProperty("x-death")]
        public int retryCount { get; set; }
        [JsonProperty("x-request-id")]
        public String requestId { get; set; }
        [JsonProperty("date")]
        public String date { get; set; }
    }
}
