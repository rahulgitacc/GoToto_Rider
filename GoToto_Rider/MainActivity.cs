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
using Android;
using Android.Support.V4.App;
using Android.Gms.Location;
using GoToto_Rider.Helpers;
using Android.Content;
using Google.Places;
using System.Collections.Generic;
using Android.Graphics;
using Android.Support.Design.Widget;
using GoToto_Rider.EventListeners;
using GoToto_Rider.Fragments;
using GoToto_Rider.DataModels;

namespace GoToto_Rider
{
    [Activity(Label = "@string/app_name", Theme = "@style/goTotoTheme", MainLauncher = false)]
    public class MainActivity : AppCompatActivity, IOnMapReadyCallback
    {

        //Firebase
        UserProfileEventListener profileEventListener = new UserProfileEventListener();
        CreateRequestEventListener requestListener;

        //Views
        Android.Support.V7.Widget.Toolbar mainToolbar;
        Android.Support.V4.Widget.DrawerLayout drawerLayout;

        GoogleMap mainMap;

        //TextViews
        TextView pickupLocationText;
        TextView destinationText;

        //Buttons
        Button favouritePlacesButton;
        Button locationSetButton;
        Button requestDriverButton;
        RadioButton pickupRadio;
        RadioButton destinationRadio;

        //Imageview
        ImageView centerMarker;

        //Layouts
        RelativeLayout layoutPickUp;
        RelativeLayout layoutDestination;

        //Bottomsheets
        BottomSheetBehavior tripDetailsBottonsheetBehavior;

        //Fragments
        RequestDriver requestDriverFragment;

        readonly string[] permissionGroupLocation = { Manifest.Permission.AccessFineLocation, Manifest.Permission.AccessCoarseLocation };
        const int requestLocationId = 0;

        LocationRequest mLocationRequest;
        FusedLocationProviderClient locationClient;
        Android.Locations.Location mLastLocation;
        LocationCallbackHelper mLocationCallback;

        static int UPDATE_INTERVAL = 5; //5 SECONDS
        static int FASTEST_INTERVAL = 5;
        static int DISPLACEMENT = 3; //meters

        //Helpers
        MapFunctionHelper mapHelper;

        //TripDetails
        LatLng pickupLocationLatlng;
        LatLng destinationLatLng;
        string pickupAddress;
        string destinationAddress;

        //DataModels
        NewTripDetails newTripDetails;

        //Flags
        int addressRequest = 1;
        bool takeAddressFromSearch;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            ConnectControl();

            SupportMapFragment mapFragment = (SupportMapFragment)SupportFragmentManager.FindFragmentById(Resource.Id.map);
            mapFragment.GetMapAsync(this);
            CheckLocationPermission();
            CreateLocationRequest();
            GetMyLocation();
            StartLocationUpdates();
            InitializePlaces();
            profileEventListener.Create();
        }

        void ConnectControl()
        {
            //DrawerLayout
            drawerLayout = (Android.Support.V4.Widget.DrawerLayout)FindViewById(Resource.Id.drawerLayout);

            //ToolBar
            mainToolbar = (Android.Support.V7.Widget.Toolbar)FindViewById(Resource.Id.mainToolbar);
            SetSupportActionBar(mainToolbar);
            SupportActionBar.Title = "";
            Android.Support.V7.App.ActionBar actionBar = SupportActionBar;
            actionBar.SetHomeAsUpIndicator(Resource.Mipmap.ic_menu_action);
            actionBar.SetDisplayHomeAsUpEnabled(true);

            //TextView 
            pickupLocationText = (TextView)FindViewById(Resource.Id.pickupLocationText);
            destinationText = (TextView)FindViewById(Resource.Id.destinationText);

            favouritePlacesButton = (Button)FindViewById(Resource.Id.favouritePlacesButton);
            locationSetButton = (Button)FindViewById(Resource.Id.locationsSetButton);
            requestDriverButton = (Button)FindViewById(Resource.Id.requestDriverButton);
            pickupRadio = (RadioButton)FindViewById(Resource.Id.pickupRadio);
            destinationRadio = (RadioButton)FindViewById(Resource.Id.DestinationRadio);
            pickupRadio.Click += PickupRadio_Click;
            destinationRadio.Click += DestinationRadio_Click;
            requestDriverButton.Click += RequestDriverButton_Click;
            favouritePlacesButton.Click += FavouritePlacesButton_Click;
            locationSetButton.Click += LocationSetButton_Click;

            //Layouts
            layoutPickUp = (RelativeLayout)FindViewById(Resource.Id.layoutPickUp);
            layoutDestination = (RelativeLayout)FindViewById(Resource.Id.layoutDestination);

            layoutPickUp.Click += LayoutPickUp_Click;
            layoutDestination.Click += LayoutDestination_Click;

            //Imageview
            centerMarker = (ImageView)FindViewById(Resource.Id.centerMarker);

            //Bottomsheet
            FrameLayout tripDetailsView = (FrameLayout)FindViewById(Resource.Id.tripdetails_bottomsheet);
            tripDetailsBottonsheetBehavior = BottomSheetBehavior.From(tripDetailsView);
        }

