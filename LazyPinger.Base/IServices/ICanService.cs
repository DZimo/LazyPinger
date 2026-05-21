using LazyPinger.Base.Models.Network;

namespace LazyPinger.Base.IServices
{
    public interface ICanService
    {
        bool IsConnected { get; }

        string? ConnectedInterface { get; }

        List<string> GetAvailableInterfaces();

        Task<bool> OpenAsync(string interfaceName, CanBaudRate baudRate);

        void Close();

        event EventHandler<CanFrame>? MessageReceived;
    }
}
