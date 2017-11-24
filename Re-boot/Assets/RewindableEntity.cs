using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

internal struct PositionFlash
{
    public readonly float DeltaTime;
    public Vector3 Position;

    public PositionFlash(float deltaTime, Vector3 position) : this()
    {
        this.DeltaTime = deltaTime;
        this.Position = position;
    }
}

namespace DefaultNamespace
{
    public abstract class RewindableEntity : NetworkBehaviour
    {
        private const float SavedTimeLimit = 50.0f;
        private const float RewindDuration = 3.0f;

        private List<PositionFlash> _savedPositions;
        private float _totalSavedTime;

        public void Init()
        {
            _savedPositions = new List<PositionFlash>();
        }

        [Server]
        public void SaveTemporalFlash(Vector3 position, float time)
        {
            _totalSavedTime += time;
            _savedPositions.Add(new PositionFlash(time, position));

            if (!(_totalSavedTime > SavedTimeLimit)) return;

            _totalSavedTime -= _savedPositions[0].DeltaTime;
            _savedPositions.RemoveAt(0);
        }

        [Server]
        public Vector3 FindObjectFlashPosition()
        {
            var rewinded = 0.0f;
            var i = _savedPositions.Count - 1;
            var count = 0;
            for (; i > 0 && rewinded < RewindDuration; --i, count++)
            {
                rewinded += _savedPositions[i].DeltaTime;
            }

            var position = _savedPositions[i].Position;
            _savedPositions.RemoveRange(i + 1, count);
            return position;
        }
    }
}