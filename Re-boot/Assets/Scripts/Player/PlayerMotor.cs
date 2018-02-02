using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Motor of the player, only manages camera, and movement. <see cref="PlayerControl"/> sends the values needed,
/// and this component moves the player in itself. Requires a <see cref="Rigidbody"/> to be present on the player, else
/// will not be able to move.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : MonoBehaviour
{

    [SerializeField] private Camera _camera;

    private Vector3 _velocity = Vector3.zero;
    private Vector3 _rotation = Vector3.zero;
    private Vector3 _jumpForce = Vector3.zero;
    private float _xCameraRotation = 0f;
    private float _xCurrentCameraRotation = 0f;
    private bool _grounded;

    [SerializeField] private float _cameraRotationLimit = 85f;

    private Rigidbody _rigidbody;

	// Use this for initialization
	void Start ()
	{
	    _rigidbody = GetComponent<Rigidbody>();
	}

    public void Move(Vector3 velocity)
    {
        _velocity = velocity;
    }

    public void Rotate(Vector3 rotation)
    {
        _rotation = rotation;
    }

    public void RotateCamera(float xCameraRotation)
    {
        _xCameraRotation = xCameraRotation;
    }

    public void ApplyJump(Vector3 jumpForce)
    {
            _jumpForce = jumpForce;
    }

    void FixedUpdate()
    {
        PerformMovement();
        PerformRotation();
    }

    private void PerformRotation()
    {
        _rigidbody.MoveRotation(_rigidbody.rotation * Quaternion.Euler(_rotation));
        if (_camera != null)
        {
            // Set the rotation and clamp it 
            _xCurrentCameraRotation += -_xCameraRotation;
            _xCurrentCameraRotation = Mathf.Clamp(_xCurrentCameraRotation, -_cameraRotationLimit, _cameraRotationLimit);

            //Apply rotation to the transform of the camera
            _camera.transform.localEulerAngles = new Vector3(_xCurrentCameraRotation, 0, 0);
        }
    }

    void PerformMovement()
    {
        if (_velocity != Vector3.zero)
        {
            _rigidbody.MovePosition(_rigidbody.position + _velocity * Time.fixedDeltaTime);
        }

        if (_jumpForce != Vector3.zero && _grounded)
        {
            _rigidbody.AddForce(_jumpForce * Time.fixedDeltaTime, ForceMode.Impulse);
            _grounded = false;
        }
    }

    void OnCollisionStay(Collision other)
    {
        _grounded = true;
    }
}
