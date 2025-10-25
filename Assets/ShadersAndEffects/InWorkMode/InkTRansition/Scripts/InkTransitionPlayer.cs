using UnityEngine;
using System.Collections;

public class InkTransitionPlayer : MonoBehaviour
{
    public Animator anim;
    public string clipName = "InkReveal";

    public Animator playerAnim;
    public string playerIntroClip = "Getting_up";

    public MonoBehaviour playerController;

    public bool autoRunOnStart = true;

    Coroutine routine;

    void Awake()
    {
        if (playerController != null) playerController.enabled = false;

        if (playerAnim != null && !string.IsNullOrEmpty(playerIntroClip))
        {
            playerAnim.Play(playerIntroClip, 0, 0f);
            playerAnim.Update(0f);
            playerAnim.speed = 0f;
        }

        gameObject.SetActive(true);
    }

    void Start()
    {
        if (autoRunOnStart)
        {
            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(RunSequence());
        }
    }

    public void PlayReveal()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(RunSequence());
    }

    IEnumerator RunSequence()
    {
        anim.Play(clipName, 0, 0f);
        yield return null;
        float inkLen = anim.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(inkLen);

        if (playerAnim != null)
        {
            playerAnim.speed = 1f;
            playerAnim.CrossFadeInFixedTime(playerIntroClip, 0f, 0, 0f);
        }

        yield return null;
        float introLen = playerAnim.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(introLen);

        if (playerController != null) playerController.enabled = true;

        gameObject.SetActive(false);

        routine = null;
    }
}
