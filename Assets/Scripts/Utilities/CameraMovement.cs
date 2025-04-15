using System;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] float speedMultiplier;

    InputSystem_Actions inputActions;
    
    // Start is called before the first frame update
    void Start()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Enable();
    }
    
    // Update is called once per frame
    void Update()
    {
        Vector2 direction = inputActions.Player.Move.ReadValue<Vector2>();
        
        Vector3 direction3D = new Vector3(direction.x, 0, direction.y);
        
        transform.position += direction3D * (speedMultiplier * Time.deltaTime);
    }

    void OnDisable()
    {
        inputActions.Disable();
    }
}
