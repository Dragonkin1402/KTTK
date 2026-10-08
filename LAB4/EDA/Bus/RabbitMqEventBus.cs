using System.Text;
using System.Text.Json;
using EDA.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EDA.Bus
{
    /// <summary>
    /// Event Channel dùng RabbitMQ.
    /// - 1 exchange kiểu Direct, routing key = tên class của event.
    /// - Mỗi Subscribe tạo 1 queue riêng (exclusive, auto-delete) bind vào routing key đó
    ///   => mọi subscriber đều nhận được 1 bản sao của event (publish/subscribe fan-out).
    /// - Event được serialize thành JSON.
    /// </summary>
    public class RabbitMqEventBus : IEventBus
    {
        private const string ExchangeName = "eda.events";

        private readonly IConnection _connection;
        private readonly IModel _publishChannel;
        private readonly List<IModel> _consumerChannels = new();
        private readonly object _publishLock = new();

        public RabbitMqEventBus(string host = "localhost", string user = "guest", string password = "guest")
        {
            var factory = new ConnectionFactory
            {
                HostName = host,
                UserName = user,
                Password = password
            };

            _connection = factory.CreateConnection("EDA-Lab4");
            _publishChannel = _connection.CreateModel();
            _publishChannel.ExchangeDeclare(ExchangeName, ExchangeType.Direct, durable: true);
        }

        public void Publish<T>(T @event) where T : IEvent
        {
            var routingKey = @event.GetType().Name;
            var body = JsonSerializer.SerializeToUtf8Bytes(@event, @event.GetType());

            // IModel không thread-safe -> khóa khi publish
            lock (_publishLock)
            {
                var props = _publishChannel.CreateBasicProperties();
                props.ContentType = "application/json";
                props.Persistent = true;
                _publishChannel.BasicPublish(ExchangeName, routingKey, props, body);
            }
        }

        public void Subscribe<T>(Action<T> handler) where T : IEvent
        {
            var routingKey = typeof(T).Name;

            // Mỗi subscription dùng 1 channel riêng
            var channel = _connection.CreateModel();
            channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct, durable: true);

            var queueName = channel.QueueDeclare(
                queue: "",
                durable: false,
                exclusive: true,
                autoDelete: true).QueueName;

            channel.QueueBind(queueName, ExchangeName, routingKey);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (_, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var evt = JsonSerializer.Deserialize<T>(json);
                    if (evt != null)
                        handler(evt);
                    channel.BasicAck(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RabbitMqEventBus] Lỗi xử lý {routingKey}: {ex.Message}");
                    channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                }
            };

            channel.BasicConsume(queueName, autoAck: false, consumer);
            _consumerChannels.Add(channel);
        }

        public void Dispose()
        {
            foreach (var ch in _consumerChannels) ch.Close();
            _publishChannel.Close();
            _connection.Close();
        }
    }
}
