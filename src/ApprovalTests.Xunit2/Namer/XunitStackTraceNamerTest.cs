using System.IO;
using System.Runtime.CompilerServices;
using ApprovalTests;
using ApprovalUtilities.Utilities;
using System.Threading.Tasks;
using ApprovalTests.Core;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using ApprovalTests.Writers;
using Xunit;

namespace ApprovalTests.Xunit2.Namer
{
    using Namers;
    using Namers.StackTraceParsers;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using Xunit;

    public class XunitStackTraceNamerTest
    {
        [Fact]
        public async Task AsyncTestApprovalName()
        {
            var name = new UnitTestFrameworkNamer().Name;
            var sourcePath = new UnitTestFrameworkNamer().SourcePath;

            await AsyncStringResult();

            Assert.Equal("XunitStackTraceNamerTest.AsyncTestApprovalName", name);
            Assert.True(File.Exists($@"{sourcePath}\XunitStackTraceNamerTest.cs"));
        }

        [Fact]
        public async Task FullAsyncTest()
        {
            await AsyncStringResult();
            Approvals.Verify("Async");
        }

        [Fact]
        public async Task AsyncExtensionTestFails()
        {
            await AsyncStringResult().VerifyFails();
        }

        [Fact]
        public async Task AsyncExtensionTestPasses()
        {
            await AsyncStringResult().VerifyPasses();
        }

        [Fact]
        public async Task AsyncExtensionTestPassesWithScenario()
        {
            await AsyncStringResult().VerifyPasses("Scenario");
        }

        [Fact]
        public void TestApprovalName()
        {
            var name = new UnitTestFrameworkNamer().Name;
            Assert.Equal("XunitStackTraceNamerTest.TestApprovalName", name);
        }

        [InheritedFact]
        public void TestApprovalName_InheritedFact()
        {
            var name = new UnitTestFrameworkNamer().Name;
            Assert.Equal("XunitStackTraceNamerTest.TestApprovalName_InheritedFact", name);
        }

        [Fact]
        public void TestApprovalNamerFailureMessage()
        {
            var parser = new StackTraceParser();
            var exception = ExceptionUtilities.GetException(() => parser.Parse(new StackTrace(6)));

            Approvals.Verify(exception.Message);
        }

        private static async Task<string> AsyncStringResult()
        {
            await Task.Delay(10);
            return "Async with Delay";
        }
    }
}

public class InheritedFactAttribute : FactAttribute
{
}

public static class ApprovalExtensions
{
    public static async Task VerifyFails(this Task<string> textTask)
    {
        Approvals.Verify(await textTask);
    }

    public static async Task VerifyPasses(
        this Task<string> textTask,
        string scenarioName = null,
        [CallerFilePath] string filePath = null,
        [CallerMemberName] string memberName = null)
    {
        Approvals.Verify(
            WriterFactory.CreateTextWriter(await textTask),
            new ManualNamer(filePath, memberName, scenarioName),
            DiffReporter.INSTANCE);
    }
}

public class ManualNamer : IApprovalNamer
{
    private readonly string additionalInfo;
    private readonly string name;
    private readonly string filePath;

    public ManualNamer(string filePath, string name, string additionalInfo = null)
    {
        this.filePath = filePath;
        this.name = name;
        this.additionalInfo = additionalInfo;
    }

    public string SourcePath => Path.GetDirectoryName(filePath);

    public string Name => ApprovalResults.Scrub(additionalInfo != null ? $"{name}.{additionalInfo}" : name);
}