        private void RequestDriverButton_Click(object sender, EventArgs e)
        {
            requestDriverFragment = new RequestDriver(mapHelper.EstimateFares());
            requestDriverFragment.Cancelable = false;
            var trans = SupportFragmentManager.BeginTransaction();
            requestDriverFragment.Show(trans, "Request");
            requestDriverFragment.CancelRequest += RequestDriverFragment_CancelRequest;

            newTripDetails = new NewTripDetails();
            newTripDetails.DestinationAddress = destinationAddress;
            newTripDetails.PickupAddress = pickupAddress;
            newTripDetails.DestinationLat = destinationLatLng.Latitude;
            newTripDetails.DestinationLng = destinationLatLng.Longitude;
            newTripDetails.DistanceString = mapHelper.distanceString;
            newTripDetails.DistanceValue = mapHelper.distance;
            newTripDetails.DurationString = mapHelper.durationstring;
            newTripDetails.DurationValue = mapHelper.duration;
            newTripDetails.EstimateFare = mapHelper.EstimateFares();
            newTripDetails.Paymentmethod = "cash";
            newTripDetails.PickupLat = pickupLocationLatlng.Latitude;
            newTripDetails.PickupLng = pickupLocationLatlng.Longitude;
            newTripDetails.Timestamp = DateTime.Now;

            requestListener = new CreateRequestEventListener(newTripDetails);
            requestListener.CreateRequest();
        }

        void TripLocationsSet()
        {
            favouritePlacesButton.Visibility = ViewStates.Invisible;
            locationSetButton.Visibility = ViewStates.Visible;
        }

        private async void LocationSetButton_Click(object sender, EventArgs e)
        {
            locationSetButton.Text = "Please wait...";
            locationSetButton.Enabled = false;

            string json;
            json = await mapHelper.GetDirectionJsonAsync(pickupLocationLatlng, destinationLatLng);

            if (!string.IsNullOrEmpty(json))
            {
                TextView txtFare = (TextView)FindViewById(Resource.Id.tripEstimateFareText);
                TextView txtTime = (TextView)FindViewById(Resource.Id.newTripTimeText);

                mapHelper.DrawTripOnMap(json);

                // Set Estimate Fares and Time
                txtFare.Text = "₹" + mapHelper.EstimateFares().ToString() + " - " + (mapHelper.EstimateFares() + 10).ToString();
                txtTime.Text = mapHelper.durationstring;

                //Display BottomSheet
                tripDetailsBottonsheetBehavior.State = BottomSheetBehavior.StateExpanded;

                //DisableViews
                TripDrawnOnMap();
            }

            locationSetButton.Text = "Done";
            locationSetButton.Enabled = true;
        }

        private void FavouritePlacesButton_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void DestinationRadio_Click(object sender, EventArgs e)
        {
            addressRequest = 1;
            pickupRadio.Checked = true;
            destinationRadio.Checked = false;
            takeAddressFromSearch = false;
            centerMarker.SetColorFilter(Color.DarkGreen);
        }

        private void PickupRadio_Click(object sender, EventArgs e)
        {
            addressRequest = 2;
            destinationRadio.Checked = true;
            pickupRadio.Checked = false;
            takeAddressFromSearch = false;
            centerMarker.SetColorFilter(Color.Red);
        }

        void LayoutPickUp_Click(object sender, System.EventArgs e)
        {
            List<Place.Field> field = new List<Place.Field>();
            field.Add(Place.Field.Id);
            field.Add(Place.Field.Name);
            field.Add(Place.Field.LatLng);
            field.Add(Place.Field.Address);

            Intent intent = new Autocomplete.IntentBuilder(AutocompleteActivityMode.Overlay, field)
                .SetCountry("IN")
                .Build(this);

            StartActivityForResult(intent, 1);
        }

        void LayoutDestination_Click(object sender, System.EventArgs e)
        {

            List<Place.Field> field = new List<Place.Field>();
            field.Add(Place.Field.Id);
            field.Add(Place.Field.Name);
            field.Add(Place.Field.LatLng);
            field.Add(Place.Field.Address);

            Intent intent = new Autocomplete.IntentBuilder(AutocompleteActivityMode.Overlay, field)
                .SetCountry("IN")
                .Build(this);

            StartActivityForResult(intent, 2);
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

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            
            if (grantResults.Length < 1)
            {
                return;
            }
            if (grantResults[0] == (int)Android.Content.PM.Permission.Granted)
            {
                StartLocationUpdates();
            }
            else
            {
                Toast.MakeText(this, "Permission was denied", ToastLength.Short).Show();
            }
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            bool success = googleMap.SetMapStyle(MapStyleOptions.LoadRawResourceStyle(this, Resource.Raw.silvermapstyle));
            mainMap = googleMap;
            mainMap.CameraIdle += MainMap_CameraIdle;
            string mapkey = Resources.GetString(Resource.String.mapkey);
            mapHelper = new MapFunctionHelper(mapkey, mainMap);
        }

