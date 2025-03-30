## ProducerConsumer
Producer &amp; Consumer Pattern Implementation With Channel in C#. It's a concurrency design pattern where the producer component generally generates and writes the data. The consumer component reads and computes data asynchronously. I used bounded channel to buffer the message data in between. 
### Data Generation
The data I used here are of two types:
  a) SMS data 
  ```C#
    public class SmsMessage(Guid id, String phone, String content) : IMessage
    {
        public override Guid Id => id;
        public override String Address => phone;
        public override String Content => content;
    
        public override void Send()
        {
            return;
        }
    
        public override String ToJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }

  ```
  
  b) Email
  ```C#
   public class EmailMessage(Guid id, String email, String content) : IMessage
    {
        public override Guid Id => id;
        public override String Address => email;
        public override String Content => content;
    
        public override void Send()
        {
            return;
        }
    
        public override string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }
    
        public static EmailMessage Default()
        {
            return new EmailMessage(Guid.NewGuid(), String.Empty, string.Empty);
        }
    }

  ```
For the data generation, the random message generator is used and written using custom text generator function.
```C#
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
```
### Message Controller
The message controller creates the bounded channel with randomly defined channel size and uses it to execute the producer and consumer jobs.
```C#
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
```

### The Producer
```C#
  public class ProducerService(ChannelWriter<Topic> writer)
  {
      public async Task ProduceTopic(Topic topic, CancellationToken token = default)
      {
          await writer.WriteAsync(topic, token);
          await Task.Delay(500);
          ConsoleLogger.Print($"\nProduced topic: \n\t{topic.ToJson()}");
      }
  }

  //---
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

  //---
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
 ```

### The Consumer
```C#
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
  ///---
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
```
Finally, it runs all the tasks in parallel and ensures them to complete too. This pattern can be implmented in a distributed infrastructures using RabbitMQ(Cassandra), Kafka etc for large scale systems.

