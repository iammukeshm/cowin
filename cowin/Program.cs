using Blazored.LocalStorage;
using cowin.Managers.Preferences;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MudBlazor;
using MudBlazor.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Polly;

namespace cowin
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.Services.AddMudServices(configuration =>
            {
                configuration.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
                configuration.SnackbarConfiguration.HideTransitionDuration = 100;
                configuration.SnackbarConfiguration.ShowTransitionDuration = 100;
                configuration.SnackbarConfiguration.VisibleStateDuration = 3000;
                configuration.SnackbarConfiguration.ShowCloseIcon = false;
            });
            builder.Services
                .AddScoped(sp => sp
                .GetRequiredService<IHttpClientFactory>()
                .CreateClient("cowin-api"))
                .AddHttpClient("cowin-api", client => client.BaseAddress = new Uri("https://cdn-api.co-vin.in/"))
                .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(new[]
                           {
                                TimeSpan.FromSeconds(1),
                                TimeSpan.FromSeconds(3),
                                TimeSpan.FromSeconds(6)
                           }, (result, timeSpan, retryCount, context) => {
                               Console.WriteLine($"Request failed with {result.Result.StatusCode}. Retry count = {retryCount}. Waiting {timeSpan} before next retry.");
                           }));
            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddScoped<ClientPreferenceManager>();
            await builder.Build().RunAsync();
        }
    }
}
