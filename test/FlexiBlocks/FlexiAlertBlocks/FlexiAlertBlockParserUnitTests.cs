﻿using Jering.Markdig.Extensions.FlexiBlocks.FlexiAlertBlocks;
using Jering.Markdig.Extensions.FlexiBlocks.FlexiOptionsBlocks;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;
using Moq;
using System;
using Xunit;

namespace Jering.Markdig.Extensions.FlexiBlocks.Tests.FlexiAlertBlocks
{
    public class FlexiAlertBlockParserUnitTests
    {
        private readonly MockRepository _mockRepository = new MockRepository(MockBehavior.Default) { DefaultValue = DefaultValue.Mock };

        [Fact]
        public void Constructor_ThrowsArgumentNullExceptionIfExtensionOptionsIsNull()
        {
            // Act and assert
            Assert.Throws<ArgumentNullException>(() => new FlexiAlertBlockParser(_mockRepository.Create<IFlexiOptionsBlockService>().Object, null));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullExceptionIfFlexiOptionsBlockServiceIsNull()
        {
            // Act and assert
            Assert.Throws<ArgumentNullException>(() => new FlexiAlertBlockParser(null, new FlexiAlertBlocksExtensionOptions()));
        }

        [Fact]
        public void TryOpenFlexiBlock_ReturnsBlockStateNoneIfCurrentLineHasCodeIndent()
        {
            // Arrange
            BlockProcessor dummyBlockProcessor = MarkdigTypesFactory.CreateBlockProcessor();
            dummyBlockProcessor.Column = 4; // IsCodeIndent is an expression bodied member that derives its value from Column
            ExposedFlexiAlertBlockParser testSubject = CreateExposedFlexiAlertBlockParser();

            // Act
            BlockState result = testSubject.ExposedTryOpenFlexiBlock(dummyBlockProcessor);

            // Assert
            Assert.Equal(BlockState.None, result);
        }

        [Fact]
        public void TryOpenFlexiBlock_IfSuccessfulCreatesNewFlexiAlertBlockAndReturnsBlockStateContinue()
        {
            // Arrange
            const int dummyInitialColumn = 2;
            const int dummyInitialStart = 1;
            var dummyStringSlice = new StringSlice("dummyString") { Start = dummyInitialStart };
            BlockProcessor dummyBlockProcessor = MarkdigTypesFactory.CreateBlockProcessor();
            dummyBlockProcessor.Line = dummyStringSlice;
            dummyBlockProcessor.Column = dummyInitialColumn;
            var dummyFlexiAlertBlockOptions = new FlexiAlertBlockOptions();
            Mock<ExposedFlexiAlertBlockParser> mockTestSubject = CreateMockExposedFlexiAlertBlockParser();
            mockTestSubject.CallBase = true;
            mockTestSubject.Setup(a => a.CreateFlexiAlertBlockOptions(dummyBlockProcessor)).Returns(dummyFlexiAlertBlockOptions);

            // Act
            BlockState result = mockTestSubject.Object.ExposedTryOpenFlexiBlock(dummyBlockProcessor);

            // Assert
            _mockRepository.VerifyAll();
            Assert.Equal(BlockState.Continue, result);
            Assert.Equal(dummyInitialStart + 1, dummyBlockProcessor.Line.Start); // Skips '!'
            Assert.Equal(dummyInitialColumn + 1, dummyBlockProcessor.Column); // Skips '!'
            var resultFlexiAlertBlock = dummyBlockProcessor.NewBlocks.Peek() as FlexiAlertBlock;
            Assert.NotNull(resultFlexiAlertBlock);
            Assert.Same(dummyFlexiAlertBlockOptions, resultFlexiAlertBlock.FlexiAlertBlockOptions);
            Assert.Equal(dummyInitialColumn, resultFlexiAlertBlock.Column); // Includes '!'
            Assert.Equal(dummyInitialStart, resultFlexiAlertBlock.Span.Start); // Includes '!'
            Assert.Equal(dummyStringSlice.End, resultFlexiAlertBlock.Span.End);
        }

        [Fact]
        public void TryContinueFlexiBlock_ReturnsBlockStateNoneIfCurrentLineHasCodeIndent()
        {
            // Arrange
            BlockProcessor dummyBlockProcessor = MarkdigTypesFactory.CreateBlockProcessor();
            dummyBlockProcessor.Column = 4; // IsCodeIndent is an expression bodied member that derives its value from Column
            ExposedFlexiAlertBlockParser testSubject =CreateExposedFlexiAlertBlockParser();

            // Act
            BlockState result = testSubject.ExposedTryContinueFlexiBlock(dummyBlockProcessor, null);

            // Assert
            Assert.Equal(BlockState.None, result);
        }

        [Fact]
        public void TryContinueFlexiBlock_ReturnsBlockStateNoneIfCurrentLineDoesNotBeginWithExclamationMarkAndIsNotBlank()
        {
            // Arrange
            BlockProcessor dummyBlockProcessor = MarkdigTypesFactory.CreateBlockProcessor();
            dummyBlockProcessor.Line = new StringSlice("dummyString");
            ExposedFlexiAlertBlockParser testSubject = CreateExposedFlexiAlertBlockParser();

            // Act
            BlockState result = testSubject.ExposedTryContinueFlexiBlock(dummyBlockProcessor, null);

            // Assert
            Assert.Equal(BlockState.None, result);
        }

        [Fact]
        public void TryContinueFlexiBlock_ReturnsBlockStateBreakDiscardIfCurrentLineIsBlank()
        {
            // Arrange
            BlockProcessor dummyBlockProcessor = MarkdigTypesFactory.CreateBlockProcessor();
            dummyBlockProcessor.Line = new StringSlice("");
            ExposedFlexiAlertBlockParser testSubject = CreateExposedFlexiAlertBlockParser();

            // Act
            BlockState result = testSubject.ExposedTryContinueFlexiBlock(dummyBlockProcessor, null);

            // Assert
            Assert.Equal(BlockState.BreakDiscard, result);
        }

        [Fact]
        public void TryContinueFlexiBlock_ReturnsBlockStateContinueIfBlockCanBeContinued()
        {
            // Arrange
            var dummyStringSlice = new StringSlice("!dummyString");
            BlockProcessor dummyBlockProcessor = MarkdigTypesFactory.CreateBlockProcessor();
            dummyBlockProcessor.Line = dummyStringSlice;
            var dummyFlexiAlertBlock = new FlexiAlertBlock(null);
            ExposedFlexiAlertBlockParser testSubject = CreateExposedFlexiAlertBlockParser();

            // Act
            BlockState result = testSubject.ExposedTryContinueFlexiBlock(dummyBlockProcessor, dummyFlexiAlertBlock);

            // Assert
            Assert.Equal(BlockState.Continue, result);
            Assert.Equal(dummyStringSlice.End, dummyFlexiAlertBlock.Span.End);
            Assert.Equal(dummyStringSlice.Start + 1, dummyBlockProcessor.Start); // Skips !
        }

        [Fact]
        public void CreateFlexiAlertBlockOptions_RetrievesIconMarkupFromExtensionOptionsIfItIsntSpecified()
        {
            // Arrange
            const int dummyLineIndex = 1;
            const string dummyAlertType = "dummyAlertType";
            const string dummyIconMarkup = "dummyIconMarkup";
            BlockProcessor dummyBlockProcessor = MarkdigTypesFactory.CreateBlockProcessor();
            dummyBlockProcessor.LineIndex = dummyLineIndex;
            Mock<IFlexiOptionsBlockService> mockFlexiOptionsBlockService = _mockRepository.Create<IFlexiOptionsBlockService>();
            mockFlexiOptionsBlockService.Setup(j => j.TryPopulateOptions(dummyBlockProcessor, It.IsAny<FlexiAlertBlockOptions>(), dummyLineIndex));
            var dummyExtensionOptions = new FlexiAlertBlocksExtensionOptions { DefaultBlockOptions = new FlexiAlertBlockOptions(type: dummyAlertType) };
            dummyExtensionOptions.IconMarkups[dummyAlertType] = dummyIconMarkup;
            ExposedFlexiAlertBlockParser testSubject = CreateExposedFlexiAlertBlockParser(dummyExtensionOptions, mockFlexiOptionsBlockService.Object);

            // Act
            FlexiAlertBlockOptions result = testSubject.CreateFlexiAlertBlockOptions(dummyBlockProcessor);

            // Assert
            _mockRepository.VerifyAll();
            Assert.Equal(dummyIconMarkup, result.IconMarkup);
        }

        public class ExposedFlexiAlertBlockParser : FlexiAlertBlockParser
        {
            public ExposedFlexiAlertBlockParser(FlexiAlertBlocksExtensionOptions extensionOptions, IFlexiOptionsBlockService flexiOptionsBlockService) :
                base(flexiOptionsBlockService, extensionOptions)
            {
            }

            public BlockState ExposedTryOpenFlexiBlock(BlockProcessor processor)
            {
                return TryOpenFlexiBlock(processor);
            }

            public BlockState ExposedTryContinueFlexiBlock(BlockProcessor processor, Block block)
            {
                return TryContinueFlexiBlock(processor, block);
            }
        }

        private ExposedFlexiAlertBlockParser CreateExposedFlexiAlertBlockParser(FlexiAlertBlocksExtensionOptions extensionOptions = null,
            IFlexiOptionsBlockService flexiOptionsBlockService = null)
        {
            return new ExposedFlexiAlertBlockParser(
                extensionOptions ?? new FlexiAlertBlocksExtensionOptions(),
                flexiOptionsBlockService ?? _mockRepository.Create<IFlexiOptionsBlockService>().Object);
        }

        private Mock<ExposedFlexiAlertBlockParser> CreateMockExposedFlexiAlertBlockParser(FlexiAlertBlocksExtensionOptions extensionOptions = null,
            IFlexiOptionsBlockService flexiOptionsBlockService = null)
        {
            return _mockRepository.Create<ExposedFlexiAlertBlockParser>(
                extensionOptions ?? new FlexiAlertBlocksExtensionOptions(),
                flexiOptionsBlockService ?? _mockRepository.Create<IFlexiOptionsBlockService>().Object);
        }
    }
}