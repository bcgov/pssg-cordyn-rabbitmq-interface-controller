using Newtonsoft.Json;
using System.Collections.Generic;


namespace cornet_dynamics_rabbitMQ_interface.Objects
{
    public class RabbitMessages
    {
        [JsonProperty("messages")]
        public List<QueueMessage> messages { get; set; }
    }
}
