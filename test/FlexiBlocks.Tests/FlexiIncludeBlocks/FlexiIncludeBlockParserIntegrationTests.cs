﻿using Jering.Markdig.Extensions.FlexiBlocks.FlexiIncludeBlocks;
using Markdig;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Xunit;

namespace Jering.Markdig.Extensions.FlexiBlocks.Tests.FlexiIncludeBlocks
{
    public class FlexiIncludeBlockParserIntegrationTests : IClassFixture<FlexiIncludeBlockParserIntegrationTestsFixture>
    {
        private readonly FlexiIncludeBlockParserIntegrationTestsFixture _fixture;

        public FlexiIncludeBlockParserIntegrationTests(FlexiIncludeBlockParserIntegrationTestsFixture fixture)
        {
            _fixture = fixture;
        }

        // TODO ProcessText
        // TODO Close
        // TODO TryContinue
        // TODO TryOpen

        [Theory]
        [MemberData(nameof(FlexiIncludeBlockParser_ThrowsIfACircularIncludeIsFound_Data))]
        public void FlexiIncludeBlockParser_ThrowsIfACircularIncludeIsFound(string dummyEntryMarkdown, string dummyMarkdown1, string dummyMarkdown2, string dummyMarkdown3, 
            string expectedCycleDescription)
        {
            // Arrange
            File.WriteAllText(Path.Combine(_fixture.TempDirectory, $"{nameof(dummyMarkdown1)}.md"), dummyMarkdown1);
            File.WriteAllText(Path.Combine(_fixture.TempDirectory, $"{nameof(dummyMarkdown2)}.md"), dummyMarkdown2);
            File.WriteAllText(Path.Combine(_fixture.TempDirectory, $"{nameof(dummyMarkdown3)}.md"), dummyMarkdown3);

            // Need to dispose of services between tests so that FileCacheService's in memory cache doesn't affect results
            var services = new ServiceCollection();
            services.AddFlexiBlocks();
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            using ((IDisposable)serviceProvider)
            {
                var dummyMarkdownPipelineBuilder = new MarkdownPipelineBuilder();
                dummyMarkdownPipelineBuilder.Extensions.Add(serviceProvider.GetRequiredService<FlexiIncludeBlocksExtension>());
                FlexiIncludeBlocksExtensionOptions dummyExtensionOptions = serviceProvider.GetRequiredService<IOptions<FlexiIncludeBlocksExtensionOptions>>().Value;
                dummyExtensionOptions.SourceBaseUri = _fixture.TempDirectory + "/";
                MarkdownPipeline dummyMarkdownPipeline = dummyMarkdownPipelineBuilder.Build();

                // Act and assert
                InvalidOperationException result = Assert.Throws<InvalidOperationException>(() => MarkdownParser.Parse(dummyEntryMarkdown, dummyMarkdownPipeline));
                Assert.Equal(string.Format(Strings.InvalidOperationException_CycleInIncludes, expectedCycleDescription), result.Message, ignoreLineEndingDifferences: true);
            }
        }

        public static IEnumerable<object[]> FlexiIncludeBlockParser_ThrowsIfACircularIncludeIsFound_Data()
        {
            return new object[][]
            {
                // Basic circular include
                new object[]
                {
                    @"+{
    ""contentType"": ""Markdown"",
    ""source"": ""./dummyMarkdown1.md""
}",
                    @"+{
    ""contentType"": ""Markdown"",
    ""source"": ""./dummyMarkdown2.md""
}",
                    @"+{
    ""contentType"": ""Markdown"",
    ""source"": ""./dummyMarkdown1.md""
}",
                    null,
                    @"Source: ./dummyMarkdown1.md, Line: 1 >
Source: ./dummyMarkdown2.md, Line: 1 >
Source: ./dummyMarkdown1.md, Line: 1"
                },
                // Valid includes don't affect identification of circular includes
                new object[]
                {
                    @"+{
    ""contentType"": ""Markdown"",
    ""source"": ""./dummyMarkdown1.md"",
    ""clippingAreas"": [{""startLineNumber"": 2, ""endLineNumber"": 2}]
}

+{
    ""contentType"": ""Markdown"",
    ""source"": ""./dummyMarkdown1.md""
}",
                    @"+{
    ""contentType"": ""Markdown"",
    ""source"": ""./dummyMarkdown3.md""
}

