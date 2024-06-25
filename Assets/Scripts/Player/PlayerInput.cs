using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerInput : NetworkBehaviour
{
    public Vector3 MovementInput { get; private set; }
    public Vector2 MouseInput { get; private set; }
    public event Action<bool> MouseLeftEvent;
    public event Action InteractionEvent;
    public bool PressShift { get; private set; }
    public bool TriggerSpace { get; private set; }

    bool mouseLeft = false;

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        
        MovementInput = new Vector3(x, 0, y);
        PressShift = Input.GetKey(KeyCode.LeftShift);
        TriggerSpace = Input.GetButtonDown("Jump");

        MouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        // left 마우스
        bool isLeftDown = Input.GetMouseButton(0);
        if (isLeftDown && !mouseLeft) {
            mouseLeft = true;
            MouseLeftEvent?.Invoke(true);
        } else if (!isLeftDown && mouseLeft) {
            mouseLeft = false;
            MouseLeftEvent?.Invoke(false);
        }

        // E키
        if (Input.GetKeyDown(KeyCode.E)) {
            InteractionEvent?.Invoke();
        }
    }
}
