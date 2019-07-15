using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cornet_dynamics_rabbitMQ_interface.Objects
{
    public class HeaderItems
    {
        [JsonProperty("x-death")]
        public int retryCount { get; set; }
        [JsonProperty("x-request-id")]
        public String requestId { get; set; }
    }
}
