# Shipwreck.BlazorTypeahead

Blazor wrapper for [Bootstrap 3 Typeahead](https://github.com/bassjobsen/Bootstrap-3-Typeahead)

## Usage

1. Reference [Shipwreck.BlazorTypeahead](https://www.nuget.org/packages/Shipwreck.BlazorTypeahead/) from nuget.org
2. Reference `Shipwreck.BlazorTypeahead.js` inside `Shipwreck.BlazorTypeahead` contents.
3. Add `<script>` references for jQuery and Bootstrap 3 or later in your Blazor HTML. (Bootstrap 3 Typeahead is embedded in the package.)

```csharp
// using Shipwreck.BlazorTypeahead;
// IJSRuntime js;
// ElementReference element;
js.TypeaheadAsync(element, new TypeaheadOptions<T>
{
    SourceCallback = QueryAsync,
});
```