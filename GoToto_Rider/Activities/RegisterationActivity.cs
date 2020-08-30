using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Gms.Tasks;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using GoToto_Rider.EventListeners;

namespace GoToto_Rider.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/goTotoTheme", MainLauncher = false)]
    public class RegisterationActivity : AppCompatActivity
    {
        TextInputLayout fullNameText;
        TextInputLayout phoneText;
        TextInputLayout emailText;
        TextInputLayout passwordText;
        CoordinatorLayout rootView;
        Button registerButton;

        FirebaseAuth mAuth;
        FirebaseDatabase database;
        TaskCompletionListener TaskCompletionListener = new TaskCompletionListener();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Register);
            // Create your application here
            InitializeFirebase();
            mAuth = FirebaseAuth.Instance;
            ConnectControl();
        }

        void ConnectControl()
        {
            fullNameText = (TextInputLayout)FindViewById(Resource.Id.fullNameText);
            phoneText = (TextInputLayout)FindViewById(Resource.Id.phoneText);
            emailText = (TextInputLayout)FindViewById(Resource.Id.emailText);
            passwordText = (TextInputLayout)FindViewById(Resource.Id.passwordText);
            rootView = (CoordinatorLayout)FindViewById(Resource.Id.rootView);
            registerButton = (Button)FindViewById(Resource.Id.registerButton);

            registerButton.Click += RegisterButton_Click;
        }

        private void RegisterButton_Click(object sender, EventArgs e)
        {
            string fullname, phone, email, password;

            fullname = fullNameText.EditText.Text;
            phone = phoneText.EditText.Text;
            email = emailText.EditText.Text;
            password = passwordText.EditText.Text;

            if (fullname.Length < 3)
            {
                Snackbar.Make(rootView, "Please enter a valid name", Snackbar.LengthShort).Show();
                return;
            }
            else if (phone.Length < 9)
            {
                Snackbar.Make(rootView, "Please enter a valid phone number", Snackbar.LengthShort).Show();
                return;
            }
            else if (!email.Contains("@"))
            {
                Snackbar.Make(rootView, "Please enter a valid email", Snackbar.LengthShort).Show();
                return;
            }
            else if (password.Length < 8)
            {
                Snackbar.Make(rootView, "Please enter a password upto 8 characters", Snackbar.LengthShort).Show();
                return;
            }
            RegisterUser(fullname, phone, email, password);
        }

        void InitializeFirebase()
        {
            var app = FirebaseApp.InitializeApp(this);

            if (app == null)
            {
                var options = new FirebaseOptions.Builder()

                    .SetApplicationId("go-toto")
                    .SetApiKey("AIzaSyAAZ5JqcviM30nmGt0R5iYkVUsR8Jiikjc")
                    .SetDatabaseUrl("https://go-toto.firebaseio.com")
                    .SetStorageBucket("go-toto.appspot.com")
                    .Build();

                app = FirebaseApp.InitializeApp(this, options);
                database = FirebaseDatabase.GetInstance(app);
            }
            else
            {
                database = FirebaseDatabase.GetInstance(app);
            }
        }

        void RegisterUser(string name, string phone, string email, string password)
        {
            TaskCompletionListener.Success += TaskCompletionListener_Success;
            TaskCompletionListener.Failure += TaskCompletionListener_Failure;

            mAuth.CreateUserWithEmailAndPassword(email, password)
                .AddOnSuccessListener(this, TaskCompletionListener)
                .AddOnFailureListener(this, TaskCompletionListener);

        }

        private void TaskCompletionListener_Failure(object sender, EventArgs e)
        {
            Snackbar.Make(rootView, "User Registration failed", Snackbar.LengthShort).Show();
        }

        private void TaskCompletionListener_Success(object sender, EventArgs e)
        {
            Snackbar.Make(rootView, "User Registration was Successful", Snackbar.LengthShort).Show();
        }
    }
}