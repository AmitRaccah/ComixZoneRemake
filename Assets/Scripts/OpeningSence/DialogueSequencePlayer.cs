using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class DialogueStep
{
    public GameObject bubble;
    [TextArea] public string text;
    public float duration = 2f;

    public TMP_Text tmpText;
    public Text uiText;
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

            if (s.bubble != null)
            {
                if (s.tmpText != null)
                {
                    s.tmpText.text = s.text;
                }
                else if (s.uiText != null)
                {
                    s.uiText.text = s.text;
                }

                s.bubble.SetActive(true);
            }

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
            if (steps[i] != null && steps[i].bubble != null)
                steps[i].bubble.SetActive(false);
        }
    }
}
