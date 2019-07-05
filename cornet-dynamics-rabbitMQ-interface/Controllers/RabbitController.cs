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
            Console.WriteLine("{0}: Get all messages request has been recieved. {1}", DateTime.Now, Environment.NewLine);
            RabbitService rabbitService = new RabbitService();
            return rabbitService.GetRabbitMessages();
        }

        [HttpGet]
        [Route("message")]
        public RabbitMessages Get([FromQuery] string id,[FromQuery] string guid)
        {
            Console.WriteLine("{0}: Get message request has been recieved. Requested id: {1}, guid: {2} {3}", DateTime.Now, id, guid, Environment.NewLine);
            RabbitService rabbitService = new RabbitService();
            return rabbitService.GetRabbitMessageById(id, guid);
        }

        [HttpPost]
        [Route("requeue")]
        public IActionResult ReQueuePost([FromQuery] string id, [FromQuery] string guid)
        {
            Console.WriteLine("{0}: Re-Queue message request has been recieved. Requested id: {1}, guid: {2} {3}", DateTime.Now, id, guid, Environment.NewLine);
            RabbitService rabbitService = new RabbitService();
            if (rabbitService.ReQueueMessage(id, guid))
            {
                return Ok("Message has been re-queued");
            }
            else
            {
                return NotFound("Message not found");
            }
        }
        [HttpPost]
        [Route("requeueall")]
        public IActionResult ReQueueMultiplePost()
        {
            Console.WriteLine("{0}: Re-Queue all messages request has been recieved. {1}", DateTime.Now, Environment.NewLine);
            RabbitService rabbitService = new RabbitService();
            if (rabbitService.ReQueueMessages())
            {
                return Ok("Message has been de-queued");
            }
            else
            {
                return NotFound("Not all messages succeeded");
            }

        }
        [HttpDelete]
        [Route("deletemessage")]
        public IActionResult DeleteMessagePost([FromQuery] string id, [FromQuery] string guid)
        {
            Console.WriteLine("{0}: De-Queue message request has been recieved. Requested id: {1}, guid: {2} {3}", DateTime.Now, id, guid, Environment.NewLine);
            RabbitService rabbitService = new RabbitService();
            if (rabbitService.DeleteMessage(id, guid))
            {
                return Ok("Message has been de-queued");
            }
            else
            {
                return NotFound("Message not found nothing removed from queue");
            }
        }
        [HttpDelete]
        [Route("deletemessages")]
        public IActionResult DeleteMessagesPost()
        {
            Console.WriteLine("{0}: Queue purge message request has been recieved. {1}", DateTime.Now, Environment.NewLine);
            RabbitService rabbitService = new RabbitService();
            rabbitService.DeleteMessages();
            return Ok("Queue has been purged");
        }
    }
}
