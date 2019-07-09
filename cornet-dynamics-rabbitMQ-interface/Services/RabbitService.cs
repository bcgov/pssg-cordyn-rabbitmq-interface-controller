using cornet_dynamics_rabbitMQ_interface.Clients;
using cornet_dynamics_rabbitMQ_interface.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace cornet_dynamics_rabbitMQ_interface.Services
{
    public class RabbitService
    {
        /// <summary>
        /// Get the messages from rabbit
        /// </summary>
        /// <returns>
        /// List of rabbit messages
        /// </returns>
        public RabbitMessages GetRabbitMessages()
        {
            RabbitClient rabbitClient = new RabbitClient();
            return rabbitClient.GetMessages();

        }
        /// <summary>
        /// Search for messages
        /// </summary>
        /// <param name="id">search primary key</param>
        /// <param name="guid">search guid</param>
        /// <returns>
        /// List of resulting messages
        /// </returns>
        public RabbitMessages GetRabbitMessageById(String id, String guid)
        {
            RabbitClient rabbitClient = new RabbitClient();
            RabbitMessages rabbitMessages = rabbitClient.GetMessages();
            RabbitMessages retRabbitMessages = new RabbitMessages();
            retRabbitMessages.messages = new List<ParkingLotMessage>();
            foreach (ParkingLotMessage parkingLotMessage in rabbitMessages.messages)
            {
                QueueMessage queueMessage = JsonConvert.DeserializeObject<QueueMessage>(parkingLotMessage.payload);
                if (queueMessage.eventId == id && queueMessage.guid == guid)
                {
                    retRabbitMessages.messages.Add(parkingLotMessage);
                }
            }
            return retRabbitMessages;
        }
        /// <summary>
        /// Re-queue a message. This removes the message from the parking lot
        /// </summary>
        /// <param name="id">search primary key</param>
        /// <param name="guid">search guid</param>
        /// <returns>
        /// Success or failure
        /// </returns>
        public bool ReQueueMessage(String id, String guid)
        {
            RabbitClient rabbitClient = new RabbitClient();
            QueueMessage queueMessage = rabbitClient.DeQueueMessage(id, guid);
            if (queueMessage != null)
            {
                HttpResponseMessage httpResponseMessage = rabbitClient.QueueRabbitMessage(queueMessage);
                return httpResponseMessage.IsSuccessStatusCode;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Requeue a list of messages
        /// </summary>
        /// <returns>
        /// success or failure
        /// </returns>
        public bool ReQueueMessages()
        {
            bool success = true;
            //Get all current messages
            RabbitMessages rabbitMessages = GetRabbitMessages();
            RabbitClient rabbitClient = new RabbitClient();
            try
            {
                rabbitClient.QueueRabbitMessages(rabbitMessages);
                rabbitClient.DeQueueMessages(rabbitMessages);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in Re-Queue all messages {0}", e.Message);
                success = false;
            }
            return success;
        }
        /// <summary>
        /// de-queue a message
        /// </summary>
        /// <param name="id">search primary key</param>
        /// <param name="guid">search guid</param>
        /// <returns>
        /// success or failure
        /// </returns>
        public bool DeleteMessage(String id, String guid)
        {
            RabbitClient rabbitClient = new RabbitClient();
            QueueMessage queueMessage= rabbitClient.DeQueueMessage(id, guid);
            if (queueMessage != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Remove all messages from the queue
        /// </summary>
        /// <returns>
        /// http response from api
        /// </returns>
        public HttpResponseMessage DeleteMessages()
        {
            RabbitClient rabbitClient = new RabbitClient();
            return rabbitClient.PurgeQueue();
        } 

    }
}
