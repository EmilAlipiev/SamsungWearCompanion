using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace WearCompanion
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public IWearableAgent _wearableAgent { get; set; }

        public MainPage()
        {
            InitializeComponent();

            _wearableAgent = Xamarin.Forms.DependencyService.Resolve<IWearableAgent>();

            serviceSwitch.Toggled += ServiceSwitch_Toggled;
        }

        private void ServiceSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            if(serviceSwitch.IsEnabled)
            {
                _wearableAgent.StartService();
            }
            else
            {
                _wearableAgent.StopService();
            }
        }

        private void Send_Clicked(object sender, EventArgs e)
        {
            _wearableAgent.SendMessage(entry.Text);
        }
    }
}
