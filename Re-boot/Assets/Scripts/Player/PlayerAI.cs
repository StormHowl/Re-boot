using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

internal enum AIState
{
    PATROLLING,
    CHASING
}

public class PlayerAI : NetworkBehaviour
{
    private const string PlayerTag = "Player";
    private CharacterController _controller;

    public GameObject WaypointsHolder;
    public List<Vector3> Waypoints = new List<Vector3>();
    public float TargetTolerance = 2f;
    public int WaypointIndex;
    public float Speed = 10f;

    [SyncVar] private Vector3 _targetPosition;

    private Animator _animator;
    private PlayerWeapon _playerWeapon;

    private float _patrolTimer;
    public float WaitingTime;

    private float _remainingDistance;
    private float _stoppingDistance = 3f;

    public float Sight = 40f;

    private bool isMoving = false;

    private static int AIName = 0;
    private AIState _state;

    private GameObject _chasedEnemy;

    private PlayerShoot _playerShoot;
    public float TimeBetweenShots = 0.25f;
    private float _lastShootTime;

	public bool wantARewind;

    public bool WantARewind
    {
        get { return wantARewind; }
    }

    void Start()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();
        _playerShoot = GetComponent<PlayerShoot>();
        // _playerWeapon = GetComponent<PlayerWeapon>();
        _lastShootTime = 0.0f;

        _controller.enabled = true;
		wantARewind = false;
        transform.name = "AI" + AIName++;
        NGameManager.Instance.RegisterPlayer(transform.name, GetComponent<Player>());
        _state = AIState.PATROLLING;

        GetComponent<PlayerShoot>().Weapon.Dispersion = 0.01f;


        if (isServer)
            NewGoal();
    }

    void Update()
    {
        if (isServer)
        {
			/*if (GetComponent<Player>().GetLife() < 20)
            {
                wantARewind = true;
            }*/
            CheckForEnemies();
            switch (_state)
            {
                case AIState.PATROLLING:
                    Patrolling();
                    break;
                case AIState.CHASING:
                    Chase();
                    Shoot();
                    break;
            }
        }
        else
        {
            if (_targetPosition != null)
            {
                MoveToward(_targetPosition);
            }
        }
    }

    [Server]
    void Patrolling()
    {
        if (Waypoints.Count > 0)
        {
            if (_remainingDistance < _stoppingDistance)
            {
                if (WaypointIndex == Waypoints.Count - 1)
                {
                    NewGoal();
                    Waypoints.AddRange(
                        NGameManager.Instance.GenerationManager.GetPathTo(transform.position, _targetPosition));
                }
                else
                {
                    WaypointIndex++;
                }

                _targetPosition = Waypoints[WaypointIndex];
            }

            MoveToward(_targetPosition);
        }
        else
        {
            NewGoal();
            Waypoints.AddRange(NGameManager.Instance.GenerationManager.GetPathTo(transform.position, _targetPosition));
        }
    }

    [Server]
    public void CheckForEnemies()
    {
        _chasedEnemy = null;
        Vector3 center = transform.position + _controller.center;
        var hits = Physics.SphereCastAll(center, Sight, Vector3.forward, Sight);

        foreach (var hit in hits)
        {
            if (hit.collider.CompareTag("BodyCollider") || hit.collider.CompareTag("HeadCollider"))
            {
                if (hit.collider.transform.parent.parent.transform.name != transform.name)
                {
                    _chasedEnemy = hit.collider.transform.parent.parent.gameObject;
                    _state = AIState.CHASING;
                    break;
                }
            }
        }

        if (_chasedEnemy == null && _state == AIState.CHASING)
        {
            _state = AIState.PATROLLING;
            NewGoal();
            Waypoints.AddRange(NGameManager.Instance.GenerationManager.GetPathTo(transform.position, _targetPosition));
        }
    }

    [Server]
    public void Chase()
    {
        _targetPosition = _chasedEnemy.transform.position;
        MoveToward(_chasedEnemy.transform.position);
    }

    [Server]
    public void Shoot()
    {
        if (Time.time > (_lastShootTime + TimeBetweenShots))
        {
            _playerShoot.ReloadIfEmpty();
            _playerShoot.ChangeOrientation(Vector3.Dot(
                (_playerShoot.BulletSpawn.position + transform.forward) - _playerShoot.BulletSpawn.position,
                _chasedEnemy.transform.position - _playerShoot.BulletSpawn.position));
            _playerShoot.Shoot();
            _lastShootTime = Time.time;
        }
    }

    public void MoveToward(Vector3 position)
    {
        //Update position
        Vector3 direction = (position - transform.position).normalized * Speed;
        direction.y = 0;
        _controller.SimpleMove(direction);

        // Update rotation
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

        _remainingDistance = direction.magnitude;
    }

    public void NewGoal()
    {
        var destination = new Vector3(
            Random.Range(10,
                NGameManager.Instance.GenerationManager.SizeMapX *
                NGameManager.Instance.GenerationManager.Settings.Length - 10), 0,
            Random.Range(10,
                NGameManager.Instance.GenerationManager.SizeMapY *
                NGameManager.Instance.GenerationManager.Settings.Length - 10));
        _targetPosition = destination;
        WaypointIndex = 1;
    }
}