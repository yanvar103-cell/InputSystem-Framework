 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Scripts.Player;
using Game.Scripts.LiveObjects;

public class PlayerManager : MonoBehaviour
{
    private GameInputs _input;
    [SerializeField] private Player _player;
    [SerializeField] private InteractableZone _interact;
    
    void Start()
    {
        InitializeInputs();
    }

    private void Update()
    {
        GetDirection();
    }

    void InitializeInputs()
    {
        _input = new GameInputs();
        _input.Player.Enable();
        
    }

    public Vector2 GetDirection()
    {
        Vector2 _direction = _input.Player.Movement.ReadValue<Vector2>();
        return _direction;
    }

    private void OnEnable()
    {
        //subscribing Enable/DisablePlayerInput() to existing events as this class has own GameInputs reference
        Drone.OnEnterFlightMode += DisablePlayerInput;
        Drone.onExitFlightmode += EnablePlayerInput;
        Forklift.onDriveModeEntered += DisablePlayerInput;
        Forklift.onDriveModeExited += EnablePlayerInput;
    }

    private void OnDisable()
    {
        Drone.OnEnterFlightMode -= DisablePlayerInput;
        Drone.onExitFlightmode -= EnablePlayerInput;
        Forklift.onDriveModeEntered -= DisablePlayerInput;
        Forklift.onDriveModeExited -= EnablePlayerInput;
    }

    private void DisablePlayerInput()
    {
        _input.Player.Disable();
    }

    private void EnablePlayerInput()
    {
        _input.Player.Enable();
    }
}
