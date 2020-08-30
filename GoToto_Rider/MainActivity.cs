using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using System;
using Firebase.Database;
using Firebase;

namespace GoToto_Rider
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class MainActivity : AppCompatActivity
    {
        Button btnTestConnection;
        FirebaseDatabase database;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            btnTestConnection = (Button)FindViewById(Resource.Id.mybutton);
            btnTestConnection.Click += BtnTestConnecion_Click;
        }

        private void BtnTestConnecion_Click(object sender, EventArgs e)
        {
            InitialzeDatabase();
        }
        void InitialzeDatabase()
        {
            var app = FirebaseApp.InitializeApp(this);
            if (app == null)
            {
                var option = new FirebaseOptions.Builder()
                    .SetApplicationId("go-toto")
                    .SetApiKey("AIzaSyAAZ5JqcviM30nmGt0R5iYkVUsR8Jiikjc")
                    .SetDatabaseUrl("https://go-toto.firebaseio.com")
                    .SetStorageBucket("go-toto.appspot.com")
                    .Build();
                app = FirebaseApp.InitializeApp(this, option);
                database = FirebaseDatabase.GetInstance(app);
            }
            else
            {
                database = FirebaseDatabase.GetInstance(app);
            }
            DatabaseReference dbref = database.GetReference("UserSupport");
            dbref.SetValue("Ticket1");

            Toast.MakeText(this, "Completed", ToastLength.Short).Show();
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}