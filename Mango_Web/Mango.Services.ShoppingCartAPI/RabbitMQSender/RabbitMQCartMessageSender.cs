using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace Mango.Services.ShoppingCartAPI.RabbitMQSender
{
    public class RabbitMQCartMessageSender : IRabbitMQCartMessageSender
    {
        private readonly string _hostname;
        private readonly string _password;
        private readonly string _username;

        private IConnection _connection;
        private const string ExchangeName = "PublishSubcribePaymentUpdate_Exchange";

        public RabbitMQCartMessageSender()
        {
            _hostname = "localhost";
            _password = "guest";
            _username = "guest";
        }
        public void SendMessage(string message, string queueName)
        {
            //Normal
            //if (ConnectionExist())
            //{
            //    using var channel = _connection.CreateModel();
            //    channel.QueueDeclare(queue: queueName, false, false, false, arguments: null);
            //    var json = JsonConvert.SerializeObject(message);
            //    var body = Encoding.UTF8.GetBytes(json);
            //    channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
            //}

            //Fanout
            if (ConnectionExist())
            {
                using var channel = _connection.CreateModel();
                channel.ExchangeDeclare(ExchangeName, ExchangeType.Fanout, durable: false);
                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);
                channel.BasicPublish(exchange: ExchangeName, "", basicProperties: null, body: body);
            }
        }

        private void CreateConnection()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _hostname,
                Password = _password,
                UserName = _username
            };
            _connection = factory.CreateConnection();
        }

        private bool ConnectionExist()
        {
            if(_connection != null)
            {
                return true;
            }
            CreateConnection();
            return true;
        }
    }
}
