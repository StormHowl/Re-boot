using System.Collections;
using System.Collections.Generic;
using Rewind;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Script which manages the Rewind capacity of the player.
/// </summary>
[RequireComponent(typeof(RewindManager))]
public class PlayerRewind : RewindableEntity, IRewindEntity
{
    private float _cooldown = 3f;
    private float _rewindAnimationSpeed = 500f;
    [SyncVar] public float CurrentCooldown = 0f;

    private PlayerUi _ui;

    private bool _isRewinding;
    private PositionFlash[] _tempPositions;
    private int _tempPositionsIndex;
    private float _reachedDistance = 1.0f;

    public bool InputDisabled = false;

    void Start()
    {
        if (isServer)
        {
            // Function of the abstract class RewindableEntity
            Init();
            // Add this entity to the rewind manager, to say we want to be rewinded.
            NGameManager.Instance.RewindManager.AddEntity(this);
        }
    }

    public void SetPlayerUi(PlayerUi ui)
    {
        _ui = ui;
    }

    void Update()
    {
        if (CurrentCooldown > 0f)
            UpdateCooldown();

        if (_isRewinding)
        {
            DoRewindUpdate();
        }
        else
        {
            if (isServer)
                SaveTemporalFlash(transform.position, Time.deltaTime, transform.rotation, GetComponent<Player>().CurrentHealth);

            if (!InputDisabled && Input.GetKey(KeyCode.C) && CurrentCooldown <= 0f)
                CmdUseRewind();
            if (GetComponent<Player>().IsAi && GetComponent<PlayerAI>().WantARewind && CurrentCooldown <= 0f)
                CmdUseRewind();
        }
    }

    /// <summary>
    /// Called by the client when possible and executed on the server. Calls the rewind on the <see cref="RewindManager"/>, which then broadcasts it to all players.
    /// </summary>
    [Command]
    public void CmdUseRewind()
    {
        if (CurrentCooldown <= 0f)
        {
            NGameManager.Instance.RewindManager.Rewind(this);
        }
    }

    /// <summary>
    /// Method called at each frame which makes the cooldown decrease. Also updates the Ui.
    /// </summary>
    public void UpdateCooldown()
    {
        float minus = 1 / _cooldown * Time.deltaTime;
        CurrentCooldown -= Time.deltaTime;

        if (isLocalPlayer)
            _ui.UpdateCooldownValue(minus);
    }

    private void DoRewindUpdate()
    {
        float speed = _tempPositionsIndex < _tempPositions.Length / 10 ||
                      _tempPositionsIndex > _tempPositions.Length / 10 * 9
            ? _rewindAnimationSpeed * _tempPositionsIndex / 10
            : _rewindAnimationSpeed;

        float dist = Vector3.Distance(_tempPositions[_tempPositionsIndex].Position, transform.position);
        transform.position = Vector3.MoveTowards(transform.position, _tempPositions[_tempPositionsIndex].Position,
            Time.deltaTime * speed);
        transform.rotation = _tempPositions[_tempPositionsIndex].Rotation;

        // Increment previous positions in order to go back in time
        if (dist <= _reachedDistance)
            _tempPositionsIndex += 2;

        // End of rewind
        if (_tempPositionsIndex >= _tempPositions.Length)
        {
            _isRewinding = false;
            GetComponent<Player>().ChangeComponentsState(true);
            if (_ui != null)
                _ui.SetRewindOverlay(false);
        }
    }

    /// <summary>
    /// Method called by the <see cref="RewindManager"/> when a player asks a rewind.
    /// </summary>
    [Server]
    public void Rewind()
    {
        //RpcRewind(FindObjectFlashPosition());
        RpcRewindWithPositions(GetAllRewindPositions());
    }

    /// <summary>
    /// Called to execute the real rewind: movement to a certain position (supposedly back in past).
    /// </summary>
    /// <param name="position"></param>
    [ClientRpc]
    public void RpcRewind(Vector3 position)
    {
        transform.position = position;
    }

    [ClientRpc]
    public void RpcRewindWithPositions(PositionFlash[] positions)
    {
        _isRewinding = true;
        _tempPositions = positions;
        _tempPositionsIndex = 0;
        GetComponent<Player>().ChangeComponentsState(false);
        if (_ui != null)
            _ui.SetRewindOverlay(true);
    }

    /// <summary>
    /// Called by <see cref="TeamManager"/> after a rewind has been used to set accordingly to a player or its team the good
    /// cooldown value.
    /// </summary>
    /// <param name="cooldown"></param>
    [ClientRpc]
    public void RpcSetCooldown(float cooldown)
    {
        if (CurrentCooldown <= 0f)
        {
            _cooldown = cooldown;
            CurrentCooldown = cooldown;

            if (isLocalPlayer && _ui != null)
                _ui.SetCooldownUsed(cooldown);
        }
    }


    void OnDestroy()
    {
        // Remove the reference to this object in the RewindManager when removed
        if(isServer)
            NGameManager.Instance.RewindManager.RemoveEntity(this);
    }
}