using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Samsung.Sap;

[assembly: Xamarin.Forms.Dependency(typeof(WearCompanion.Droid.SAPprovider))]
namespace WearCompanion.Droid
{
    public class SAPprovider : ISAPprovider
    {
        public event EventHandler OnDataRecevied;
        public Peer peer { get; set; }

        public SAPprovider()
        {

        }
        public bool Disconnect()
        {
            if ((peer != null && peer.Connection?.Status == ConnectionStatus.Connected))
            {
                peer.Connection.Close();
                peer = null;
                Console.WriteLine("Connection Closed");
                return true;
            }
            else
            {
                Console.WriteLine("Connection not open");
                return false;
            }
        }
        public async Task<string> GetAgent(string msg)
        {
            string received = "";
            try
            {

                var agent = await Samsung.Sap.Agent.GetAgent(Samsung.Sap.Service.Profiles.First());

                var peers = await agent.FindPeers();
                if (peers.Count() == 0)
                {
                    Console.WriteLine("There are no peers to connect to.");
                }
                else
                {
                    peer = peers.First();
                    await peer.Connection.Open();
                    peer.Connection.Send(agent.Channels.First().Value, System.Text.Encoding.ASCII.GetBytes(msg));
                }

                agent = await Samsung.Sap.Agent.GetAgent("/my/profile", onConnect: con =>
                {
                    if (con.Peer.ProfileVersion == agent.ProfileVersion)
                    {
                        con.DataReceived += DataRecevied;
                        return true; // accept connection from peers with the same profile version as ours
                    }
                    else
                    {
                        return false; // reject other connections
                    }
                });
            }
            catch (Exception exc)
            {
                Console.WriteLine("Communication failure: " + exc.Message);
            }
            return received;
        }

        public void DataRecevied(object sender, DataReceivedEventArgs e)
        {
            string message = System.Text.Encoding.UTF8.GetString(e.Data);

            Console.WriteLine($"Received {e.Data.Length} bytes from {e.Peer.DeviceName}/{e.Channel.ID}");
            OnDataRecevied(this, new SAPDataReceivedEventArgs(message));
        }
    }
}