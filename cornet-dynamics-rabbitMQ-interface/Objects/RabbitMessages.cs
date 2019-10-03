using Newtonsoft.Json;
using System.Collections.Generic;


namespace pssg_rabbitmq_interface.Objects
{
    public class RabbitMessages
    {
        [JsonProperty("messages")]
        public List<ParkingLotMessage> messages { get; set; }
    }
}
