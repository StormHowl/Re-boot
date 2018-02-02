using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Rewind
{
    public enum SceneryStateEnum
    {
        DESTROYED, CREATED
    }

    internal struct SceneryState
    {
        public float Time;
        public SceneryStateEnum State;

        public SceneryState(float time, SceneryStateEnum state)
        {
            Time = time;
            State = state;
        }
    }

    public abstract class RewindableSceneryElement : NetworkBehaviour
    {
        private float _creationTime;
        private List<SceneryState> _states;
        public SceneryStateEnum LastState;

        public GameObject Graphics;

        public void Init()
        {
            _states = new List<SceneryState>();
            LastState = SceneryStateEnum.CREATED;
            _creationTime = Time.time;
        }
        
        public void SetState(bool state)
        {
            Graphics.SetActive(state);
            OnStateChanged(state);
        }

        [Server]
        public void ChangeState(SceneryStateEnum state)
        {
            _states.Add(new SceneryState(Time.time, state));
            LastState = state;
        }

        /// <summary>
        /// Function to get what was the state of the element reverted x time before
        /// </summary>
        /// <param name="time"></param>
        /// <returns>true if it is active or created, orelse false</returns>
        [Server]
        public bool GetStateBackTime(float time)
        {
            float backTime = Time.time - time;
            if (backTime < _creationTime)
                return false;

            for (int i = _states.Count - 1; i >= 0; --i)
            {
                if (_states[i].Time >= backTime)
                    return _states[i].State != SceneryStateEnum.CREATED;
            }

            return true;
        }

        public abstract void OnStateChanged(bool state);
        public abstract void TakeDamages(int amount);
    }
}