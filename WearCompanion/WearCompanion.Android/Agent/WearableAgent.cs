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

namespace WearCompanion.Droid.Agent
{
    public class WearableAgent : IWearableAgent
    {
        public ContextWrapper ContextWrapper { get; set; }

        public void StartService()
        {
            var wearableServiceIntent = new Intent(Application.Context, typeof(ProviderService));
            wearableServiceIntent.SetAction(ProviderServiceIntents.Action_StartService);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                ContextWrapper.StartForegroundService(wearableServiceIntent);
            }
            else
            {
                ContextWrapper.StartService(wearableServiceIntent);
            }
        }

        public void StopService()
        {
            var wearableServiceIntent = new Intent(Application.Context, typeof(ProviderService));
            wearableServiceIntent.SetAction(ProviderServiceIntents.Action_StopService);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                ContextWrapper.StartForegroundService(wearableServiceIntent);
            }
            else
            {
                ContextWrapper.StartService(wearableServiceIntent);
            }
}

        public void SendMessage(string message)
        {
            Intent  intent = new Intent(Application.Context, typeof(ProviderService));
            intent.PutExtra(ProviderServiceIntents.SendData, message);
            Application.Context.StartService(intent);
        }
    }
}