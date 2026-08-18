using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Scripts.UI;
using UnityEngine.InputSystem; //new Input System library


namespace Game.Scripts.LiveObjects
{
    public class InteractableZone : MonoBehaviour
    {
        private enum ZoneType
        {
            Collectable,
            Action,
            HoldAction
        }

        private enum KeyState
        {
            Press,
            PressHold
        }

        [SerializeField]
        private ZoneType _zoneType;
        [SerializeField]
        private int _zoneID;
        [SerializeField]
        private int _requiredID;
        [SerializeField]
        [Tooltip("Press the (---) Key to .....")]
        private string _displayMessage;
        [SerializeField]
        private GameObject[] _zoneItems;
        private bool _inZone = false;
        private bool _itemsCollected = false;
        private bool _actionPerformed = false;
        [SerializeField]
        private Sprite _inventoryIcon;
        [SerializeField]
        private KeyCode _zoneKeyInput;
        [SerializeField]
        private KeyState _keyState;
        [SerializeField]
        private GameObject _marker;

        private bool _inHoldState = false;

        private static int _currentZoneID = 0;
        public static int CurrentZoneID
        { 
            get 
            { 
               return _currentZoneID; 
            }
            set
            {
                _currentZoneID = value; 
                         
            }
        }

        //custom game logic events
        public static event Action<InteractableZone> onZoneInteractionComplete;
        public static event Action<int> onHoldStarted;
        public static event Action<int, float> onHoldEnded;//added float duration to implement hold tap input in crate destructing

        private float _holdStartTime;//to calculate hold duration in HoldInteract_started/canceled()
        
        private GameInputs _input;//defining an InputAsset

        private void Awake()//initializes before OnEnable
        {
            InitializeInput();
        }
        void InitializeInput()//initializing input reference with overriding bindings
        {
            _input = new GameInputs();//reference(each InteractableZone GETS ITS OWN separate copy)
            // override the binding to use THIS zone's specific key(every object has it's own key binding defined in _zoneKeyInput)
            _input.Player.Interact.ApplyBindingOverride($"<Keyboard>/{_zoneKeyInput.ToString().ToLower()}");
            _input.Player.HoldInteract.ApplyBindingOverride($"<Keyboard>/{_zoneKeyInput.ToString().ToLower()}");
            //Debug.Log($"Zone {_zoneID} bound to key: {_zoneKeyInput.ToString().ToLower()}");
        }
        
        private void OnEnable()//Unity calls OnEnable() automatically when the GameObject becomes active in the scene after Awake()
        {
            InteractableZone.onZoneInteractionComplete += SetMarker;

            _input.Player.Interact.Enable();//starts listening for that key at all
            _input.Player.Interact.performed += Interact_performed;//subscribe a method to run WHEN it's triggered
            _input.Player.HoldInteract.Enable();//starts listening for that key at all
            _input.Player.HoldInteract.started += HoldInteract_started;//for the action began (key just pressed down)
            _input.Player.HoldInteract.canceled += HoldInteract_canceled;//for the action stopped (key released)
        }

        private void OnDisable()//mirror image of OnEnable, cleanup. Unity calls OnDisable() automatically when the GameObject becomes inactive(SetActive(false))
        {
            InteractableZone.onZoneInteractionComplete -= SetMarker;
            
            _input.Player.Interact.performed -= Interact_performed;// unsubscribe
            _input.Player.Interact.Disable();// stop listening
            _input.Player.HoldInteract.started -= HoldInteract_started;//
            _input.Player.HoldInteract.canceled -= HoldInteract_canceled;//
            _input.Player.HoldInteract.Disable();// stop listening
        }

        private void HoldInteract_started(InputAction.CallbackContext context)//Keyboard Input System event(bridge method)
        {
            //Debug.Log($"HoldInteract_started fired! _inZone={_inZone}, _keyState={_keyState}, _inHoldState={_inHoldState}");
            if (!_inZone || _keyState != KeyState.PressHold || _inHoldState) return;
            _inHoldState = true;
            _holdStartTime = Time.time;//record when hold began

            switch (_zoneType)
            {
                case ZoneType.HoldAction:
                    PerformHoldAction();
                    break;
            }
        }

        private void HoldInteract_canceled(InputAction.CallbackContext context)//Keyboard Input System event(bridge method)
        {
            //Debug.Log($"HoldInteract_canceled fired for zone {_zoneID}! _keyState={_keyState}");
            if (!_inZone || _keyState != KeyState.PressHold) return; //check _inZone to prevent PressHold conflict between zone 3 and 6
            _inHoldState = false;

            float _duration = Time.time - _holdStartTime;//returns how long key holded
            //Debug.Log($"Duration: {_duration}, invoking onHoldEnded for zone {_zoneID}");
            onHoldEnded?.Invoke(_zoneID, _duration); //passes zoneID and duration
        }

