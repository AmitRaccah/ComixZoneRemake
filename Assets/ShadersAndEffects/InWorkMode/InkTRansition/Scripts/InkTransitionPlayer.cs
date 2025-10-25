using UnityEngine;
using System.Collections;

public class InkIntroController : MonoBehaviour
{
    public GameObject inkRoot;
    public Animator inkAnimator;
    public string inkClipName = "InkReveal";

    public Animator playerAnimator;
    public string getUpStateName = "Getting_up";
    public string idleStateName = "Idle Walk Run Blend";

    public IEnumerator PlayIntro(PlayerCutsceneLock lockLayer)
    {
        if (lockLayer != null) lockLayer.FullLockBeforeIntro();

        if (playerAnimator != null && !string.IsNullOrEmpty(getUpStateName))
        {
            playerAnimator.Play(getUpStateName, 0, 0f);
            playerAnimator.Update(0f);
            playerAnimator.speed = 0f;
        }

        if (inkRoot != null) inkRoot.SetActive(true);

        if (inkAnimator != null && !string.IsNullOrEmpty(inkClipName))
        {
            inkAnimator.Play(inkClipName, 0, 0f);
        }

        yield return null;

        float inkLen = 0f;
        if (inkAnimator != null)
        {
            inkLen = inkAnimator.GetCurrentAnimatorStateInfo(0).length;
        }

        if (inkLen > 0f)
            yield return new WaitForSeconds(inkLen);

        if (playerAnimator != null)
        {
            playerAnimator.speed = 1f;
        }

        yield return null;

        float getUpLen = 0f;
        if (playerAnimator != null)
        {
            getUpLen = playerAnimator.GetCurrentAnimatorStateInfo(0).length;
        }

        if (getUpLen > 0f)
            yield return new WaitForSeconds(getUpLen);

        if (inkRoot != null) inkRoot.SetActive(false);

        if (playerAnimator != null && !string.IsNullOrEmpty(idleStateName))
        {
            playerAnimator.CrossFadeInFixedTime(idleStateName, 0f, 0, 0f);
        }

        if (lockLayer != null) lockLayer.IdleLockAfterIntro();
    }
}
