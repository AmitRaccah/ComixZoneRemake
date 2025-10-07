using UnityEngine;

public class InkTransitionPlayer : MonoBehaviour
{
    public Animator anim;
    public string clipName = "InkReveal";

    public void PlayReveal()
    {
        gameObject.SetActive(true);
        anim.Play(clipName, -1, 0f);
        CancelInvoke();
        Invoke(nameof(Hide), anim.GetCurrentAnimatorStateInfo(0).length);
    }

    void Hide() => gameObject.SetActive(false);
}