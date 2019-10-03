using Newtonsoft.Json;
using System;

namespace pssg_rabbitmq_interface.Objects
{
    public class RabbitRequest
    {
        public RabbitRequest(int count, String encoding, int truncate, String ackmode)
        {
            this.count = count;
            this.encoding = encoding;
            this.truncate = truncate;
            this.ackmode = ackmode;
        }
        [JsonProperty("count")]
        public int count { get; set; }
        [JsonProperty("encoding")]
        public String encoding { get; set; }
        [JsonProperty("truncate")]
        public int truncate { get; set; }
        [JsonProperty("ackmode")]
        public String ackmode { get; set; }
    }
}
