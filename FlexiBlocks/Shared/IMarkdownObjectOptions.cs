﻿namespace Jering.Markdig.Extensions.FlexiBlocks
{
    public interface IMarkdownObjectOptions<T>
    {
        HtmlAttributeDictionary Attributes { get; set; }

        T Clone();
    }
}
