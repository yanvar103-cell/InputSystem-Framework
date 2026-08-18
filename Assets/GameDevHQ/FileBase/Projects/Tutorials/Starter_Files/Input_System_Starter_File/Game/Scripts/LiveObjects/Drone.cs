using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Game.Scripts.UI;
using UnityEngine.InputSystem; //new Input System library

namespace Game.Scripts.LiveObjects
{
    public class Drone : MonoBehaviour
    {
        private enum Tilt
        {
            NoTilt, Forward, Back, Left, Right
        }

        [SerializeField]
        private Rigidbody _rigidbody;
        [SerializeField]
        private float _speed = 5f;
        private bool _inFlightMode = false;
        [SerializeField]
        private Animator _propAnim;
        [SerializeField]
        private CinemachineVirtualCamera _droneCam;
        [SerializeField]
        private InteractableZone _interactableZone;
        

        public static event Action OnEnterFlightMode;
        public static event Action onExitFlightmode;

        private GameInputs _input; //defining an InputAsset

        private void Awake()
        {
            _input = new GameInputs();
            _input.Drone.Exit.performed += Exit_performed;
        }

        private void Exit_performed(InputAction.CallbackContext context)//new Input System to Exit command
        {
            _inFlightMode = false;
            onExitFlightmode?.Invoke();
            ExitFlightMode();
        }

        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += EnterFlightMode;
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= EnterFlightMode;
            _input.Drone.Exit.performed -= Exit_performed; //unsubscribe
        }

        private void EnterFlightMode(InteractableZone zone)
        {
            if (_inFlightMode != true && zone.GetZoneID() == 4) // drone Scene
            {
                _propAnim.SetTrigger("StartProps");
                _droneCam.Priority = 11;
                _inFlightMode = true;
                OnEnterFlightMode?.Invoke();
                UIManager.Instance.DroneView(true);
                _interactableZone.CompleteTask(4);
                
                //Switch between Player and Drone control(action maps)
                _input.Player.Disable();
                _input.Drone.Enable();
            }
        }

        private void ExitFlightMode()
        {            
            _droneCam.Priority = 9;
            _inFlightMode = false;
            UIManager.Instance.DroneView(false);

            //switch back to Player action map
            _input.Drone.Disable();
            _input.Player.Enable();
        }

        private void Update()
        {
            if (_inFlightMode)
            {
                CalculateTilt();
                CalculateMovementUpdate();

                /*if (Input.GetKeyDown(KeyCode.Escape))//Legacy input, replaced by Exit_performed method
                {
                    _inFlightMode = false;
                    onExitFlightmode?.Invoke();
                    ExitFlightMode();
                }*/
            }
        }

        private void FixedUpdate()
        {
            _rigidbody.AddForce(transform.up * (9.81f), ForceMode.Acceleration);
            if (_inFlightMode)
                CalculateMovementFixedUpdate();
        }

        private void CalculateMovementUpdate()
        {
            float _rotateInput = _input.Drone.Rotate.ReadValue<float>();//getting Rotate input 1D Axis Value every frame

            if (_rotateInput != 0)//instead of using Input.GetKey(KeyCode.LeftArrow)
            {
                var tempRot = transform.localRotation.eulerAngles;
                tempRot.y += _speed / 3 * _rotateInput; // positive = right arrow, negative = left arrow
                transform.localRotation = Quaternion.Euler(tempRot);
            }
            /*if (Input.GetKey(KeyCode.RightArrow))//no need this state _rotateInput Value handles both - and + multiplier
            {
                var tempRot = transform.localRotation.eulerAngles;
                tempRot.y += _speed / 3;
                transform.localRotation = Quaternion.Euler(tempRot);
            }*/
        }

        private void CalculateMovementFixedUpdate()
        {
            float _thrustInput = _input.Drone.Thrust.ReadValue<float>();

            if (_thrustInput != 0)//instead of using Input.GetKey(KeyCode.Space)
            {
                _rigidbody.AddForce(transform.up * _speed * _thrustInput, ForceMode.Acceleration);
            }
            /*if (Input.GetKey(KeyCode.V))
            {
                _rigidbody.AddForce(-transform.up * _speed, ForceMode.Acceleration);
            }*/
        }

        private void CalculateTilt()
        {
            Vector2 _tilt = _input.Drone.Tilt.ReadValue<Vector2>();//getting Vector2 input value every frame

            if (_tilt.x < 0) //instead of using Input.GetKey(KeyCode.A)
                transform.rotation = Quaternion.Euler(00, transform.localRotation.eulerAngles.y, 30);
            else if (_tilt.x > 0) //instead of using Input.GetKey(KeyCode.D)
                transform.rotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, -30);
            else if (_tilt.y > 0) //instead of using Input.GetKey(KeyCode.W)
                transform.rotation = Quaternion.Euler(30, transform.localRotation.eulerAngles.y, 0);
            else if (_tilt.y < 0) //instead of using Input.GetKey(KeyCode.S)
                transform.rotation = Quaternion.Euler(-30, transform.localRotation.eulerAngles.y, 0);
            else 
                transform.rotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, 0);
        }
    }
}
