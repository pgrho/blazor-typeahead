using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace Shipwreck.BlazorTypeahead
{
    public static class Typeahead
    {
        private static readonly Dictionary<int, ITypeaheadProxy> _Proxies
            = new Dictionary<int, ITypeaheadProxy>();

        internal static void Register(ITypeaheadProxy proxy)
        {
            lock (_Proxies)
            {
                _Proxies[proxy.GetHashCode()] = proxy;
            }
        }

        internal static bool Unregister(ITypeaheadProxy proxy)
        {
            lock (_Proxies)
            {
                return _Proxies.Remove(proxy.GetHashCode());
            }
        }

        public static async Task<TypeaheadProxy<T>> TypeaheadAsync<T>(this IJSRuntime runtime, ElementReference element, TypeaheadOptions<T> options)
        {
            var proxy = new TypeaheadProxy<T>(runtime, element, options);
            await proxy.InitializeAsync().ConfigureAwait(false);
            return proxy;
        }

        [JSInvokable]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static async Task<string> GetJsonItemsAsync(int proxyHashCode, string text, int selectionStart, int selectionEnd)
        {
            ITypeaheadProxy proxy;
            lock (_Proxies)
            {
                if (!_Proxies.TryGetValue(proxyHashCode, out proxy))
                {
                    return "[]";
                }
            }

            var items = await proxy.QueryAsync(text, selectionStart, selectionEnd).ConfigureAwait(false);

            return JsonConvert.SerializeObject(items.Select(e => new
            {
                name = e.Html,
                hashCode = e.HashCode
            }));
        }

        [JSInvokable]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void OnItemSelected(int proxyHashCode, int itemHashCode)
        {
            ITypeaheadProxy proxy;
            lock (_Proxies)
            {
                if (!_Proxies.TryGetValue(proxyHashCode, out proxy))
                {
                    return;
                }
            }

            proxy?.AfterSelect(itemHashCode);
        }
    }
}