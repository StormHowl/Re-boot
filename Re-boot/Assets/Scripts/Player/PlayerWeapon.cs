using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Good little serializable class to gather every single detail about a weapon.
/// Utility is to be able to modify everything from the Unity Inspector. Used by <see cref="PlayerShoot.CmdShoot"/>.
/// </summary>
[System.Serializable]
public class PlayerWeapon
{
    public string Name = "AK-47";

    public float Damage = 10f;
    public float BulletSpeed = 20f;
    public float FireRate = 20f;
    public float LifeTime = 3f;
    public float Dispersion = 0.01f;

    public int CartridgeClipSize = 10;
}
