using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cornet_dynamics_rabbitMQ_interface.Objects
{
    public class PropertiesHeader
    {
        [JsonProperty("headers")]
        public HeaderItems headerItems { get; set; }
    }
}
