﻿using Markdig.Extensions.Tables;
using Markdig.Renderers;
using Markdig.Syntax;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Jering.Markdig.Extensions.FlexiBlocks.FlexiTableBlocks
{
    /// <summary>
    /// A renderer that renders FlexiTableBlocks as HTML.
    /// </summary>
    public class FlexiTableBlockRenderer : FlexiBlockRenderer<Table>
    {
        private readonly FlexiTableBlockOptions _defaultFlexiTableBlockOptions;
        private readonly HtmlRenderer _stripRenderer;
        private readonly StringWriter _stringWriter;

        /// <summary>
        /// Creates a <see cref="FlexiTableBlockRenderer"/> instance.
        /// </summary>
        /// <param name="extensionOptionsAccessor">The accessor for <see cref="FlexiTableBlocksExtensionOptions"/>.</param>
        public FlexiTableBlockRenderer(IOptions<FlexiTableBlocksExtensionOptions> extensionOptionsAccessor)
        {
            _defaultFlexiTableBlockOptions = extensionOptionsAccessor?.Value.DefaultBlockOptions ?? throw new ArgumentNullException(nameof(extensionOptionsAccessor));
            _stringWriter = new StringWriter();
            _stripRenderer = new HtmlRenderer(_stringWriter)
            {
                EnableHtmlForBlock = false,
                EnableHtmlForInline = false
            };
        }

        /// <summary>
        /// Renders a FlexiTableBlock as HTML.
        /// </summary>
        /// <param name="renderer">The renderer to write to.</param>
        /// <param name="obj">The FlexiTableBlock to render.</param>
        protected override void WriteFlexiBlock(HtmlRenderer renderer, Table obj)
        {
            // Table's created using the pipe table syntax do not have their own FlexiTableOptions. This is because PipeTableParser is an inline parser and so does not work 
            // with FlexiOptionsBlocks.
            FlexiTableBlockOptions flexiTableBlockOptions = (FlexiTableBlockOptions)obj.GetData(FlexiTableBlocksExtension.FLEXI_TABLE_BLOCK_OPTIONS_KEY) ?? _defaultFlexiTableBlockOptions;
            bool renderWrapper = !string.IsNullOrWhiteSpace(flexiTableBlockOptions.WrapperElement);
            bool renderLabelAttribute = !string.IsNullOrWhiteSpace(flexiTableBlockOptions.LabelAttribute);

            renderer.EnsureLine();
            // TODO merge attributes? - ideally, PipeTableParser should be converted to a BlockParser so that the GenericAttributes extension is not required
            renderer.Write("<table").
                WriteAttributes(flexiTableBlockOptions.Attributes).
                WriteAttributes(obj).
                WriteLine(">");

            bool hasBody = false;
            bool hasAlreadyHeader = false;
            bool isHeaderOpen = false;

            bool hasColumnWidth = false;
            foreach (var tableColumnDefinition in obj.ColumnDefinitions)
            {
                if (tableColumnDefinition.Width != 0.0f && tableColumnDefinition.Width != 1.0f)
                {
                    hasColumnWidth = true;
                    break;
                }
            }

            if (hasColumnWidth)
            {
                foreach (var tableColumnDefinition in obj.ColumnDefinitions)
                {
                    double width = Math.Round(tableColumnDefinition.Width * 100) / 100;
                    string widthValue = string.Format(CultureInfo.InvariantCulture, "{0:0.##}", width);
                    renderer.WriteLine($"<col style=\"width:{widthValue}%\">");
                }
            }

            // Store th contents
            List<string> labels = null;

            foreach (var rowObj in obj)
            {
                var row = (TableRow)rowObj;
                if (row.IsHeader)
                {
                    // Don't allow more than 1 thead
                    if (!hasAlreadyHeader)
                    {
                        if (renderLabelAttribute)
                        {
                            labels = new List<string>(row.Count);
                        }
                        renderer.WriteLine("<thead>");
                        isHeaderOpen = true;
                    }
                    hasAlreadyHeader = true;
                }
                else if (!hasBody)
                {
                    if (isHeaderOpen)
                    {
                        renderer.WriteLine("</thead>");
                        isHeaderOpen = false;
                    }

                    renderer.WriteLine("<tbody>");
                    hasBody = true;
                }

                renderer.WriteLine("<tr>");
                for (int i = 0; i < row.Count; i++)
                {
                    Block cellObj = row[i];
                    var cell = (TableCell)cellObj;

                    if (row.IsHeader && renderLabelAttribute)
                    {
                        _stripRenderer.Write(cell);
                        labels.Add(_stringWriter.ToString());
                        _stringWriter.GetStringBuilder().Length = 0;
                    }

                    renderer.
                        EnsureLine().
                        Write(row.IsHeader ? "<th" : "<td");

                    if (!row.IsHeader && renderLabelAttribute && i < labels.Count)
                    {
                        renderer.Write($" {flexiTableBlockOptions.LabelAttribute}=\"{labels[i]}\"");
                    }
                    if (cell.ColumnSpan != 1)
                    {
                        renderer.Write($" colspan=\"{cell.ColumnSpan}\"");
                    }
                    if (cell.RowSpan != 1)
                    {
                        renderer.Write($" rowspan=\"{cell.RowSpan}\"");
                    }
                    if (obj.ColumnDefinitions.Count > 0)
                    {
                        int columnIndex = cell.ColumnIndex < 0 || cell.ColumnIndex >= obj.ColumnDefinitions.Count
                            ? i
                            : cell.ColumnIndex;
                        columnIndex = columnIndex >= obj.ColumnDefinitions.Count ? obj.ColumnDefinitions.Count - 1 : columnIndex;
                        TableColumnAlign? alignment = obj.ColumnDefinitions[columnIndex].Alignment;
                        if (alignment.HasValue)
                        {
                            switch (alignment)
                            {
                                case TableColumnAlign.Center:
                                    renderer.Write(" style=\"text-align: center;\"");
                                    break;
                                case TableColumnAlign.Right:
                                    renderer.Write(" style=\"text-align: right;\"");
                                    break;
                                case TableColumnAlign.Left:
                                    renderer.Write(" style=\"text-align: left;\"");
                                    break;
                            }
                        }
                    }
                    renderer.
                        WriteAttributes(cell).
                        Write(">");

                    if (!row.IsHeader && renderWrapper)
                    {
                        renderer.Write($"<{flexiTableBlockOptions.WrapperElement}>");
                    }

                    bool previousImplicitParagraph = renderer.ImplicitParagraph;
                    if (cell.Count == 1)
                    {
                        renderer.ImplicitParagraph = true;
                    }

                    renderer.Write(cell);
                    renderer.ImplicitParagraph = previousImplicitParagraph;

                    if (!row.IsHeader && renderWrapper)
                    {
                        renderer.Write($"</{flexiTableBlockOptions.WrapperElement}>");
                    }
                    renderer.WriteLine(row.IsHeader ? "</th>" : "</td>");
                }
                renderer.WriteLine("</tr>");
            }

            if (hasBody)
            {
                renderer.WriteLine("</tbody>");
            }
            else if (isHeaderOpen)
            {
                renderer.WriteLine("</thead>");
            }
            renderer.WriteLine("</table>");
        }
    }
}
