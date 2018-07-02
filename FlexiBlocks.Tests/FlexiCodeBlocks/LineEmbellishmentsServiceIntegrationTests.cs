﻿using FlexiBlocks.FlexiCode;
using System;
using System.Collections.Generic;
using Xunit;

namespace FlexiBlocks.Tests.FlexiCode
{
    public class LineEmbellishmentsServiceIntegrationTests
    {
        [Theory]
        [MemberData(nameof(EmbellishLines_EmbellishesLines_Data))]
        public void EmbellishLines_EmbellishesLines(SerializableWrapper<List<LineNumberRange>> dummyLineNumberRanges,
            SerializableWrapper<List<LineRange>> dummyHighlightLineRanges,
            string dummyPrefixForClasses,
            string expectedResult)
        {
            // Arrange
            const string dummyText = @"line 1
line 2
line 3
line 4
line 5
line 6
line 7
line 8
line 9
line 10";
            var lineEmbellishmentService = new LineEmbellishmentsService();

            // Act
            string result = lineEmbellishmentService.EmbellishLines(dummyText, dummyLineNumberRanges?.Value, dummyHighlightLineRanges?.Value, dummyPrefixForClasses);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        public static IEnumerable<object[]> EmbellishLines_EmbellishesLines_Data()
        {
            return new object[][]
            {
                // Both line numbers and highlighting
                new object[]{
                    new SerializableWrapper<List<LineNumberRange>>(
                        new List<LineNumberRange> { new LineNumberRange(1, 4, 1), new LineNumberRange(7, 10, 7) }
                    ),
                    new SerializableWrapper<List<LineRange>>(
                        new List<LineRange> { new LineRange(2, 2), new LineRange(8, 9) }
                    ),
                    null,
                    @"<span class=""line""><span class=""line-number"">1</span><span class=""line-text"">line 1</span></span>
<span class=""line highlight""><span class=""line-number"">2</span><span class=""line-text"">line 2</span></span>
<span class=""line""><span class=""line-number"">3</span><span class=""line-text"">line 3</span></span>
<span class=""line""><span class=""line-number"">4</span><span class=""line-text"">line 4</span></span>
<span class=""line""><span class=""line-text"">line 5</span></span>
<span class=""line""><span class=""line-text"">line 6</span></span>
<span class=""line""><span class=""line-number"">7</span><span class=""line-text"">line 7</span></span>
<span class=""line highlight""><span class=""line-number"">8</span><span class=""line-text"">line 8</span></span>
<span class=""line highlight""><span class=""line-number"">9</span><span class=""line-text"">line 9</span></span>
<span class=""line""><span class=""line-number"">10</span><span class=""line-text"">line 10</span></span>"
                },
                // Both line numbers and highlighting using -1 to specify end lines
                new object[]{
                    new SerializableWrapper<List<LineNumberRange>>(
                        new List<LineNumberRange> { new LineNumberRange(1, 4, 1), new LineNumberRange(7, -1, 7) }
                    ),
                    new SerializableWrapper<List<LineRange>>(
                        new List<LineRange> { new LineRange(2, 2), new LineRange(9, -1) }
                    ),
                    null,
                    @"<span class=""line""><span class=""line-number"">1</span><span class=""line-text"">line 1</span></span>
<span class=""line highlight""><span class=""line-number"">2</span><span class=""line-text"">line 2</span></span>
<span class=""line""><span class=""line-number"">3</span><span class=""line-text"">line 3</span></span>
<span class=""line""><span class=""line-number"">4</span><span class=""line-text"">line 4</span></span>
<span class=""line""><span class=""line-text"">line 5</span></span>
<span class=""line""><span class=""line-text"">line 6</span></span>
<span class=""line""><span class=""line-number"">7</span><span class=""line-text"">line 7</span></span>
<span class=""line""><span class=""line-number"">8</span><span class=""line-text"">line 8</span></span>
<span class=""line highlight""><span class=""line-number"">9</span><span class=""line-text"">line 9</span></span>
<span class=""line highlight""><span class=""line-number"">10</span><span class=""line-text"">line 10</span></span>"
                },
                // Only line numbers (empty list of highlight line ranges) 
                new object[]{
                    new SerializableWrapper<List<LineNumberRange>>(
                        new List<LineNumberRange> { new LineNumberRange(1, 4, 2), new LineNumberRange(7, -1, 7) }
                    ),
                    new SerializableWrapper<List<LineRange>>(
						new List<LineRange>()
					),
                    null,
                    @"<span class=""line""><span class=""line-number"">2</span><span class=""line-text"">line 1</span></span>
<span class=""line""><span class=""line-number"">3</span><span class=""line-text"">line 2</span></span>
<span class=""line""><span class=""line-number"">4</span><span class=""line-text"">line 3</span></span>
<span class=""line""><span class=""line-number"">5</span><span class=""line-text"">line 4</span></span>
<span class=""line""><span class=""line-text"">line 5</span></span>
<span class=""line""><span class=""line-text"">line 6</span></span>
<span class=""line""><span class=""line-number"">7</span><span class=""line-text"">line 7</span></span>
<span class=""line""><span class=""line-number"">8</span><span class=""line-text"">line 8</span></span>
<span class=""line""><span class=""line-number"">9</span><span class=""line-text"">line 9</span></span>
<span class=""line""><span class=""line-number"">10</span><span class=""line-text"">line 10</span></span>"
                },
                // Only line numbers (null list of highlight line ranges)
                new object[]{
                    new SerializableWrapper<List<LineNumberRange>>(
                        new List<LineNumberRange> { new LineNumberRange(1, 4, 1), new LineNumberRange(7, -1, 7) }
                    ),
                    null,
                    null,
                    @"<span class=""line""><span class=""line-number"">1</span><span class=""line-text"">line 1</span></span>
<span class=""line""><span class=""line-number"">2</span><span class=""line-text"">line 2</span></span>
<span class=""line""><span class=""line-number"">3</span><span class=""line-text"">line 3</span></span>
<span class=""line""><span class=""line-number"">4</span><span class=""line-text"">line 4</span></span>
<span class=""line""><span class=""line-text"">line 5</span></span>
<span class=""line""><span class=""line-text"">line 6</span></span>
<span class=""line""><span class=""line-number"">7</span><span class=""line-text"">line 7</span></span>
<span class=""line""><span class=""line-number"">8</span><span class=""line-text"">line 8</span></span>
<span class=""line""><span class=""line-number"">9</span><span class=""line-text"">line 9</span></span>
<span class=""line""><span class=""line-number"">10</span><span class=""line-text"">line 10</span></span>"
                },
                // Only highlighting (null list of line number line ranges)
                new object[]{
                    new SerializableWrapper<List<LineNumberRange>>(
                        new List<LineNumberRange>()
                    ),
                    new SerializableWrapper<List<LineRange>>(
						new List<LineRange> { new LineRange(2, 2), new LineRange(9, -1) }
					),
                    null,
                    @"<span class=""line""><span class=""line-text"">line 1</span></span>
<span class=""line highlight""><span class=""line-text"">line 2</span></span>
<span class=""line""><span class=""line-text"">line 3</span></span>
<span class=""line""><span class=""line-text"">line 4</span></span>
<span class=""line""><span class=""line-text"">line 5</span></span>
<span class=""line""><span class=""line-text"">line 6</span></span>
<span class=""line""><span class=""line-text"">line 7</span></span>
<span class=""line""><span class=""line-text"">line 8</span></span>
<span class=""line highlight""><span class=""line-text"">line 9</span></span>
<span class=""line highlight""><span class=""line-text"">line 10</span></span>"
                },
                // Only highlighting (null list of line number line ranges)
                new object[]{
                    null,
                    new SerializableWrapper<List<LineRange>>(
						new List<LineRange> { new LineRange(2, 2), new LineRange(9, -1) }
					),
                    null,
                    @"<span class=""line""><span class=""line-text"">line 1</span></span>
<span class=""line highlight""><span class=""line-text"">line 2</span></span>
<span class=""line""><span class=""line-text"">line 3</span></span>
<span class=""line""><span class=""line-text"">line 4</span></span>
<span class=""line""><span class=""line-text"">line 5</span></span>
<span class=""line""><span class=""line-text"">line 6</span></span>
<span class=""line""><span class=""line-text"">line 7</span></span>
<span class=""line""><span class=""line-text"">line 8</span></span>
<span class=""line highlight""><span class=""line-text"">line 9</span></span>
<span class=""line highlight""><span class=""line-text"">line 10</span></span>"
                },
                // Prefix specified
                new object[]{
                    new SerializableWrapper<List<LineNumberRange>>(
                        new List<LineNumberRange> { new LineNumberRange(1, 4, 1), new LineNumberRange(7, 10, 7) }
                    ),
                    new SerializableWrapper<List<LineRange>>(
						new List<LineRange> { new LineRange(2, 2), new LineRange(8, 9) }
					),
                    "dummy-prefix-",
                    @"<span class=""dummy-prefix-line""><span class=""dummy-prefix-line-number"">1</span><span class=""dummy-prefix-line-text"">line 1</span></span>
<span class=""dummy-prefix-line dummy-prefix-highlight""><span class=""dummy-prefix-line-number"">2</span><span class=""dummy-prefix-line-text"">line 2</span></span>
<span class=""dummy-prefix-line""><span class=""dummy-prefix-line-number"">3</span><span class=""dummy-prefix-line-text"">line 3</span></span>
<span class=""dummy-prefix-line""><span class=""dummy-prefix-line-number"">4</span><span class=""dummy-prefix-line-text"">line 4</span></span>
<span class=""dummy-prefix-line""><span class=""dummy-prefix-line-text"">line 5</span></span>
<span class=""dummy-prefix-line""><span class=""dummy-prefix-line-text"">line 6</span></span>
<span class=""dummy-prefix-line""><span class=""dummy-prefix-line-number"">7</span><span class=""dummy-prefix-line-text"">line 7</span></span>
<span class=""dummy-prefix-line dummy-prefix-highlight""><span class=""dummy-prefix-line-number"">8</span><span class=""dummy-prefix-line-text"">line 8</span></span>
<span class=""dummy-prefix-line dummy-prefix-highlight""><span class=""dummy-prefix-line-number"">9</span><span class=""dummy-prefix-line-text"">line 9</span></span>
<span class=""dummy-prefix-line""><span class=""dummy-prefix-line-number"">10</span><span class=""dummy-prefix-line-text"">line 10</span></span>"
                },
            };
        }

        [Theory]
        [MemberData(nameof(ValidateRanges_ThrowsExceptionIfLineNumberRangesLineRangeIsNotASubsetOfActualLines_Data))]
        public void ValidateRanges_ThrowsExceptionIfLineNumberRangesLineRangeIsNotASubsetOfActualLines(SerializableWrapper<List<LineNumberRange>> dummyLineNumberRangesWrapper, int dummyNumLines)
        {
            // Arrange
            var lineEmbellishmentService = new LineEmbellishmentsService();

            // Act and assert
            InvalidOperationException result = Assert.Throws<InvalidOperationException>(() => lineEmbellishmentService.ValidateRanges(dummyLineNumberRangesWrapper.Value, null, dummyNumLines));
            Assert.Equal(string.Format(Strings.InvalidOperationException_InvalidLineNumberLineRange, dummyLineNumberRangesWrapper.Value[0].LineRange.ToString(), dummyNumLines), result.Message);
        }

        public static IEnumerable<object[]> ValidateRanges_ThrowsExceptionIfLineNumberRangesLineRangeIsNotASubsetOfActualLines_Data()
        {
            return new object[][]
            {
                // Range of lines exceeds actual number of lines
                new object[]{
                    new SerializableWrapper<List<LineNumberRange>>(
						new List<LineNumberRange> { new LineNumberRange(1, 5, 1) }
					),
                    4
                },
                // If end line is -1, range of lines ends at last line, but throw if start line is after last line
                new object[]{
                    new SerializableWrapper<List<LineNumberRange>>(
						new List<LineNumberRange> { new LineNumberRange(4, -1, 1) }
					),
                    3
                }
            };
        }

        [Theory]
        [MemberData(nameof(ValidateRanges_ThrowsExceptionIfHighlightLineRangeIsNotASubsetOfActualLines_Data))]
        public void ValidateRanges_ThrowsExceptionIfHighlightLineRangeIsNotASubsetOfActualLines(SerializableWrapper<List<LineRange>> dummyHighlightLineRangesWrapper, int dummyNumLines)
        {
            // Arrange
            var lineEmbellishmentService = new LineEmbellishmentsService();

            // Act and assert
            InvalidOperationException result = Assert.Throws<InvalidOperationException>(() => lineEmbellishmentService.ValidateRanges(null, dummyHighlightLineRangesWrapper.Value, dummyNumLines));
            Assert.Equal(string.Format(Strings.InvalidOperationException_InvalidHighlightLineRange, dummyHighlightLineRangesWrapper.Value[0].ToString(), dummyNumLines), result.Message);
        }

        public static IEnumerable<object[]> ValidateRanges_ThrowsExceptionIfHighlightLineRangeIsNotASubsetOfActualLines_Data()
        {
            return new object[][]
            {
                // Range of lines exceeds actual number of lines
                new object[]{
                    new SerializableWrapper<List<LineRange>>(
						new List<LineRange> { new LineRange(1, 5) }
					),
                    4
                },
                // If end line is -1, range of lines ends at last line, but throw if start line is after last line
                new object[]{
                    new SerializableWrapper<List<LineRange>>(
						new List<LineRange> { new LineRange(4, -1) }
					),
                    3
                }
            };
        }

        [Theory]
        [MemberData(nameof(ValidateRanges_DoesNothingIfSuccessful_Data))]
        public void ValidateRanges_DoesNothingIfSuccessful(SerializableWrapper<List<LineNumberRange>> dummyLineNumberRangesWrapper, SerializableWrapper<List<LineRange>> dummyHighlightLineRangesWrapper, int dummyNumLines)
        {
            // Arrange
            var lineEmbellishmentService = new LineEmbellishmentsService();

            // Act
            lineEmbellishmentService.ValidateRanges(dummyLineNumberRangesWrapper?.Value, dummyHighlightLineRangesWrapper?.Value, dummyNumLines);
        }

        public static IEnumerable<object[]> ValidateRanges_DoesNothingIfSuccessful_Data()
        {
            return new object[][]
            {
                // Lines in line ranges = actual number of lines
                new object[]{
                    new SerializableWrapper<List<LineNumberRange>>(
						new List<LineNumberRange> { new LineNumberRange(1, 5, 1) }
					),
                    new SerializableWrapper<List<LineRange>>(
						new List<LineRange> { new LineRange(1, 5) }
					),
                    5
                },
                // Only line number ranges (lines in line ranges = actual number of lines)
                new object[]{
                    new SerializableWrapper<List<LineNumberRange>>(
						new List<LineNumberRange> { new LineNumberRange(1, 5, 1) }
					),
                    null,
                    5
                },
                // Only highlight ranges (lines in line ranges = actual number of lines)
                new object[]{
                    null,
                    new SerializableWrapper<List<LineRange>>(
						new List<LineRange> { new LineRange(1, 5) }
					),
                    5
                },
                // Lines in line ranges < actual number of lines
                new object[]{
                    new SerializableWrapper<List<LineNumberRange>>(
						new List<LineNumberRange> { new LineNumberRange(1, 5, 1) }
					),
                    new SerializableWrapper<List<LineRange>>(
						new List<LineRange> { new LineRange(1, 5) }
					),
                    6
                },
                // Lines in line ranges end at -1
                new object[]{
                    new SerializableWrapper<List<LineNumberRange>>(
                        new List<LineNumberRange> { new LineNumberRange(1, -1, 1) }
                    ),
                    new SerializableWrapper<List<LineRange>>(
						new List<LineRange> { new LineRange(1, -1) }
					),
                    5
                }
            };
        }

        [Theory]
        [MemberData(nameof(CompareLineNumberRanges_ThrowsExceptionIfLineNumbersOverlap_Data))]
        public void CompareLineNumberRanges_ThrowsExceptionIfLineNumbersOverlap(SerializableWrapper<LineNumberRange> dummyXLineNumberRangeWrapper, SerializableWrapper<LineNumberRange> dummyYLineNumberRangeWrapper)
        {
            // Arrange
            var lineEmbellishmentService = new LineEmbellishmentsService();

            // Act and assert
            InvalidOperationException result = Assert.Throws<InvalidOperationException>(() => lineEmbellishmentService.CompareLineNumberRanges(dummyXLineNumberRangeWrapper.Value, dummyYLineNumberRangeWrapper.Value));
            Assert.Equal(string.Format(Strings.InvalidOperationException_LineNumbersCannotOverlap, dummyXLineNumberRangeWrapper.Value.ToString(), dummyYLineNumberRangeWrapper.Value.ToString()), result.Message);
        }

        public static IEnumerable<object[]> CompareLineNumberRanges_ThrowsExceptionIfLineNumbersOverlap_Data()
        {
            return new object[][]
            {
                // Lines overlap
                new object[]{
                    new SerializableWrapper<LineNumberRange>(
						new LineNumberRange(1, 5, 2)
					),
                    new SerializableWrapper<LineNumberRange>(
						new LineNumberRange(5, 10, 10)
					)
                },
                // Line numbers overlap
                new object[]{
                    new SerializableWrapper<LineNumberRange>(
						new LineNumberRange(1, 5, 2)
					),
                    new SerializableWrapper<LineNumberRange>(
						new LineNumberRange(7, 10, 6)
					)
                },
                // Line numbers overlap
                new object[]{
                    new SerializableWrapper<LineNumberRange>(
						new LineNumberRange(7, 10, 6)
					),
                    new SerializableWrapper<LineNumberRange>(
						new LineNumberRange(1, 5, 2)
					)
                },
                // Line numbers wrong order
                new object[]{
                    new SerializableWrapper<LineNumberRange>(
						new LineNumberRange(1, 5, 10)
					),
                    new SerializableWrapper<LineNumberRange>(
						new LineNumberRange(6, 8, 2)
					)
                },
                // Line numbers wrong order
                new object[]{
                    new SerializableWrapper<LineNumberRange>(
						new LineNumberRange(6, 8, 2)
					),
                    new SerializableWrapper<LineNumberRange>(
						new LineNumberRange(1, 5, 10)
					)
                }
            };
        }

        [Theory]
        [MemberData(nameof(CompareLineNumberRanges_ReturnsCompareResultIfSuccessful_Data))]
        public void CompareLineNumberRanges_ReturnsCompareResultIfSuccessful(SerializableWrapper<LineNumberRange> dummyXLineNumberRangeWrapper, SerializableWrapper<LineNumberRange> dummyYLineNumberRangeWrapper, int expectedResult)
        {
            // Arrange
            var lineEmbellishmentService = new LineEmbellishmentsService();

            // Act
            int result = lineEmbellishmentService.CompareLineNumberRanges(dummyXLineNumberRangeWrapper.Value, dummyYLineNumberRangeWrapper.Value);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        public static IEnumerable<object[]> CompareLineNumberRanges_ReturnsCompareResultIfSuccessful_Data()
        {
            return new object[][]
            {
                // Before
                new object[]{
                    new SerializableWrapper<LineNumberRange>(
						new LineNumberRange(1, 5, 1)
					),
                    new SerializableWrapper<LineNumberRange>(
						new LineNumberRange(7, 10, 7)
					),
                    -1
                },
                // After
                new object[]{
                    new SerializableWrapper<LineNumberRange>(
						new LineNumberRange(7, 10, 7)
					),
                    new SerializableWrapper<LineNumberRange>(
						new LineNumberRange(1, 5, 1)
					),
                    1
                },
            };
        }

        [Fact]
        public void CompareHighlightLineRanges_ThrowsExceptionIfLineRangesOverlap()
        {
            // Arrange
            var lineEmbellishmentService = new LineEmbellishmentsService();
            // Line ranges overlap at line 2
            var dummyXLineRange = new LineRange(1, 2);
            var dummyYLineRange = new LineRange(2, 3);

            // Act and assert
            InvalidOperationException result = Assert.Throws<InvalidOperationException>(() => lineEmbellishmentService.CompareHighlightLineRanges(dummyXLineRange, dummyYLineRange));
            Assert.Equal(string.Format(Strings.InvalidOperationException_LineRangesForHighlightingCannotOverlap, dummyXLineRange.ToString(), dummyYLineRange.ToString()), result.Message);
        }

        [Theory]
        [MemberData(nameof(CompareHighlightLineRanges_ReturnsCompareResultIfSuccessful_Data))]
        public void CompareHighlightLineRanges_ReturnsCompareResultIfSuccessful(SerializableWrapper<LineRange> dummyXHighlightLineRangeWrapper, SerializableWrapper<LineRange> dummyYHighlightLineRangeWrapper, int expectedResult)
        {
            // Arrange
            var lineEmbellishmentService = new LineEmbellishmentsService();

            // Act
            int result = lineEmbellishmentService.CompareHighlightLineRanges(dummyXHighlightLineRangeWrapper.Value, dummyYHighlightLineRangeWrapper.Value);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        public static IEnumerable<object[]> CompareHighlightLineRanges_ReturnsCompareResultIfSuccessful_Data()
        {
            return new object[][]
            {
                // Before
                new object[]{
                    new SerializableWrapper<LineRange>(
						new LineRange(1, 2)
					),
                    new SerializableWrapper<LineRange>(
						new LineRange(3, 4)
					),
                    -1
                },
                // After
                new object[]{
                    new SerializableWrapper<LineRange>(
						new LineRange(3, 4)
					),
                    new SerializableWrapper<LineRange>(
						new LineRange(1, 2)
					),
                    1
                }
            };
        }

        [Theory]
        [MemberData(nameof(EmbellishLines_ReturnsTextIfBothListsOfRangesAreNullOrEmpty_Data))]
        public void EmbellishLines_ReturnsTextIfBothListsOfRangesAreNullOrEmpty(SerializableWrapper<List<LineNumberRange>> dummyLineNumberRangesWrapper, SerializableWrapper<List<LineRange>> dummyLineRangesWrapper)
        {
            // Arrange
            const string dummyText = "dummyText";
            var lineEmbellishmentService = new LineEmbellishmentsService();

            // Act  
            string result = lineEmbellishmentService.EmbellishLines(dummyText, dummyLineNumberRangesWrapper?.Value, dummyLineRangesWrapper?.Value);

            // Assert
            Assert.Equal(dummyText, result);
        }

        public static IEnumerable<object[]> EmbellishLines_ReturnsTextIfBothListsOfRangesAreNullOrEmpty_Data()
        {
            return new object[][]
            {
                new object[]{null, null},
                new object[]{
                    new SerializableWrapper<List<LineNumberRange>>(
						new List<LineNumberRange>()
					),
                    null
                },
                new object[]{
                    null,
                    new SerializableWrapper<List<LineRange>>(
                        new List<LineRange>()
                    )
                },
                new object[]{
                    new SerializableWrapper<List<LineNumberRange>>(
						new List<LineNumberRange>()
					),
                    new SerializableWrapper<List<LineRange>>(
                        new List<LineRange>()
                    )
                }
            };
        }
    }
}