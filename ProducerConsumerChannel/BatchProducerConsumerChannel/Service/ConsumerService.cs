using BatchProducerConsumerChannel.Model;
using System.Threading.Channels;

namespace BatchProducerConsumerChannel.Service;

public class ConsumerService(ChannelReader<Topic> reader)
{
    public async Task ConsumeTopic(CancellationToken token)
    {
        ConsoleLogger.Print($"Consumer service starting at: {DateTime.Now}");
        try
        {
            await foreach (var topic in reader.ReadAllAsync(token))
            {
                ConsoleLogger.Print($"\nConsumed: \n\t{topic.ToJson()}");
                await Task.Delay(500, token);
            }
        }
        catch (Exception ex)
        {
            ConsoleLogger.Print($"\nConsumer service failed to consume topic: {ex.Message}");
        }
    }
}
