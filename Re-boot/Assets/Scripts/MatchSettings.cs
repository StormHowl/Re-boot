using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Nice class to set from the unity inspector all interesting parameters for the match settings.
/// </summary>
[System.Serializable]
public class MatchSettings
{
    public float RespawnTime = 3f;
    public int StartingLives = 10;

    [Header("Rewind : ")]
    public float RewindTime = 4f;
    public float GlobalCooldown = 7f;
    public float TeamCooldown = 3f;
    public float CooldownPerPlayer = 4f;
}
