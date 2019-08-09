using System;
using System.Collections.Generic;
using System.Text;

namespace WearCompanion
{
    public class SAPDataReceivedEventArgs : EventArgs
    {
        public string Message { get; set; }
        public SAPDataReceivedEventArgs(string msg)
        {
            Message = msg;
        }
    }

}
