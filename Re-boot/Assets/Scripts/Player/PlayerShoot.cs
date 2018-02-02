using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Class which gets input concerning shoot from the player. Possesses a <see cref="PlayerWeapon"/> which permits to spawn bullets with specific configurations.
/// </summary>
[RequireComponent(typeof(Player))]
public class PlayerShoot : NetworkBehaviour
{
    public PlayerWeapon Weapon;
    public GameObject BulletPrefab;

    // Not really useful
    [SerializeField] private Camera _camera;
    [SerializeField] public Transform BulletSpawn;

    public const string HeadColliderTag = "HeadCollider";
    public const string BodyColliderTag = "BodyCollider";

    private PlayerUi _ui;

    private int _currentCartridgeClipSize;

    //Unused for now, but could be good
    //[SerializeField] private LayerMask _mask;
    //Because the server may be a host, we store just player name
    public string PlayerName;
    public bool InputDisabled = false;

    void Start()
    {
        // We want to be sure to shoot only if we have a camera
        if (_camera == null && !GetComponent<Player>().isAi)
        {
            Debug.LogError("PlayerShoot: No Camera detected!");
            enabled = false;
        }

        BulletSpawn.transform.rotation = Quaternion.identity;
    }

    void Update()
    {
        if (!InputDisabled)
        {
            if (Input.GetButtonDown("Fire1") && _currentCartridgeClipSize > 0)
            {
                InvokeRepeating("Shoot", 0f, 1f / Weapon.FireRate);
            }
            else if (Input.GetButtonUp("Fire1") || _currentCartridgeClipSize <= 0)
            {
                CancelInvoke("Shoot");
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                ReloadCartridgeClip();
            }
        }
    }

    public void SetPlayerUi(PlayerUi ui)
    {
        if (ui == null)
        {
            Debug.LogError("PlayerShoot: No PlayerUi component on Player UI prefab");
        }
        else
        {
            _ui = ui;
            // Called here to update the Ui.
            SetWeapon(Weapon);
        }
    }

    /// <summary>
    /// Affects the player with the new weapon, and updates Ui.
    /// </summary>
    /// <param name="weapon"></param>
    public void SetWeapon(PlayerWeapon weapon)
    {
        Weapon = weapon;
        _currentCartridgeClipSize = Weapon.CartridgeClipSize;

        if (isLocalPlayer)
        {
            _ui.SetMaxCartridgeClipContent(Weapon.CartridgeClipSize);
            _ui.SetCurrentCartridgeClipContent(Weapon.CartridgeClipSize);
        }
    }

    /// <summary>
    /// Wrapper for shooting. Repeatedly called for rapid fire.
    /// </summary>
    public void Shoot()
    {
        CmdShoot(BulletSpawn.transform.position, BulletSpawn.transform.rotation);
        // If after shooting our cartridge is empty
        if (!HasFired())
        {
            CancelInvoke("Shoot");
        }
    }

    public void ReloadIfEmpty()
    {
        if (_currentCartridgeClipSize <= 0)
        {
            ReloadCartridgeClip();
        }
    }

    /// <summary>
    /// Method to shoot a bullet, only executed on server. Instantiates the bullet from a prefab, changes its configuration with the current <see cref="PlayerWeapon"/>
    /// configuration, and broadcasts its spawn to all players.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    [Command]
    void CmdShoot(Vector3 position, Quaternion rotation)
    {
        var bullet = Instantiate(
            BulletPrefab,
            position,
            rotation);

        bullet.GetComponent<BulletPhysics>().SetParameters(transform.name, Weapon);

        NetworkServer.Spawn(bullet);

        Destroy(bullet, Weapon.LifeTime);
    }

    /// <summary>
    /// Method to say we have fired, and to update Ui.
    /// </summary>
    bool HasFired()
    {
        _currentCartridgeClipSize--;
       
        if (isLocalPlayer)
            _ui.SetCurrentCartridgeClipContent(_currentCartridgeClipSize);

        return _currentCartridgeClipSize > 0;
    }

    void ReloadCartridgeClip()
    {
        _currentCartridgeClipSize = Weapon.CartridgeClipSize;

        if(isLocalPlayer)
            _ui.SetCurrentCartridgeClipContent(_currentCartridgeClipSize);
    }

    public void ChangeOrientation(float degreesX)
    {
        // BulletSpawn.transform.Rotate(new Vector3(degreesX, 0, 0));
    }
}
