using System;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem; //new Input System library

namespace Game.Scripts.LiveObjects
{
    public class Forklift : MonoBehaviour
    {
        [SerializeField]
        private GameObject _lift, _steeringWheel, _leftWheel, _rightWheel, _rearWheels;
        [SerializeField]
        private Vector3 _liftLowerLimit, _liftUpperLimit;
        [SerializeField]
        private float _speed = 5f, _liftSpeed = 1f;
        [SerializeField]
        private CinemachineVirtualCamera _forkliftCam;
        [SerializeField]
        private GameObject _driverModel;
        private bool _inDriveMode = false;
        [SerializeField]
        private InteractableZone _interactableZone;

        public static event Action onDriveModeEntered;
        public static event Action onDriveModeExited;

        private GameInputs _input; //defining an InputAsset

        private void Awake()
        {
            _input = new GameInputs();
            _input.ForkLift.Exit.performed += Exit_performed;
        }

        private void Exit_performed(InputAction.CallbackContext context)//new Input system method for exit command instead of polling in void Update
        {
            ExitDriveMode();
        }

        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += EnterDriveMode;
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= EnterDriveMode;
            _input.ForkLift.Exit.performed -= Exit_performed; //unsubscribe
        }

        private void EnterDriveMode(InteractableZone zone)
        {
            if (_inDriveMode !=true && zone.GetZoneID() == 5) //Enter ForkLift
            {
                _inDriveMode = true;
                _forkliftCam.Priority = 11;
                onDriveModeEntered?.Invoke();
                _driverModel.SetActive(true);
                _interactableZone.CompleteTask(5);

                //switching from Player to Forklift
                _input.Player.Disable();
                _input.ForkLift.Enable();

            }
        }

        private void ExitDriveMode()
        {
            _inDriveMode = false;
            _forkliftCam.Priority = 9;            
            _driverModel.SetActive(false);
            onDriveModeExited?.Invoke();

            //switching back to Player
            _input.ForkLift.Disable();
            _input.Player.Enable();
            
        }

        private void Update()
        {
            if (_inDriveMode == true)
            {
                LiftControls();
                CalcutateMovement();
                /*if (Input.GetKeyDown(KeyCode.Escape)) //replaced by Exit_performed(InputAction.CallbackContext context)
                    ExitDriveMode();*/
            }

        }

        private void CalcutateMovement()
        {
            Vector2 _direction = _input.ForkLift.Movement.ReadValue<Vector2>();//getting input Vector2 value from context as _direction

            float h = _direction.x;//Input.GetAxisRaw("Horizontal")
            float v = _direction.y;//Input.GetAxisRaw("Vertical");
            var direction = new Vector3(0, 0, v);
            var velocity = direction * _speed;

            transform.Translate(velocity * Time.deltaTime);

            if (Mathf.Abs(v) > 0)
            {
                var tempRot = transform.rotation.eulerAngles;
                tempRot.y += h * _speed / 2;
                transform.rotation = Quaternion.Euler(tempRot);
            }
        }

        private void LiftControls()
        {
            float _liftDirection = _input.ForkLift.Lift.ReadValue<float>();
            
            if (_liftDirection > 0)//Input.GetKey(KeyCode.R)
                LiftUpRoutine();
            else if (_liftDirection < 0)//Input.GetKey(KeyCode.T)
                LiftDownRoutine();

        }

        private void LiftUpRoutine()
        {
            if (_lift.transform.localPosition.y < _liftUpperLimit.y)
            {
                Vector3 tempPos = _lift.transform.localPosition;
                tempPos.y += Time.deltaTime * _liftSpeed;
                _lift.transform.localPosition = new Vector3(tempPos.x, tempPos.y, tempPos.z);
            }
            else if (_lift.transform.localPosition.y >= _liftUpperLimit.y)
                _lift.transform.localPosition = _liftUpperLimit;
        }

        private void LiftDownRoutine()
        {
            if (_lift.transform.localPosition.y > _liftLowerLimit.y)
            {
                Vector3 tempPos = _lift.transform.localPosition;
                tempPos.y -= Time.deltaTime * _liftSpeed;
                _lift.transform.localPosition = new Vector3(tempPos.x, tempPos.y, tempPos.z);
            }
            else if (_lift.transform.localPosition.y <= _liftUpperLimit.y)
                _lift.transform.localPosition = _liftLowerLimit;
        }
    }
}