+{
    ""contentType"": ""Markdown"",
    ""source"": ""./dummyMarkdown2.md""
}",
                    @"+{
    ""contentType"": ""Code"",
    ""source"": ""./dummyMarkdown1.md""
}

+{
    ""contentType"": ""Markdown"",
    ""source"": ""./dummyMarkdown1.md""
}",
                    "This is a line",
                    @"Source: ./dummyMarkdown1.md, Line: 6 >
Source: ./dummyMarkdown2.md, Line: 6 >
Source: ./dummyMarkdown1.md, Line: 6"
                },
                // Circular includes that uses clipping areas are caught
                new object[]
                {
                    @"+{
    ""contentType"": ""Markdown"",
    ""source"": ""./dummyMarkdown1.md"",
    ""clippingAreas"": [{""startLineNumber"": 2, ""endLineNumber"": 2}]
}

+{
    ""contentType"": ""Markdown"",
    ""source"": ""./dummyMarkdown1.md"",
    ""clippingAreas"": [{""startLineNumber"": 6, ""endLineNumber"": -1}]
}",
                    @"+{
    ""contentType"": ""Markdown"",
    ""source"": ""./dummyMarkdown3.md""
}

+{
    ""contentType"": ""Markdown"",
    ""source"": ""./dummyMarkdown2.md"",
    ""clippingAreas"": [{""startLineNumber"": 6, ""endLineNumber"": -1}]
}",
                    @"+{
    ""contentType"": ""Code"",
    ""source"": ""./dummyMarkdown1.md""
}

+{
    ""contentType"": ""Markdown"",
    ""source"": ""./dummyMarkdown1.md""
}",
                    "This is a line",
                    @"Source: ./dummyMarkdown1.md, Line: 6 >
