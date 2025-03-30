using System.Net.Sockets;
using System.Text.Json;
using BatchProducerConsumerChannel.Service;

namespace BatchProducerConsumerChannel.Model;

public abstract class IMessage
{
    public abstract Guid Id { get; }
    public abstract String Address { get; }
    public abstract String Content { get; }
    
    public abstract void Send();
    public abstract String ToJson();
}

public class EmailMessage(Guid id, String email, String content) : IMessage
{
    public override Guid Id => id;
    public override String Address => email;
    public override String Content => content;

    public override void Send()
    {
        throw new NotImplementedException();
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

public enum MessageType
{
    Email,
    Sms
}

public class Topic(Guid id, MessageType type, IMessage message)
{
    public Guid Id => id;
    public MessageType Type => type;
    public IMessage Message => message;
    public String ToJson()
    {
        return JsonSerializer.Serialize(this);
    }
}
