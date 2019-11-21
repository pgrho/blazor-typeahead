using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

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

            using (var ms = new MemoryStream())
            using (var jw = new Utf8JsonWriter(ms))
            {
                jw.WriteStartArray();
                foreach (var e in items)
                {
                    jw.WriteStartObject();

                    jw.WriteString("name", e.Html);

                    jw.WriteNumber("hashCode", e.HashCode);

                    jw.WriteEndObject();
                }
                jw.WriteEndArray();

                jw.Flush();

                return Encoding.UTF8.GetString(ms.ToArray());
            }
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