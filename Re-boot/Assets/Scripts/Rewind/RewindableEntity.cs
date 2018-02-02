using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public struct PositionFlash
{
    public readonly float DeltaTime;
    public Vector3 Position;
    public Quaternion Rotation;
    public int Health;

    public PositionFlash(float deltaTime, Vector3 position, Quaternion rotation, int health) : this()
    {
        this.DeltaTime = deltaTime;
        this.Position = position;
        this.Rotation = rotation;
        this.Health = Health;
    }
}

namespace Rewind
{
    public abstract class RewindableEntity : NetworkBehaviour
    {
        private const float SavedTimeLimit = 50.0f;
        public const float RewindTime = 3.0f;

        private List<PositionFlash> _savedPositions;
        private float _totalSavedTime;

        public void Init()
        {
            _savedPositions = new List<PositionFlash>();
        }

        public void SetState(bool state)
        {
            gameObject.SetActive(state);
            enabled = state;
        }

        [Server]
        public void SaveTemporalFlash(Vector3 position, float time, Quaternion rotation, int health)
        {
            _totalSavedTime += time;
            _savedPositions.Add(new PositionFlash(time, position, rotation, health));

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
            for (; i > 0 && rewinded < RewindTime; --i, count++)
            {
                rewinded += _savedPositions[i].DeltaTime;
            }

            var position = _savedPositions[i].Position;
            _savedPositions.RemoveRange(i + 1, count);
            return position;
        }

        [Server]
        public PositionFlash[] GetAllRewindPositions()
        {
            var rewinded = 0.0f;
            var i = _savedPositions.Count - 1;
            var count = 0;
            List<PositionFlash> positions = new List<PositionFlash>();
            for (; i > 0 && rewinded < RewindTime; --i, count++)
            {
                rewinded += _savedPositions[i].DeltaTime;
                positions.Add(_savedPositions[i]);
            }

            _savedPositions.RemoveRange(i + 1, count);
            return positions.ToArray();
        }
    }
}