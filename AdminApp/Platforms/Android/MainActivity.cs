using Android.App;
using Android.Content.PM;
using Android.OS;
using Firebase;

namespace AdminApp
{
		[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
		public class MainActivity : MauiAppCompatActivity
		{
				protected override void OnCreate(Bundle savedInstanceState)
				{
						try
						{
								base.OnCreate(savedInstanceState);
								// Your initialization code here
						}
						catch (Exception ex)
						{
								Console.WriteLine($"Error in OnCreate: {ex.Message}");
								// Log the error or show an alert
						}
				}
		}
}