        private void Interact_performed(InputAction.CallbackContext context)//Keyboard Input System event(bridge method)
        {
            if (!_inZone || _keyState == KeyState.PressHold) return;
            switch(_zoneType)
            {
                case ZoneType.Collectable:
                    if (_itemsCollected == false)
                    {
                        CollectItems();
                        _itemsCollected = true;
                        UIManager.Instance.DisplayInteractableZoneMessage(false);
                    }
                    break;
                case ZoneType.Action:
                    if (_actionPerformed == false)
                    {
                        PerformAction();
                        _actionPerformed = true;
                        UIManager.Instance.DisplayInteractableZoneMessage(false);
                    }
                    break;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && _currentZoneID > _requiredID)
            {
                Debug.Log("Entered zone, _inZone should become true. CurrentZoneID: " + _currentZoneID + " RequiredID: " + _requiredID);
                switch (_zoneType)
                {
                    case ZoneType.Collectable:
                        if (_itemsCollected == false)
                        {
                            _inZone = true;
                            if (_displayMessage != null)
                            {
                                string message = $"Press the {_zoneKeyInput.ToString()} key to {_displayMessage}.";
                                UIManager.Instance.DisplayInteractableZoneMessage(true, message);
                            }
                            else
                                UIManager.Instance.DisplayInteractableZoneMessage(true, $"Press the {_zoneKeyInput.ToString()} key to collect");
                        }
                        break;

                    case ZoneType.Action:
                        if (_actionPerformed == false)
                        {
                            _inZone = true;
                            if (_displayMessage != null)
                            {
                                string message = $"Press the {_zoneKeyInput.ToString()} key to {_displayMessage}.";
                                UIManager.Instance.DisplayInteractableZoneMessage(true, message);
                            }
                            else
                                UIManager.Instance.DisplayInteractableZoneMessage(true, $"Press the {_zoneKeyInput.ToString()} key to perform action");
                        }
                        break;

                    case ZoneType.HoldAction:
                        _inZone = true;
                        if (_displayMessage != null)
                        {
                            string message = $"Press the {_zoneKeyInput.ToString()} key to {_displayMessage}.";
                            UIManager.Instance.DisplayInteractableZoneMessage(true, message);
                        }
                        else
                            UIManager.Instance.DisplayInteractableZoneMessage(true, $"Hold the {_zoneKeyInput.ToString()} key to perform action");
                        break;
                }
            }
        }

        /*private void Update()
        {
            if (_inZone == true)
            {

                if (Input.GetKeyDown(_zoneKeyInput) && _keyState != KeyState.PressHold)
                {
                    //press
                    switch (_zoneType)
                    {
                        case ZoneType.Collectable:
                            if (_itemsCollected == false)
                            {
                                CollectItems();
                                _itemsCollected = true;
                                UIManager.Instance.DisplayInteractableZoneMessage(false);
                            }
                            break;

                        case ZoneType.Action:
                            if (_actionPerformed == false)
                            {
                                PerformAction();
                                _actionPerformed = true;
                                UIManager.Instance.DisplayInteractableZoneMessage(false);
                            }
                            break;
                    }
                }
                else if (Input.GetKey(_zoneKeyInput) && _keyState == KeyState.PressHold && _inHoldState == false)
                {
                    _inHoldState = true;

                   

                    switch (_zoneType)
                    {                      
                        case ZoneType.HoldAction:
                            PerformHoldAction();
                            break;           
                    }
                }

                if (Input.GetKeyUp(_zoneKeyInput) && _keyState == KeyState.PressHold)
                {
                    _inHoldState = false;
                    onHoldEnded?.Invoke(_zoneID);
                }
            }
        }*///deactivated update polling, using event subscription
       
        private void CollectItems()
        {
            foreach (var item in _zoneItems)
            {
                item.SetActive(false);
            }

            UIManager.Instance.UpdateInventoryDisplay(_inventoryIcon);

            CompleteTask(_zoneID);

            onZoneInteractionComplete?.Invoke(this);

        }

        private void PerformAction()
        {
            foreach (var item in _zoneItems)
            {
                item.SetActive(true);
            }

            if (_inventoryIcon != null)
                UIManager.Instance.UpdateInventoryDisplay(_inventoryIcon);

            onZoneInteractionComplete?.Invoke(this);
        }

        private void PerformHoldAction()
        {
            UIManager.Instance.DisplayInteractableZoneMessage(false);
            onHoldStarted?.Invoke(_zoneID);
        }

        public GameObject[] GetItems()
        {
            return _zoneItems;
        }

        public int GetZoneID()
        {
            return _zoneID;
        }

        public void CompleteTask(int zoneID)
        {
            if (zoneID == _zoneID)
            {
                _currentZoneID++;
                onZoneInteractionComplete?.Invoke(this);
            }
        }

        public void ResetAction(int zoneID)
        {
            if (zoneID == _zoneID)
                _actionPerformed = false;
        }

        public void SetMarker(InteractableZone zone)
        {
            if (_zoneID == _currentZoneID)
                _marker.SetActive(true);
            else
                _marker.SetActive(false);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _inZone = false;
                UIManager.Instance.DisplayInteractableZoneMessage(false);
            }
        }
    }
}


