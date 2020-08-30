using System.Threading;
using Android.App;
using Android.OS;

namespace GoToto_Rider.Activities
{
    [Activity(Label = "@String/app_name", Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory = true, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait )]
    public class SpalashActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Thread.Sleep(2000);
            // Create your application here
        }

        protected override void OnResume()
        {
            base.OnResume();
            StartActivity(typeof(RegisterationActivity));
        }
    }
}