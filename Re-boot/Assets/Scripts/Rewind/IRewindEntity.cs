using UnityEngine.Networking;

namespace Rewind
{
    public interface IRewindEntity
    {

        [Server]
        void Rewind();
    }
}