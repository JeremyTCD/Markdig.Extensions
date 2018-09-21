﻿using Jering.Markdig.Extensions.FlexiBlocks.FlexiAlertBlocks;
using Jering.Markdig.Extensions.FlexiBlocks.FlexiIncludeBlocks;
using Jering.Markdig.Extensions.FlexiBlocks.FlexiOptionsBlocks;
using Markdig;
using Markdig.Parsers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Jering.Markdig.Extensions.FlexiBlocks.Tests.FlexiIncludeBlocks
{
    // As far as integration tests for FlexiIncludeBlocks go, success cases are covered in FlexiIncludeBlockSpecs, so here we just test for exceptions.
    public class FlexiIncludeBlocksIntegrationTests : IClassFixture<FlexiIncludeBlocksEndToEndTestsFixture>
    {
        private readonly FlexiIncludeBlocksEndToEndTestsFixture _fixture;

        public FlexiIncludeBlocksIntegrationTests(FlexiIncludeBlocksEndToEndTestsFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [MemberData(nameof(FlexiIncludeBlocks_ThrowsFlexiIncludeBlocksExceptionIfACycleIsFound_Data))]
        public void FlexiIncludeBlocks_ThrowsFlexiIncludeBlocksExceptionIfACycleIsFound(string dummyEntryMarkdown, int dummyEntryOffendingFIBLineNum,
            string dummyMarkdown1, int dummyMarkdown1OffendingFIBLineNum,
            string dummyMarkdown2, int dummyMarkdown2OffendingFIBLineNum,
            string dummyMarkdown3,
            string expectedCycleDescription)
        {
            // Arrange
            File.WriteAllText(Path.Combine(_fixture.TempDirectory, $"{nameof(dummyMarkdown1)}.md"), dummyMarkdown1);
            File.WriteAllText(Path.Combine(_fixture.TempDirectory, $"{nameof(dummyMarkdown2)}.md"), dummyMarkdown2);
            File.WriteAllText(Path.Combine(_fixture.TempDirectory, $"{nameof(dummyMarkdown3)}.md"), dummyMarkdown3);

            // Need to dispose of services between tests so that DiskCacheService's in memory cache doesn't affect results
            var services = new ServiceCollection();
            services.AddFlexiBlocks();
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            using ((IDisposable)serviceProvider)
            {
                var dummyMarkdownPipelineBuilder = new MarkdownPipelineBuilder();
                dummyMarkdownPipelineBuilder.UseFlexiIncludeBlocks(serviceProvider: serviceProvider);
                FlexiIncludeBlocksExtensionOptions dummyExtensionOptions = serviceProvider.GetRequiredService<IOptions<FlexiIncludeBlocksExtensionOptions>>().Value;
                dummyExtensionOptions.DefaultBlockOptions = new FlexiIncludeBlockOptions(baseUri: _fixture.TempDirectory + "/");
                MarkdownPipeline dummyMarkdownPipeline = dummyMarkdownPipelineBuilder.Build();

                // Act and assert
                FlexiBlocksException result = Assert.Throws<FlexiBlocksException>(() => MarkdownParser.Parse(dummyEntryMarkdown, dummyMarkdownPipeline));
                Assert.Equal(string.Format(Strings.FlexiBlocksException_InvalidFlexiBlock, nameof(FlexiIncludeBlock), dummyEntryOffendingFIBLineNum, 0,
                        string.Format(Strings.FlexiBlocksException_FlexiIncludeBlocks_ExceptionOccurredWhileProcessingSource,
                            dummyExtensionOptions.DefaultBlockOptions.NormalizedSourceUri + $"{nameof(dummyMarkdown1)}.md")),
                    result.Message);
                Assert.Equal(string.Format(Strings.FlexiBlocksException_InvalidFlexiBlock, nameof(FlexiIncludeBlock), dummyMarkdown1OffendingFIBLineNum, 0,
                        string.Format(Strings.FlexiBlocksException_FlexiIncludeBlocks_ExceptionOccurredWhileProcessingSource,
                            dummyExtensionOptions.DefaultBlockOptions.NormalizedSourceUri + $"{nameof(dummyMarkdown2)}.md")),
                    result.InnerException.Message);
                Assert.Equal(string.Format(Strings.FlexiBlocksException_InvalidFlexiBlock, nameof(FlexiIncludeBlock), dummyMarkdown2OffendingFIBLineNum, 0,
                        string.Format(Strings.FlexiBlocksException_FlexiIncludeBlocks_ExceptionOccurredWhileProcessingSource,
                            dummyExtensionOptions.DefaultBlockOptions.NormalizedSourceUri + $"{nameof(dummyMarkdown1)}.md")),
                    result.InnerException.InnerException.Message);
                Assert.Equal(string.Format(Strings.FlexiBlocksException_InvalidFlexiBlock, nameof(FlexiIncludeBlock), dummyMarkdown1OffendingFIBLineNum, 0,
                        Strings.FlexiBlocksException_FlexiIncludeBlocks_ExceptionOccurredWhileProcessingBlock),
                    result.InnerException.InnerException.InnerException.Message);
                Assert.Equal(string.Format(Strings.FlexiBlocksException_FlexiIncludeBlocks_CycleFound, expectedCycleDescription),
                    result.InnerException.InnerException.InnerException.InnerException.Message,
                    ignoreLineEndingDifferences: true);
            }
        }

        public static IEnumerable<object[]> FlexiIncludeBlocks_ThrowsFlexiIncludeBlocksExceptionIfACycleIsFound_Data()
        {
            return new object[][]
            {
                // Basic circular include
                new object[]
                {
                    @"+{
""type"": ""markdown"",
""sourceUri"": ""./dummyMarkdown1.md""
}",
                    1,
                    @"+{
""type"": ""markdown"",
""sourceUri"": ""./dummyMarkdown2.md""
}",
                    1,
                    @"+{
""type"": ""markdown"",
""sourceUri"": ""./dummyMarkdown1.md""
}",
                    1,
                    null,
                    @"Source URI: ./dummyMarkdown1.md, Line: 1 >
Source URI: ./dummyMarkdown2.md, Line: 1 >
Source URI: ./dummyMarkdown1.md, Line: 1"
                },
                // Valid includes don't affect identification of circular includes
                new object[]
                {
                    @"+{
""type"": ""markdown"",
""sourceUri"": ""./dummyMarkdown1.md"",
""clippings"": [{""startLineNumber"": 2, ""endLineNumber"": 2}]
}

+{
""type"": ""markdown"",
""sourceUri"": ""./dummyMarkdown1.md""
}",
                    7,
                    @"+{
""type"": ""markdown"",
""sourceUri"": ""./dummyMarkdown3.md""
}

+{
""type"": ""markdown"",
""sourceUri"": ""./dummyMarkdown2.md""
}",
                    6,
                    @"+{
""type"": ""Code"",
""sourceUri"": ""./dummyMarkdown1.md""
}

