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
using Java.Interop;
using static Android.OS.PowerManager;

namespace WearCompanion.Droid
{
    [Service(Exported = true, Name = "WearCompanion.Droid.ProviderService")]
    public class ProviderService : SAAgent
    {
        /// <summary>
        ///     The service running notification identifier
        /// </summary>
        private const int NotificationId = 999999;

        /// <summary>
        ///     Determine if the service is started
        /// </summary>
        private bool _isServiceStarted;

        /// <summary>
        ///     Determine if the service is started as a foreground service
        /// </summary>
        private bool _isServiceStartedInForeground;

        private readonly string _uniqueChannelId = "ff20e147-fcd3-43bb-9116-0b89f6f07704";
        private readonly string _channelName = "ServiceCoachServiceChannel";
        private const NotificationImportance Importance = NotificationImportance.Low;


        public static readonly string TAG = typeof(ProviderService).Name;
        public static readonly Java.Lang.Class SASOCKET_CLASS = Java.Lang.Class.FromType(typeof(ProviderServiceSocket)).Class;
        public static readonly int CHANNEL_ID = 104;

        //private readonly string _wakeLockTag = "nl.ns.servicecoach.FOREGROUNDSERVER";
        //private PowerManager _powerManager;
        //private WakeLock _wakeLock;

        private readonly Task _task;

        public Action<string> _onComplete;
        public ProviderServiceSocket _mSocketServiceProvider = null;

        public IBinder mBinder { get; private set; }

        [Export(SuperArgumentsString = "\"ProviderService\", ProviderService_ProviderServiceSocket.class")]
        public ProviderService() : base("ProviderService", SASOCKET_CLASS)
        {

        }

        public override void OnCreate()
        {
            Android.Util.Log.Debug(TAG, "ProviderService.OnCreate");

            base.OnCreate();

            var mAccessory = new SA();
            try
            {
                mAccessory.Initialize(this);
            }
            catch (SsdkUnsupportedException)
            {
                // try to handle SsdkUnsupportedException
            }
            catch (Exception ex)
            {
                ex.ToString();

                /*
                * Your application can not use Samsung Accessory SDK. Your application should work smoothly
                * without using this SDK, or you may want to notify user and close your application gracefully
                * (release resources, stop Service threads, close UI thread, etc.)
                */
                StopSelf();
            }

            _isServiceStarted = true;

            //_powerManager = (PowerManager)GetSystemService(PowerService);
            //_wakeLock = _powerManager.NewWakeLock(WakeLockFlags.Partial, _wakeLockTag);
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            Task.Run(() => HandleCommandAsync(intent));

            return base.OnStartCommand(intent, flags, startId);
        }

        private void HandleCommandAsync(Intent intent)
        {
            if (intent != null)
            {
                // Handle Actions
                HandleIntentAction(intent);

                // Handle Extra's
                HandleIntentExtra(intent);
            }
            else
            {
                if (!_isServiceStartedInForeground)
                {
                    Android.Util.Log.Debug(TAG, "Starting service without intent: " + nameof(ProviderService));
                    RegisterForegroundService();
                }
            }

            if (_isServiceStartedInForeground)
            {
                Android.Util.Log.Debug(TAG, "Refresh notification for: " + nameof(ProviderService));
                RefreshNotification();
            }
        }

        private void HandleIntentExtra(Intent intent)
        {
            if (intent.Extras == null)
            {
                return;
            }

            if (intent.HasExtra(ProviderServiceIntents.SendData))
            {
                Android.Util.Log.Debug(TAG, "Received command: ActiviteitDelen");
                Android.Util.Log.Debug(TAG, "Sending activiteit to wearable");

                var bericht = intent.GetStringExtra(ProviderServiceIntents.SendData);
                SendDataToWearableAsync(CHANNEL_ID, bericht);
            }
        }

        private void HandleIntentAction(Intent intent)
        {
            if (intent.Action == null)
            {
                return;
            }

            if (intent.Action.Equals(ProviderServiceIntents.Action_StartService))
            {
                // ensure CPU is not sleeping
                //_wakeLock?.Acquire();

                Android.Util.Log.Debug(TAG, "Received command: StartService");

                if (!_isServiceStartedInForeground)
                {
                    Android.Util.Log.Debug(TAG, "Starting service: " + nameof(ProviderService));
                    RegisterForegroundService();
                }
            }
            else if (intent.Action.Equals(ProviderServiceIntents.Action_StopService))
            {
                //if (_wakeLock != null && _wakeLock.IsHeld)
                //{
                //    _wakeLock.Release();
                //}

                Android.Util.Log.Debug(TAG, "Received command: StopService");

                if (_isServiceStartedInForeground)
                {
                    Android.Util.Log.Debug(TAG, "Stopping service: " + nameof(ProviderService));
                    UnregisterForegroundService();
                }
            }
        }

