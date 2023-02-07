using Mirror;
using UnityEngine;

namespace Code.Networking
{
    public struct MessageTransform: NetworkMessage
    {
        public Vector3 Position;
        public Vector3 EulerAngles;
        public uint NetID;
        
        public void SetData(Transform transform, uint netID)
        {
            Position = transform.position;
            EulerAngles = transform.eulerAngles;
            NetID = netID;
        }
    }
}