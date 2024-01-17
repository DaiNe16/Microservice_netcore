using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Mango.Services.EmailAPI.Messaging
{
    public class RabbitMQConsumer : BackgroundService
    {
        private IConnection _connection;
        private IModel _channel;
        private const string ExchangeName = "PublishSubcribePaymentUpdate_Exchange";
        private string queueName = "";

        public RabbitMQConsumer()
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            //Queue
            //_channel.QueueDeclare("checkoutqueue", false, false, false, arguments: null);
            //Fanout
            _channel.ExchangeDeclare(exchange: ExchangeName, ExchangeType.Fanout);
            queueName = _channel.QueueDeclare().QueueName;
            _channel.QueueBind(queueName, ExchangeName, "");
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, e) =>
            {
                var content = Encoding.UTF8.GetString(e.Body.ToArray());
                HandleMessage(content).GetAwaiter().GetResult();
                _channel.BasicAck(e.DeliveryTag, false);
            };
            //Queue
            //_channel.BasicConsume("checkoutqueue", false, consumer);
            //Fanout
            _channel.BasicConsume(queueName, false, consumer);

            return Task.CompletedTask;
        }

        private async Task HandleMessage(string content)
        {
            Console.WriteLine(content);
        }
    }
}
