using UnityEngine;
#if ARSDK_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class FlyCamera : MonoBehaviour
{
    /*
    Made simple to use (drag and drop, done) for regular keyboard layout
    wasd : basic movement
    shift : Makes camera accelerate
    space : Moves camera on X and Z axis only.  So camera doesn't gain any height*/

    float mainSpeed = 5.0f; //regular speed
    float shiftAdd = 5.0f; //multiplied by how long shift is held.  Basically running
    float maxShift = 10.0f; //Maximum speed when holdin gshift
    float camSens = 0.1f; //How sensitive it with mouse

    private Vector3 lastMouse = Vector3.zero; //kind of in the middle of the screen, rather than at the top (play)
    private float totalRun = 1.0f;

    void Update()
    {
#if UNITY_EDITOR
        if (!GetKeyPressed(KeyInput.LeftCtrl))
        {
            return;
        }
#else
        return;
#endif

        if (GetKeyDown(KeyInput.LeftCtrl))
        {
            lastMouse = Vector3.zero;
            return;
        }

        if (lastMouse == Vector3.zero)
        {
            lastMouse = GetMousePos();
            return;
        }

        lastMouse = GetMousePos() - lastMouse;
        lastMouse = new Vector3(-lastMouse.y * camSens, lastMouse.x * camSens, 0);
        lastMouse = new Vector3(transform.eulerAngles.x + lastMouse.x, transform.eulerAngles.y + lastMouse.y, 0);
        transform.eulerAngles = lastMouse;
        lastMouse = GetMousePos();
        //Mouse  camera angle done.

        float prev_y = transform.position.y;

        //Keyboard commands
        Vector3 p = GetBaseInput();
        if (p.sqrMagnitude > 0)
        { // only move while a direction key is pressed
            if (GetKeyPressed(KeyInput.LeftShift))
            {
                totalRun += Time.deltaTime;
                p = p * totalRun * shiftAdd;
                p.x = Mathf.Clamp(p.x, -maxShift, maxShift);
                p.y = Mathf.Clamp(p.y, -maxShift, maxShift);
                p.z = Mathf.Clamp(p.z, -maxShift, maxShift);
            }
            else
            {
                totalRun = Mathf.Clamp(totalRun * 0.5f, 1f, 1000f);
                p = p * mainSpeed;
            }

            p = p * Time.deltaTime;
            Vector3 newPosition = transform.position;
            if (GetKeyPressed(KeyInput.Space))
            { //If player wants to move on X and Z axis only
                transform.Translate(p);
                newPosition.x = transform.position.x;
                newPosition.z = transform.position.z;
                transform.position = newPosition;
            }
            else
            {
                transform.Translate(p);
            }
        }

        transform.position = new Vector3(transform.position.x, prev_y, transform.position.z);
    }

    private Vector3 GetBaseInput()
    { //returns the basic values, if it's 0 than it's not active.
        Vector3 p_Velocity = new Vector3();
        if (GetKeyPressed(KeyInput.W))
        {
            p_Velocity += new Vector3(0, 0, 1);
        }
        if (GetKeyPressed(KeyInput.S))
        {
            p_Velocity += new Vector3(0, 0, -1);
        }
        if (GetKeyPressed(KeyInput.A))
        {
            p_Velocity += new Vector3(-1, 0, 0);
        }
        if (GetKeyPressed(KeyInput.D))
        {
            p_Velocity += new Vector3(1, 0, 0);
        }
        return p_Velocity;
    }

    private enum KeyInput { LeftCtrl, LeftShift, Space, W, S, A, D }

#if ARSDK_INPUT_SYSTEM
    private static Key ToKey(KeyInput k) => k switch
    {
        KeyInput.LeftCtrl => Key.LeftCtrl,
        KeyInput.LeftShift => Key.LeftShift,
        KeyInput.Space => Key.Space,
        KeyInput.W => Key.W,
        KeyInput.S => Key.S,
        KeyInput.A => Key.A,
        KeyInput.D => Key.D,
        _ => Key.None
    };

    private bool GetKeyPressed(KeyInput k)
    {
        return Keyboard.current != null && Keyboard.current[ToKey(k)].isPressed;
    }

    private bool GetKeyDown(KeyInput k)
    {
        return Keyboard.current != null && Keyboard.current[ToKey(k)].wasPressedThisFrame;
    }

    private Vector3 GetMousePos()
    {
        if (Mouse.current == null) return Vector3.zero;
        var pos = Mouse.current.position.ReadValue();
        return new Vector3(pos.x, pos.y, 0);
    }
#elif ENABLE_LEGACY_INPUT_MANAGER
    private static KeyCode ToKeyCode(KeyInput k) => k switch {
        KeyInput.LeftCtrl  => KeyCode.LeftControl,
        KeyInput.LeftShift => KeyCode.LeftShift,
        KeyInput.Space     => KeyCode.Space,
        KeyInput.W         => KeyCode.W,
        KeyInput.S         => KeyCode.S,
        KeyInput.A         => KeyCode.A,
        KeyInput.D         => KeyCode.D,
        _ => KeyCode.None
    };

    private bool GetKeyPressed(KeyInput k) {
        return Input.GetKey(ToKeyCode(k));
    }

    private bool GetKeyDown(KeyInput k) {
        return Input.GetKeyDown(ToKeyCode(k));
    }

    private Vector3 GetMousePos() {
        return Input.mousePosition;
    }
#endif
}