+{
""type"": ""markdown"",
""sourceUri"": ""./dummyMarkdown1.md""
}",
                    6,
                    "This is a line",
                    @"Source URI: ./dummyMarkdown1.md, Line: 6 >
Source URI: ./dummyMarkdown2.md, Line: 6 >
Source URI: ./dummyMarkdown1.md, Line: 6"
                },
                // Circular includes that uses clippings are caught
                new object[]
                {
                    @"+{
""type"": ""markdown"",
""sourceUri"": ""./dummyMarkdown1.md"",
""clippings"": [{""startLineNumber"": 2, ""endLineNumber"": 2}]
}

+{
""type"": ""markdown"",
""sourceUri"": ""./dummyMarkdown1.md"",
""clippings"": [{""startLineNumber"": 6, ""endLineNumber"": -1}]
}",
                    7,
                    @"+{
""type"": ""markdown"",
""sourceUri"": ""./dummyMarkdown3.md""
}

+{
""type"": ""markdown"",
""sourceUri"": ""./dummyMarkdown2.md"",
""clippings"": [{""startLineNumber"": 6, ""endLineNumber"": -1}]
}",
                    6,
                    @"+{
""type"": ""Code"",
""sourceUri"": ""./dummyMarkdown1.md""
}

+{
""type"": ""markdown"",
""sourceUri"": ""./dummyMarkdown1.md""
}",
                    6,
                    "This is a line",
                    @"Source URI: ./dummyMarkdown1.md, Line: 6 >
Source URI: ./dummyMarkdown2.md, Line: 6 >
Source URI: ./dummyMarkdown1.md, Line: 6"
                }
            };
        }

        // This test is similar to the theory above. The thing is that, messages differ for before/after content.
        // The exception chain is stupidly long. It is a cycle, and we need the context for each FlexiIncludeBlock, but 
        // some kind of simplification should be attempted if time permits.
        [Fact]
        public void FlexiIncludeBlocks_ThrowsFlexiIncludeBlocksExceptionIfACycleThatPassesThroughBeforeOrAfterContentIsFound()
        {
            // Arrange
            const string dummyEntryMarkdown = @"+{
    ""type"": ""markdown"",
    ""sourceUri"": ""./dummyMarkdown1.md""
}";
            const int dummyEntryOffendingFIBLineNum = 1;
            const string dummyMarkdown1 = @"+{
    ""type"": ""markdown"",
    ""sourceUri"": ""./dummyMarkdown3.md"",
    ""clippings"": [{
                        ""beforeContent"": ""This is a line.
+{
                            \""type\"": \""markdown\"",
                            \""sourceUri\"": \""./dummyMarkdown2.md\""
                        }""
                    }]
}";
            const int dummyMarkdown1OffendingFIBLineNum = 1;
            const string dummyMarkdown2 = @"+{
    ""type"": ""markdown"",
    ""sourceUri"": ""./dummyMarkdown3.md"",
    ""clippings"": [{
                        ""afterContent"": ""+{
                            \""type\"": \""markdown\"",
                            \""sourceUri\"": \""./dummyMarkdown1.md\""
                        }""
                    }]
}";
            const int dummyMarkdown2OffendingFIBLineNum = 2;
            const string dummyMarkdown3 = "This is a line.";
            const string expectedCycleDescription = @"Source URI: ./dummyMarkdown1.md, Line: 1 >
