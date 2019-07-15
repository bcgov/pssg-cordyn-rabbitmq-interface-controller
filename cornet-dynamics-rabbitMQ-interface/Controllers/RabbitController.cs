using System;
using cornet_dynamics_rabbitMQ_interface.Objects;
using cornet_dynamics_rabbitMQ_interface.Services;
using Microsoft.AspNetCore.Mvc;

namespace cornet_dynamics_rabbitMQ_interface.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RabbitController : ControllerBase
    {
        /// <summary>
        /// Get a list of message on a rabbitmq queue
        /// </summary>
        /// <returns>
        /// List of messages from the queue
        /// </returns>
        [HttpGet]
        [Route("messages")]
        public RabbitMessages GetMessages()
        {
            Console.WriteLine("{0}: Get all messages request has been recieved. {1}", DateTime.Now, Environment.NewLine);
            RabbitService rabbitService = new RabbitService();
            return rabbitService.GetRabbitMessages();
        }
         /// <summary>
         /// Get list of messages for a given id/guid
         /// </summary>
         /// <param name="id">search primary key</param>
         /// <param name="guid">search guid</param>
         /// <returns>
         /// List of rabbit messages
         /// </returns>
        [HttpGet]
        [Route("message")]
        public RabbitMessages Get([FromQuery] string id,[FromQuery] string guid)
        {
            Console.WriteLine("{0}: Get message request has been recieved. Requested id: {1}, guid: {2} {3}", DateTime.Now, id, guid, Environment.NewLine);
            RabbitService rabbitService = new RabbitService();
            return rabbitService.GetRabbitMessageById(id, guid);
        }
        /// <summary>
        /// Re-queue a message
        /// </summary>
        /// <param name="id">search primary key</param>
        /// <param name="guid">search guid</param>
        /// <returns>
        /// Ok or not found
        /// </returns>
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
        /// <summary>
        /// Move all messages from one queue to another
        /// </summary>
        /// <returns>
        /// Ok or notfound
        /// </returns>
        [HttpPost]
        [Route("requeueall")]
        public IActionResult ReQueueMultiplePost()
        {
            Console.WriteLine("{0}: Re-Queue all messages request has been recieved. {1}", DateTime.Now, Environment.NewLine);
            RabbitService rabbitService = new RabbitService();
            if (rabbitService.ReQueueMessages())
            {
                return Ok("Messages has been re-queued");
            }
            else
            {
                return NotFound("Not all messages succeeded");
            }

        }
        /// <summary>
        /// Remove a message from a queue
        /// </summary>
        /// <param name="id">search primary key</param>
        /// <param name="guid">search guid</param>
        /// <returns>
        /// Ok or not found
        /// </returns>
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
        /// <summary>
        /// Delete all messages from a queue
        /// </summary>
        /// <returns>
        /// Return ok on success or bad request when failed
        /// </returns>
        [HttpDelete]
        [Route("deletemessages")]
        public IActionResult DeleteMessagesPost()
        {
            Console.WriteLine("{0}: Queue purge message request has been recieved. {1}", DateTime.Now, Environment.NewLine);
            RabbitService rabbitService = new RabbitService();
            if (rabbitService.DeleteMessages().IsSuccessStatusCode)
            {
                return Ok("Queue has been purged");
            }
            else
            {
                return BadRequest("Delete failed");
            }
        }
    }
}
