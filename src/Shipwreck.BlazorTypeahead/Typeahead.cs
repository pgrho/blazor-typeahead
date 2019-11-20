using System.Collections.Generic;
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