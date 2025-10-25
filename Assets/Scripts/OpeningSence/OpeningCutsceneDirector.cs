using UnityEngine;
using System.Collections;

public class OpeningCutsceneDirector : MonoBehaviour
{
    public InkIntroController intro;
    public DialogueSequencePlayer dialogue;
    public PlayerCutsceneLock lockLayer;

    public bool autoRunOnStart = true;

    Coroutine runRoutine;

    void Start()
    {
        if (autoRunOnStart)
        {
            if (runRoutine != null) StopCoroutine(runRoutine);
            runRoutine = StartCoroutine(RunCutscene());
        }
    }

    public void PlayCutscene()
    {
        if (runRoutine != null) StopCoroutine(runRoutine);
        runRoutine = StartCoroutine(RunCutscene());
    }

    IEnumerator RunCutscene()
    {
        if (intro != null)
            yield return StartCoroutine(intro.PlayIntro(lockLayer));

        if (dialogue != null)
            yield return StartCoroutine(dialogue.PlaySequence());

        if (lockLayer != null)
            lockLayer.FinalUnlock();

        runRoutine = null;
    }
}
