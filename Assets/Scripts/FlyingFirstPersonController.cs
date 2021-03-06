using System;
using UnityEngine;
using Rewired;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using UnityStandardAssets.Characters.FirstPerson;
using Random = UnityEngine.Random;

public enum ActorMovementState {
	WaitingToStart,
	Grounded,
	Flying
}

[RequireComponent(typeof (CharacterController))]
[RequireComponent(typeof (AudioSource))]
public class FlyingFirstPersonController : MonoBehaviour
{
    [SerializeField] private bool m_IsWalking;
    [SerializeField] private float m_WalkSpeed;
    [SerializeField] private float m_RunSpeed;
    [SerializeField] [Range(0f, 1f)] private float m_RunstepLenghten;
    [SerializeField] private float m_JumpSpeed;
    [SerializeField] private float m_StickToGroundForce;
    [SerializeField] private float m_GravityMultiplier;
    [SerializeField] private MouseLook m_MouseLook;
    [SerializeField] private bool m_UseFovKick;
    [SerializeField] private FOVKick m_FovKick = new FOVKick();
    [SerializeField] private bool m_UseHeadBob;
    [SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob();
    [SerializeField] private LerpControlledBob m_JumpBob = new LerpControlledBob();
    [SerializeField] private float m_StepInterval;
    [SerializeField] private AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.
    [SerializeField] private AudioClip m_JumpSound;           // the sound played when character leaves the ground.
    [SerializeField] private AudioClip m_LandSound;           // the sound played when character touches back on ground.

	[SerializeField] private float m_flySpeed;
	[SerializeField] private float m_flyBoostSpeed;

	private ActorMovementState m_movementState = ActorMovementState.WaitingToStart;
	private ActorMovementState MovementState {
		get { return m_movementState; }
		set {
			if (m_movementState != value) {
                Debug.Log("Player is " + value.ToString());
				if (value == ActorMovementState.Flying) {
					PlayJumpSound();
					GameManager.Instance.Messenger.SendMessage(this, "PlayerIsFlying");
					GameManager.Instance.Messenger.SendMessage(this, "PlayerIsFlying");
				}
				else if (value == ActorMovementState.Grounded) {
					StartCoroutine(m_JumpBob.DoBobCycle());
					PlayLandingSound();
					m_MoveDir.y = 0f;
					GameManager.Instance.Messenger.SendMessage(this, "PlayerIsGrounded");
				}

				m_movementState = value;
			}
		}
	}

    private Camera m_Camera;
    private bool m_Jump;
    private float m_YRotation;
    private Vector2 m_Input;
    private Vector3 m_MoveDir = Vector3.zero;
    private CharacterController m_CharacterController;
    private CollisionFlags m_CollisionFlags;
    private bool m_PreviouslyGrounded = false;
    private Vector3 m_OriginalCameraPosition;
    private float m_StepCycle;
    private float m_NextStep;
    private AudioSource m_AudioSource;

    // Use this for initialization
    private void Start()
    {
        m_CharacterController = GetComponent<CharacterController>();
        m_Camera = Camera.main;
        m_OriginalCameraPosition = m_Camera.transform.localPosition;
        m_FovKick.Setup(m_Camera);
        m_HeadBob.Setup(m_Camera, m_StepInterval);
        m_StepCycle = 0f;
        m_NextStep = m_StepCycle/2f;
        m_AudioSource = GetComponent<AudioSource>();
		m_MouseLook.Init(transform , m_Camera.transform);
    }
		
    // Update is called once per frame
    private void Update()
    {
		// Rotate the camera
        RotateView();

        // Actor has just become grounded
		if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
        {
			MovementState = ActorMovementState.Grounded;
        }

		// We've fallen off a ledge in this case.
		// I think :/
		if (!m_CharacterController.isGrounded && MovementState != ActorMovementState.Flying && m_PreviouslyGrounded)
        {
            m_MoveDir.y = 0f;
        }

        m_PreviouslyGrounded = m_CharacterController.isGrounded;
    }

    private void FixedUpdate()
    {
		if (MovementState == ActorMovementState.Flying) {
			FixedUpdateFlying();
		}
		else {
			FixedUpdateGrounded();
		}
    }

	private void FixedUpdateGrounded() {
		float speed;
		GetGroundedInput(out speed);

		if (m_Jump)
		{
			MovementState = ActorMovementState.Flying;
		}

		// always move along the camera forward as it is the direction that it being aimed at
		Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

		// get a normal for the surface that is being touched to move along it
		RaycastHit hitInfo;
		Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
			m_CharacterController.height / 2f, ~0, QueryTriggerInteraction.Ignore);
		desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

		m_MoveDir.x = desiredMove.x * speed;
		m_MoveDir.y = m_Jump ? m_JumpSpeed : -m_StickToGroundForce;
		m_MoveDir.z = desiredMove.z * speed;
		m_MoveDir += Physics.gravity * m_GravityMultiplier;

		m_CollisionFlags = m_CharacterController.Move(m_MoveDir*Time.fixedDeltaTime);

		ProgressStepCycle(speed);
		UpdateCameraPosition(speed);

		m_MouseLook.UpdateCursorLock();
	}

	private void FixedUpdateFlying() {
		float speed;
		GetFlyingInput(out speed);

		// always move along the camera forward as it is the direction that it being aimed at
		Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

		// get a normal for the surface that is being touched to move along it
		RaycastHit hitInfo;
		Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
			m_CharacterController.height / 2f, ~0, QueryTriggerInteraction.Ignore);
		desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

