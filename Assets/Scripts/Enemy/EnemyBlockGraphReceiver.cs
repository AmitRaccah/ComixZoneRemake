using UnityEngine;

[RequireComponent(typeof(EnemyBlockDriver))]
[RequireComponent(typeof(Animator))]
public class EnemyBlockGraphReceiver : MonoBehaviour
{
    [SerializeField] private string graphParam = "AI_RequestBlock";
    private EnemyBlockDriver driver;
    private Animator anim;
    private bool last;

    void Awake()
    {
        driver = GetComponent<EnemyBlockDriver>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        bool want = anim.GetBool(graphParam);
        if (want != last)
        {
            if (want) driver.BeginBlock();
            else driver.EndBlock();
            last = want;
        }
    }
}
