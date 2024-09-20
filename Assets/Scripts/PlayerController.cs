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
    private float moveInput = 0f;
    private bool isGrounded = false;
    private bool isMounted = false;
    private bool damageTest = false;
    private bool upInput = false;
    private bool downInput = false;
    private bool hasDashed = false;

    enum State { Idle, Arrow, Dance, Dash, Attack, Fall, Fireball, Jump, Kick, Lay, Move }

    State animation = State.Idle;

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
        dashInput = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        upInput = Input.GetKey(KeyCode.W);
        downInput = Input.GetKey(KeyCode.S);
        moveInput = Input.GetAxisRaw("Horizontal");
        damageTest = Input.GetKey(KeyCode.Backspace);

        //{ Idle, Arrow, Dance, Dash, Attack, Fall, Fireball, Jump, Kick, Lay, Move }

        switch (animation)
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

        if (damageTest) animation = State.Lay;

    }

    private void FlipX()
    {
        if (moveInput < 0f)
        {
            playerSprite.flipX = false;
        }
        else if (moveInput > 0f)
        {
            playerSprite.flipX = true;
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

    private bool IsInAttackState()
    {
        return playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("NormalAttack") ||
        playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("UpAttack") ||
        playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("DownAttack") ||
        playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("HorseAttack") ||
        playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("JumpAttack");
    }

    private void IdleCommands()
    {
        if (arrowInput)
        {
            animation = State.Arrow;
        }
        else if (fireballInput)
        {
            animation = State.Fireball;
        }
        else if (attackInput)
        {
            animation = State.Attack;
        }
        else if (moveInput != 0f)
        {
            animation = State.Move;
        }
        else if (kickInput)
        {
            animation = State.Kick;
        }
        else if (isGrounded)
        {
            if(jumpInput)
            {
                playerRb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
                animation = State.Jump;
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
    }

    private void ArrowCommands()
    {
        playerAnimator.Play("Arrow");

        if(IsAnimationFinished("Arrow"))
        {
            if (isGrounded)
            {
                animation = State.Idle;
            }
            else
            {
                animation = State.Fall;
            }
        }
    }

    private void DanceCommands()
    {
        playerAnimator.Play("Dance");

        if(IsAnimationFinished("Dance"))
        {
            if (isGrounded)
            {
                animation = State.Idle;
            }
            else
            {
                animation = State.Fall;
            }
        }
    }

    private void DashCommands()
    {
        if(!hasDashed)
        {
            hasDashed = true;
            playerRb.velocity = new Vector2(moveInput * runSpeed, playerRb.velocity.y);
            playerAnimator.Play("Dash");

            StartCoroutine(Delay());
        }
    }

    private void AttackCommands()
    {
        if (IsInAttackState())
        {
            return; 
        }
        
        FlipX();

        if (isGrounded && upInput)
        {
            playerAnimator.Play("UpAttack");
        }
        else if (isGrounded && downInput)
        {
            playerAnimator.Play("DownAttack");
        }
        else if (isGrounded)
        {
            playerAnimator.Play("NormalAttack");
        }
        else if (isMounted)
        {
            playerAnimator.Play("HorseAttack");
        }
        else if (!isGrounded)
        {
            playerAnimator.Play("JumpAttack");

            if (IsAnimationFinished("JumpAttack"))
            {
                animation = State.Fall;
            }
        }

        if (damageTest)
        {
            animation = State.Lay;
        }
        else if (IsAnimationFinished("UpAttack") || IsAnimationFinished("DownAttack") || 
        IsAnimationFinished("NormalAttack") || IsAnimationFinished("HorseAttack"))
        {
            animation = State.Idle;
        }
    }

    private void FallCommands()
    {
        playerAnimator.Play("Fall");

        MoveX(walkSpeed);

        if(attackInput)
        {
            animation = State.Attack;
        }
        else if(isGrounded)
        {
            playerAnimator.Play("Land");  

            if(IsAnimationFinished("Land"))
            {
                animation = State.Idle;
            }
        }

    }

    private void FireballCommands()
    {
        playerAnimator.Play("Fireball");

        if(IsAnimationFinished("Fireball"))
        {
            if (isGrounded)
            {
                animation = State.Idle;
            }
            else
            {
                animation = State.Fall;
            }
        }
    }

    private void JumpCommands()
    {
        playerAnimator.Play("Jump");

        MoveX(walkSpeed);

        if (attackInput)
        {
            animation = State.Attack;
        }
        else if (playerRb.velocity.y < 0)
        {
            animation = State.Fall;
        }
    }

    private void KickCommands()
    {
        playerAnimator.Play("Kick");

        if(IsAnimationFinished("Kick"))
        {
            if (isGrounded)
            {
                animation = State.Idle;
            }
            else
            {
                animation = State.Fall;
            }
        }
    }

    private void LayCommands()
    {
        playerAnimator.Play("Lay");

        if (IsAnimationFinished("Lay"))
        {
            damageTest = false;
            animation = State.Idle;
        }
    }

    private void MoveCommands()
    {
        if(isGrounded && dashInput)
        {
            playerAnimator.Play("Run");
            MoveX(runSpeed);
        }
        else if (!isGrounded && dashInput)
        {
            animation = State.Dash;
        }
        else if (moveInput == 0f && isMounted)
        {
            animation = State.Idle;
        }
        else if (moveInput == 0f && isGrounded)
        {
            animation = State.Idle;
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

        if (isMounted)
        {
            animation = State.Idle;
        }
        else animation = State.Fall;
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
        }

    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Mount"))
        {
            isMounted = false;
        }
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}
