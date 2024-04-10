using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal enum JumpState {
    SlowJump,
    MediumJump,
    FastJump
}

public class MarioController : MonoBehaviour {
    private bool keySpace, keyD, keyA, keyShift, keySpaceDown;
    private float xvel, yvel;
    private Animator animator;
    public static MarioController mario;
    private SpriteRenderer spriteRenderer;

    private bool isJumping, isSkidding;
    private JumpState jump;

    private const float conversion = 65536;
    private const float maxRunX = 10496 / conversion;
    private const float maxWalkX = 6400 / conversion;
    private const float walkAcc = 152 / conversion;
    private const float runAcc = 228 / conversion;

    private const float skidPower = 416 / conversion;
    private const float releaseDeAcc = 208 / conversion;

    private const float fastJumpPower = 20480 / conversion;
    private const float jumpPower = 16384 / conversion;

    private const float fastJumpReq = 9472 / conversion;
    private const float midJumpReq = 4096 / conversion;

    private const float fastJumpDecay = 2304 / conversion;
    private const float fastJumpDecayUp = 640 / conversion;
    private const float medJumpDecay = 1536 / conversion;
    private const float medJumpDecayUp = 460 / conversion;
    private const float slowJumpDecay = 1792 / conversion;
    private const float slowJumpDecayUp = 490 / conversion;

    // Start is called before the first frame update
    void Start() {
        animator = GetComponent<Animator>();
        mario = this;
        xvel = 0;
        yvel = 0;
        isJumping = false;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update() {
        keyD = Input.GetKey(KeyCode.D);
        keyA = Input.GetKey(KeyCode.A);
        keySpace = Input.GetKey(KeyCode.Space);
        keySpaceDown = Input.GetKeyDown(KeyCode.Space);
        keyShift = Input.GetKey(KeyCode.LeftShift);
    }

    private void Move(Vector2 move) {
        Vector2 curPos = transform.position;
        transform.position += new Vector3(move.x, move.y, 0);
    }

    private void FixedUpdate() {
        bool isMoving = false;
        bool isSkidding = false;

        if (keySpaceDown) {
            isJumping = true;
            if (Mathf.Abs(xvel) > fastJumpReq) {
                jump = JumpState.FastJump;
                yvel = fastJumpPower;
            }
            else if (Mathf.Abs(xvel) > midJumpReq) {
                jump = JumpState.MediumJump;
                yvel = jumpPower;
            }
            else {
                jump = JumpState.SlowJump;
                yvel = jumpPower;
            }
        }

        if (keyD) {
            isMoving = true;
            if (xvel >= 0) {
                if (keyShift) {
                    xvel += runAcc;
                }
                else {
                    xvel += walkAcc;
                }
            }
            else {
                xvel += skidPower;
                isSkidding = true;
            }
        }

        if (keyA) {
            isMoving = true;
            if (xvel <= 0) {
                if (keyShift) {
                    xvel -= runAcc;
                }
                else {
                    xvel -= walkAcc;
                }
            }
            else {
                xvel -= skidPower;
                isSkidding = true;
            }
        }

        if (!isMoving) {
            if (xvel > 0) {
                xvel -= releaseDeAcc;
                if (xvel < 0) {
                    xvel = 0;
                }
            }
            else {
                xvel += releaseDeAcc;
                if (xvel > 0) {
                    xvel = 0;
                }
            }
        }

        float maxSpeed = keyShift ? maxRunX : maxWalkX;
        if (xvel >= maxSpeed) {
            xvel = maxSpeed;
        }
        else if (xvel < maxSpeed) {
            xvel = -maxSpeed;
        }

        if (keySpace) {
            switch (jump) {
            case JumpState.SlowJump:
                yvel -= slowJumpDecayUp;
                break;
            case JumpState.MediumJump:
                yvel -= medJumpDecayUp;
                break;
            case JumpState.FastJump:
                yvel -= fastJumpDecayUp;
                break;
            default:
                break;
            }
        }
/*        else {
            switch (jump) {
            case JumpState.SlowJump:
                yvel -= slowJumpDecay;
                break;
            case JumpState.MediumJump:
                yvel -= medJumpDecay;
                break;
            case JumpState.FastJump:
                yvel -= fastJumpDecay;
                break;
            default:
                break;
            }
        }*/

        Move(new Vector2(xvel, yvel));

        animator.SetFloat("xvel", Mathf.Abs(xvel));
        animator.SetBool("isSkidding", isSkidding);
        animator.SetBool("isJumping", isJumping);
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "ground") {
            xvel = 0;
        }
    }
}
