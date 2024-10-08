using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using CommunityToolkit.Maui;

namespace AdminApp
{
		public static class MauiProgram
		{
				public static MauiApp CreateMauiApp()
				{
						var builder = MauiApp.CreateBuilder();
						builder
								.UseMauiApp<App>()
								.UseMauiCommunityToolkitMediaElement(); // Add this line

						return builder.Build();

#if DEBUG
						builder.Logging.AddDebug();
#endif

						return builder.Build();
				}
		}
}
