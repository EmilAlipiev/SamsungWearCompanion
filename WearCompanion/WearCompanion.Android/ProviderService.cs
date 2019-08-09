using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using Com.Samsung.Accessory;
using Com.Samsung.Android.Sdk;
using Com.Samsung.Android.Sdk.Accessory;

[assembly: Xamarin.Forms.Dependency(typeof(WearCompanion.Droid.ProviderService))]
namespace WearCompanion.Droid
{
    public class ProviderService : SAAgent
    {
        public Action<string> _onComplete;
        public static readonly string TAG = typeof(ProviderService).Name;
        public static readonly Java.Lang.Class SASOCKET_CLASS = Java.Lang.Class.FromType(typeof(ProviderServiceSocket)).Class;
        public IBinder mBinder { get; private set; }
        public ProviderServiceSocket mSocketServiceProvider = null;
        Handler mHandler = new Handler();
        private Context _context;
        private bool _isRunning;
        private Task _task;
        private static readonly int CHANNEL_ID = 104;

        public ProviderService() : base("ProviderService", SASOCKET_CLASS)
        {

        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            _isRunning = false;

            if (_task != null && _task.Status == TaskStatus.RanToCompletion)
            {
                _task.Dispose();
            }
        }
        public override IBinder OnBind(Intent intent)
        {
            // This method must always be implemented
            Android.Util.Log.Debug(TAG, "OnBind");
            this.mBinder = new AgentBinder(this);
            return this.mBinder;
        }

        public override bool OnUnbind(Intent intent)
        {
            // This method is optional to implement
            Android.Util.Log.Debug(TAG, "OnUnbind");
            return base.OnUnbind(intent);
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {      
            FindPeerAgents();

            return base.OnStartCommand(intent, flags, startId);
        }

        public override void OnCreate()
        {
            base.OnCreate();

            if ((Build.VERSION.SdkInt >= BuildVersionCodes.O))
            {
                NotificationManager notificationManager = null;
                String channel_id = "sample_channel_01";
                if ((notificationManager == null))
                {
                    String channel_name = "Accessory_SDK_Sample";
                    notificationManager = ((NotificationManager)(GetSystemService(Context.NotificationService)));
                    NotificationChannel notiChannel = new NotificationChannel(channel_id, channel_name, NotificationManager.ImportanceLow);
                    notificationManager.CreateNotificationChannel(notiChannel);
                }

                int notifyID = 1;

                NotificationCompat.Builder builder = new NotificationCompat.Builder(Application.Context)
                .SetAutoCancel(true)
                .SetContentTitle(TAG)
                .SetContentText("connected to your watch")
                .SetPriority(1)
                .SetVisibility(1)
                .SetCategory(Android.App.Notification.CategoryEvent);
                StartForeground(notifyID, builder.Build());
            }


            _context = this;
            _isRunning = false;
         
            var mAccessory = new SA();
            try
            {
                mAccessory.Initialize(this);
            }
            catch (SsdkUnsupportedException e)
            {
                // try to handle SsdkUnsupportedException
            }
            catch (Exception e1)
            {
                e1.ToString();

                /*
                * Your application can not use Samsung Accessory SDK. Your application should work smoothly
                * without using this SDK, or you may want to notify user and close your application gracefully
                * (release resources, stop Service threads, close UI thread, etc.)
                */
                StopSelf();
            }

            var isFeatureEnabled = mAccessory.IsFeatureEnabled(SA.DeviceAccessory);

          

        }

        //private void DoWork(string msg)
        //{
        //    try
        //    {
        //        FindPeers(msg);


        //    }
        //    catch (Exception e)
        //    {
              
        //    }
        //    finally
        //    {
        //        StopSelf();
        //    }
        //}


        public string Message { get; set; }
       
        protected override void OnFindPeerAgentsResponse(SAPeerAgent[] p0, int result)
        {
#if DEBUG
            Console.WriteLine(TAG, "onFindPeerAgentResponse : result =" + result);
#endif
            if (result == PeerAgentFound)
            {
                foreach (var peerAgent in p0)
                {
                    //  Cache(peerAgent);
                    RequestServiceConnection(peerAgent);
                }
            }
        }

        protected override void OnServiceConnectionRequested(SAPeerAgent p0)
        {
            if (p0 != null)
                AcceptServiceConnectionRequest(p0);
        }

        protected override void OnServiceConnectionResponse(SAPeerAgent p0, SASocket socket, int result)
        {
            // Cache(socket);
            if ((result == SAAgent.ConnectionSuccess))
            {
                if ((socket != null))
                {
                    mSocketServiceProvider = ((ProviderServiceSocket)(socket));
                    mSocketServiceProvider.Send(CHANNEL_ID, System.Text.Encoding.ASCII.GetBytes(Message));
                }

            }
            else if ((result == SAAgent.ConnectionAlreadyExist))
            {
#if DEBUG
                Console.WriteLine("onServiceConnectionResponse, CONNECTION_ALREADY_EXIST");
#endif

            }

        }

        private bool processUnsupportedException(SsdkUnsupportedException e)
        {
#if DEBUG
            Console.WriteLine(e.ToString());
#endif

            if (e.Equals(SsdkUnsupportedException.VendorNotSupported)
                        || e.Equals(SsdkUnsupportedException.DeviceNotSupported))
            {
                StopSelf();
            }
            else if (e.Equals(SsdkUnsupportedException.LibraryNotInstalled))
            {
#if DEBUG
                Console.WriteLine(TAG + "You need to install Samsung Accessory SDK to use this application.");
#endif

            }
            else if (e.Equals(SsdkUnsupportedException.LibraryUpdateIsRequired))
            {
#if DEBUG
                Console.WriteLine(TAG + "You need to update Samsung Accessory SDK to use this application.");

#endif
            }
            else if (e.Equals(SsdkUnsupportedException.LibraryUpdateIsRecommended))
            {
#if DEBUG
                Console.WriteLine(TAG + TAG, "We recommend that you update your Samsung Accessory SDK before using this application.");

#endif
                return false;
            }

            return true;
        }

        public bool CloseConnection()
        {
            if ((mSocketServiceProvider != null))
            {
                mSocketServiceProvider.Close();
                mSocketServiceProvider = null;
                return true;
            }
            else
            {
                return false;
            }

        }

        public class AgentBinder : Binder
        {
            public AgentBinder(ProviderService service)
            {
                this.Service = service;
            }

            public ProviderService Service { get; private set; }
        }
        public class ProviderServiceSocket : SASocket
        {

            public ProviderServiceSocket() : base(p0: "ProviderServiceSocket")
            {

            }


            public override void OnReceive(int channelId, byte[] bytes)
            {
                // Check received data 
                string message = System.Text.Encoding.UTF8.GetString(bytes);
#if DEBUG
                Console.WriteLine("Received: ", message);

#endif
            }

            protected override void OnServiceConnectionLost(int p0)
            {
                // ResetCache();
                Close();
            }

            public override void OnError(int p0, string p1, int p2)
            {

                // Error handling
            }
        }

    }

}