# Changelog
This project uses [semantic versioning](http://semver.org/spec/v2.0.0.html). Refer to 
*[Semantic Versioning in Practice](https://www.jering.tech/articles/semantic-versioning-in-practice)*
for an overview of semantic versioning.

## [Unreleased](https://github.com/JeringTech/Markdig.Extensions.FlexiBlocks/compare/1.0.0-beta.0...HEAD)

## [1.0.0-beta.0](https://github.com/JeringTech/Markdig.Extensions.FlexiBlocks/compare/1.0.0-alpha.2...1.0.0-beta.0) - Nov 19, 2019
### Additions
- Added several HtmlRenderer extension methods. ([637475c](https://github.com/JeringTech/Markdig.Extensions.FlexiBlocks/commit/637475c1d9e6b369e1167563fc3bcfdd4836fc81))
### Changes
- Removed title attributes from all rendered HTML. Title attributes aren't accessible on many devices, e.g touch screen devices, so relying on
them is discouraged by the specs. ([d43cb4c](https://github.com/JeringTech/Markdig.Extensions.FlexiBlocks/commit/d43cb4c38b9d4ca21944d590ebb57c4c3696bec8))
- FlexiVideoBlock video element `preload` attribute set to `auto` instead of `none`. Edge doesn't play videos with `preload` `none` in HTML. ([a93740b](https://github.com/JeringTech/Markdig.Extensions.FlexiBlocks/commit/a93740b7cbd63c3a101e68c45fd348590d10580a))
- FlexiCodeBlock syntax highlighters upgraded to Prism 1.17.1 and Highlight.js 9.16.2.

## [1.0.0-alpha.2](https://github.com/JeringTech/Markdig.Extensions.FlexiBlocks/compare/1.0.0-alpha.1...1.0.0-alpha.2) - Nov 1, 2019
### Additions
- Added the following extensions:
  - FlexiVideoBlocks ([4db5d66](https://github.com/JeringTech/Markdig.Extensions.FlexiBlocks/commit/4db5d66b4380bc2c3be8093570659f960b2ade5e))
  - FlexiPictureBlocks ([575e73a](https://github.com/JeringTech/Markdig.Extensions.FlexiBlocks/commit/575e73aa0e193b5e3afb77b59188f6f226298acf))
- Added shared types for media blocks. ([b14f017](https://github.com/JeringTech/Markdig.Extensions.FlexiBlocks/commit/b14f017898ef62010df29bcdb75f95dac1b8680f))
### Changes
- FlexiIncludeBlocks ([5955dc2](https://github.com/JeringTech/Markdig.Extensions.FlexiBlocks/commit/5955dc2de01c13142f2fc755c96864771865c7df))
  - Renamed IncludeBlocks to FlexiIncludeBlocks.
  - Changed syntax from `+{...}` to `i{...}`.
- FlexiOptionsBlocks ([ba5940c](https://github.com/JeringTech/Markdig.Extensions.FlexiBlocks/commit/ba5940c7d59f7fdfdb8db3aa8af6bc12b3cf5345))
  - Renamed OptionsBlocks to FlexiOptionsBlocks.
  - Changed syntax from `@{...}` to `o{...}`.
- All `has/no/is/not_*` classes changed to `has/no/is/not-*`. They're now proper BEM boolean modifier classes.

## [1.0.0-alpha.1](https://github.com/JeringTech/Markdig.Extensions.FlexiBlocks/compare/1.0.0-alpha.0...1.0.0-alpha.1) - Sep 10, 2019
### Additions
- Added the following extensions:
  - FlexiQuoteBlocks
  - FlexiFigureBlocks
  - FlexiBannerBlocks
  - FlexiCardsBlocks
  - FlexiTabsBlocks
- Added shared types for multipart-blocks.
### Changes
- IncludeBlocks
  - `Clipping.Start` and `Clipping.End` renamed to `Clipping.StartLine` and `Clipping.EndLine`.
- FlexiCodeBlocks 
  - `LineRange.Start` and `LineRange.End` renamed to `LineRange.StartLine` and `LineRange.EndLine`.
  - `NumberedLineRange.Start` and `NumberedLineRange.End` renamed to `NumberedLineRange.StartLine` and `NumberedLineRange.EndLine`.
  - `PhraseGroup.Included` renamed to `PhraseGroup.IncludedMatches`.

## [1.0.0-alpha.0](https://github.com/JeringTech/Markdig.Extensions.FlexiBlocks/compare/0.15.0...1.0.0-alpha.0) - Aug 7, 2019
### Changes
- Breaking changes made throughout project. Major changes:
  - Generated HTML now has classes consistent with [BEM](https://en.bem.info/) for easier block styling.
  - Block options now have less verbose names. E.g "start" instead of "startNumber".
  - New features for several blocks.
  
  View [specs](https://github.com/JeringTech/Markdig.Extensions.FlexiBlocks/tree/master/specs) for updated
usage instructions.

## [0.15.0](https://github.com/JeringTech/Markdig.Extensions.FlexiBlocks/compare/0.14.0...0.15.0) - Jan 19, 2019
### Changes
- Reverted injection of `<br>` elements into empty lines in FlexiCodeBlocks. Use the following CSS to ensure empty lines
are displayed: `.line-text:empty:after { content: "\00a0" }`. ([73ede19](https://github.com/JeringTech/Markdig.Extensions.FlexiBlocks/commit/73ede19179cd81f91a55b22c9ae0da86fae4929a))
### Fixes
- Bumped Jering.IocServices.Newtonsoft.Json, Jering.Web.SyntaxHighlighters.HighlightJS and Jering.Web.SyntaxHighlighters.Prism.
This fixes some rare concurrency issues.

## [0.14.0](https://github.com/JeringTech/Markdig.Extensions.FlexiBlocks/compare/0.13.0...0.14.0) - Jan 2, 2019
### Additions
- Added `Region` property to the `Clipping` type. ([f85b9be](https://github.com/JeringTech/Markdig.Extensions.FlexiBlocks/commit/f85b9be0d6f1aa46b663477262f146e4e4b3dc3a))
### Changes
- FlexiTableBlocks now wraps `<table>` elements in `<div>`s. ([9d03887](https://github.com/JeringTech/Markdig.Extensions.FlexiBlocks/commit/9d038876df2b44feb142132dd841d2639008da53))
### Fixes
- Fixed FlexiSectionBlocks not processing inlines in heading blocks. ([73c013e](https://github.com/JeringTech/Markdig.Extensions.FlexiBlocks/commit/73c013edd25d66cdc5d55a3425ed0ae35703c578))
- Fixed FlexiCodeBlocks empty lines not containing anything. ([e8ff3e8](https://github.com/JeringTech/Markdig.Extensions.FlexiBlocks/commit/e8ff3e868ddf1b2dd3a276a550b2700818010a2f))
- Fixed FlexiSectionBlocks located immediately after lists being nested in preceding FlexiSectionBlocks regardless of level. ([03816db](https://github.com/JeringTech/Markdig.Extensions.FlexiBlocks/commit/eab77757c2686525944357550c968539dc7b1946))
- Fixed line embellishing done by FlexiCodeBlocks for markup fragments with multi-line elements. ([ff1c644](https://github.com/JeringTech/Markdig.Extensions.FlexiBlocks/commit/ff1c644784820df34ec06c8dbd5ab484cdfb16b4))

## [0.13.0](https://github.com/JeringTech/Markdig.Extensions.FlexiBlocks/compare/0.12.0...0.13.0) - Dec 7, 2018
### Changes
- `FlexiBlocksExtension.Setup` overloads are no longer overridable. `FlexiBlocksExtension` implementers should implement
`FlexiBlocksExtension.SetupParsers` and `FlexiBlocksExtension.SetupRenderers` instead.
- Renamed `Context` enum to `FlexiBlockExceptionContext`.
- `SourceRetrieverService.GetSource` now logs warning instead of debug messages when retrieval attempts fail.
### Fixes
- Fixed `NullReferenceException` thrown by `FlexiTableBlockRenderer` when a table has no head row. 

## [0.12.0](https://github.com/JeringTech/Markdig.Extensions.FlexiBlocks/compare/0.11.0...0.12.0) - Dec 3, 2018
### Changes
- `FlexiSectionBlockRenderer` is now a singleton service.
- Bumped bumped `Jering.Web.SyntaxHighlighters.HighlightJS` and `Jering.Web.SyntaxHighlighters.Prism`.
- Nuget package now includes source-linked symbols.
- Changed target frameworks from `netstandard2.0` and `netstandard1.3` to `netstandard2.0` and `net461`.
- Updated Nuget package metadata.
- Improved `FlexiBlocksMarkdownPipelineBuilderExtensions`
  - Removed its constructor and members `GetServiceProvider` and `SetDefaultServiceProvider`.
  - Added members `GetOrCreateServiceProvider`, `DisposeServiceProvider` and `Configure<TOptions>`.
- FlexiBlocksException constructor no longer throws an `ArgumentNullException`.
### Fixes
- Made `FlexiCodeBlockRenderer` thread safe.

## [0.11.0](https://github.com/JeringTech/Markdig.Extensions.FlexiBlocks/compare/0.10.0...0.11.0) - Oct 18, 2018
### Additions
- Added methods `FlexiIncludeBlocksExtension.GetFlexiIncludeBlockTrees` and `FlexiIncludeBlocksExtension.GetIncludedSourcesAbsoluteUris`.
These methods report the depedencies of a processed markdown document.
### Changes
- Cleaned up architecture for extension options. 
- Minor changes to Nuget package title and description.

## [0.10.0](https://github.com/JeringTech/Markdig.Extensions.FlexiBlocks/compare/0.9.0...0.10.0) - Oct 15, 2018
### Additions
- FlexiCodeBlocks now always renders at least two `<span>`s for each line of code. One with class `line` and
one with class `line-text`.
- FlexiCodeBlocks now renders an icon to represent hidden lines when line numbers aren't contiguous.
- FlexiCodeBlocks now renders copy icon within a `<button>` element.
- FlexSectionBlocks now renders link icon within a `<button>` element.
### Changes
- Renamed `FlexiCodeBlockOptions.LineNumbers` to `FlexiCodeBlockOptions.LineNumbers`. This
reflects under the hood changes to the type that the list contains.
- FlexiBlocksException no longer appends "Flexi" to block type names that do not begin with "Flexi".

## [0.9.0](https://github.com/JeringTech/Markdig.Extensions.FlexiBlocks/compare/0.8.0...0.9.0) - Oct 12, 2018
### Fixes
- Fixed a FlexiSectionBlockParser bug that was causing it to consume the leading whitespace of every line.

## [0.8.0](https://github.com/JeringTech/Markdig.Extensions.FlexiBlocks/compare/0.7.0...0.8.0) - Oct 11, 2018
### Additions
- FlexiTableBlocks now have a default class, "flexi-table-block", assigned to their outermost elements.
### Changes
- Replaced "fab" with "flexi-alert-block" in FlexiAlertBlock class names.
- Replaced "fcb" with "flexi-code-block" in FlexiCodeBlock class names.
- Replaced "section-level" with "flexi-section-block" in FlexiSectionBlock class names.

## [0.7.0](https://github.com/JeringTech/Markdig.Extensions.FlexiBlocks/compare/0.6.0...0.7.0) - Oct 10, 2018
### Additions
- FlexiCodeBlocks now have a default class, "flexi-code-block", assigned to their outermost elements.

## [0.6.0](https://github.com/JeringTech/Markdig.Extensions.FlexiBlocks/compare/0.5.0...0.6.0) - Oct 10, 2018
### Additions
- Exposed the `ServiceProvider` used by `FlexiBlocksMarkdownPipelineBuilderExtensions`.

## [0.5.0](https://github.com/JeringTech/Markdig.Extensions.FlexiBlocks/compare/0.4.0...0.5.0) - Oct 4, 2018
### Changes
- Bumped `Jering.Web.SyntaxHighlighters.HighlightJS` and `Jering.Web.SyntaxHighlighters.HighlightJS`.
### Fixes
- Fixed Nuget package description formatting.

## [0.4.0](https://github.com/JeringTech/Markdig.Extensions.FlexiBlocks/compare/0.3.0...0.4.0) - Sep 29, 2018
### Changes
- Improved Nuget package description, added a title for the package.
### Fixes
- Fixed inherited intellisense comments not appearing when using the netstandard1.3 assembly.
- Fixed some tests getting skipped.

## [0.3.0](https://github.com/JeringTech/Markdig.Extensions.FlexiBlocks/compare/0.2.0...0.3.0) - Sep 29, 2018
### Changes
- Solution-wide cleanup.

## [0.2.0](https://github.com/JeringTech/Markdig.Extensions.FlexiBlocks/compare/0.1.0...0.2.0) - Jul 25, 2018
### Changes
- Bumped syntax highlighter versions.

## [0.1.0](https://github.com/JeringTech/Markdig.Extensions.FlexiBlocks/compare/0.1.0...0.1.0) - Jul 3, 2018
Initial release.
