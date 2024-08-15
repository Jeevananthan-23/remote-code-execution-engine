using MassTransit;
using Shared;

namespace Workers
{

    public class ApiBodyConsumer : IConsumer<ApiBody>
    {
        public async Task Consume(ConsumeContext<ApiBody> context)
        {
            var apiBody = context.Message;
            Console.WriteLine("Message recived");
            await CodeRunner.CreateFilesAsync(apiBody);
        }
    }

}
