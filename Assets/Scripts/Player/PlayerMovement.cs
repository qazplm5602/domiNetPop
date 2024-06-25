using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

enum PlayerMoveStatus {
    Idle = 0,
    Walk,
    Run
}

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField, Range(0, 100)] float walkSpeed = 10;
    [SerializeField, Range(0, 100)] float runSpeed = 25;
    [SerializeField, Range(0, 50)] float jumpPower = 3;

    [SerializeField] Vector3 boxSize;
    [SerializeField] LayerMask whatIsGround;

    Vector3 moveDir;

    CharacterController _controller;
    PlayerInput input;
    Animator animator;

    
    private void Awake() {
        animator = transform.Find("visual").GetComponent<Animator>();
        _controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInput>();
    }

    private void Update() {
        if (!IsOwner) return;

        Move();
        SmoothAnimation();
    }

    PlayerMoveStatus moveStatus = PlayerMoveStatus.Idle;
    readonly int animJumpId = Animator.StringToHash("Jump");
    readonly int animGroundId = Animator.StringToHash("isGround");
    void Move() {
        if (_controller.isGrounded) {
            moveDir = input.MovementInput.normalized;

            // 플레이어 방향대로 바꿩 ( 로컬 좌표 -> 월드 좌표 )
            moveDir = transform.TransformDirection(moveDir);
        
            // 속도
            moveDir *= input.PressShift ? runSpeed : walkSpeed;

            if (input.TriggerSpace) {
                moveDir.y = jumpPower; // 점프
                animator.SetTrigger(animJumpId);
            }
        }

        if ( Mathf.Abs(moveDir.x) > 0.1f || Mathf.Abs(moveDir.z) > 0.1f) {
            moveStatus = input.PressShift ? PlayerMoveStatus.Run : PlayerMoveStatus.Walk;
        } else {
            moveStatus = PlayerMoveStatus.Idle;
        }

        // 중력 500배ㅐㅐ
        // moveDir.y -= 20.0f * Time.deltaTime;
        moveDir += Physics.gravity * Time.deltaTime;
        animator.SetBool(animGroundId, IsGrounded());

        _controller.Move(moveDir * Time.deltaTime);
    }

    readonly int animId = Animator.StringToHash("Blend");
    float blendVal = 0;
    void SmoothAnimation() {
        blendVal = Mathf.Lerp(blendVal, (int)moveStatus, Time.deltaTime * 20);
        animator.SetFloat(animId, blendVal);
    }

    private void OnDrawGizmos()
    {

        Gizmos.color = Color.cyan;
        Gizmos.DrawCube(transform.position - transform.up * 0, boxSize);
    }

    bool IsGrounded() {
        // return Physics.BoxCast(transform.position, boxSize, -transform.up * 0, transform.rotation, 5, whatIsGround);
        return Physics.BoxCast(transform.position + Vector3.up, boxSize, -transform.up, transform.rotation, 1, whatIsGround);
    }

    // TP
    [ClientRpc]
    public void SetPlayerCoordsClientRpc(Vector3 coords) {
        transform.position = coords;
    }

    [ClientRpc]
    public void SetPlayerHeadingClientRpc(Vector3 rotate) {
        transform.rotation = Quaternion.Euler(rotate);
    }
}
