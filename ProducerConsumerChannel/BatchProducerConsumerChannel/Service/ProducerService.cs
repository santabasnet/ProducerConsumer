using BatchProducerConsumerChannel.Model;
using System.Threading.Channels;

namespace BatchProducerConsumerChannel.Service;

public class ProducerService(ChannelWriter<Topic> writer)
{
    public async Task ProduceTopic(Topic topic, CancellationToken token = default)
    {
        await writer.WriteAsync(topic, token);
        await Task.Delay(500);
        ConsoleLogger.Print($"\nProduced topic: \n\t{topic.ToJson()}");
    }
}
