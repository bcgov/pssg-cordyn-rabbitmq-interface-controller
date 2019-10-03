using pssg_rabbitmq_interface.Clients;
using pssg_rabbitmq_interface.Objects;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace pssg_rabbitmq_interface.Services
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
        /// <returns>
        /// List of resulting messages
        /// </returns>
        public RabbitMessages GetRabbitMessageById(String id)
        {
            RabbitClient rabbitClient = new RabbitClient();
            RabbitMessages rabbitMessages = rabbitClient.GetMessages();
            RabbitMessages retRabbitMessages = new RabbitMessages
            {
                messages = new List<ParkingLotMessage>()
            };
            foreach (ParkingLotMessage parkingLotMessage in rabbitMessages.messages)
            {
                if (parkingLotMessage.properties.headerItems.requestId == id)
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
        public bool ReQueueMessage(String id)
        {
            RabbitClient rabbitClient = new RabbitClient();
            RabbitMessages rabbitMessages = rabbitClient.DeQueueMessage(id);
            if (rabbitMessages.messages.Count > 0)
            {

                HttpResponseMessage httpResponseMessage = rabbitClient.QueueRabbitMessage(rabbitMessages);
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
        public bool DeleteMessage(String id)
        {
            RabbitClient rabbitClient = new RabbitClient();
            RabbitMessages rabbitMessages = rabbitClient.DeQueueMessage(id);
            if (rabbitMessages.messages.Count > 0)
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
