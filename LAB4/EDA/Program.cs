using EDA.Bus;
using EDA.Services;

// Chọn Event Channel:  dotnet run                -> in-memory
//                      dotnet run -- rabbitmq    -> RabbitMQ (localhost:5672)
//                      hoặc biến môi trường EVENT_BUS=rabbitmq, RABBITMQ_HOST=...
var mode = (args.Length > 0 ? args[0] : Environment.GetEnvironmentVariable("EVENT_BUS") ?? "inmemory")
           .ToLowerInvariant();
var host = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";

Console.WriteLine($"=== Event channel: {mode} ===");

using IEventBus bus = mode == "rabbitmq"
    ? new RabbitMqEventBus(host)
    : new InMemoryEventBus();

// Subscriber phải được tạo TRƯỚC khi publish (để queue được bind sẵn)
var analyticsService = new AnalyticsService(bus);
var fraudService = new FraudDetectionService(bus);
var accountService = new AccountService(bus);

accountService.CreateAccount("TVD", "David");
accountService.CreateAccount("ABC", "Anna");

accountService.Deposit("TVD", 50000);
accountService.Withdraw("TVD", 10000);   // = ngưỡng -> bình thường
accountService.Withdraw("TVD", 15000);   // > 10000  -> Fraud cảnh báo
accountService.Transfer("TVD", "ABC", 3000);

// Với RabbitMQ, handler chạy bất đồng bộ -> chờ để thấy hết log
if (mode == "rabbitmq")
    Thread.Sleep(2000);
