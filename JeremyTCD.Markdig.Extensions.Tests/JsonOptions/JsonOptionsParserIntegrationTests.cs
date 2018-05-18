﻿using JeremyTCD.Markdig.Extensions.JsonOptions;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace JeremyTCD.Markdig.Extensions.Tests.JsonOptions
{
    public class JsonOptionsParserIntegrationTests
    {
        [Fact]
        public void TryOpen_ReturnsBlockStateNoneIfInCodeIndent()
        {
            // Arrange
            BlockProcessor dummyBlockProcessor = MarkdigTypesFactory.CreateBlockProcessor();
            // These three lines just set IsCodeIndent to true
            dummyBlockProcessor.Column = 0;
            dummyBlockProcessor.RestartIndent();
            dummyBlockProcessor.Column = 4;
            var jsonOptionsParser = new JsonOptionsParser();

            // Act
            BlockState result = jsonOptionsParser.TryOpen(dummyBlockProcessor);

            // Assert
            Assert.True(dummyBlockProcessor.IsCodeIndent);
            Assert.Equal(BlockState.None, result);
        }

        [Theory]
        [MemberData(nameof(TryOpen_ReturnsBlockStateNoneIfLineDoesNotBeginWithExpectedCharacters_Data))]
        public void TryOpen_ReturnsBlockStateNoneIfLineDoesNotBeginWithExpectedCharacters(string line)
        {
            // Arrange
            BlockProcessor dummyBlockProcessor = MarkdigTypesFactory.CreateBlockProcessor();
            dummyBlockProcessor.Line = new StringSlice(line);
            var jsonOptionsParser = new JsonOptionsParser();

            // Act
            BlockState result = jsonOptionsParser.TryOpen(dummyBlockProcessor);

            // Assert
            Assert.Equal(BlockState.None, result);
        }

        public static IEnumerable<object[]> TryOpen_ReturnsBlockStateNoneIfLineDoesNotBeginWithExpectedCharacters_Data()
        {
            return new object[][]
            {
                // No whitespace between @ and {
                new string[]{"@ {"},
                new string[]{"@\n{"}
            };
        }

        [Fact]
        public void TryOpen_SetsProcessorLineStartCreatesJsonOptionsBlockAndReturnsBlockStateIfSuccessful()
        {
            // Arrange
            const int dummyColumn = 1;
            BlockProcessor dummyBlockProcessor = MarkdigTypesFactory.CreateBlockProcessor();
            dummyBlockProcessor.Column = dummyColumn;
            dummyBlockProcessor.Line = new StringSlice("@{dummy");
            const BlockState dummyBlockState = BlockState.Continue;
            var mockJsonOptionsParser = new Mock<JsonOptionsParser>
            {
                CallBase = true
            };
            mockJsonOptionsParser.Setup(j => j.TryContinue(dummyBlockProcessor, It.IsAny<JsonOptionsBlock>())).Returns(dummyBlockState);

            // Act
            BlockState result = mockJsonOptionsParser.Object.TryOpen(dummyBlockProcessor);

            // Assert
            Assert.Equal(dummyBlockState, result);
            Assert.Equal(1, dummyBlockProcessor.Line.Start);
            dummyBlockProcessor.NewBlocks.TryPop(out Block block);
            Assert.IsType<JsonOptionsBlock>(block);
            Assert.Equal(dummyColumn, block.Column);
            Assert.Equal(1, block.Span.Start);
        }

        [Theory]
        [MemberData(nameof(TryContinue_ReturnsBlockStateBreakSavesBlockToDocumentDataAndSetsBlockSpanEndAndEndLineIfLineIsACompleteJsonString_Data))]
        public void TryContinue_ReturnsBlockStateBreakSavesBlockToDocumentDataAndSetsBlockSpanEndAndEndLineIfLineIsACompleteJsonString(string dummyLine)
        {
            // Arrange
            const int dummyEndLine = 1;
            var dummyJsonOptionsBlock = new JsonOptionsBlock(null);
            BlockProcessor dummyBlockProcessor = MarkdigTypesFactory.CreateBlockProcessor();
            dummyBlockProcessor.Line = new StringSlice(dummyLine);
            dummyBlockProcessor.Document.Add(dummyJsonOptionsBlock); // Sets document as parent of JsonOptionsBlock
            dummyBlockProcessor.LineIndex = dummyEndLine;
            var jsonOptionsParser = new JsonOptionsParser();

            // Act
            BlockState result = jsonOptionsParser.TryContinue(dummyBlockProcessor, dummyJsonOptionsBlock);

            // Assert
            Assert.Equal(BlockState.Break, result);
            Assert.Equal(dummyLine.Length - 1, dummyJsonOptionsBlock.Span.End);
            Assert.False(dummyJsonOptionsBlock.EndsInString);
            Assert.Equal(0, dummyJsonOptionsBlock.NumOpenBrackets);
            Assert.Equal(dummyJsonOptionsBlock, dummyBlockProcessor.Document.GetData(JsonOptionsParser.JSON_OPTIONS));
            Assert.Equal(dummyEndLine, dummyJsonOptionsBlock.EndLine);
        }

        public static IEnumerable<object[]> TryContinue_ReturnsBlockStateBreakSavesBlockToDocumentDataAndSetsBlockSpanEndAndEndLineIfLineIsACompleteJsonString_Data()
        {
            return new object[][]
            {
                new string[]{"{\"option\": \"value\"}"},
                // Strings can contain curly brackets
                new string[]{"{\"option\": \"{value}\"}"},
                // Strings can contain escaped quotes
                new string[]{"{\"option\": \"\\\"value\\\"\"}"},
            };
        }

        [Fact]
        public void TryContinue_ThrowsExceptionIfDocumentDataAlreadyContainsAJsonOptionsBlock()
        {
            // Arrange
            const string dummyPendingJson = "dummyPendingJson";
            const int dummyPendingLine = 1;
            const int dummyPendingColumn = 2;
            var dummyPendingJsonOptionsBlock = new JsonOptionsBlock(null)
            {
                Lines = new StringLineGroup(dummyPendingJson),
                Line = dummyPendingLine,
                Column = dummyPendingColumn
            };
            const string dummyJson = "{\"option\": \"value\"}";
            BlockProcessor dummyBlockProcessor = MarkdigTypesFactory.CreateBlockProcessor();
            dummyBlockProcessor.Line = new StringSlice(dummyJson);
            dummyBlockProcessor.Document.SetData(JsonOptionsParser.JSON_OPTIONS, dummyPendingJsonOptionsBlock);
            var dummyJsonOptionsBlock = new JsonOptionsBlock(null);
            var jsonOptionsParser = new JsonOptionsParser();

            // Act and assert
            InvalidOperationException result = Assert.Throws<InvalidOperationException>(() => jsonOptionsParser.TryContinue(dummyBlockProcessor, dummyJsonOptionsBlock));

            // Assert
            Assert.Equal(string.Format(Strings.InvalidOperationException_UnusedJsonOptions, dummyPendingJson, dummyPendingLine, dummyPendingColumn), result.Message);
        }

        [Theory]
        [MemberData(nameof(TryContinue_ReturnsBlockStateContinueIfLineIsAPartialJsonString_Data))]
        public void TryContinue_ReturnsBlockStateContinueIfLineIsAPartialJsonString(string dummyLine, bool expectedEndsInString, int expectedNumOpenBrackets)
        {
            // Arrange
            BlockProcessor dummyBlockProcessor = MarkdigTypesFactory.CreateBlockProcessor();
            dummyBlockProcessor.Line = new StringSlice(dummyLine);
            var dummyJsonOptionsBlock = new JsonOptionsBlock(null);
            var jsonOptionsParser = new JsonOptionsParser();

            // Act
            BlockState result = jsonOptionsParser.TryContinue(dummyBlockProcessor, dummyJsonOptionsBlock);

            // Assert
            Assert.Equal(BlockState.Continue, result);
            Assert.Equal(expectedEndsInString, dummyJsonOptionsBlock.EndsInString);
            Assert.Equal(expectedNumOpenBrackets, dummyJsonOptionsBlock.NumOpenBrackets);
        }

        public static IEnumerable<object[]> TryContinue_ReturnsBlockStateContinueIfLineIsAPartialJsonString_Data()
        {
            return new object[][]
            {
                new object[]{"{\"option\": \"value\",", false, 1},
                new object[]{"{\"option\": \"val", true, 1},
                new object[]{"{\"option\": { \"subOption\":", false, 2},
            };
        }
    }
}
