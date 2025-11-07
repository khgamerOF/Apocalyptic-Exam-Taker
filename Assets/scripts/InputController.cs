using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    public static InputController instance;
    private PlayerInput playerInput;
    private PlayerInputActions playerInputActions;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        playerInput = GetComponent<PlayerInput>();
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
    }

    public bool Jump()
    {
        float jumpFloat = playerInputActions.Player.SouthButton.ReadValue<float>();

        if(jumpFloat >= 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
