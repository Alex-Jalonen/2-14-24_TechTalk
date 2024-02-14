using System.Threading.Tasks;
using NUnit.Framework;

public class AsyncoroutineTests
{
    private const int TIMEOUT_MS = 10000;

    [Test]
    [Timeout(TIMEOUT_MS)]
    public void AsyncoroutineTestsSimplePasses()
    {
        Assert.DoesNotThrow(() => TestAsyncSimplePasses().Wait(TIMEOUT_MS));
    }

    private async Task TestAsyncSimplePasses()
    {
        //TODO: This all probably needs to happen in game unit tests hmmmm....
    }
}

