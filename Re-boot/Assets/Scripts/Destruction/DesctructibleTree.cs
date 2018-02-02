using System.Collections;
using System.Collections.Generic;
using Rewind;
using UnityEngine;

public class DesctructibleTree : RewindableSceneryElement
{

    private const int BaseLife = 60;
    private int _currentLife = BaseLife;

    void Start()
    {
        Init();
    }

    public override void TakeDamages(int amount)
    {
        _currentLife -= amount;
        if (_currentLife <= 0)
        {
            ChangeState(SceneryStateEnum.DESTROYED);
            SetState(false);
        }
    }

    public override void OnStateChanged(bool state)
    {
        if (state)
        {
            _currentLife = 60;
        }
        else
        {
            _currentLife = 0;
        }
    }
}
