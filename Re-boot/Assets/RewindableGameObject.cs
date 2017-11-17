using System;
using System.Collections.Generic;
using System.Linq;
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
    public class RewindableGameObject
    {
        private const float SavedTimeLimit = 50.0f;
        private const float RewindDuration = 3.0f;

        private readonly List<PositionFlash> _temporalSaves;
        private float _totalSavedTime;

        public RewindableGameObject()
        {
            _temporalSaves = new List<PositionFlash>();
            _totalSavedTime = 0.0f;
        }

        public void SaveTemporalFlash(Vector3 position, float time)
        {
            _totalSavedTime += time;
            _temporalSaves.Add(new PositionFlash(time, position));

            if (!(_totalSavedTime > SavedTimeLimit)) return;

            _totalSavedTime -= _temporalSaves[0].DeltaTime;
            _temporalSaves.RemoveAt(0);
        }
        
        public Vector3 FindObjectFlashPosition()
        {   
            var rewinded = 0.0f;
            var i = _temporalSaves.Count - 1;
            var count = 0;
            for (; i > 0 && rewinded < RewindDuration; --i, count++)
            {
                rewinded += _temporalSaves[i].DeltaTime;
            }
            
            var position = _temporalSaves[i].Position;
            _temporalSaves.RemoveRange(i+1, count);
            return position;
        }
    }
}