        void TripDrawnOnMap()
        {
            layoutDestination.Clickable = false;
            layoutPickUp.Clickable = false;
            pickupRadio.Enabled = false;
            destinationRadio.Enabled = false;
            takeAddressFromSearch = true;
            centerMarker.Visibility = ViewStates.Invisible;
        }

        private async void MainMap_CameraIdle(object sender, EventArgs e)
        {
            if (!takeAddressFromSearch)
            {
                if (addressRequest == 1)
                {
                    pickupLocationLatlng = mainMap.CameraPosition.Target;
                    pickupAddress = await mapHelper.FindCordinateAddress(pickupLocationLatlng);
                    pickupLocationText.Text = pickupAddress;
                }
                else if (addressRequest == 2)
                {
                    destinationLatLng = mainMap.CameraPosition.Target;
                    destinationAddress = await mapHelper.FindCordinateAddress(destinationLatLng);
                    destinationText.Text = destinationAddress;
                    TripLocationsSet();
                }
            }
        }

        bool CheckLocationPermission()
        {
            bool permissionGranted = false;

            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Android.Content.PM.Permission.Granted &&
                ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) != Android.Content.PM.Permission.Granted)
            {
                permissionGranted = false;
                RequestPermissions(permissionGroupLocation, requestLocationId);
            }
            else
            {
                permissionGranted = true;
            }

            return permissionGranted;
        }

        async void GetMyLocation()
        {
            if (!CheckLocationPermission())
            {
                return;
            }

            mLastLocation = await locationClient.GetLastLocationAsync();
            if (mLastLocation != null)
            {
                LatLng myposition = new LatLng(mLastLocation.Latitude, mLastLocation.Longitude);
                mainMap.MoveCamera(CameraUpdateFactory.NewLatLngZoom(myposition, 17));
            }
        }

        void CreateLocationRequest()
        {
            mLocationRequest = new LocationRequest();
            mLocationRequest.SetInterval(UPDATE_INTERVAL);
            mLocationRequest.SetFastestInterval(FASTEST_INTERVAL);
            mLocationRequest.SetPriority(LocationRequest.PriorityHighAccuracy);
            mLocationRequest.SetSmallestDisplacement(DISPLACEMENT);
            locationClient = LocationServices.GetFusedLocationProviderClient(this);
            mLocationCallback = new LocationCallbackHelper();
            mLocationCallback.MyLocation += MLocationCallback_MyLocation;
        }

        private void MLocationCallback_MyLocation(object sender, LocationCallbackHelper.OnLocationCapturedEventArgs e)
        {
            mLastLocation = e.Location;
            LatLng myposition = new LatLng(mLastLocation.Latitude, mLastLocation.Longitude);
            mainMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(myposition, 12));
        }

        void StartLocationUpdates()
        {
            if (CheckLocationPermission())
            {
                locationClient.RequestLocationUpdates(mLocationRequest, mLocationCallback, null);
            }
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Android.App.Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == 1)
            {
                if (resultCode == Android.App.Result.Ok)
                {
                    takeAddressFromSearch = true;
                    pickupRadio.Checked = false;
                    destinationRadio.Checked = false;

                    var place = Autocomplete.GetPlaceFromIntent(data);
                    pickupLocationText.Text = place.Name.ToString();
                    pickupAddress = place.Name.ToString();
                    pickupLocationLatlng = place.LatLng;
                    mainMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(place.LatLng, 15));
                    centerMarker.SetColorFilter(Color.DarkGreen);
                }
            }

            if (requestCode == 2)
            {
                if (resultCode == Android.App.Result.Ok)
                {
                    takeAddressFromSearch = true;
                    pickupRadio.Checked = false;
                    destinationRadio.Checked = false;

                    var place = Autocomplete.GetPlaceFromIntent(data);
                    destinationText.Text = place.Name.ToString();
                    destinationAddress = place.Name.ToString();
                    destinationLatLng = place.LatLng;
                    mainMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(place.LatLng, 15));
                    centerMarker.SetColorFilter(Color.Red);
                    TripLocationsSet();
                }
            }
        }

        void InitializePlaces()
        {
            var mapkey = Resources.GetString(Resource.String.mapkey);
            if (!PlacesApi.IsInitialized)
            {
                PlacesApi.Initialize(this, mapkey);
            }
        }

        void RequestDriverFragment_CancelRequest(object sender, EventArgs e)
        {
            //User cancels request before driver accepts it
            if (requestDriverFragment != null && requestListener != null)
            {
                requestListener.CancelRequest();
                requestListener = null;
                requestDriverFragment.Dismiss();
                requestDriverFragment = null;
            }
            StartActivity(typeof(MainActivity));
        }
    }
}