using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rewind;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Script which setups the player in itself, places the right components in the right places, changes the layers
/// concerning draw for cameras, places the Uis ...
/// </summary>
[RequireComponent(typeof(Player))]
[RequireComponent(typeof(PlayerShoot))]
public class PlayerSetup : NetworkBehaviour
{
    [SerializeField] private Behaviour[] _componentsToDisable;
    [SerializeField] private string _remotePlayerLayerName = "RemotePlayer";
    [SerializeField] private string _dontDrawLayerName = "DontDraw";

    [SerializeField] private GameObject _playerGraphics;
    [SerializeField] private GameObject _playerUiPrefab;
    private GameObject _playerUiInstance;

    public GameObject classHeart;

    private PlayerConfiguration _configuration;

    public bool IsAi = false;

    // private Camera sceneCamera;

    #region Player configuration

    void Start()
    {
        if (!IsAi)
        {
            if (!isLocalPlayer)
            {
                // If we are not the local player, then we disable everything given in the Unity Inspector in the _componentsToDisable field.
                DisableComponents();
                // We say we are a remote player (so not the local player).
                AssignRemotePlayerLayer();
            }
            else
            {
                // Place to disable a scene default camera if we have one (currently not, but for instance if we want to see the whole game when dead...)
                // (typically scene when in menu)
                /*sceneCamera = Camera.main;
                if (sceneCamera != null)
                {
                    sceneCamera.gameObject.SetActive(false);
                }*/

                // Disable player graphics for local player
                SetLayerRecursively(_playerGraphics, LayerMask.NameToLayer(_dontDrawLayerName));

                // Create Player UI
                _playerUiInstance = Instantiate(_playerUiPrefab);
                _playerUiInstance.name = _playerUiPrefab.name;

                // Configure Ui
                ConfigureUi();
            }
        }
        else
        {
            AssignRemotePlayerLayer();
        }

        GetComponent<Player>().Setup();

        // Once everything is set, we just change menu selection
        if (!IsAi && isLocalPlayer)
            _playerUiInstance.GetComponent<PlayerUi>().ActivateClassSelectionUi();
    }

    void ConfigureUi()
    {
        //Health
        GetComponent<Player>().SetPlayerUi(_playerUiInstance.GetComponent<PlayerUi>());
        //bullets
        GetComponent<PlayerShoot>().SetPlayerUi(_playerUiInstance.GetComponent<PlayerUi>());
        //bricks
        GetComponent<PlayerBuild>().SetPlayerUi(_playerUiInstance.GetComponent<PlayerUi>());
        //rewind
        GetComponent<PlayerRewind>().SetPlayerUi(_playerUiInstance.GetComponent<PlayerUi>());

        //Class change
        _playerUiInstance.GetComponent<PlayerUi>().SetMenuPlayerReference(this);
    }

    /// <summary>
    /// Method called by <see cref="SelectionMenu.ChangeClass"/> to change our class configuration. Just applies the settings to each component 
    /// (health for <see cref="Player"/>, physics for <see cref="PlayerControl"/> and <see cref="PlayerWeapon"/> for <see cref="PlayerShoot"/>).
    /// </summary>
    /// <param name="configuration"></param>
    public void SetConfiguration(PlayerConfiguration configuration, Color color)
    {
        _configuration = configuration;

        classHeart.GetComponent<MeshRenderer>().material.color = color;
        GetComponent<Player>().ChangeConfiguration(_configuration.MaxHealth);
        GetComponent<PlayerShoot>().SetWeapon(_configuration.Weapon);
        GetComponent<PlayerControl>()
            .ChangeConfiguration(_configuration.Speed, _configuration.SprintSpeed, _configuration.JumpSpeed);

        GetComponent<Player>().SetDefaults();
        if (_playerUiInstance != null)
            _playerUiInstance.GetComponent<PlayerUi>().ActivateInGameUi();
    }

    #endregion

    #region Layers & components

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    void DisableComponents()
    {
        _componentsToDisable.ToList().ForEach(c => c.enabled = false);
    }

    void AssignRemotePlayerLayer()
    {
        gameObject.layer = LayerMask.NameToLayer(_remotePlayerLayerName);
    }

    #endregion


    public override void OnStartClient()
    {
        base.OnStartClient();

        NGameManager.Instance.RegisterPlayer(GetComponent<NetworkIdentity>().netId.ToString(), GetComponent<Player>());
    }

    void OnDisable()
    {
        // Destroy player UI
        Destroy(_playerUiInstance);

        //Re-enable scene camera
        /* if (sceneCamera != null)
        {
            sceneCamera.gameObject.SetActive(true);
        }*/

        // We remove player from the team it was in and unregister it from the entire game.
        NGameManager.Instance.TeamManager.RemovePlayer(GetComponent<Player>());
        NGameManager.Instance.UnregisterPlayer(transform.name);
    }
}