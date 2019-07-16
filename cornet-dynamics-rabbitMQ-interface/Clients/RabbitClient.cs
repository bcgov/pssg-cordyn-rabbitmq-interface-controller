using cornet_dynamics_rabbitMQ_interface.Objects;
using cornet_dynamics_rabbitMQ_interface.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace cornet_dynamics_rabbitMQ_interface.Clients
{
    public class RabbitClient
    {
        private readonly String routingKey = ConfigurationManager.FetchConfig("RabbitMq:MainQueue:Route");
        private readonly String exchange = ConfigurationManager.FetchConfig("RabbitMq:MainQueue:Exchange");
        private readonly String username = ConfigurationManager.FetchConfig("RabbitMq:Authentication:Username");
        private readonly String password = ConfigurationManager.FetchConfig("RabbitMq:Authentication:Password");
        //Parking Lot settings
        private readonly String parkingLotQueue = ConfigurationManager.FetchConfig("RabbitMq:ParkingQueue:Queue");
        private readonly String parkingLotExchange = ConfigurationManager.FetchConfig("RabbitMq:ParkingQueue:Exchange");

        private readonly String rabbitGetQueueItemsEndpoint = ConfigurationManager.FetchConfig("RabbitMq:Endpoints:ParkingLotGet");
        private readonly String rabbitDeleteQueueMessages = ConfigurationManager.FetchConfig("RabbitMq:Endpoints:ParkingLotPurge");
        private readonly String rabbitUri = ConfigurationManager.FetchConfig("RabbitMq:Endpoints:RabbitInstance");
        private readonly int count = int.Parse(ConfigurationManager.FetchConfig("RabbitMq:Settings:Count"));
        private readonly String encoding = ConfigurationManager.FetchConfig("RabbitMq:Settings:Encoding");
        private readonly int truncate = int.Parse(ConfigurationManager.FetchConfig("RabbitMq:Settings:Truncate"));
        private readonly String ackmode = ConfigurationManager.FetchConfig("RabbitMq:Settings:Ackmode");
        /// <summary>
        /// Api call to rabbitmq
        /// </summary>
        /// <returns>
        /// Return all messages in the queue
        /// </returns>
        public RabbitMessages GetMessages()
        {
            RabbitMessages rabbitMessages = new RabbitMessages();
            rabbitMessages.messages = new List<ParkingLotMessage>();
            using (HttpClient httpClient = new HttpClient())
            {
                byte[] byteArray = Encoding.ASCII.GetBytes(String.Format("{0}:{1}", username, password));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                RabbitRequest rabbitRequest = new RabbitRequest(count, encoding, truncate, ackmode);
                HttpResponseMessage httpResponseMessage = httpClient.PostAsJsonAsync(rabbitGetQueueItemsEndpoint, rabbitRequest).Result;
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    String respData = httpResponseMessage.Content.ReadAsStringAsync().Result;
                    rabbitMessages.messages = JsonConvert.DeserializeObject<List<ParkingLotMessage>>(respData); 
                } 
                else
                {
                    throw new Exception(httpResponseMessage.StatusCode.ToString());
                }
            }
            return rabbitMessages;
        }
        /// <summary>
        /// Place message onto queue
        /// </summary>
        /// <param name="queueMessage">message</param>
        /// <returns>
        /// Return a ok or bad request
        /// </returns>
        public HttpResponseMessage QueueRabbitMessage(RabbitMessages rabbitMessages)
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.UserName = username;
            factory.Password = password;
            factory.HostName = rabbitUri;
            try {
                using (IConnection conn = factory.CreateConnection())
                {
                    IModel channel = conn.CreateModel();
                    channel.ExchangeDeclare(exchange, ExchangeType.Direct, true);
                    foreach (ParkingLotMessage parkingLotMessage in rabbitMessages.messages)
                    {
                        var properties = channel.CreateBasicProperties();
                        properties.Persistent = false;
                        Dictionary<string, object> dictionary = new Dictionary<string, object>();
                        dictionary.Add("x-death", 0);
                        dictionary.Add("x-request-id", parkingLotMessage.properties.headerItems.requestId);
                        dictionary.Add("date", parkingLotMessage.properties.headerItems.date);
                        properties.Headers = dictionary;
                        channel.BasicPublish(exchange, routingKey, properties, Encoding.UTF8.GetBytes(parkingLotMessage.payload));
                    }
                }
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
            }
        }
        /// <summary>
        /// Re-queue a list of messages
        /// </summary>
        /// <param name="rabbitMessage">rabbit message object containing a list of messages</param>
        /// <returns></returns>
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

                channel.ExchangeDeclare(exchange, ExchangeType.Direct, true);
                foreach(ParkingLotMessage parkingLotMessage in rabbitMessage.messages) {
                    JObject jsonObject = JsonConvert.DeserializeObject<JObject>(parkingLotMessage.payload);
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    Dictionary<string, object> dictionary = new Dictionary<string, object>();
                    dictionary.Add("x-death", 0);
                    dictionary.Add("x-request-id", parkingLotMessage.properties.headerItems.requestId);
                    dictionary.Add("date", parkingLotMessage.properties.headerItems.date);
                    properties.Headers = dictionary;
                    channel.BasicPublish(exchange, routingKey, properties, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jsonObject)));
                }
            }
            return new HttpResponseMessage();
        }
        /// <summary>
        /// De queue a single message
        /// </summary>
        /// <param name="id">Primary key of the message</param>
        /// <param name="guid">generated guid of the message</param>
        /// <returns>
        /// The message object that has been de-queued
        /// </returns>
        public RabbitMessages DeQueueMessage(String id)
        {
            ConnectionFactory factory = new ConnectionFactory();
            RabbitMessages rabbitMessages = new RabbitMessages();
            rabbitMessages.messages = new List<ParkingLotMessage>();
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
                    if (System.Text.Encoding.UTF8.GetString((byte[])result.BasicProperties.Headers["x-request-id"]) == id)
                    {
                        //Ack the message to dequeue
                        channel.BasicAck(result.DeliveryTag, false);
                        ParkingLotMessage parkingLotMessage = new ParkingLotMessage();
                        parkingLotMessage.payload = System.Text.Encoding.UTF8.GetString(body, 0, body.Length);
                        parkingLotMessage.properties = new PropertiesHeader {
                            headerItems = new HeaderItems {
                                retryCount = 0,
                                requestId = System.Text.Encoding.UTF8.GetString((byte[])result.BasicProperties.Headers["x-request-id"]),
                                date = System.Text.Encoding.UTF8.GetString((byte[])result.BasicProperties.Headers["date"])
                            }
                        };

                        rabbitMessages.messages.Add(parkingLotMessage);
                    }
                }

            }
            return rabbitMessages;
        }
        /// <summary>
        /// De-queue a list of messages 
        /// </summary>
        /// <param name="rabbitMessage">
        /// list of messages
        /// </param>
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

                    //In a future refactor we will move the id to the header of the message. So we can compare the headers id values and then we are not dependent on the message itslef
                    if (rabbitMessage.messages.Find(pm => pm.properties.headerItems.requestId == System.Text.Encoding.UTF8.GetString((byte[])result.BasicProperties.Headers["x-request-id"])) != null)
                    {
                        //Ack the message to dequeue
                        channel.BasicAck(result.DeliveryTag, false);
                    }
                }
            }
        }
        /// <summary>
        /// Call api to remove all messages from the queue
        /// </summary>
        /// <returns>
        /// Returns response from api call
        /// </returns>
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
