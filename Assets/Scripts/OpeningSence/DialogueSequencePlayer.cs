using UnityEngine;
using System.Collections;

[System.Serializable]
public class DialogueStep
{
    public GameObject bubble;
    public GameObject textObj;
    public float duration = 2f;
}

public class DialogueSequencePlayer : MonoBehaviour
{
    public DialogueStep[] steps;

    public IEnumerator PlaySequence()
    {
        HideAll();

        for (int i = 0; i < steps.Length; i++)
        {
            DialogueStep s = steps[i];
            if (s == null) continue;

            HideAll();

            if (s.bubble != null) s.bubble.SetActive(true);
            if (s.textObj != null) s.textObj.SetActive(true);

            float waitT = s.duration;
            if (waitT > 0f)
                yield return new WaitForSeconds(waitT);
            else
                yield return null;
        }

        HideAll();
    }

    void HideAll()
    {
        for (int i = 0; i < steps.Length; i++)
        {
            DialogueStep s = steps[i];
            if (s == null) continue;

            if (s.bubble != null) s.bubble.SetActive(false);
            if (s.textObj != null) s.textObj.SetActive(false);
        }
    }
}
