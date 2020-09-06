using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Firebase.Database;
using GoToto_Rider.DataModels;
using GoToto_Rider.Helpers;
using Java.Util;

namespace GoToto_Rider.EventListeners
{
    public class CreateRequestEventListener : Java.Lang.Object, IValueEventListener
    {
        NewTripDetails newTrip;
        FirebaseDatabase database;
        DatabaseReference newTripRef;
        public void OnCancelled(DatabaseError error)
        {

        }

        public void OnDataChange(DataSnapshot snapshot)
        {

        }

        public CreateRequestEventListener(NewTripDetails mNewTrip)
        {
            newTrip = mNewTrip;
            database = AppDataHelper.GetDatabase();
        }

        public void CreateRequest()
        {
            newTripRef = database.GetReference("rideRequest").Push();

            HashMap location = new HashMap();
            location.Put("latitude", newTrip.PickupLat);
            location.Put("longitude", newTrip.PickupLng);

            HashMap destination = new HashMap();
            destination.Put("latitude", newTrip.DestinationLat);
            destination.Put("longitude", newTrip.DestinationLng);

            HashMap myTrip = new HashMap();

            newTrip.RideID = newTripRef.Key;
            myTrip.Put("location", location);
            myTrip.Put("destination", destination);
            myTrip.Put("destination_address", newTrip.DestinationAddress);
            myTrip.Put("pickup_address", newTrip.PickupAddress);
            myTrip.Put("rider_id", AppDataHelper.GetCurrentUser().Uid);
            myTrip.Put("payment_method", newTrip.Paymentmethod);
            myTrip.Put("created_at", newTrip.Timestamp.ToString());
            myTrip.Put("driver_id", "waiting");
            myTrip.Put("rider_name", AppDataHelper.GetFullName());
            myTrip.Put("rider_phone", AppDataHelper.GetPhone());

            newTripRef.AddValueEventListener(this);
            newTripRef.SetValue(myTrip);
        }

        public void CancelRequest()
        {
            newTripRef.RemoveEventListener(this);
            newTripRef.RemoveValue();
        }
    }
}