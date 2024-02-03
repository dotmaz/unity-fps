using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
	[RequireComponent(typeof(PlayerInput))]
#endif
	public class FirstPersonController : MonoBehaviour
	{	
		[Header("Recoil")]
		public float RecoilKickBack = .02f; // The intensity of the recoil
		public float RecoilRecoverySpeed = 0.5f; // Speed at which the camera returns to original position
		private float _recoilAmount = 0f; // Dynamic amount of recoil to apply
		public float MaxRecoilX = 20f;

		private bool _isRecoveringFromRecoil = false;
		public float _recoilRecoveryTime = 0.5f; // Duration in seconds for the recoil to recover
		private float _recoilRecoveryTimer = 0f; // Timer to track the recovery duration

		[Header("Gun Sway")]
		public Transform WeaponPosition;
		private Vector3 _originalWeaponPosition;
		private Quaternion _originalWeaponRotation;

		public float SwayAmount = 0.02f;
		public float MaxSwayAmount = 0.06f;
		public float SwaySmoothness = 4f;

		public float RotationSwayAmount = 0.5f;
		public float MaxRotationSwayAmount = 1f;
		public float RotationSwaySmoothness = 3f;

		[Header("Head Bobbing")]
		public float BobbingSpeed = 16f;
		public float BobbingAmount = 0.04f;
		public float Midpoint = 2.0f;

		private float _defaultPosY = 0;
		private float _timer = 0;

		// UI manager
		public UIManager uiManager;


		[Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 4.0f;
		[Tooltip("Sprint speed of the character in m/s")]
		public float SprintSpeed = 6.0f;
		[Tooltip("Rotation speed of the character")]
		public float RotationSpeed = .3f;
		[Tooltip("Acceleration and deceleration")]
		public float SpeedChangeRate = 10.0f;

		[Space(10)]
		[Tooltip("The height the player can jump")]
		public float JumpHeight = 1.2f;
		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float Gravity = -15.0f;

		[Space(10)]
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		public float JumpTimeout = 0.1f;
		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;
		[Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.14f;
		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.5f;
		[Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;

		[Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject CinemachineCameraTarget;
		[Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 90.0f;
		[Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -90.0f;

		// cinemachine
		private float _cinemachineTargetPitch;

		// player
		private float _speed;
		private float _rotationVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = 53.0f;

		// timeout deltatime
		private float _jumpTimeoutDelta;
		private float _fallTimeoutDelta;

	
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
		private PlayerInput _playerInput;
#endif
		private CharacterController _controller;
		private StarterAssetsInputs _input;
		private GameObject _mainCamera;

		private const float _threshold = 0.00f;

		private bool IsCurrentDeviceMouse
		{
			get
			{
				#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
				return _playerInput.currentControlScheme == "KeyboardMouse";
				#else
				return false;
				#endif
			}
		}

		private void Awake()
		{
			// get a reference to our main camera
			if (_mainCamera == null)
			{
				_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}
		}

		private void Start()
		{
			_controller = GetComponent<CharacterController>();
			_input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
			_playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

			// reset our timeouts on start
			_jumpTimeoutDelta = JumpTimeout;
			_fallTimeoutDelta = FallTimeout;
			_defaultPosY = CinemachineCameraTarget.transform.localPosition.y;
			_originalWeaponPosition = WeaponPosition.localPosition;
			_originalWeaponRotation = WeaponPosition.localRotation;
		}

		private void Update()
		{
			JumpAndGravity();
			GroundedCheck();
			Move();

			ApplyHeadBob();
			ApplyRecoil();
		}

		private void ApplyHeadBob()
		{
			if (Mathf.Abs(_input.move.x) > 0.1f || Mathf.Abs(_input.move.y) > 0.1f) // Player is moving
			{
				_timer += Time.deltaTime * (BobbingSpeed * (_input.sprint ? 1.2f : 1f));
				float waveSlice = Mathf.Sin(_timer);
				float lateralWave = Mathf.Cos(_timer * 0.9f); // Slightly out of sync with the vertical movement

				// Create a more complex lateral movement pattern
				float horizontalBob = lateralWave * BobbingAmount * 0.3f; // Adjust the multiplier for subtlety

				// Keep the vertical bob as is
				float verticalBob = waveSlice * BobbingAmount * (_input.sprint ? 1.2f : 1f);

				// Apply bobbing effect with nuanced lateral movement
				CinemachineCameraTarget.transform.localPosition = new Vector3(
					_originalWeaponPosition.x + horizontalBob, // Apply nuanced lateral movement
					_defaultPosY + verticalBob, // Apply vertical movement
					CinemachineCameraTarget.transform.localPosition.z);
			}
			else
			{
				// Reset head position when not moving
				_timer = 0;
				CinemachineCameraTarget.transform.localPosition = Vector3.Lerp(CinemachineCameraTarget.transform.localPosition, 
					new Vector3(_originalWeaponPosition.x, _defaultPosY, CinemachineCameraTarget.transform.localPosition.z), Time.deltaTime * BobbingSpeed);
			}
		}

		private void ApplyRecoil()
		{
			if (_recoilAmount > 0f)
			{
				// Apply recoil
				_cinemachineTargetPitch -= _recoilAmount;
				
				// Ensure the pitch after recoil doesn't exceed your pitch limits
				_cinemachineTargetPitch = Mathf.Clamp(_cinemachineTargetPitch, BottomClamp, TopClamp);
				
				// Start recoil recovery
				_isRecoveringFromRecoil = true;
				_recoilRecoveryTimer = _recoilRecoveryTime;

				// Reset recoil amount after applying
				_recoilAmount = 0f;
			}

			// Handle recoil recovery with a constant force
			if (_isRecoveringFromRecoil)
			{
				// Apply a constant recovery rate instead of trying to lerp back to a pre-recoil position
				float recoveryRate = RecoilKickBack / _recoilRecoveryTime * Time.deltaTime; // Adjust this formula as needed
				_cinemachineTargetPitch += recoveryRate * RecoilRecoverySpeed; // Apply constant recovery

				// Update the recovery timer
				_recoilRecoveryTimer -= Time.deltaTime;
				if (_recoilRecoveryTimer <= 0f)
				{
					// Stop recovery after the timer expires
					_isRecoveringFromRecoil = false;
				}
			}

			// Clamp the pitch after recovery to ensure it doesn't exceed bounds due to constant recovery
			_cinemachineTargetPitch = Mathf.Clamp(_cinemachineTargetPitch, BottomClamp, TopClamp);

			// Apply the updated pitch to the CinemachineCameraTarget
			CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);
		}



		public void TriggerRecoil(){
			
			 _recoilAmount += RecoilKickBack;
    		//  _recoilAmount = Mathf.Clamp(_recoilAmount, 0, MaxRecoilX); // Ensure recoil doesn't exceed max value
		}

		private void LateUpdate()
		{
			CameraRotation();
			ApplyGunSway();
		}
		
		private void ApplyGunSway()
		{
			// if (IsAimingDownSights()) // You need to implement this method.
			// {
			// 	return; // Skip sway when ADS.
			// }

			// Determine the sway based on horizontal and vertical input
			float movementX = -_input.move.x * SwayAmount;
			float movementY = -_input.move.y * SwayAmount;
			movementX = Mathf.Clamp(movementX, -MaxSwayAmount, MaxSwayAmount);
			movementY = Mathf.Clamp(movementY, -MaxSwayAmount, MaxSwayAmount);

			Vector3 finalPosition = new Vector3(movementX, movementY, 0);
			WeaponPosition.localPosition = Vector3.Lerp(WeaponPosition.localPosition, _originalWeaponPosition + finalPosition, Time.deltaTime * SwaySmoothness);

			// Calculate rotation sway based on horizontal movement
			float rotationX = -_input.move.y * RotationSwayAmount; // Tilt based on vertical movement
			float rotationY = _input.move.x * RotationSwayAmount; // Tilt based on horizontal movement
			rotationX = Mathf.Clamp(rotationX, -MaxRotationSwayAmount, MaxRotationSwayAmount);
			rotationY = Mathf.Clamp(rotationY, -MaxRotationSwayAmount, MaxRotationSwayAmount);

			// Incorporate vertical velocity into the sway for a more dynamic effect
			// Tilt the weapon forward or backward based on the vertical velocity
			float verticalVelocityEffect = _verticalVelocity * 0.01f; // Scale the effect based on your preference
			rotationX += verticalVelocityEffect; // Adjust rotationX to include the effect of vertical movement

			Quaternion finalRotation = Quaternion.Euler(new Vector3(rotationX, rotationY, 0));
			WeaponPosition.localRotation = Quaternion.Slerp(WeaponPosition.localRotation, _originalWeaponRotation * finalRotation, Time.deltaTime * RotationSwaySmoothness);
		}


		private void GroundedCheck()
		{
			// set sphere position, with offset
			Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
			Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
		}

		private void CameraRotation()
		{
			if(uiManager.isMenuActive) return;
			// if there is an input
			if (_input.look.sqrMagnitude >= _threshold)
			{
				//Don't multiply mouse input by Time.deltaTime
				float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
				
				_cinemachineTargetPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;
				_rotationVelocity = _input.look.x * RotationSpeed * deltaTimeMultiplier;

				// clamp our pitch rotation
				_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

				// Update Cinemachine camera target pitch
				CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

				// rotate the player left and right
				transform.Rotate(Vector3.up * _rotationVelocity);
			}
		}

		private void Move()
		{
			// set target speed based on move speed, sprint speed and if sprint is pressed
			float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

			// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

			// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is no input, set the target speed to 0
			if (_input.move == Vector2.zero){
				_input.move.x = 0.0f;
       			 _input.move.y = 0.0f;
				 targetSpeed = 0.0f;
			} 

			// a reference to the players current horizontal velocity
			float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

			float speedOffset = 0.1f;
			float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

			// accelerate or decelerate to target speed
			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				// creates curved result rather than a linear one giving a more organic speed change
				// note T in Lerp is clamped, so we don't need to clamp our speed
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

				// round speed to 3 decimal places
				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}
			else
			{
				_speed = targetSpeed;
			}

			// normalise input direction
			Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

			// note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is a move input rotate player when the player is moving
			if (_input.move != Vector2.zero)
			{
				// move
				inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
			}

			// move the player
			_controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
		}

		private void JumpAndGravity()
		{
			if (Grounded)
			{
				// reset the fall timeout timer
				_fallTimeoutDelta = FallTimeout;

				// stop our velocity dropping infinitely when grounded
				if (_verticalVelocity < 0.0f)
				{
					_verticalVelocity = -2f;
				}

				// Jump
				if (_input.jump && _jumpTimeoutDelta <= 0.0f)
				{
					// the square root of H * -2 * G = how much velocity needed to reach desired height
					_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
				}

				// jump timeout
				if (_jumpTimeoutDelta >= 0.0f)
				{
					_jumpTimeoutDelta -= Time.deltaTime;
				}
			}
			else
			{
				// reset the jump timeout timer
				_jumpTimeoutDelta = JumpTimeout;

				// fall timeout
				if (_fallTimeoutDelta >= 0.0f)
				{
					_fallTimeoutDelta -= Time.deltaTime;
				}

				// if we are not grounded, do not jump
				_input.jump = false;
			}

			// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
			if (_verticalVelocity < _terminalVelocity)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}
		}

		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;

			// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
			Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
		}
	}
}