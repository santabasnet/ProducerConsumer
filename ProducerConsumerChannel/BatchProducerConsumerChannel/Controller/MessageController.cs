namespace BatchProducerConsumerChannel.Controller;

using System.Threading.Channels;
using BatchProducerConsumerChannel.Model;
using BatchProducerConsumerChannel.Service;
using BatchProducerConsumerChannel.Utils;

public class MessageController
{
    private int GetProducerCount()
    {
        return BoundGenerator.New().Next(Literal.MaxProducers);
    }

    private int GetConsumerCount()
    {
        return BoundGenerator.New().Next(Literal.MaxConsumers);
    }

    private int GetChannelSizeCount(int pCount, int cCount)
    {
        var cntMax = pCount >= cCount ? cCount : pCount;
        var prefix = BoundGenerator.New().Next(Literal.MaxBufferSize);
        if (prefix > cntMax) return prefix;
        return prefix + cntMax;
    }

    private List<ProducerService> CollectProducer(
        Channel<Topic> channel,
        int producers
    )
    {
        return Enumerable
            .Range(0, producers)
            .Select(index => new ProducerService(channel.Writer))
            .ToList();
    }

    private List<Task> BeginConsumer(
        Channel<Topic> channel,
        int consumers,
        CancellationToken cancellation
    )
    {
        return Enumerable
            .Range(0, consumers)
            .Select(index => new ConsumerService(channel.Reader).ConsumeTopic(cancellation))
            .ToList();
    }

    private IMessage GetMessage(MessageType messageType) => messageType switch
    {
        MessageType.Email => new EmailMessage(
            id: Guid.NewGuid(),
            email: EmailAddressGenerator.Next(),
            content: ContentGenerator.Next()
        ),
        MessageType.Sms => new SmsMessage(
            id: Guid.NewGuid(),
            phone: PhoneNumberGenerator.Next(),
            content: ContentGenerator.Next()
        )
    };

    private Topic GetTopic()
    {
        var messageType = BoundGenerator.New().Next(Literal.TopicBound) switch
        {
            0 => MessageType.Email,
            _ => MessageType.Sms
        };

        return new Topic(Guid.NewGuid(), messageType, GetMessage(messageType));
    }

    private List<Task> BeginProducer(
        int batchSize,
        List<ProducerService> producers
    )
    {
        var factor = (batchSize % producers.Count) + 1;
        return Enumerable
            .Range(0, batchSize)
            .Select(index => producers[batchSize % factor].ProduceTopic(GetTopic()))
            .ToList();
    }

    private async Task Produce(
        Channel<Topic> channel,
        int noOfProducers,
        int batchSize,
        CancellationTokenSource source
    )
    {
        var producers = CollectProducer(channel, noOfProducers);
        var jobs = BeginProducer(batchSize, producers);
        await Task.WhenAll(jobs);
        channel.Writer.Complete();
        await channel.Reader.Completion;
        await source.CancelAsync();
        ConsoleLogger.Print("\nConsumption of topics finished successfully.");
    }

    public async Task Execute(CancellationTokenSource source)
    {
        var producerCount = GetProducerCount();
        var consumerCount = GetConsumerCount();
        var channelSize = GetChannelSizeCount(producerCount, consumerCount);

        ConsoleLogger.Print(
            $"Running {producerCount} producers, {consumerCount} consumers and channels size is {channelSize}."
        );
        var channel = Channel.CreateBounded<Topic>(channelSize);
        var tasksReader = new List<Task>(BeginConsumer(channel, consumerCount, source.Token))
        {
            Produce(channel, producerCount, channelSize, source)
        };
        await Task.WhenAll(tasksReader);
        channel.Reader.Completion.Wait();
        ConsoleLogger.Print("Execution finished successfully.");
    }
}
