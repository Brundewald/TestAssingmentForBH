using Mirror;
using UnityEngine;

namespace Code.Networking
{
    public struct MessageTransform: NetworkMessage
    {
        public Transform Position;
    }
}