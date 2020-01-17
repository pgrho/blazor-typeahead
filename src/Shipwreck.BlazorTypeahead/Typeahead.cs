using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Shipwreck.BlazorTypeahead
{
    public static class Typeahead
    {
        public static async Task<TypeaheadProxy<T>> TypeaheadAsync<T>(this IJSRuntime runtime, ElementReference element, TypeaheadOptions<T> options)
        {
            var proxy = new TypeaheadProxy<T>(runtime, element, options);
            await proxy.InitializeAsync().ConfigureAwait(false);
            return proxy;
        }
    }
}