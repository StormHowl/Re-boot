using UnityEngine.Networking;

namespace DefaultNamespace
{
    public interface IRewindEntity
    {
        [Server]
        void Rewind();
    }
}