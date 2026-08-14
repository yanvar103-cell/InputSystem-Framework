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
}
