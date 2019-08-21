using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using ForcaDeVendasMobile.Dependencias;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(ForcaDeVendasMobile.Droid.DepenciasAndroid.SharedContent))]
namespace ForcaDeVendasMobile.Droid.DepenciasAndroid
{
    public class SharedContent : ISharedContent
    {
        public void SharedHtml(string html, string name)
        {
            Android.Net.Uri uri = ReadFile(name);
            if (uri != null)
            {
                Intent sharingIntent = new Intent(Intent.ActionSend);
                sharingIntent.SetType("files/html");                
                sharingIntent.PutExtra(Intent.ExtraStream, uri);
                var mainActivity = Forms.Context as MainActivity;

                mainActivity.StartActivity(Intent.CreateChooser(sharingIntent, "Compartilhamento"));
            }
            else
            {
                Utils.Util.Toast("Relatório não encontrado");
            }
        }
        public void SaveFile(string html, string name)
        {
            var path = global::Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath;
            var fileName = Path.Combine(path.ToString(), name + ".html");

            using (var writer = System.IO.File.CreateText(fileName))
            {
                writer.WriteLine(html);
            }
        }

        private Android.Net.Uri ReadFile(string name)
        {
            try
            {
                Java.IO.File root = global::Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads);
                string pathAttached = name + ".html";
                Java.IO.File file = new Java.IO.File(root, pathAttached);
                return Android.Net.Uri.FromFile(file);
            }
            catch (Exception)
            {

                return null;
            }
        }
    }
}