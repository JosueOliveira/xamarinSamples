using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Speech;
using ForcaDeVendasMobile.Utils;
using Plugin.CurrentActivity;
using Plugin.Permissions;
using Xamarin.Forms;
using Plugin.CurrentActivity;
using Plugin.Fingerprint;

 
 
 

namespace ForcaDeVendasMobile.Droid
{
    [Activity(Label = "", Icon = "@drawable/Logo", Theme = "@style/MainTheme", MainLauncher = false, 
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private readonly int VOICE = 10;
        private readonly int ResquestFineLocations = 200;
        private readonly int ResquestFingerPrint = 300;
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            try
            {
                base.OnCreate(bundle);
                CrossFingerprint.SetCurrentActivityResolver(() => CrossCurrentActivity.Current.Activity);
                global::Xamarin.Forms.Forms.Init(this, bundle);
                StrictMode.VmPolicy.Builder builder = new StrictMode.VmPolicy.Builder();
                StrictMode.SetVmPolicy(builder.Build());
                LoadApplication(new App());
            }
            catch (Exception e)
            {
                Util.Alert(e.Message);
            }
        }

        protected override void OnStart()
        {
            base.OnStart();            

            //if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            //{
            //    Permission location = CheckSelfPermission(Android.Manifest.Permission.AccessFineLocation);
            //    if (location < 0)
            //    {                    
            //        string [] permission = { Android.Manifest.Permission.AccessFineLocation};

            //        RequestPermissions(permission, ResquestFineLocations);
            //    }                
            //}             
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == VOICE)
            {
                if (resultCode == Result.Ok)
                {
                    var matches = data.GetStringArrayListExtra(RecognizerIntent.ExtraResults);
                    if (matches.Count != 0)
                    {
                        string textInput = matches[0];

                        // limit the output to 500 characters
                        if (textInput.Length > 500)
                            textInput = textInput.Substring(0, 500);
                       
                        MessagingCenter.Send(textInput, "comandos");

                    } 

                }
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            
        }

    }
}

