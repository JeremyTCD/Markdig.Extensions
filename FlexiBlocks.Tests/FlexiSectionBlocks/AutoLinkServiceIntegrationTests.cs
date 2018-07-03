﻿using Jering.Markdig.Extensions.FlexiBlocks.FlexiSectionBlocks;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using System.Collections.Generic;
using Xunit;

namespace Jering.Markdig.Extensions.FlexiBlocks.Tests.FlexiSectionBlocks
{
    public class AutoLinkServiceIntegrationTests
    {
        [Fact]
        public void SetupAutoLink_CreatesSectionLinkReferenceDefinitionAndAddsItToMapOfSectionLinkReferenceDefinitions()
        {
            // Arrange
            const string dummyHeadingText = "dummyHeadingText";
            var dummyHeadingBlock = new HeadingBlock(null)
            {
                Lines = new StringLineGroup(dummyHeadingText)
            };
            var dummyFlexiSectionBlock = new FlexiSectionBlock(null);
            BlockProcessor dummyBlockProcessor = MarkdigTypesFactory.CreateBlockProcessor();
            var autoLinkService = new AutoLinkService();

            // Act
            autoLinkService.SetupAutoLink(dummyBlockProcessor, dummyFlexiSectionBlock, dummyHeadingBlock);

            // Assert
            var resultSlrds = dummyBlockProcessor.Document.GetData(AutoLinkService.AUTO_LINKS_KEY) as Dictionary<string, SectionLinkReferenceDefinition>;
            Assert.NotNull(resultSlrds);
            resultSlrds.TryGetValue(dummyHeadingText, out SectionLinkReferenceDefinition resultSlrd);
            Assert.NotNull(resultSlrd);
            Assert.Equal(dummyFlexiSectionBlock, resultSlrd.FlexiSectionBlock);
        }

        [Fact]
        public void DocumentOnProcessInlinesBegin_AddsSectionLinkReferenceDefinitionsWithoutOverridingExistingLinkReferenceDefinitions()
        {
            // Arrange
            const string dummySlrd1Key = "dummySlrd1Key";
            var dummySlrd1 = new SectionLinkReferenceDefinition();
            const string dummySlrd2Key = "dummySlrd2Key";
            var dummySlrd2 = new SectionLinkReferenceDefinition();
            var dummySlrds = new Dictionary<string, SectionLinkReferenceDefinition>()
            {
                { dummySlrd1Key, dummySlrd1 },
                { dummySlrd2Key, dummySlrd2 }
            };
            var dummyLfd = new LinkReferenceDefinition();
            var dummyDocument = new MarkdownDocument();
            dummyDocument.SetData(AutoLinkService.AUTO_LINKS_KEY, dummySlrds);
            dummyDocument.SetLinkReferenceDefinition(dummySlrd1Key, dummyLfd);
            InlineProcessor dummyInlineProcessor = MarkdigTypesFactory.CreateInlineProcessor(markdownDocument: dummyDocument);
            var autoLinkService = new AutoLinkService();

            // Act
            autoLinkService.DocumentOnProcessInlinesBegin(dummyInlineProcessor, null);

            // Assert
            Dictionary<string, LinkReferenceDefinition> resultLinks = dummyDocument.GetLinkReferenceDefinitions().Links;
            Assert.Equal(2, resultLinks.Count);
            Assert.Same(dummyLfd, resultLinks[dummySlrd1Key]); // Not overriden
            Assert.Same(dummySlrd2, resultLinks[dummySlrd2Key]); // Added
            Assert.False(resultLinks.ContainsKey(AutoLinkService.AUTO_LINKS_KEY));
        }

        [Fact]
        public void CreateLinkInline_CreatesLinkInline()
        {
            // Arrange
            const string dummyID = "dummyID";
            const string dummyTitle = "dummyTitle";
            var dummyFlexiSectionBlock = new FlexiSectionBlock(null)
            {
                FlexiSectionBlockOptions = new FlexiSectionBlockOptions() { Attributes = new HtmlAttributeDictionary { { "id", dummyID } } }
            };
            var dummySlrd = new SectionLinkReferenceDefinition()
            {
                FlexiSectionBlock = dummyFlexiSectionBlock,
                Title = dummyTitle
            };
            var autoLinkService = new AutoLinkService();

            // Act
            var result = autoLinkService.CreateLinkInline(null, dummySlrd, null) as LinkInline;

            // Assert
            Assert.NotNull(result);
            Assert.Equal($"#{dummyID}", result.GetDynamicUrl());
            Assert.Equal(dummyTitle, result.Title);
        }
    }
}