		m_MoveDir.x = desiredMove.x * speed;
		m_MoveDir.y *= speed;
		m_MoveDir.z = desiredMove.z * speed;

		m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);
		m_MouseLook.UpdateCursorLock();
	}

	/// <summary>
	/// Processes input when the player is grounded.
	/// </summary>
	/// <param name="speed">Speed.</param>
	private void GetGroundedInput(out float speed) {
		// Read input
		float strafeAmount =  ReInput.players.Players[0].GetAxis("Strafe");
		float forwardAmount = ReInput.players.Players[0].GetAxis("Walk forward");
		m_Jump = ReInput.players.Players[0].GetButtonDown("Jump");
		bool waswalking = m_IsWalking;

		#if !MOBILE_INPUT
		// On standalone builds, walk/run speed is modified by a key press.
		// keep track of whether or not the character is walking or running
		m_IsWalking = !ReInput.players.Players[0].GetButton("Run");
		#endif
		// set the desired speed to be walking or running
		speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
		m_Input = new Vector2(strafeAmount, forwardAmount);

		// normalize input if it exceeds 1 in combined length:
		if (m_Input.sqrMagnitude > 1)
		{
			m_Input.Normalize();
		}

		// handle speed change to give an fov kick
		// only if the player is going to a run, is running and the fovkick is to be used
		if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
		{
			StopAllCoroutines();
			StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
		}
	}

	/// <summary>
	/// Processes input when the player is flying.
	/// </summary>
	/// <param name="speed">Speed.</param>
	private void GetFlyingInput(out float speed) {
		float yawAmount =  ReInput.players.Players[0].GetAxis("Yaw");
		float thrustAmount = ReInput.players.Players[0].GetAxis("Thrust");
		m_MoveDir.y = ReInput.players.Players[0].GetAxis("Altitude");

		bool waswalking = m_IsWalking;

		#if !MOBILE_INPUT
		// On standalone builds, walk/run speed is modified by a key press.
		// keep track of whether or not the character is walking or running
		m_IsWalking = !ReInput.players.Players[0].GetButton("Extra Thrust");
		#endif
		// set the desired speed to be walking or running
		speed = m_IsWalking ? m_flySpeed : m_flyBoostSpeed;
		m_Input = new Vector2(yawAmount, thrustAmount);

		// normalize input if it exceeds 1 in combined length:
		if (m_Input.sqrMagnitude > 1)
		{
			m_Input.Normalize();
		}

		// handle speed change to give an fov kick
		// only if the player is going to a run, is running and the fovkick is to be used
		if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
		{
			StopAllCoroutines();
			StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
		}
	}

	/// <summary>
	/// Updates the camera position for head-bob
	/// </summary>
	/// <param name="speed">Speed.</param>
	private void UpdateCameraPosition(float speed)
	{
		Vector3 newCameraPosition;
		if (!m_UseHeadBob)
		{
			return;
		}
		if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
		{
			m_Camera.transform.localPosition =
				m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
					(speed*(m_IsWalking ? 1f : m_RunstepLenghten)));
			newCameraPosition = m_Camera.transform.localPosition;
			newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
		}
		else
		{
			newCameraPosition = m_Camera.transform.localPosition;
			newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
		}
		m_Camera.transform.localPosition = newCameraPosition;
	}

	/// <summary>
	/// Rotates the view based on Mouselook.
	/// @TODO Replace with Rewired.
	/// </summary>
    private void RotateView()
    {
        m_MouseLook.LookRotation (transform, m_Camera.transform);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        //dont move the rigidbody if the character is on top of it
        if (m_CollisionFlags == CollisionFlags.Below)
        {
            return;
        }

        if (body == null || body.isKinematic)
        {
            return;
        }

		// Bounce the actor off the object slightly.
        body.AddForceAtPosition(m_CharacterController.velocity*0.1f, hit.point, ForceMode.Impulse);
    }

	/// <summary>
	/// Controls how often footsteps hit the ground (e.g. for controlling footstep sounds).
	/// </summary>
	/// <param name="speed">Speed.</param>
	private void ProgressStepCycle(float speed)
	{
		if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
		{
			m_StepCycle += (m_CharacterController.velocity.magnitude + (speed*(m_IsWalking ? 1f : m_RunstepLenghten)))*
				Time.fixedDeltaTime;
		}

		if (!(m_StepCycle > m_NextStep))
		{
			return;
		}

		m_NextStep = m_StepCycle + m_StepInterval;

		PlayFootStepAudio();
	}

	/// <summary>
	/// Plays the landing sound.
	/// </summary>
	private void PlayLandingSound()
	{
		m_AudioSource.clip = m_LandSound;
		m_AudioSource.Play();
		m_NextStep = m_StepCycle + .5f;
	}

	/// <summary>
	/// Plays the jump sound.
	/// </summary>
	private void PlayJumpSound()
	{
		m_AudioSource.clip = m_JumpSound;
		m_AudioSource.Play();
	}

	/// <summary>
	/// Plays the footstep audio.
	/// </summary>
	private void PlayFootStepAudio()
	{
		if (MovementState != ActorMovementState.Grounded)
		{
			return;
		}

		// pick & play a random footstep sound from the array,
		// excluding sound at index 0
		int n = Random.Range(1, m_FootstepSounds.Length);
		m_AudioSource.clip = m_FootstepSounds[n];
		m_AudioSource.PlayOneShot(m_AudioSource.clip);

		// move picked sound to index 0 so it's not picked next time
		m_FootstepSounds[n] = m_FootstepSounds[0];
		m_FootstepSounds[0] = m_AudioSource.clip;
	}
}
