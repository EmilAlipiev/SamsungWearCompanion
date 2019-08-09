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

namespace WearCompanion.Droid
{
    public class SaWearService
    {
        public void StartService()
        {
            var intent = new Intent(this, typeof(ProviderService));
            StartService(intent);
        }
    }
}