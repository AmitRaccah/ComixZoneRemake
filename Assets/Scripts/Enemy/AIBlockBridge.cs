using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BlockController))]
public class AIBlockBridge : MonoBehaviour
{
    public string requestBool = "AI_RequestBlock";

    Animator anim;
    BlockController bc;
    int hash;

    void Awake()
    {
        anim = GetComponent<Animator>();
        bc = GetComponent<BlockController>();
        hash = Animator.StringToHash(requestBool);
    }

    void Update()
    {
        bool wantBlock = anim.GetBool(hash);
        bc.SetBlockInput(wantBlock);
    }
}
