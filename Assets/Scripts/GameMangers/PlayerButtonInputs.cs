using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerButtonInput : MonoBehaviour
{
    private InputSystem_Actions inputActions;

    public float pullStrength = 10f;
    public float pullRadius = 2f;
    private bool isDragging = false;

    public BoxCollider2D boundaryCollider; // Assign in inspector

    void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    void OnEnable()
    {
        inputActions.Enable();
    }

    void OnDisable()
    {
        inputActions.Disable();
    }

    void Update()
    {
        
    }

    void OnDrawGizmos()
    {
      
    }
}