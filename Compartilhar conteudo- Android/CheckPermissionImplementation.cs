using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Content.PM;
using Android.Content;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using ForcaDeVendasMobile.Dependencias;
using Xamarin.Forms; 

[assembly: Xamarin.Forms.Dependency(typeof(ForcaDeVendasMobile.Droid.CheckPermissionImplementation))]
namespace ForcaDeVendasMobile.Droid
{
    public class CheckPermissionImplementation : IPermission
    {
        #region readonly props
        private readonly int ResquestFineLocations = 200;
        private readonly int RequestStoragePermission = 300; 
        #endregion

        #region LocationPermission
        public bool CheckLocationPermission()
        {
            var mainActivity = Forms.Context as MainActivity;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {
                Permission location = mainActivity.CheckCallingPermission(Android.Manifest.Permission.AccessFineLocation);
                if (location < 0)
                {
                    string[] permission = { Android.Manifest.Permission.AccessFineLocation };

                    mainActivity.RequestPermissions(permission, ResquestFineLocations);
                }
            }

            return true;
        } 
        #endregion

        #region StoragePermission
        public bool CheckStoragePermisssion()
        {
            try
            {
                var mainActivity = Forms.Context as MainActivity;
                if (Build.VERSION.SdkInt >= BuildVersionCodes.NMr1)
                {

                    if (mainActivity.CheckSelfPermission(Android.Manifest.Permission.WriteExternalStorage) == Permission.Denied ||
                        mainActivity.CheckSelfPermission(Android.Manifest.Permission.ReadExternalStorage) == Permission.Denied)
                    {
                        return false;
                    }

                }

                return true;
            }
            catch (Exception e)
            {

                return false;
            }
        }

        public void RequestStoragePersmission()
        {
            var mainActivity = Forms.Context as MainActivity;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.NMr1)
            {

                if (mainActivity.CheckSelfPermission(Android.Manifest.Permission.WriteExternalStorage) == Permission.Denied ||
                       mainActivity.CheckSelfPermission(Android.Manifest.Permission.ReadExternalStorage) == Permission.Denied)
                {
                    string[] permission = { Android.Manifest.Permission.WriteExternalStorage, Android.Manifest.Permission.ReadExternalStorage };
                    mainActivity.RequestPermissions(permission, RequestStoragePermission);
                }
            }
        }

        #endregion

        #region BiometriaPermission
        public bool SuporteDigitalImplementation()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.NMr1)
                return true;
            else
                return false;
        } 
        #endregion
    }
}