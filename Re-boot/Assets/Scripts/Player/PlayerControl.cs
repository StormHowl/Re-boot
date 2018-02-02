using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Stupid component which just cares about the physics of moving of the character.
/// Its role is to get the user input, to calculate exactly how the player has to move,
/// and to send everything to the <see cref="PlayerMotor"/>. Logically requires the component
/// <see cref="PlayerMotor"/> to be loaded.
/// </summary>
[RequireComponent(typeof(PlayerMotor))]
public class PlayerControl : MonoBehaviour
{
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _sprintSpeed = 30f;
    [SerializeField] private float _sensitivity = 4f;
    [SerializeField] private float _jumpSpeed = 20f;

    // Component catching
    private PlayerMotor _motor;

    // Animation
    private NetworkAnimator _animator;

	void Start ()
	{
	    _motor = GetComponent<PlayerMotor>();
	    _animator = GetComponentInChildren<NetworkAnimator>();

	    _animator.SetParameterAutoSend(0, true);
	    _animator.SetParameterAutoSend(1, true);
	}
	
	void Update ()
	{
	    if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.H))
	    {
	        Cursor.lockState = CursorLockMode.None;
	    }
	    if (Input.GetMouseButtonUp(0))
	    {
	        Cursor.lockState = CursorLockMode.Locked;
	    }

        // Calculate movement
	    float xMov = Input.GetAxisRaw("Horizontal"), yMov = Input.GetAxisRaw("Vertical");
	    Vector3 movH = transform.right * xMov, movV = transform.forward * yMov;
	    Vector3 velocity = (movH + movV).normalized * (Input.GetKey(KeyCode.LeftShift) ? _sprintSpeed : _speed);
	    _motor.Move(velocity);

        //Calculate rotation
	    float yRot = Input.GetAxisRaw("Mouse X");
	    Vector3 rotation = new Vector3(0f, yRot, 0f) * _sensitivity;
	    _motor.Rotate(rotation);

        //Calculate Camera rotation
	    float xRot = Input.GetAxisRaw("Mouse Y");
        float xCameraRotation = xRot * _sensitivity;
	    _motor.RotateCamera(xCameraRotation);

	    // Do animations
	    _animator.animator.SetInteger("Speed", yMov != 0 ? (Input.GetKey(KeyCode.LeftShift) ? 2 : 1) : 0);

        // Jump
        Vector3 _jumpForce = Vector3.zero;
	    if (Input.GetButton("Jump"))
	    {
	        _jumpForce = Vector3.up * _jumpSpeed;
            _animator.animator.SetTrigger("Jumping");
            // buggy concerning networking
            // TODO
            _animator.SetTrigger("Jumping");
	    }
        _motor.ApplyJump(_jumpForce);


	}

    public void ChangeConfiguration(float speed, float sprintSpeed, float jumpSpeed)
    {
        _speed = speed;
        _sprintSpeed = sprintSpeed;
        _jumpSpeed = jumpSpeed;
    }
}