BeforeContent, Line: 2 >
Source URI: ./dummyMarkdown2.md, Line: 1 >
AfterContent, Line: 1 >
Source URI: ./dummyMarkdown1.md, Line: 1";
            File.WriteAllText(Path.Combine(_fixture.TempDirectory, $"{nameof(dummyMarkdown1)}.md"), dummyMarkdown1);
            File.WriteAllText(Path.Combine(_fixture.TempDirectory, $"{nameof(dummyMarkdown2)}.md"), dummyMarkdown2);
            File.WriteAllText(Path.Combine(_fixture.TempDirectory, $"{nameof(dummyMarkdown3)}.md"), dummyMarkdown3);

            // Need to dispose of services between tests so that DiskCacheService's in memory cache doesn't affect results
            var services = new ServiceCollection();
            services.AddFlexiBlocks();
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            using ((IDisposable)serviceProvider)
            {
                var dummyMarkdownPipelineBuilder = new MarkdownPipelineBuilder();
                dummyMarkdownPipelineBuilder.UseFlexiIncludeBlocks(serviceProvider: serviceProvider);
                FlexiIncludeBlocksExtensionOptions dummyExtensionOptions = serviceProvider.GetRequiredService<IOptions<FlexiIncludeBlocksExtensionOptions>>().Value;
                dummyExtensionOptions.DefaultBlockOptions = new FlexiIncludeBlockOptions(baseUri: _fixture.TempDirectory + "/");
                MarkdownPipeline dummyMarkdownPipeline = dummyMarkdownPipelineBuilder.Build();

                // Act and assert
                FlexiBlocksException result = Assert.Throws<FlexiBlocksException>(() => MarkdownParser.Parse(dummyEntryMarkdown, dummyMarkdownPipeline));
                Assert.Equal(string.Format(Strings.FlexiBlocksException_InvalidFlexiBlock, nameof(FlexiIncludeBlock), dummyEntryOffendingFIBLineNum, 0,
                        string.Format(Strings.FlexiBlocksException_FlexiIncludeBlocks_ExceptionOccurredWhileProcessingSource,
                            dummyExtensionOptions.DefaultBlockOptions.NormalizedSourceUri + $"{nameof(dummyMarkdown1)}.md")),
                    result.Message);
                Assert.Equal(string.Format(Strings.FlexiBlocksException_InvalidFlexiBlock, nameof(FlexiIncludeBlock), dummyMarkdown1OffendingFIBLineNum, 0,
                        string.Format(Strings.FlexiBlocksException_FlexiIncludeBlocks_ExceptionOccurredWhileProcessingContent, nameof(ClippingProcessingStage.BeforeContent))),
                    result.InnerException.Message);
                Assert.Equal(string.Format(Strings.FlexiBlocksException_InvalidFlexiBlock, nameof(FlexiIncludeBlock), dummyMarkdown2OffendingFIBLineNum, 0,
                        string.Format(Strings.FlexiBlocksException_FlexiIncludeBlocks_ExceptionOccurredWhileProcessingSource,
                            dummyExtensionOptions.DefaultBlockOptions.NormalizedSourceUri + $"{nameof(dummyMarkdown2)}.md")),
                    result.InnerException.InnerException.Message);
                Assert.Equal(string.Format(Strings.FlexiBlocksException_InvalidFlexiBlock, nameof(FlexiIncludeBlock), dummyMarkdown1OffendingFIBLineNum, 0,
                        string.Format(Strings.FlexiBlocksException_FlexiIncludeBlocks_ExceptionOccurredWhileProcessingContent, nameof(ClippingProcessingStage.AfterContent))),
                    result.InnerException.InnerException.InnerException.Message);
                Assert.Equal(string.Format(Strings.FlexiBlocksException_InvalidFlexiBlock, nameof(FlexiIncludeBlock), dummyMarkdown1OffendingFIBLineNum, 0,
                        string.Format(Strings.FlexiBlocksException_FlexiIncludeBlocks_ExceptionOccurredWhileProcessingSource,
                            dummyExtensionOptions.DefaultBlockOptions.NormalizedSourceUri + $"{nameof(dummyMarkdown1)}.md")),
                    result.InnerException.InnerException.InnerException.InnerException.Message);
                Assert.Equal(string.Format(Strings.FlexiBlocksException_InvalidFlexiBlock, nameof(FlexiIncludeBlock), dummyMarkdown1OffendingFIBLineNum, 0,
                        Strings.FlexiBlocksException_FlexiIncludeBlocks_ExceptionOccurredWhileProcessingBlock),
                    result.InnerException.InnerException.InnerException.InnerException.InnerException.Message);
                Assert.Equal(string.Format(Strings.FlexiBlocksException_FlexiIncludeBlocks_CycleFound, expectedCycleDescription),
                    result.InnerException.InnerException.InnerException.InnerException.InnerException.InnerException.Message,
                    ignoreLineEndingDifferences: true);
            }
        }

        [Fact]
        public void FlexiIncludeBlocks_ThrowsFlexiIncludeBlocksExceptionIfAnIncludedSourceHasInvalidBlocks()
        {
            // Arrange
            const string dummyClassFormat = "dummy-{0}-{1}";
            const string dummyEntryMarkdown = @"+{
    ""type"": ""markdown"",
    ""sourceUri"": ""./dummyMarkdown1.md""
}";
            string dummyMarkdown1 = $@"@{{
    ""classFormat"": ""{dummyClassFormat}""
}}
! This is a FlexiAlertBlock.
";
            File.WriteAllText(Path.Combine(_fixture.TempDirectory, $"{nameof(dummyMarkdown1)}.md"), dummyMarkdown1);

            var dummyFlexiIncludeBlocksExtensionOptions = new FlexiIncludeBlocksExtensionOptions
            {
                DefaultBlockOptions = new FlexiIncludeBlockOptions(baseUri: _fixture.TempDirectory + "/")
            };
            var dummyMarkdownPipelineBuilder = new MarkdownPipelineBuilder();
            dummyMarkdownPipelineBuilder.
                UseFlexiIncludeBlocks(dummyFlexiIncludeBlocksExtensionOptions).
                UseFlexiAlertBlocks().
                UseFlexiOptionsBlocks();
            MarkdownPipeline dummyMarkdownPipeline = dummyMarkdownPipelineBuilder.Build();

            // Act and assert
            FlexiBlocksException result = Assert.Throws<FlexiBlocksException>(() => MarkdownParser.Parse(dummyEntryMarkdown, dummyMarkdownPipeline));
            // From bottom to top, this is the exception chain:
            // FormatException > FlexiBlocksException for invalid option > FlexiBlocksException for invalid FlexiOptionsBlock > FlexiBlocksException for invalid FlexiIncludeBlock
            Assert.Equal(string.Format(Strings.FlexiBlocksException_InvalidFlexiBlock, nameof(FlexiIncludeBlock), 1, 0,
                    string.Format(Strings.FlexiBlocksException_FlexiIncludeBlocks_ExceptionOccurredWhileProcessingSource,
                        dummyFlexiIncludeBlocksExtensionOptions.DefaultBlockOptions.NormalizedSourceUri + $"{nameof(dummyMarkdown1)}.md")),
                result.Message);
            Assert.Equal(string.Format(Strings.FlexiBlocksException_InvalidFlexiBlock, nameof(FlexiOptionsBlock), 1, 0, Strings.FlexiBlocksException_ExceptionOccurredWhileProcessingABlock),
                result.InnerException.Message);
            Assert.Equal(string.Format(Strings.FlexiBlocksException_OptionIsAnInvalidFormat, nameof(FlexiAlertBlockOptions.ClassFormat), dummyClassFormat),
                result.InnerException.InnerException.Message);
            Assert.IsType<FormatException>(result.InnerException.InnerException.InnerException);
        }
    }

    public class FlexiIncludeBlocksEndToEndTestsFixture : IDisposable
    {
        public string TempDirectory { get; } = Path.Combine(Path.GetTempPath(), nameof(FlexiIncludeBlocksIntegrationTests)); // Dummy file for creating dummy file streams

        public FlexiIncludeBlocksEndToEndTestsFixture()
        {
            TryDeleteDirectory();
            Directory.CreateDirectory(TempDirectory);
        }

        private void TryDeleteDirectory()
        {
            try
            {
                Directory.Delete(TempDirectory, true);
            }
            catch
            {
                // Do nothing
            }
        }

        public void Dispose()
        {
            TryDeleteDirectory();
        }
    }
}
