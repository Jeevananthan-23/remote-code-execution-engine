using MassTransit;
using Workers;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        // Configure MassTransit with RabbitMQ
        services.AddMassTransit(x =>
        {
            x.AddConsumer<ApiBodyConsumer>(); // Register the consumer

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("amqp://guest:guest@rabbitmq:5672"); // Update with your RabbitMQ host if needed

                cfg.ReceiveEndpoint("apibody-queue", e =>
                {
                    e.ConfigureConsumer<ApiBodyConsumer>(context); // Configure the endpoint
                });
            });
        });

        // Register the background service
        // services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
