using System.Collections;
using System.Collections.Generic;
using Rewind;
using UnityEngine;

/// <summary>
/// Script concerning the bullet which is spawned by <see cref="PlayerShoot.CmdShoot"/>. 
/// </summary>
public class BulletPhysics : MonoBehaviour
{
    private const string PlayerTag = "Player";
    private const string AITag = "AI";
    private const string WallTag = "Wall";
    private const string TreeTag = "Tree";

    private string _playerName;
    private float _headShotDamageRate = 2f;
    private PlayerWeapon _weapon;

    private Vector3 _startingPosition;
    private float _maxDistance;

    public void SetParameters(string playerName, PlayerWeapon weapon)
    {
        _weapon = weapon;
        _playerName = playerName;
        _startingPosition = transform.position;
        _maxDistance = _weapon.LifeTime * _weapon.BulletSpeed;
        Vector3 dispersion = new Vector3(Random.Range(-_weapon.Dispersion, _weapon.Dispersion),
            Random.Range(-_weapon.Dispersion, _weapon.Dispersion),
            Random.Range(-_weapon.Dispersion, _weapon.Dispersion));
        GetComponent<Rigidbody>()
            .AddForce(((transform.forward + dispersion) * _weapon.BulletSpeed), ForceMode.VelocityChange);
    }

    void OnCollisionEnter(Collision other)
    {
        var hit = other.gameObject;
        Debug.Log("Layer hit : " + LayerMask.LayerToName(other.collider.gameObject.layer) + " with tag : " + hit.tag + " with name " + hit.transform.name);

        // If we hit a player, and this player is not us (the local player)
        if ((hit.CompareTag(PlayerTag) || hit.CompareTag(AITag)) && hit.transform.name != _playerName)
        {
            Player player = hit.CompareTag(AITag) ? NGameManager.Instance.GetPlayer("Player AI0") : NGameManager.Instance.GetPlayer(hit.transform.name);

            if (other.collider.gameObject.layer == LayerMask.NameToLayer("HeadCollider"))
            {
                DoDamage(player, true);
            }
            else if (other.collider.gameObject.layer == LayerMask.NameToLayer("BodyCollider"))
            {
                DoDamage(player, false);
            }
        }
        else if (hit.CompareTag(WallTag))
        {
            hit.transform.GetComponent<DestructibleWall>().TakeDamages(20);
        } else if (hit.CompareTag(TreeTag))
        {
            hit.transform.GetComponent<DesctructibleTree>().TakeDamages(20);
        }

        if (hit.transform.name != _playerName)
            Destroy(gameObject);
    }

    void DoDamage(Player player, bool headshot)
    {
        float damage = _weapon.Damage * (headshot ? _headShotDamageRate : 1);

        if (_weapon.Name != "Sniper")
        {
            // If bullet went 25% of its max distance
            float distance = Vector3.Distance(transform.position, _startingPosition);
            if (distance > _maxDistance / 5)
            {
                damage *= (distance / _maxDistance);
            }
        }

        player.RpcTakeDamage(damage);
    }
}