Source: ./dummyMarkdown2.md, Line: 6 >
Source: ./dummyMarkdown1.md, Line: 6"
                }
            };
        }

        [Theory]
        [MemberData(nameof(DedentAndCollapseLeadingWhiteSpace_DedentsAndCollapsesLeadingWhiteSpace_Data))]
        public void DedentAndCollapseLeadingWhiteSpace_DedentsAndCollapsesLeadingWhiteSpace(string dummyLine, int dummyDedentLength, int dummyCollapseRatio, string expectedResult)
        {
            // Arrange
            var dummyStringSlice = new StringSlice(dummyLine);
            FlexiIncludeBlockParser testSubject = CreateFlexiIncludBlockParser();

            // Act
            testSubject.DedentAndCollapseLeadingWhiteSpace(ref dummyStringSlice, dummyDedentLength, dummyCollapseRatio);

            // Assert
            Assert.Equal(expectedResult, dummyStringSlice.ToString());
        }

        public static IEnumerable<object[]> DedentAndCollapseLeadingWhiteSpace_DedentsAndCollapsesLeadingWhiteSpace_Data()
        {
            return new object[][]
            {
                new object[]{"    dummyLine", 2, 1, "  dummyLine"}, // Dedent
                new object[]{"  dummyLine", 2, 2, "dummyLine" }, // Dedent till there is no leading white space
                new object[]{"  ", 3, 0, "" }, // Dedent till string is empty
                new object[]{"    dummyLine", 0, 2, "  dummyLine"}, // Collapse
                new object[]{"     dummyLine", 0, 2, "  dummyLine"}, // Collapse with number of leading white spaces indivisible by collapse ratio
                new object[]{" dummyLine", 0, 3, "dummyLine"}, // Collapse till there is no leading white space
                new object[]{"     dummyLine", 3, 2, " dummyLine"}, // Dedent and collapse
                new object[]{"dummyLine", 2, 2, "dummyLine" }, // Do nothing to line with no leading white space
            };
        }

        [Fact]
        public void ReplaceFlexIncludeBlock_WrapsContentInACodeBlockIfIncludeOptionsContentTypeIsCode()
        {
            // Arrange
            var dummyContent = new ReadOnlyCollection<string>(new string[] { "dummy", "content" });
            BlockProcessor dummyBlockProcessor = CreateBlockProcessor();
            var dummyFlexiIncludeBlock = new FlexiIncludeBlock(null);
            dummyBlockProcessor.Document.Add(dummyFlexiIncludeBlock); // Set document as parent of flexi include block
            var dummyIncludeOptions = new IncludeOptions("dummySource"); // Default content type is Code
            FlexiIncludeBlockParser testSubject = CreateFlexiIncludBlockParser();

            // Act
            testSubject.ReplaceFlexiIncludeBlock(dummyBlockProcessor, dummyFlexiIncludeBlock, dummyContent, dummyIncludeOptions);

            // Assert
            Assert.Single(dummyBlockProcessor.Document);
            var resultCodeBlock = dummyBlockProcessor.Document[0] as FencedCodeBlock;
            Assert.NotNull(resultCodeBlock);
            Assert.Equal(string.Join("\n", dummyContent), resultCodeBlock.Lines.ToString());
        }

        [Fact]
        public void ReplaceFlexIncludeBlock_AddsBeforeAndAfterTextIfTheyAreNotNull()
        {
            // Arrange
            var dummyContent = new ReadOnlyCollection<string>(new string[] { "dummy", "content" });
            BlockProcessor dummyBlockProcessor = CreateBlockProcessor();
            var dummyFlexiIncludeBlock = new FlexiIncludeBlock(null);
            dummyBlockProcessor.Document.Add(dummyFlexiIncludeBlock); // Set document as parent of flexi include block
            const string dummyBeforeText = "# dummy before";
            const string dummyAfterText = "> dummy\n > after";
            var dummyClippingArea = new ClippingArea(1, -1, beforeText: dummyBeforeText, afterText: dummyAfterText);
            var dummyClippingAreas = new ClippingArea[] { dummyClippingArea };
            var dummyIncludeOptions = new IncludeOptions("dummySource", ContentType.Markdown, clippingAreas: dummyClippingAreas);
            FlexiIncludeBlockParser testSubject = CreateFlexiIncludBlockParser();

            // Act
            testSubject.ReplaceFlexiIncludeBlock(dummyBlockProcessor, dummyFlexiIncludeBlock, dummyContent, dummyIncludeOptions);

            // Assert
            Assert.Equal(3, dummyBlockProcessor.Document.Count);
            // First block (from before text)
            var resultHeadingBlock = dummyBlockProcessor.Document[0] as HeadingBlock;
            Assert.NotNull(resultHeadingBlock);
            Assert.Equal("dummy before", resultHeadingBlock.Lines.ToString());
            // Second block (from content)
            var resultParagraphBlock1 = dummyBlockProcessor.Document[1] as ParagraphBlock;
            Assert.NotNull(resultParagraphBlock1);
            Assert.Equal(string.Join("\n", dummyContent), resultParagraphBlock1.Lines.ToString());
            // Third block (from after text)
            var resultQuoteBlock = dummyBlockProcessor.Document[2] as QuoteBlock;
            Assert.NotNull(resultQuoteBlock);
            var resultParagraphBlock2 = resultQuoteBlock[0] as ParagraphBlock;
            Assert.NotNull(resultParagraphBlock2);
            Assert.Equal("dummy\nafter", resultParagraphBlock2.Lines.ToString());
        }

        [Fact]
        public void ReplaceFlexIncludeBlock_ThrowsInvalidOperationExceptionIfNoLineContainsStartLineSubStringOfAClippingArea()
        {
            // Arrange
            var dummyContent = new ReadOnlyCollection<string>(new string[] { "dummy", "content" });
            const string dummyStartLineSubstring = "dummyStartLineSubstring";
            BlockProcessor dummyBlockProcessor = CreateBlockProcessor();
            var dummyFlexiIncludeBlock = new FlexiIncludeBlock(null);
            var dummyClippingArea = new ClippingArea(0, -1, dummyStartLineSubstring);
            var dummyIncludeOptions = new IncludeOptions("dummySource", clippingAreas: new ClippingArea[] { dummyClippingArea });
            FlexiIncludeBlockParser testSubject = CreateFlexiIncludBlockParser();

            // Act and assert
            InvalidOperationException result = Assert.Throws<InvalidOperationException>(() => testSubject.ReplaceFlexiIncludeBlock(dummyBlockProcessor, dummyFlexiIncludeBlock, dummyContent, dummyIncludeOptions));
            Assert.Equal(string.Format(Strings.InvalidOperationException_InvalidClippingAreaNoLineContainsStartLineSubstring, dummyStartLineSubstring),
                result.Message);
        }

        [Fact]
        public void ReplaceFlexIncludeBlock_ThrowsInvalidOperationExceptionIfNoLineContainsEndLineSubStringOfAClippingArea()
        {
            var dummyContent = new ReadOnlyCollection<string>(new string[] { "dummy", "content" });
            const string dummyEndLineSubstring = "dummyEndLineSubstring";
            BlockProcessor dummyBlockProcessor = CreateBlockProcessor();
            var dummyFlexiIncludeBlock = new FlexiIncludeBlock(null);
            var dummyClippingArea = new ClippingArea(1, 0, endDemarcationLineSubstring: dummyEndLineSubstring);
            var dummyIncludeOptions = new IncludeOptions("dummySource", clippingAreas: new ClippingArea[] { dummyClippingArea });
            FlexiIncludeBlockParser testSubject = CreateFlexiIncludBlockParser();

            // Act and assert
            InvalidOperationException result = Assert.Throws<InvalidOperationException>(() => testSubject.ReplaceFlexiIncludeBlock(dummyBlockProcessor, dummyFlexiIncludeBlock, dummyContent, dummyIncludeOptions));
            Assert.Equal(string.Format(Strings.InvalidOperationException_InvalidClippingAreaNoLineContainsEndLineSubstring, dummyEndLineSubstring),
                result.Message);
        }

        [Theory]
        [MemberData(nameof(ReplaceFlexiIncludeBlock_ClipsLinesAccordingToStartAndEndLineNumbersAndSubstrings_Data))]
        public void ReplaceFlexiIncludeBlock_ClipsLinesAccordingToStartAndEndLineNumbersAndSubstrings(SerializableWrapper<ClippingArea[]> dummyClippingAreasWrapper, string[] expectedResult)
        {
            // Arrange
            var dummyContent = new ReadOnlyCollection<string>(new string[] { "line1", "line2", "line3", "line4", "line5" });
            BlockProcessor dummyBlockProcessor = CreateBlockProcessor();
            var dummyFlexiIncludeBlock = new FlexiIncludeBlock(null);
            dummyBlockProcessor.Document.Add(dummyFlexiIncludeBlock); // Set document as parent of flexi include block
            var dummyIncludeOptions = new IncludeOptions("dummySource", ContentType.Markdown, clippingAreas: dummyClippingAreasWrapper.Value);
            FlexiIncludeBlockParser testSubject = CreateFlexiIncludBlockParser();

            // Act
            testSubject.ReplaceFlexiIncludeBlock(dummyBlockProcessor, dummyFlexiIncludeBlock, dummyContent, dummyIncludeOptions);

            // Assert
            Assert.Single(dummyBlockProcessor.Document);
            var resultParagraphBlock = dummyBlockProcessor.Document[0] as ParagraphBlock;
            Assert.NotNull(resultParagraphBlock);
            Assert.Equal(string.Join("\n", expectedResult), resultParagraphBlock.Lines.ToString());
        }

        public static IEnumerable<object[]> ReplaceFlexiIncludeBlock_ClipsLinesAccordingToStartAndEndLineNumbersAndSubstrings_Data()
        {
            return new object[][]
            {
                // Single clipping area that includes all lines using line numbers
                new object[]
                {
                    new SerializableWrapper<ClippingArea[]>(new ClippingArea[] { new ClippingArea(1, 5)}),
                    new string[] { "line1", "line2", "line3", "line4", "line5" }
                },
                // Single clipping area that includes all lines using -1 as end line number
                new object[]
                {
                    new SerializableWrapper<ClippingArea[]>(new ClippingArea[] { new ClippingArea(1, -1)}),
                    new string[] { "line1", "line2", "line3", "line4", "line5" }
                },
                // Single clipping area that includes a single line using line numbers
                new object[]
                {
                    new SerializableWrapper<ClippingArea[]>(new ClippingArea[] { new ClippingArea(3, 3)}),
                    new string[] { "line3" }
                },
                // Single clipping area that includes a single line using substrings
                new object[]
                {
                    new SerializableWrapper<ClippingArea[]>(new ClippingArea[] { new ClippingArea(0, 0, startDemarcationLineSubstring: "line2", endDemarcationLineSubstring: "line4")}),
                    new string[] { "line3" }
                },
                // Single clipping area that includes a single line using line numbers and substrings
                new object[]
                {
                    new SerializableWrapper<ClippingArea[]>(new ClippingArea[] { new ClippingArea(0, 5, startDemarcationLineSubstring: "line4")}),
                    new string[] { "line5" }
                },
                // Single clipping area that includes a subset of lines using line numbers
                new object[]
                {
                    new SerializableWrapper<ClippingArea[]>(new ClippingArea[] { new ClippingArea(2, 4)}),
                    new string[] { "line2", "line3", "line4" }
                },
                // Single clipping area that includes a subset of lines using substrings
                new object[]
                {
                    new SerializableWrapper<ClippingArea[]>(new ClippingArea[] { new ClippingArea(0, 0, startDemarcationLineSubstring: "line1", endDemarcationLineSubstring: "line5")}),
                    new string[] { "line2", "line3", "line4" }
                },
                // Single clipping area that includes a subset of lines using line numbers and substrings
                new object[]
                {
                    new SerializableWrapper<ClippingArea[]>(new ClippingArea[] { new ClippingArea(2, 0, endDemarcationLineSubstring: "line5")}),
                    new string[] { "line2", "line3", "line4" }
                },
                // Multiple clipping areas that do not overlap
                new object[]
                {
                    new SerializableWrapper<ClippingArea[]>(new ClippingArea[] {
                        new ClippingArea(1, 2),
                        new ClippingArea(0, 0, startDemarcationLineSubstring: "line2", endDemarcationLineSubstring: "line5"),
                        new ClippingArea(0, 5, startDemarcationLineSubstring: "line4")
                    }),
                    new string[] { "line1", "line2", "line3", "line4", "line5" }
                },
                // Multiple clipping areas that overlap
                new object[]
                {
                    new SerializableWrapper<ClippingArea[]>(new ClippingArea[] {
                        new ClippingArea(1, 3),
                        new ClippingArea(0, 0, startDemarcationLineSubstring: "line1", endDemarcationLineSubstring: "line5"),
                        new ClippingArea(0, 5, startDemarcationLineSubstring: "line3")
                    }),
                    new string[] { "line1", "line2", "line3", "line2", "line3", "line4", "line4", "line5" }
                },
            };
        }

        private FlexiIncludeBlockParser CreateFlexiIncludBlockParser(IOptions<FlexiIncludeBlocksExtensionOptions> extensionOptionsAccessor = null,
            IContentRetrievalService contentRetrievalService = null)
        {
            return new FlexiIncludeBlockParser(extensionOptionsAccessor, contentRetrievalService);
        }

        /// <summary>
        /// Create a default BlockProcessor. Markdig's defaults are specified in MarkdownPipelineBuilder, but aren't accessible through it.
        /// </summary>
        private BlockProcessor CreateBlockProcessor()
        {
            var stringBuilders = new StringBuilderCache();

            var parsers = new OrderedList<BlockParser>()
            {
                new ThematicBreakParser(),
                new HeadingBlockParser(),
                new QuoteBlockParser(),
                new ListBlockParser(),

                new HtmlBlockParser(),
                new FencedCodeBlockParser(),
                new IndentedCodeBlockParser(),
                new ParagraphBlockParser(),
            };

            var markdownDocument = new MarkdownDocument();

            return new BlockProcessor(stringBuilders, markdownDocument, new BlockParserList(parsers));
        }
    }
}