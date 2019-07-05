using cornet_dynamics_rabbitMQ_interface.Objects;
using cornet_dynamics_rabbitMQ_interface.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace cornet_dynamics_rabbitMQ_interface.Clients
{
    public class RabbitClient
    {
        private readonly String routingKey = ConfigurationManager.FetchConfig("RabbitMq:MainQueue:Route");
        private readonly String exchange = ConfigurationManager.FetchConfig("RabbitMq:MainQueue:Exchange");
        private readonly String queue = ConfigurationManager.FetchConfig("RabbitMq:MainQueue:Queue");
        private readonly String username = ConfigurationManager.FetchConfig("RabbitMq:Authentication:Username");
        private readonly String password = ConfigurationManager.FetchConfig("RabbitMq:Authentication:Password");
        //Parking Lot settings
        private readonly String parkingLotQueue = ConfigurationManager.FetchConfig("RabbitMq:ParkingQueue:Queue");
        private readonly String parkingLotExchange = ConfigurationManager.FetchConfig("RabbitMq:ParkingQueue:Exchange");
        private readonly String parkingLotRoute = ConfigurationManager.FetchConfig("RabbitMq:ParkingQueue:Route");

        private readonly String rabbitGetQueueItemsEndpoint = ConfigurationManager.FetchConfig("RabbitMq:Endpoints:ParkingLotGet");
        private readonly String rabbitDeleteQueueMessages = ConfigurationManager.FetchConfig("RabbitMq:Endoints:ParkingLotPurge");
        private readonly String rabbitUri = ConfigurationManager.FetchConfig("RabbitMq:Endpoints:RabbitInstance");
        private readonly int count = int.Parse(ConfigurationManager.FetchConfig("RabbitMq:Settings:Count"));
        private readonly String encoding = ConfigurationManager.FetchConfig("RabbitMq:Settings:Encoding");
        private readonly int truncate = int.Parse(ConfigurationManager.FetchConfig("RabbitMq:Settings:Truncate"));
        private readonly String ackmode = ConfigurationManager.FetchConfig("RabbitMq:Settings:Ackmode");
        public RabbitMessages GetMessages()
        {
            RabbitMessages rabbitMessages = new RabbitMessages();
            rabbitMessages.messages = new List<QueueMessage>();
            using (HttpClient httpClient = new HttpClient())
            {
                byte[] byteArray = Encoding.ASCII.GetBytes(String.Format("{0}:{1}", username, password));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                RabbitRequest rabbitRequest = new RabbitRequest(count, encoding, truncate, ackmode);
                HttpResponseMessage httpResponseMessage = httpClient.PostAsJsonAsync(rabbitGetQueueItemsEndpoint, rabbitRequest).Result;
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    String respData = httpResponseMessage.Content.ReadAsStringAsync().Result;
                    List<ParkingLotMessage> parkingLotMessages = JsonConvert.DeserializeObject<List<ParkingLotMessage>>(respData);
                    foreach (ParkingLotMessage parkingLotMessage in parkingLotMessages)
                    {
                        QueueMessage queueMessage = JsonConvert.DeserializeObject<QueueMessage>(parkingLotMessage.payload);
                        rabbitMessages.messages.Add(queueMessage);
                    }
                } 
                else
                {
                    throw new Exception(httpResponseMessage.StatusCode.ToString());
                }
            }
            return rabbitMessages;
        }
        /// <summary>
        /// We need to find the messageand move it to the mainqueue
        /// </summary>
        /// <param name="rabbitObject"></param>
        /// <param name="exchange"></param>
        /// <param name="route"></param>
        /// <returns></returns>
        public HttpResponseMessage QueueRabbitMessage(QueueMessage queueMessage)
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.UserName = username;
            factory.Password = password;
            factory.HostName = rabbitUri;
            using (IConnection conn = factory.CreateConnection())
            {
                IModel channel = conn.CreateModel();
                var properties = channel.CreateBasicProperties();
                properties.Persistent = false;
                Dictionary<string, object> dictionary = new Dictionary<string, object>();
                dictionary.Add("error-count", 0);
                properties.Headers = dictionary;
                channel.ExchangeDeclare(exchange, ExchangeType.Direct, true);
                channel.BasicPublish(exchange, routingKey, properties, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(queueMessage)));
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
        }
        public HttpResponseMessage QueueRabbitMessages(RabbitMessages rabbitMessage)
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.UserName = username;
            factory.Password = password;
            factory.HostName = rabbitUri;
            using (IConnection conn = factory.CreateConnection())
            {
                IModel channel = conn.CreateModel();
                var properties = channel.CreateBasicProperties();
                properties.Persistent = false;
                Dictionary<string, object> dictionary = new Dictionary<string, object>();
                dictionary.Add("error-count", 0);
                properties.Headers = dictionary;
                channel.ExchangeDeclare(exchange, ExchangeType.Direct, true);
                foreach(QueueMessage queueMessage in rabbitMessage.messages) { 
                    channel.BasicPublish(exchange, routingKey, properties, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(queueMessage)));
                }
            }
            return new HttpResponseMessage();
        }
        public QueueMessage DeQueueMessage(String id, String guid)
        {
            ConnectionFactory factory = new ConnectionFactory();
            bool found = false;
            QueueMessage queueMessage = new QueueMessage();
            factory.UserName = username;
            factory.Password = password;
            factory.HostName = rabbitUri;
            using (IConnection conn = factory.CreateConnection())
            {
                IModel channel = conn.CreateModel();
                channel.ExchangeDeclare(parkingLotExchange, ExchangeType.Direct, true);
                for (BasicGetResult result; (result = channel.BasicGet(parkingLotQueue, false)) != null;)
                {
                    byte[] body = result.Body;
                    queueMessage = JsonConvert.DeserializeObject<QueueMessage>(System.Text.Encoding.UTF8.GetString(body, 0, body.Length));
                    if (queueMessage.eventId == id && queueMessage.guid == guid)
                    {
                        //Ack the message to dequeue
                        channel.BasicAck(result.DeliveryTag, false);
                        found = true;
                        //Exit the loop
                        break;
                    }
                }

            }
            if (found)
            {
                return queueMessage;
            }
            else
            {
                return null;
            }

            
        }
        public void DeQueueMessages(RabbitMessages rabbitMessage)
        {
            ConnectionFactory factory = new ConnectionFactory();
            QueueMessage queueMessage = new QueueMessage();
            factory.UserName = username;
            factory.Password = password;
            factory.HostName = rabbitUri;
            using (IConnection conn = factory.CreateConnection())
            {
                IModel channel = conn.CreateModel();
                channel.ExchangeDeclare(parkingLotExchange, ExchangeType.Direct, true);
                for (BasicGetResult result; (result = channel.BasicGet(parkingLotQueue, false)) != null;)
                {
                    byte[] body = result.Body;
                    queueMessage = JsonConvert.DeserializeObject<QueueMessage>(System.Text.Encoding.UTF8.GetString(body, 0, body.Length));
                    if (rabbitMessage.messages.Find(qm => qm.eventId == queueMessage.eventId && qm.guid == queueMessage.guid) != null)
                    {
                        //Ack the message to dequeue
                        channel.BasicAck(result.DeliveryTag, false);
                    }
                }
            }
        }
        public HttpResponseMessage PurgeQueue()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                byte[] byteArray = Encoding.ASCII.GetBytes(String.Format("{0}:{1}", username, password));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                return httpClient.DeleteAsync(rabbitDeleteQueueMessages).Result;
            }
        }
    }
}
