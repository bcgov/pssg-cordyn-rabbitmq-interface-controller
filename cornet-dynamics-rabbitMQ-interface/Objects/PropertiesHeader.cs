using Newtonsoft.Json;

namespace pssg_rabbitmq_interface.Objects
{
    public class PropertiesHeader
    {
        [JsonProperty("headers")]
        public HeaderItems headerItems { get; set; }
    }
}
