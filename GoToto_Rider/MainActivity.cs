using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using System;
using Firebase.Database;
using Firebase;
using Android.Views;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;

namespace GoToto_Rider
{
    [Activity(Label = "@string/app_name", Theme = "@style/goTotoTheme", MainLauncher = false)]
    public class MainActivity : AppCompatActivity, IOnMapReadyCallback
    {
        FirebaseDatabase database;
        Android.Support.V7.Widget.Toolbar mainToolbar;
        Android.Support.V4.Widget.DrawerLayout drawerLayout;

        GoogleMap mainMap;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            ConnectControl();

            SupportMapFragment mapFragment = (SupportMapFragment)SupportFragmentManager.FindFragmentById(Resource.Id.map);
            mapFragment.GetMapAsync(this);
        }

        void ConnectControl()
        {
            drawerLayout = (Android.Support.V4.Widget.DrawerLayout)FindViewById(Resource.Id.drawerLayout);

            mainToolbar = (Android.Support.V7.Widget.Toolbar)FindViewById(Resource.Id.mainToolbar);
            SetSupportActionBar(mainToolbar);
            SupportActionBar.Title = "";
            Android.Support.V7.App.ActionBar actionBar = SupportActionBar;
            actionBar.SetHomeAsUpIndicator(Resource.Mipmap.ic_menu_action);
            actionBar.SetDisplayHomeAsUpEnabled(true);

        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    drawerLayout.OpenDrawer((int)GravityFlags.Left);
                    return true;

                default:
                    return base.OnOptionsItemSelected(item);


            }
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

        public void OnMapReady(GoogleMap googleMap)
        {
            bool success = googleMap.SetMapStyle(MapStyleOptions.LoadRawResourceStyle(this, Resource.Raw.silvermapstyle));
            mainMap = googleMap;
        }
    }
}