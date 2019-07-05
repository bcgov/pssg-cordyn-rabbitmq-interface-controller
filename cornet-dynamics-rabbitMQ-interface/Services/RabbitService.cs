using cornet_dynamics_rabbitMQ_interface.Clients;
using cornet_dynamics_rabbitMQ_interface.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace cornet_dynamics_rabbitMQ_interface.Services
{
    public class RabbitService
    {
        public RabbitMessages GetRabbitMessages()
        {
            RabbitClient rabbitClient = new RabbitClient();
            return rabbitClient.GetMessages();

        }
        public RabbitMessages GetRabbitMessageById(String id, String guid)
        {
            RabbitClient rabbitClient = new RabbitClient();
            RabbitMessages rabbitMessages = rabbitClient.GetMessages();
            RabbitMessages retRabbitMessages = new RabbitMessages();
            retRabbitMessages.messages = new List<QueueMessage>();
            foreach (QueueMessage queueMessage in rabbitMessages.messages)
            {
                if (queueMessage.eventId == id && queueMessage.guid == guid)
                {
                    retRabbitMessages.messages.Add(queueMessage);
                }
            }
            return retRabbitMessages;
        }
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
        public bool ReQueueMessages()
        {
            bool error = false;
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
                error = true;
            }
            return error;
        }
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
        public HttpResponseMessage DeleteMessages()
        {
            RabbitClient rabbitClient = new RabbitClient();
            return rabbitClient.PurgeQueue();
        } 

    }
}
