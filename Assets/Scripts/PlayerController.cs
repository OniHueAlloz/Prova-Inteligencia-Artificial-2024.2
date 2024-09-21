using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    [Header("Configurations")]
    [SerializeField] float jumpForce = 20f;
    [SerializeField] float walkSpeed = 10f;
    [SerializeField] float runSpeed = 20f;
    [SerializeField] float delayTime = 2f;

    private Animator playerAnimator;
    private Rigidbody2D playerRb;
    private SpriteRenderer playerSprite;

    private bool attackInput = false;
    private bool jumpInput = false;
    private bool fireballInput = false;
    private bool arrowInput = false;
    private bool danceInput = false;
    private bool dashInput = false;
    private bool kickInput = false;
    private bool runInput = false;
    private bool upInput = false;
    private bool downInput = false;
    private float moveInput = 0f;
    private bool isGrounded = false;
    private bool isMounted = false;
    private bool damageTest = false;
    private bool hasDashed = false;

    enum State { Idle, Arrow, Dance, Dash, Attack, Fall, Fireball, Jump, Kick, Lay, Move }

    State currentState = State.Idle;

    // Start is called before the first frame update
    void Start()
    {
        playerAnimator = GetComponent<Animator>();
        playerRb = GetComponent<Rigidbody2D>();
        playerSprite = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        attackInput = Input.GetKey(KeyCode.J);
        jumpInput = Input.GetKey(KeyCode.Space);
        fireballInput = Input.GetKey(KeyCode.K);
        arrowInput = Input.GetKey(KeyCode.L);
        danceInput = Input.GetKey(KeyCode.B);
        kickInput = Input.GetKey(KeyCode.H);
        upInput = Input.GetKey(KeyCode.W);
        downInput = Input.GetKey(KeyCode.S);
        runInput = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        dashInput = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        moveInput = Input.GetAxisRaw("Horizontal");
        damageTest = Input.GetKey(KeyCode.Backspace);

        //{ Idle, Arrow, Dance, Dash, Attack, Fall, Fireball, Jump, Kick, Lay, Move }

        switch (currentState)
        {
            case State.Idle: IdleCommands(); break;
            case State.Arrow: ArrowCommands(); break;
            case State.Dance: DanceCommands(); break;
            case State.Dash: DashCommands(); break;
            case State.Attack: AttackCommands(); break;
            case State.Fall: FallCommands(); break;
            case State.Fireball: FireballCommands(); break;
            case State.Jump: JumpCommands(); break;
            case State.Kick: KickCommands(); break;
            case State.Lay: LayCommands(); break;
            case State.Move: MoveCommands(); break;
        }

        if (damageTest) currentState = State.Lay;

    }

    private void FlipX()
    {
        if (moveInput < 0f && transform.localScale.x > 0)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
        else if (moveInput > 0f && transform.localScale.x < 0)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
    }

    private void MoveX(float speed)
    {
        FlipX();

        playerRb.velocity = new Vector2(moveInput * speed, playerRb.velocity.y);
    }

    private bool IsAnimationFinished(string animationName)
    {
        AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName(animationName) && stateInfo.normalizedTime >= 1.0f;
    }

    private void CloseState(string animationName)
    {
        if(IsAnimationFinished(animationName))
        {
            playerRb.gravityScale = 1;

            if (isGrounded)
            {
                currentState = State.Idle;
            }
            else currentState = State.Fall;
        }
    }

    private bool IsAttacking()
    {
        return playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("NormalAttack") ||
        playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("UpAttack") ||
        playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("DownAttack") ||
        playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("HorseAttack") ||
        playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("JumpAttack") ||
        playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Kick");
    }

    private void IdleCommands()
    {
        if (arrowInput)
        {
            currentState = State.Arrow;
        }
        else if (fireballInput)
        {
            currentState = State.Fireball;
        }
        else if (danceInput)
        {
            currentState = State.Dance;
        }
        else if (dashInput && !hasDashed)
        {
            Vector2 dashDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
            playerRb.AddForce(dashDirection * jumpForce, ForceMode2D.Impulse);
            currentState = State.Dash;
        }
        else if (attackInput || upInput || downInput)
        {
            if (IsAttacking())
            {
                return;
            }
            currentState = State.Attack;
        }
        else if (moveInput != 0f || runInput)
        {
            currentState = State.Move;
        }
        else if (kickInput)
        {
            currentState = State.Kick;
        }
        else if (isGrounded)
        {
            if(jumpInput)
            {
                playerRb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
                isGrounded = false;
                currentState = State.Jump;
            }
            else if (isMounted)
            {
                FlipX();
                playerAnimator.Play("HorseIdle");
            }
            else
            {
                FlipX();
                playerAnimator.Play("Idle");
            }
        }
        else if (!isGrounded)
        {
            currentState = State.Fall;
        }
    }

    private void ArrowCommands()
    {
        playerAnimator.Play("Arrow");

        CloseState("Arrow");
    }

    private void DanceCommands()
    {
        playerAnimator.Play("Dance");

        CloseState("Dance");
    }

    private void DashCommands()
    {
        if(!hasDashed && !isGrounded)
        {
            playerRb.gravityScale = 0;
            playerAnimator.Play("Dash");
            CloseState("Dash");
        }
        else if (!hasDashed)
        {
            playerAnimator.Play("Roll");
            CloseState("Roll");
        }
    }

    private void AttackCommands()
    {
        FlipX();

        if (damageTest)
        {
            currentState = State.Lay;
        }
        else if (isGrounded && upInput)
        {
            playerAnimator.Play("UpAttack");

            CloseState("UpAttack");
        }
        else if (isGrounded && downInput)
        {
            playerAnimator.Play("DownAttack");

            CloseState("DownAttack");
        }
        else if (isGrounded)
        {
            playerAnimator.Play("NormalAttack");

            CloseState("NormalAttack");
        }
        else if (isMounted)
        {
            playerAnimator.Play("HorseAttack");

            CloseState("HorseAttack");
        }
        else if (!isGrounded)
        {
            playerRb.gravityScale = 0;
            playerAnimator.Play("JumpAttack");

            CloseState("JumpAttack");
        }
    }

    private void FallCommands()
    {
        if (playerRb.velocity.y < 0 && !isGrounded)
        {
            playerAnimator.Play("Fall");

            MoveX(walkSpeed);

            if(attackInput)
            {
                currentState = State.Attack;
            }
            else if (dashInput && !hasDashed)
            {
                Vector2 dashDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
                playerRb.AddForce(dashDirection * jumpForce, ForceMode2D.Impulse);
                currentState = State.Dash;
            }
        }
        else
        {
            playerAnimator.Play("Land");

            if(IsAnimationFinished("Land"))
            {
                currentState = State.Idle;
            }
        }
    }

    private void FireballCommands()
    {
        playerAnimator.Play("Fireball");

        CloseState("Fireball");
    }

    private void JumpCommands()
    {
        playerAnimator.Play("Jump");

        MoveX(walkSpeed);

        if (attackInput)
        {
            currentState = State.Attack;
        }
        else if (dashInput && !hasDashed)
        {
            Vector2 dashDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
            playerRb.AddForce(dashDirection * jumpForce, ForceMode2D.Impulse);
            currentState = State.Dash;
        }
        else if (playerRb.velocity.y < 0)
        {
            currentState = State.Fall;
        }
    }

    private void KickCommands()
    {
        playerAnimator.Play("Kick");

        CloseState("Kick");
    }

    private void LayCommands()
    {
        playerAnimator.Play("Lay");

        if (IsAnimationFinished("Lay"))
        {
            damageTest = false;
            currentState = State.Idle;
        }
    }

    private void MoveCommands()
    {
        if(isGrounded && jumpInput)
        {
            playerRb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
            isGrounded = false;
            currentState = State.Jump;
        }
        else if (attackInput)
        {
            currentState = State.Attack;
        }
        else if (moveInput == 0f)
        {
            if (isGrounded)
            {
                currentState = State.Idle;
            }
            else currentState = State.Fall;
        }
        else if(isGrounded && runInput)
        {
            playerAnimator.Play("Run");
            MoveX(runSpeed);
        }
        else if (isGrounded)
        {
            playerAnimator.Play("Walk");
            MoveX(walkSpeed);
        }
    }

    private IEnumerator Delay ()
    {
        yield return new WaitForSeconds(delayTime);
        hasDashed = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Mount"))
        {
            isMounted = true;
        }
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            playerAnimator.SetBool("Grounded", isGrounded);
        }

    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Mount"))
        {
            isMounted = false;
        }
    }
}
