/* Producer-Consumer in a batch with topics running in channels.
 * Created By: Santa
 * 2025/03/28 21:17:09
 * https://github.com/santabasnet
 */

using BatchProducerConsumerChannel.Controller;
using BatchProducerConsumerChannel.Service;


Console.WriteLine("Welcome to ProducerConsumerChannel ...");
var source = new CancellationTokenSource();
try
{
    source.CancelAfter(5000);
    await new MessageController().Execute(source);
}
catch (Exception e)
{
    ConsoleLogger.Print("\nExecution was cancelled.\n" + e.ToString());
}
finally
{
    source.Dispose();
}
