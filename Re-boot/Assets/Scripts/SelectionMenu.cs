using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script concerning the change of a class. Contains the configurations for each class.
/// </summary>
public class SelectionMenu : MonoBehaviour
{
    [Header("Classes configurations")] 
    [SerializeField] public PlayerConfiguration _medicConfiguration;
    [SerializeField] public PlayerConfiguration _tankConfiguration;
    [SerializeField] public PlayerConfiguration _scoutConfiguration;
    [SerializeField] public PlayerConfiguration _soldierConfiguration;
    [SerializeField] public PlayerConfiguration _sniperConfiguration;

    private PlayerSetup _player;

    /// <summary>
    /// Called by each <see cref="Button"/> on the Ui to call the change of the configuration on the player.
    /// </summary>
    /// <param name="className"></param>
    public void ChangeClass(string className)
    {
        Debug.Log("Player " + _player.transform.name + " changed class to " + className);
        switch (className)
        {
            case "Soldier":
                _player.SetConfiguration(_soldierConfiguration, Color.blue);
                break;
            case "Tank":
                _player.SetConfiguration(_tankConfiguration, Color.red);
                break;
            case "Medic":
                _player.SetConfiguration(_medicConfiguration, Color.green);
                break;
            case "Scout":
                _player.SetConfiguration(_scoutConfiguration, Color.yellow);
                break;
            case "Sniper":
                _player.SetConfiguration(_sniperConfiguration, Color.black);
                break;
        }
    }

    public void SetPlayer(PlayerSetup player)
    {
        _player = player;
    }
}