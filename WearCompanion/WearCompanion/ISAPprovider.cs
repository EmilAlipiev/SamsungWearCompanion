using System.Threading.Tasks;

namespace WearCompanion
{
    public interface ISAPprovider
    {
       
        event System.EventHandler OnDataRecevied;
        bool Disconnect();
        Task<string> GetAgent(string msg);
    }
}