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
using ForcaDeVendasMobile.Dependencias;
using ForcaDeVendasMobile.Droid;

[assembly: Xamarin.Forms.Dependency(typeof(ToastAndroid))]
namespace ForcaDeVendasMobile.Droid
{
    public class ToastAndroid : IToast
    { 
        public void Show(string Message, bool Long)
        {
            if (Long)
                Android.Widget.Toast.MakeText(Android.App.Application.Context, Message, ToastLength.Long).Show();
            else
                Android.Widget.Toast.MakeText(Android.App.Application.Context, Message, ToastLength.Short).Show();
        }
    }
}