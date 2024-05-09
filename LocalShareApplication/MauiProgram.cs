using LocalShareApplication.Misc;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;

namespace LocalShareApplication
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                })
                .ConfigureLifecycleEvents(events =>
                {
#if WINDOWS
                    events.AddWindows(windows => windows
                           .OnClosed((window, args) => StopCommunication()));
#endif
#if ANDROID
                    events.AddAndroid(windows => windows
                           .OnStop((activity) => StopCommunication()));
#endif
                    static bool StopCommunication()
                    {
                        CommunicationManager.StopAll();
                        return true;
                    }
                });

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            var client = CommunicationManager.Client; // Start the client

            return builder.Build();
        }
    }
}
