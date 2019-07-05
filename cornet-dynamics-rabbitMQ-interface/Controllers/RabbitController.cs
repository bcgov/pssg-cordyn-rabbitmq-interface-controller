using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cornet_dynamics_rabbitMQ_interface.Objects;
using cornet_dynamics_rabbitMQ_interface.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace cornet_dynamics_rabbitMQ_interface.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RabbitController : ControllerBase
    {
        [HttpGet]
        [Route("messages")]
        public RabbitMessages GetMessages()
        {
            RabbitService rabbitService = new RabbitService();

            return rabbitService.GetRabbitMessages();
        }

        [HttpGet]
        [Route("message")]
        public RabbitMessages Get([FromQuery] string id,[FromQuery] string guid)
        {
            RabbitService rabbitService = new RabbitService();
            return rabbitService.GetRabbitMessageById(id, guid);
        }

        [HttpPost]
        [Route("requeue")]
        public IActionResult ReQueuePost([FromQuery] string id, [FromQuery] string guid)
        {
            RabbitService rabbitService = new RabbitService();
            if (rabbitService.ReQueueMessage(id, guid))
            {
                return Ok("Message has been re-queued");
            }
            else
            {
                return NotFound();
            }
        }
        [HttpPost]
        [Route("requeueall")]
        public IActionResult ReQueueMultiplePost()
        {
            RabbitService rabbitService = new RabbitService();
            if (rabbitService.ReQueueMessages())
            {
                return Ok("Message has been de-queued");
            }
            else
            {
                return NotFound();
            }

        }
        [HttpDelete]
        [Route("deletemessage")]
        public IActionResult DeleteMessagePost([FromQuery] string id, [FromQuery] string guid)
        {
            RabbitService rabbitService = new RabbitService();
            if (rabbitService.DeleteMessage(id, guid))
            {
                return Ok("Message has been de-queued");
            }
            else
            {
                return NotFound();
            }
        }
        [HttpDelete]
        [Route("deletemessages")]
        public void DeleteMessagesPost()
        {
            RabbitService rabbitService = new RabbitService();
            rabbitService.DeleteMessages();
        }
    }
}
