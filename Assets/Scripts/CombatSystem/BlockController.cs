using UnityEngine;

[RequireComponent(typeof(Animator))]
public class BlockController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float blockStartup = 0.10f;   
    [SerializeField] private float blockRecovery = 0.15f;  

    private Animator anim;
    private MovementLock mLock;
    private float stateTimer;

    private enum BState { Idle, Starting, Holding, Recovery }
    private BState curState = BState.Idle;

    public bool IsBlocking => curState == BState.Holding;
    public bool IsInRecovery => curState == BState.Recovery;


    private void Awake()
    {
        anim = GetComponent<Animator>();
        mLock = GetComponent<MovementLock>();     
    }

    private void Update()
    {
        stateTimer -= Time.deltaTime;

        switch (curState)
        {
            case BState.Starting:
                if (stateTimer <= 0f)
                    EnterHolding();
                break;

            case BState.Recovery:
                if (stateTimer <= 0f)
                    curState = BState.Idle;
                break;
        }
    }

    public void SetBlockInput(bool pressed)
    {
        if (pressed && IsInHitAnimation()) return;

        if (pressed)
        {
            if (curState == BState.Idle)
                EnterStartup();
        }
        else
        {
            if (curState == BState.Holding)
                EnterRecovery();
        }
    }


    private bool IsInHitAnimation()
    {
        AnimatorStateInfo cur = anim.GetCurrentAnimatorStateInfo(0);
        if (cur.IsTag("Hit")) return true;

        if (anim.IsInTransition(0))
        {
            AnimatorStateInfo next = anim.GetNextAnimatorStateInfo(0);
            if (next.IsTag("Hit")) return true;
        }
        return false;
    }

    private void EnterStartup()
    {
        curState = BState.Starting;
        stateTimer = blockStartup;
        anim.SetBool("IsBlocking", true);

        if (mLock != null) mLock.SetExternalLock(true);
    }

    private void EnterHolding()
    {
        curState = BState.Holding;
    }

    private void EnterRecovery()
    {
        curState = BState.Recovery;
        stateTimer = blockRecovery;
        anim.SetBool("IsBlocking", false);

        if (mLock != null) mLock.SetExternalLock(false);
    }
}
