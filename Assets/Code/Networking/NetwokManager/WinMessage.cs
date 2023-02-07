using Mirror;

namespace Code.Networking
{
    public struct WinMessage: NetworkMessage
    {
        public string Name;
        public float TimeToReset;
    }
}