        private void SendDataToWearableAsync(int channel, string message)
        {
            if (_mSocketServiceProvider != null
                && _mSocketServiceProvider.IsConnected)
            {
                _mSocketServiceProvider.Send(channel, System.Text.Encoding.ASCII.GetBytes(message));
            }
            else
            {
                Android.Util.Log.Debug(TAG, "Failed to connect to GoogleApiClient");
            }
        }

        /// <summary>
        ///     Called when [bind].
        /// </summary>
        public override IBinder OnBind(Intent intent)
        {
            // Return null because this is not needed for a started service.
            return null;
        }

        public override void OnDestroy()
        {
            Android.Util.Log.Debug(TAG, "The service is shutting down");
            _isServiceStarted = false;

            // Remove the notification from the status bar.
            var notificationManager = GetSystemService(NotificationService) as NotificationManager;
            notificationManager?.Cancel(NotificationId);

            base.OnDestroy();

            if (_task != null && _task.Status == TaskStatus.RanToCompletion)
            {
                _task.Dispose();
            }
        }

        protected override void OnFindPeerAgentsResponse(SAPeerAgent[] p0, int result)
        {
            Android.Util.Log.Debug(TAG, "onFindPeerAgentResponse : result =" + result);

            if (result == PeerAgentFound)
            {
                foreach (SAPeerAgent peerAgent in p0)
                {
                    RequestServiceConnection(peerAgent);
                }
            }
        }

        protected override void OnServiceConnectionRequested(SAPeerAgent p0)
        {
            if (p0 != null)
            {
                AcceptServiceConnectionRequest(p0);
            }
        }

        protected override void OnServiceConnectionResponse(SAPeerAgent p0, SASocket socket, int result)
        {
            if ((result == SAAgent.ConnectionSuccess))
            {
                if ((socket != null))
                {
                    _mSocketServiceProvider = ((ProviderServiceSocket)(socket));
                    _mSocketServiceProvider.Send(CHANNEL_ID, System.Text.Encoding.ASCII.GetBytes("Connected"));
                }

            }
            else if ((result == SAAgent.ConnectionAlreadyExist))
            {
                Android.Util.Log.Debug(TAG, "onServiceConnectionResponse, CONNECTION_ALREADY_EXIST");
            }
        }

        public bool CloseConnection()
        {
            if ((_mSocketServiceProvider != null))
            {
                _mSocketServiceProvider.Close();
                _mSocketServiceProvider = null;
                return true;
            }
            else
            {
                return false;
            }

        }

        private void RefreshNotification()
        {
            var notification = CreateNotification(Resources.GetString(Resource.String.NotificationTitle),
                                                  Resources.GetString(Resource.String.NotificationDesciption));

            NotificationManagerCompat.From(ApplicationContext).Notify(NotificationId, notification);
        }

        private Notification CreateNotification(string title, string contentText)
        {
            var serviceCoachActivityIntent = new Intent(this, typeof(MainActivity));
            var pendingIntent = PendingIntent.GetActivity(this, 0, serviceCoachActivityIntent, PendingIntentFlags.UpdateCurrent);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var notificationChannel = new NotificationChannel(_uniqueChannelId, _channelName, Importance);
                notificationChannel.SetShowBadge(false);

                var notificationManager = GetSystemService(NotificationService) as NotificationManager;
                notificationManager?.CreateNotificationChannel(notificationChannel);
            }

            return new NotificationCompat.Builder(this, _uniqueChannelId)
                .SetContentTitle(title)
                .SetContentText(contentText)
                .SetContentIntent(pendingIntent)
                .SetOngoing(true)
                .Build();
        }

        private void RegisterForegroundService()
        {
            Android.Util.Log.Debug(TAG, "Register foreground service");

            var notification = CreateNotification(Resources.GetString(Resource.String.NotificationTitle),
                                                  Resources.GetString(Resource.String.NotificationDesciption));

            StartForeground(NotificationId, notification);
            _isServiceStartedInForeground = true;
        }

        private void UnregisterForegroundService()
        {
            Android.Util.Log.Debug(TAG, "UnregisterForegroundService " + nameof(ProviderService));

            StopForeground(true);
            StopSelf();
            _isServiceStartedInForeground = false;
        }

        public class ProviderServiceSocket : SASocket
        {
            [Export(SuperArgumentsString = "\"ProviderServiceSocket\"")]
            public ProviderServiceSocket() : base(p0: "ProviderServiceSocket")
            {

            }

            public override void OnReceive(int channelId, byte[] bytes)
            {
                // Check received data 
                string message = System.Text.Encoding.UTF8.GetString(bytes);

                Android.Util.Log.Debug(TAG, "ProviderServiceSocket Received:" + message);
            }

            protected override void OnServiceConnectionLost(int p0)
            {
                // ResetCache();
                Android.Util.Log.Debug(TAG, "ProviderServiceSocket OnServiceConnectionLost:" + p0);
                Close();
            }

            public override void OnError(int p0, string p1, int p2)
            {
                // Error handling
            }
        }
    }
}