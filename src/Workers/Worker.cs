using MassTransit;

namespace Workers
{

    public class Worker(IBusControl busControl) : BackgroundService
    {
        private readonly IBusControl _busControl = busControl;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _busControl.StartAsync(stoppingToken);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(5000, stoppingToken); // Keep the service alive
                    Console.WriteLine("Listering to Message");
                }
            }
            finally
            {
                await _busControl.StopAsync(stoppingToken);
            }
        }
    }

}
