﻿using Markdig;
using Markdig.Parsers;
using Markdig.Renderers;
using Microsoft.Extensions.Options;
using System;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiSectionBlocks
{
    /// <summary>
    /// <para>A markdig extension for <see cref="FlexiSectionBlock"/>s.</para>
    /// <para>This extension uses ATX headings as section demarcators, wrapping each section in a sectioning content element, as recommended by the HTML spec - https://html.spec.whatwg.org/multipage/sections.html#headings-and-sections.</para>
    /// </summary>
    public class FlexiSectionBlocksExtension : FlexiBlocksExtension
    {
        private readonly FlexiSectionBlocksExtensionOptions _extensionOptions;
        private readonly FlexiSectionBlockRenderer _flexiSectionBlockRenderer;
        private readonly FlexiSectionBlockParser _flexiSectionBlockParser;

        /// <summary>
        /// Creates a <see cref="FlexiSectionBlocksExtension"/> instance.
        /// </summary>
        /// <param name="extensionOptionsAccessor">Thw accessor for <see cref="FlexiSectionBlocksExtensionOptions"/>.</param>
        /// <param name="flexiSectionBlockParser">The parser for creating <see cref="FlexiSectionBlock"/>s from markdown.</param>
        /// <param name="flexiSectionBlockRenderer">The renderer for rendering <see cref="FlexiSectionBlock"/>s as HTML.</param>
        public FlexiSectionBlocksExtension(IOptions<FlexiSectionBlocksExtensionOptions> extensionOptionsAccessor,
            FlexiSectionBlockParser flexiSectionBlockParser,
            FlexiSectionBlockRenderer flexiSectionBlockRenderer)
        {
            _extensionOptions = extensionOptionsAccessor?.Value ?? throw new ArgumentNullException(nameof(extensionOptionsAccessor));
            _flexiSectionBlockRenderer = flexiSectionBlockRenderer ?? throw new ArgumentNullException(nameof(flexiSectionBlockRenderer));
            _flexiSectionBlockParser = flexiSectionBlockParser ?? throw new ArgumentNullException(nameof(flexiSectionBlockParser));
        }

        /// <summary>
        /// Registers a <see cref="FlexiSectionBlockParser"/> if one isn't already registered.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder to register the parser for.</param>
        public override void Setup(MarkdownPipelineBuilder pipelineBuilder)
        {
            if (pipelineBuilder == null)
            {
                throw new ArgumentNullException(nameof(pipelineBuilder));
            }

            // HeadingBlockParser is a default parser registered in MarkdownPipelineBuilder's constructor.
            // FlexiSectionBlockParser makes it redundant.
            HeadingBlockParser headingBlockParser = pipelineBuilder.BlockParsers.Find<HeadingBlockParser>();
            if (headingBlockParser != null)
            {
                pipelineBuilder.BlockParsers.Remove(headingBlockParser);
            }

            if (!pipelineBuilder.BlockParsers.Contains<FlexiSectionBlockParser>())
            {
                pipelineBuilder.BlockParsers.Insert(0, _flexiSectionBlockParser);
            }
        }

        /// <summary>
        /// Registers a <see cref="FlexiSectionBlockRenderer"/> if one isn't already registered.
        /// </summary>
        /// <param name="pipeline">Unused.</param>
        /// <param name="renderer">The root renderer to register the renderer for.</param>
        public override void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            if (renderer == null)
            {
                throw new ArgumentNullException(nameof(renderer));
            }

            if (renderer is HtmlRenderer htmlRenderer && !htmlRenderer.ObjectRenderers.Contains<FlexiSectionBlockRenderer>())
            {
                htmlRenderer.ObjectRenderers.Insert(0, _flexiSectionBlockRenderer);
            }
        }
    }
}
