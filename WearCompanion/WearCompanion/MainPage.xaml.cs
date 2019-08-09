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

        public IProviderService provider { get; set; }

        public MainPage()
        {
            InitializeComponent();
        
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
 
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

        }

        private void Connect_Clicked(object sender, EventArgs e)
        {
            provider.CloseConnection();
        }

        private void Send_Clicked(object sender, EventArgs e)
        {

            try
            {
                
                provider = DependencyService.Get<IProviderService>();
                //var SAPprovider = DependencyService.Get<ISAPprovider>();
                //var result = await SAPprovider.GetAgent(entry.Text);
                //SAPprovider.OnDataRecevied += OnDataReceived;
                provider.FindPeers(entry.Text);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private async void OnDataReceived(object sender, EventArgs e)
        {
            var sapargs = e as SAPDataReceivedEventArgs;
            await DisplayAlert("message", sapargs.Message, cancel: "ok");
        }

        //private async void OnMessage(Peer peer, byte[] content)
        //{
        //    string message = System.Text.Encoding.UTF8.GetString(content) + " Time: " + DateTime.Now.ToShortTimeString();
        //    await DisplayAlert("message", message, cancel: "ok");
        //    await peer.SendMessage(System.Text.Encoding.UTF8.GetBytes(message));
        //}

        //private bool OnConnect(Connection connection)
        //{
        //    this.connection = connection;
        //    connection.StatusChanged -= Connection_StatusChanged;
        //    connection.StatusChanged += Connection_StatusChanged;
        //    connection.DataReceived -= Connection_DataReceived;
        //    connection.DataReceived += Connection_DataReceived;
        //    return true;
        //}

        //private void Connection_DataReceived(object sender, Samsung.Sap.DataReceivedEventArgs e)
        //{
        //    string message = Encoding.UTF8.GetString(e.Data) + " Time: " + DateTime.Now.ToShortTimeString();
        //    lblStatus.Text = message;
        //    connection.Send(e.Channel, Encoding.UTF8.GetBytes(message));
        //}

        //private void Connection_StatusChanged(object sender, ConnectionStatusEventArgs e)
        //{
        //    lblStatus.Text = e.Reason.ToString();
        //}


    }
}
