using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// This class manages the player in itself : life, death and what goes with it (respawn).
/// Its method Setup() is called by the PlayerSetup script and permits to valid its construction.
/// </summary>
public class Player : NetworkBehaviour
{
    [SyncVar] private bool _isDead = false;
    [SerializeField] private Behaviour[] _disableOnDeath;
    private bool[] _wasEnabled;

    public bool IsDead
    {
        get { return _isDead; }
        protected set { _isDead = value; }
    }

    [SerializeField] private int _maxHealth = 100;
    [SyncVar] private int _currentHealth;

    private PlayerUi _ui;
    private ParticleSystem _particles;

    public bool InputDisabled = false;
    public bool isAi = false;

    public bool IsAi
    {
        get { return isAi; }
    }

    public int GetLife()
    {
        return _currentHealth;
    }

    #region setup

    /// <summary>
    /// Method to setup everything in the player once in the game, called at startup by <see cref="PlayerSetup.Start"/>.
    /// </summary>
    public void Setup()
    {
        _wasEnabled = new bool[_disableOnDeath.Length];
        for (int i = 0; i < _wasEnabled.Length; ++i)
        {
            _wasEnabled[i] = _disableOnDeath[i].enabled;
        }

        ChangeComponentsState(false);
        NGameManager.Instance.AffectTeam(this);
        transform.position = NGameManager.Instance.TeamManager.GetSpawnPointOfPlayer(this);
        _particles = GetComponent<ParticleSystem>();
        _particles.Stop();
    }

    /// <summary>
    /// Restores defaults values : life, components to enable and collider state.
    /// </summary>
    public void SetDefaults()
    {
        _isDead = false;
        _currentHealth = _maxHealth;

        ChangeComponentsState(true);
        /*for (int i = 0; i < _disableOnDeath.Length; ++i)
        {
            _disableOnDeath[i].enabled = _wasEnabled[i];
        }

        ChangeColliderState(true);*/

        if (isLocalPlayer && _ui != null)
            _ui.SetMaxHealthValue(_maxHealth);
    }

    public void SetPlayerUi(PlayerUi ui)
    {
        if (ui == null)
        {
            Debug.LogError("Player: No PlayerUi component on Player UI prefab");
        }
        else
        {
            _ui = ui;
        }
    }

    public void ChangeConfiguration(int maxHealth)
    {
        _maxHealth = maxHealth;
    }

    #endregion

    #region damage, life and death

    /// <summary>
    /// Method called by the <see cref="BulletPhysics"/> when the player collides with a bullet. The method is called
    /// by the server and is broadcasted to clients. Because the field _currentHealth is [SyncVar], no need to worry about
    /// synchronisation between server and clients.
    /// </summary>
    /// <param name="amount"></param>
    [ClientRpc]
    public void RpcTakeDamage(float amount)
    {
        if (IsDead)
            return;

        _currentHealth -= (int) amount;
        Debug.Log(transform.name + " now has " + _currentHealth);

        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            Die();
        }

        if (isLocalPlayer)
            _ui.SetCurrentHealthValue(_currentHealth);
    }

    private void Die()
    {
        _particles.Play();
        IsDead = true;

        // Disable components
        ChangeComponentsState(false);

        //Notify player is dead
        Debug.Log(transform.name + " is dead");
        NGameManager.Instance.TeamManager.CmdNotifyTeamLostLive(transform.name);

        // Call respawn method
        StartCoroutine(Respawn());
    }

    [ClientRpc]
    public void RpcUpdateTeamRemainingLives(int lives)
    {
        if (_ui != null)
            _ui.SetTeamRemainingLives(lives);
    }

    /// <summary>
    /// Method which is called when the player is killed. It changes the position to the start position of the player
    /// associated to a team, and restores default values.
    /// </summary>
    /// <returns>A yield return, which contains a trigger for a certain amount of time (respawn time accessible from settings)</returns>
    private IEnumerator Respawn()
    {
        //set new start position
        transform.position = NGameManager.Instance.TeamManager.GetSpawnPointOfPlayer(this);
        if (!isAi)
            GetComponent<PlayerMotor>().Move(Vector3.zero);
        yield return new WaitForSeconds(NGameManager.Instance.Settings.RespawnTime);

        SetDefaults();
        _particles.Stop();
        Debug.Log(transform.name + " respawned");
    }

    #region components managing

    public void ChangeComponentsState(bool state)
    {
        for (int i = 0; i < _disableOnDeath.Length; ++i)
        {
            _disableOnDeath[i].enabled = state;
        }

        ChangeColliderState(state);
    }

    private void ChangeColliderState(bool state)
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = state;

        if (!isAi)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
                rb.useGravity = state;
        }
        else
        {
            GetComponent<CharacterController>().enabled = state;
        }
    }

    #endregion

    #endregion

    void Update()
    {
        if (!isLocalPlayer)
            return;

        if (!InputDisabled)
        {
            // For debug purposes
            if (Input.GetKey(KeyCode.K) || transform.position.y < -50)
                RpcTakeDamage(10f);

            // If the player wants to change character
            if (Input.GetKey(KeyCode.H))
            {
                ChangeComponentsState(false);
                _ui.ActivateClassSelectionUi();
            }
        }
    }

    public int CurrentHealth
    {
        get { return _currentHealth; }
        set { _currentHealth = value; }
    }
}