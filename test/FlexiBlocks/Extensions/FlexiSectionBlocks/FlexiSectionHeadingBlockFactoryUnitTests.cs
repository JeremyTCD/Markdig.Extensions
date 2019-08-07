﻿using Jering.Markdig.Extensions.FlexiBlocks.FlexiSectionBlocks;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Jering.Markdig.Extensions.FlexiBlocks.Tests.FlexiSectionBlocks
{
    public class FlexiSectionHeadingBlockFactoryUnitTests
    {
        private readonly MockRepository _mockRepository = new MockRepository(MockBehavior.Default) { DefaultValue = DefaultValue.Mock };

        [Fact]
        public void Create_CreateFlexiSectionHeadingBlock()
        {
            // Arrange
            const int dummyColumn = 12;
            const int dummyLineIndex = 5;
            var dummyLine = new StringSlice("dummyLine", 4, 8);
            BlockProcessor dummyBlockProcessor = MarkdigTypesFactory.CreateBlockProcessor();
            dummyBlockProcessor.Line = dummyLine;
            dummyBlockProcessor.LineIndex = dummyLineIndex;
            dummyBlockProcessor.Column = dummyColumn;
            Mock<BlockParser> dummyBlockParser = _mockRepository.Create<BlockParser>();
            Mock<IFlexiSectionBlockOptions> dummyFlexiSectionBlockOptions = _mockRepository.Create<IFlexiSectionBlockOptions>();
            Mock<FlexiSectionHeadingBlockFactory> mockTestSubject = CreateMockFlexiSectionHeadingBlockFactory();
            mockTestSubject.CallBase = true;
            mockTestSubject.Setup(t => t.SetupIDGenerationAndAutoLinking(It.IsAny<FlexiSectionHeadingBlock>(),
                dummyFlexiSectionBlockOptions.Object,
                dummyBlockProcessor));

            // Act
            FlexiSectionHeadingBlock result = mockTestSubject.Object.Create(dummyBlockProcessor, dummyFlexiSectionBlockOptions.Object, dummyBlockParser.Object);

            // Assert
            _mockRepository.VerifyAll();
            Assert.Equal(dummyColumn, result.Column);
            Assert.Equal(dummyLine.Start, result.Span.Start);
            Assert.Equal(dummyLine.End, result.Span.End);
            Assert.Equal(dummyLineIndex, result.Line);
            Assert.Equal(dummyLine.ToString(), result.Lines.ToString());
        }

        [Fact]
        public void SetupIDGenerationAndAutoLinking_AddsFlexiSectionHeadingBlockToAutoLinkableFlexiSectionHeadingBlocksIfGenerateIDAndAutoLinkableAreTrue()
        {
            // Arrange
            BlockProcessor dummyBlockProcessor = MarkdigTypesFactory.CreateBlockProcessor();
            FlexiSectionHeadingBlock dummyFlexiSectionHeadingBlock = CreateFlexiSectionHeadingBlock();
            Mock<IFlexiSectionBlockOptions> mockFlexiSectionBlockOptions = _mockRepository.Create<IFlexiSectionBlockOptions>();
            mockFlexiSectionBlockOptions.Setup(f => f.GenerateID).Returns(true);
            mockFlexiSectionBlockOptions.Setup(f => f.AutoLinkable).Returns(true);
            var dummyAutoLinkableFlexiSectionHeadingBlocks = new List<FlexiSectionHeadingBlock>();
            Mock<FlexiSectionHeadingBlockFactory> mockTestSubject = CreateMockFlexiSectionHeadingBlockFactory();
            mockTestSubject.CallBase = true;
            mockTestSubject.Setup(t => t.GetOrCreateAutoLinkableFlexiSectionHeadingBlocks(dummyBlockProcessor.Document)).Returns(dummyAutoLinkableFlexiSectionHeadingBlocks);

            // Act
            mockTestSubject.Object.SetupIDGenerationAndAutoLinking(dummyFlexiSectionHeadingBlock, mockFlexiSectionBlockOptions.Object, dummyBlockProcessor);

            // Assert
            _mockRepository.VerifyAll();
            Assert.Single(dummyAutoLinkableFlexiSectionHeadingBlocks);
            Assert.Same(dummyFlexiSectionHeadingBlock, dummyAutoLinkableFlexiSectionHeadingBlocks[0]);
        }

        [Fact]
        public void GetOrCreateAutoLinkableFlexiSectionHeadingBlocks_GetsAutoLinkableFlexiSectionHeadingBlocksIfItAlreadyExists()
        {
            // Arrange
            var dummyAutoLinkableFlexiSectionHeadingBlocks = new List<FlexiSectionHeadingBlock>();
            var dummyMarkdownDocument = new MarkdownDocument();
            dummyMarkdownDocument.SetData(FlexiSectionHeadingBlockFactory.AUTO_LINKABLE_FLEXI_SECTION_HEADING_BLOCKS, dummyAutoLinkableFlexiSectionHeadingBlocks);
            FlexiSectionHeadingBlockFactory testSubject = CreateFlexiSectionHeadingBlockFactory();

            // Act
            List<FlexiSectionHeadingBlock> result = testSubject.GetOrCreateAutoLinkableFlexiSectionHeadingBlocks(dummyMarkdownDocument);

            // Assert
            Assert.Same(dummyAutoLinkableFlexiSectionHeadingBlocks, result);
        }

        [Fact]
        public void GetOrCreateAutoLinkableFlexiSectionHeadingBlocks_CreatesAutoLinkableFlexiSectionHeadingBlocksIfItDoesNotAlreadyExist()
        {
            // Arrange
            var dummyMarkdownDocument = new MarkdownDocument();
            FlexiSectionHeadingBlockFactory testSubject = CreateFlexiSectionHeadingBlockFactory();

            // Act
            List<FlexiSectionHeadingBlock> result = testSubject.GetOrCreateAutoLinkableFlexiSectionHeadingBlocks(dummyMarkdownDocument);

            // Assert
            Assert.NotNull(result);
            Assert.Same(result, dummyMarkdownDocument.GetData(FlexiSectionHeadingBlockFactory.AUTO_LINKABLE_FLEXI_SECTION_HEADING_BLOCKS));
        }

        [Theory]
        [MemberData(nameof(DocumentOnProcessInlinesBegin_CreatesAndAddsASectionLinkReferenceDefinitionForEachAutoLinkableFlexiSectionHeadingBlock_Data))]
        public void DocumentOnProcessInlinesBegin_CreatesAndAddsASectionLinkReferenceDefinitionForEachAutoLinkableFlexiSectionHeadingBlock(List<FlexiSectionHeadingBlock> dummyAutoLinkableFlexiSectionHeadingBlocks,
            List<(string expectedLabel, string expectedUrl)> expectedLabelAndUrls)
        {
            // Arrange
            InlineProcessor dummyInlineProcessor = MarkdigTypesFactory.CreateInlineProcessor();
            dummyInlineProcessor.Document.SetData(FlexiSectionHeadingBlockFactory.AUTO_LINKABLE_FLEXI_SECTION_HEADING_BLOCKS, dummyAutoLinkableFlexiSectionHeadingBlocks);
            FlexiSectionHeadingBlockFactory testSubject = CreateFlexiSectionHeadingBlockFactory();

            // Act
            testSubject.DocumentOnProcessInlinesBegin(dummyInlineProcessor, null);

            // Assert
            Dictionary<string, LinkReferenceDefinition> linkReferenceDefinitions =
                (dummyInlineProcessor.Document.GetData(typeof(LinkReferenceDefinitionGroup)) as LinkReferenceDefinitionGroup).Links;
            Assert.Equal(expectedLabelAndUrls.Count, linkReferenceDefinitions.Count); // One expectedLabel + expectedUrl pair for each FlexiSectionHeadingBlock
            for (int i = 0; i < expectedLabelAndUrls.Count; i++)
            {
                FlexiSectionHeadingBlock dummyAutoLinkableFlexiSectionHeadingBlock = dummyAutoLinkableFlexiSectionHeadingBlocks[i];

                Assert.False(dummyAutoLinkableFlexiSectionHeadingBlock.ProcessInlines);

                (string expectedLabel, string expectedID) = expectedLabelAndUrls[i];

                Assert.Equal(expectedID, dummyAutoLinkableFlexiSectionHeadingBlock.GeneratedID);
                Assert.True(linkReferenceDefinitions.ContainsKey(expectedLabel));
                LinkReferenceDefinition linkReferenceDefinition = linkReferenceDefinitions[expectedLabel];
                Assert.Equal(expectedLabel, linkReferenceDefinition.Label);
                Assert.Equal($"#{expectedID}", linkReferenceDefinition.Url);
            }
        }

        public static IEnumerable<object[]> DocumentOnProcessInlinesBegin_CreatesAndAddsASectionLinkReferenceDefinitionForEachAutoLinkableFlexiSectionHeadingBlock_Data()
        {
            return new object[][]
            {
                // Standard
                new object[]{
                    new List<FlexiSectionHeadingBlock>{new FlexiSectionHeadingBlock(default){Lines = new StringLineGroup("Dummy Content")}},
                    new List<(string expectedLabel, string expectedUrl)>{ ("Dummy Content", "dummy-content") }
                },
                // Whitespace lines
                new object[]{
                    new List<FlexiSectionHeadingBlock>{new FlexiSectionHeadingBlock(default){Lines = new StringLineGroup(" ")}},
                    new List<(string expectedLabel, string expectedUrl)>{ (" ", "section") }
                },
                // Empty string lines
                new object[]{
                    new List<FlexiSectionHeadingBlock>{new FlexiSectionHeadingBlock(default){Lines = new StringLineGroup(string.Empty)}},
                    new List<(string expectedLabel, string expectedUrl)>{ (string.Empty, "section") }
                },
                // Duplicate IDs
                new object[]{
                    new List<FlexiSectionHeadingBlock>{
                        new FlexiSectionHeadingBlock(default){Lines = new StringLineGroup("Dummy Content")},
                        new FlexiSectionHeadingBlock(default){Lines = new StringLineGroup("Dummy Content")},
                        new FlexiSectionHeadingBlock(default){Lines = new StringLineGroup("Dummy Content")},
                        new FlexiSectionHeadingBlock(default){Lines = new StringLineGroup("Dummy Content 1")}
                    },
                    new List<(string expectedLabel, string expectedUrl)>{
                        ("Dummy Content", "dummy-content"),
                        ("Dummy Content 1", "dummy-content-1"),
                        ("Dummy Content 2", "dummy-content-2"),
                        ("Dummy Content 1 1", "dummy-content-1-1")
                    }
                }
            };
        }

        [Fact]
        public void DocumentOnProcessInlinesBegin_DoesNotReplaceExistingLinkReferenceDefinitions()
        {
            // Arrange
            const string dummyLabel = "dummyLabel";
            InlineProcessor dummyInlineProcessor = MarkdigTypesFactory.CreateInlineProcessor();
            FlexiSectionHeadingBlock dummyFlexiSectionHeadingBlock = CreateFlexiSectionHeadingBlock();
            dummyFlexiSectionHeadingBlock.Lines = new StringLineGroup(dummyLabel);
            var dummyLinkReferenceDefinition = new LinkReferenceDefinition();
            dummyInlineProcessor.Document.SetData(FlexiSectionHeadingBlockFactory.AUTO_LINKABLE_FLEXI_SECTION_HEADING_BLOCKS,
                new List<FlexiSectionHeadingBlock> { dummyFlexiSectionHeadingBlock });
            dummyInlineProcessor.Document.SetLinkReferenceDefinition(dummyLabel, dummyLinkReferenceDefinition);
            FlexiSectionHeadingBlockFactory testSubject = CreateFlexiSectionHeadingBlockFactory();

            // Act
            testSubject.DocumentOnProcessInlinesBegin(dummyInlineProcessor, null);

            // Assert
            Dictionary<string, LinkReferenceDefinition> linkReferenceDefinitions =
                (dummyInlineProcessor.Document.GetData(typeof(LinkReferenceDefinitionGroup)) as LinkReferenceDefinitionGroup).Links;
            Assert.Single(linkReferenceDefinitions);
            Assert.Same(dummyLinkReferenceDefinition, linkReferenceDefinitions.Values.First()); // Doesn't get replaced
        }

        [Fact]
        public void GetOrCreateGeneratedIDs_GetsGeneratedIDsIfItAlreadyExists()
        {
            // Arrange
            var dummyGeneratedIDs = new Dictionary<string, int>();
            var dummyMarkdownDocument = new MarkdownDocument();
            dummyMarkdownDocument.SetData(FlexiSectionHeadingBlockFactory.GENERATED_IDS, dummyGeneratedIDs);
            FlexiSectionHeadingBlockFactory testSubject = CreateFlexiSectionHeadingBlockFactory();

            // Act
            Dictionary<string, int> result = testSubject.GetOrCreateGeneratedIDs(dummyMarkdownDocument);

            // Assert
            Assert.Same(dummyGeneratedIDs, result);
        }

        [Fact]
        public void GetOrCreateGeneratedIDs_CreatesGeneratedIDsIfItDoesNotAlreadyExist()
        {
            // Arrange
            var dummyMarkdownDocument = new MarkdownDocument();
            FlexiSectionHeadingBlockFactory testSubject = CreateFlexiSectionHeadingBlockFactory();

            // Act
            Dictionary<string, int> result = testSubject.GetOrCreateGeneratedIDs(dummyMarkdownDocument);

            // Assert
            Assert.NotNull(result);
            Assert.Same(result, dummyMarkdownDocument.GetData(FlexiSectionHeadingBlockFactory.GENERATED_IDS));
        }

        private static FlexiSectionHeadingBlock CreateFlexiSectionHeadingBlock(BlockParser blockParser = default,
            string generatedID = default)
        {
            return new FlexiSectionHeadingBlock(blockParser)
            {
                Lines = new StringLineGroup(1), // Xunit calls ToString on all theory arguments, if a LeafBlock doesn't have Lines, ToString throws.
                GeneratedID = generatedID
            };
        }

        private FlexiSectionHeadingBlockFactory CreateFlexiSectionHeadingBlockFactory()
        {
            return new FlexiSectionHeadingBlockFactory();
        }

        private Mock<FlexiSectionHeadingBlockFactory> CreateMockFlexiSectionHeadingBlockFactory()
        {
            return _mockRepository.Create<FlexiSectionHeadingBlockFactory>();
        }
    }
}
