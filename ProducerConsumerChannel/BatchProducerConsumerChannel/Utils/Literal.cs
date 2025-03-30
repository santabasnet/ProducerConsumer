using System.CodeDom.Compiler;
using System.Net.Mime;

namespace BatchProducerConsumerChannel.Utils;

public abstract class Literal
{
    public static readonly int MaxBufferSize = 32;
    public static readonly int MaxProducers = 8;
    public static readonly int MaxConsumers = 8;
    public static readonly int TopicBound = 2;
}

/*
 * Singleton Implementation of Random number generator with the given bound value.
 */
public class BoundGenerator
{
    private readonly Random _random;
    private static BoundGenerator? _instance;

    private BoundGenerator()
    {
        _random = new Random();
    }

    public static BoundGenerator New()
    {
        return _instance ??= new BoundGenerator();
    }

    public int Next(int bound)
    {
        return _random.Next(0, bound) + 1;
    }
}

public class TextGenerator(int size, String text)
{
    public String? Generate()
    {
        return Enumerable.Range(0, size - 1).Select(index =>
        {
            int chIndex = BoundGenerator.New().Next(text.Length);
            return text[chIndex];
        }).ToString();
    }
}

public class EmailAddressGenerator
{
    private static readonly String chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
    private static readonly int emailLength = 16; 
    public static String Next()
    {
        var prefix = new TextGenerator(emailLength, chars).Generate(); 
        return $"{prefix}.@gmail.com";
    }
}

public class ContentGenerator
{
    private static readonly String chars = "ABCDEFGHIJKLMNO PQRSTUVWXYZabcdefghi jklmnopqrstuvwxyz #$@";
    private static readonly int contentLength = 64;

    public static String Next()
    {
        return new TextGenerator(contentLength, chars).Generate() + "";
    }
}

public class PhoneNumberGenerator
{
    private static readonly String chars = "0123456789";
    private static readonly int phoneLength = 9;

    public static String Next()
    {
        var prefix = new TextGenerator(phoneLength, chars).Generate();
        return $"9{prefix}";
    }
}
