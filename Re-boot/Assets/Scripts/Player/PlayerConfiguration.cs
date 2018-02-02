using System;

/// <summary>
/// PlayerConfiguration is just a generic serializable class which permits to build classes from the unity inspector
/// </summary>
[Serializable]
public class PlayerConfiguration
{
    //Health
    public int MaxHealth = 100;

    //Cartridge
    public PlayerWeapon Weapon;

    //Physics
    public float Speed = 25f;
    public float SprintSpeed = 50f;
    public float JumpSpeed = 20f;

    public void SetConfiguration(int maxHealth, PlayerWeapon weapon, float speed, float sprintSpeed, float jumpSpeed)
    {
        MaxHealth = maxHealth;
        Weapon = weapon;
        Speed = speed;
        SprintSpeed = sprintSpeed;
        JumpSpeed = jumpSpeed;
    }
}
