using System.Diagnostics;
using System.Text;
using Cnblogs.Architecture.IntegrationTests;
using Xunit.Abstractions;
using Xunit.Sdk;

[assembly: TestFramework($"Cnblogs.Architecture.IntegrationTests.{nameof(IntegrationTestFramework)}", "Cnblogs.Architecture.IntegrationTests")]

namespace Cnblogs.Architecture.IntegrationTests;

public class IntegrationTestFramework : XunitTestFramework
{
    public IntegrationTestFramework(IMessageSink messageSink)
        : base(messageSink)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Trace.Listeners.Add(new ConsoleTraceListener());
    }
}
