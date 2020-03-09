using System;
using System.Collections.Generic;
using System.Text;

namespace WearCompanion
{
    public interface IWearableAgent
    {
        void StartService();
        void StopService();
        void SendMessage(string message);
    }
}
