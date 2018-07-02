﻿using Markdig.Renderers;
using Markdig.Syntax;
using System.Collections.Generic;

namespace FlexiBlocks
{
    public static class HtmlRendererExtensions
    {
        public static HtmlRenderer WriteHtmlAttributeDictionary(this HtmlRenderer htmlRenderer, HtmlAttributeDictionary attributes)
        {
            if (attributes != null)
            {
                foreach (KeyValuePair<string, string> attribute in attributes)
                {
                    htmlRenderer.Write($" {attribute.Key}=\"");
                    htmlRenderer.WriteEscape(attribute.Value);
                    htmlRenderer.Write($"\"");
                }
            }

            return htmlRenderer;
        }

        public static HtmlRenderer WriteChildren(this HtmlRenderer htmlRenderer, ContainerBlock containerBlock, bool implicitParagraph)
        {
            bool initialImplicitParagraph = htmlRenderer.ImplicitParagraph;
            htmlRenderer.ImplicitParagraph = implicitParagraph;

            htmlRenderer.WriteChildren(containerBlock);

            htmlRenderer.ImplicitParagraph = initialImplicitParagraph;

            return htmlRenderer;
        }
    }
}