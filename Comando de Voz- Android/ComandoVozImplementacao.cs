using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Speech;
using Android.Views;
using Android.Widget;
using ForcaDeVendasMobile.Dependencias;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(ForcaDeVendasMobile.Droid.ComandoVozImplementacao))]
namespace ForcaDeVendasMobile.Droid
{
    public class ComandoVozImplementacao : IComandoDeVoz
    {        
        private readonly int VOICE = 10;
        public const int messageSpeakNow = 2130968580;         
        public async void Listen(string message)
        {  
            var voiceIntent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
            voiceIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);

            voiceIntent.PutExtra(RecognizerIntent.ExtraPrompt, message);//  Android.App.Application.Context.GetString(messageSpeakNow));
           
            voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 1500);
            voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 1500);
            voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 15000);
            voiceIntent.PutExtra(RecognizerIntent.ExtraMaxResults, 1);
            voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, "pt-BR");

            voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.Default);

            var mainActivity = Forms.Context as MainActivity;

            mainActivity.StartActivityForResult(voiceIntent, VOICE);                  
                       
        }
    }
}