namespace WearCompanion
{
    public interface IProviderService
    {
        bool CloseConnection();
        void FindPeers(string msg);
    }
}