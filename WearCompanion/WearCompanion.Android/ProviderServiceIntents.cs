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
    public class ProviderServiceIntents
    {
        /// <summary>
        ///     Intent action to start the service
        /// </summary>
        public const string Action_StartService = "START_SERVICE";

        /// <summary>
        ///     Intent action to stop the service
        /// </summary>
        public const string Action_StopService = "STOP_SERVICE";

        /// <summary>
        ///     Intent Send data to wearable
        /// </summary>
        public const string SendData = "SEND_DATA";
    }
}