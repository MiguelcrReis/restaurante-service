using ItemService.EventProcessor;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace ItemService.RabbitMqClient;

public class RabbitMqSubscribrer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly string _nomeDaFila;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IProcessaEvento _processaEvento;

    public RabbitMqSubscribrer(IConfiguration configuration, IProcessaEvento processaEvento)
    {
        _configuration = configuration;
        _connection = new ConnectionFactory()
        {
            HostName = _configuration["RabbitMqHost"],
            Port = Int32.Parse(_configuration["RabbitMqPort"])
        }.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(exchange: "trigger", type: ExchangeType.Fanout);
        _nomeDaFila = _channel.QueueDeclare().QueueName;
        _channel.QueueBind(queue: _nomeDaFila, exchange: "trigger", routingKey: "");
        _processaEvento = processaEvento;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        EventingBasicConsumer? consumidor = new EventingBasicConsumer(_channel);

        consumidor.Received += (ModuleHandle, ea) =>
        {
            ReadOnlyMemory<byte> body = ea.Body;
            string? mensagem = Encoding.UTF8.GetString(body.ToArray());
            _processaEvento.Processa(mensagem);
        };

        // Informa que foi consumido a fila (autoAck) 
        _channel.BasicConsume(queue: _nomeDaFila, autoAck: true, consumer: consumidor);
        return Task.CompletedTask;
    }
}
