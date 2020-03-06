using Microsoft.AspNetCore.Blazor.Hosting;
using System.Threading.Tasks;

namespace Shipwreck.BlazorTypeahead.Demo
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            return builder.Build().RunAsync();
        }
    }
}
