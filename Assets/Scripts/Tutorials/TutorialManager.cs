using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Tutorial/Tutorial Manager")]
public class TutorialManager : MonoBehaviour
{
    [System.Serializable]
    public class TutorialStage
    {
        [SerializeField] private string stageId;
        [SerializeField] private TutorialObjective[] objectives;

        private System.Action onStageCompleted;
        private int currentObjectiveIndex;

        public string StageId => stageId;

        public void Configure(TutorialManager owner)
        {
            if (objectives == null) return;
            for (int i = 0; i < objectives.Length; i++)
            {
                if (!objectives[i]) continue;
                objectives[i].Configure(owner);
                objectives[i].ResetObjective();
                objectives[i].EndObjective();
            }
        }

        public void Enter(System.Action onCompleted)
        {
            onStageCompleted = onCompleted;
            currentObjectiveIndex = -1;
            Advance();
        }

        public void Exit()
        {
            if (objectives == null) return;
            for (int i = 0; i < objectives.Length; i++)
            {
                var o = objectives[i];
                if (!o) continue;
                o.Completed -= HandleCompleted;
                o.EndObjective();
            }
        }

        void Advance()
        {
            if (objectives == null || objectives.Length == 0) { onStageCompleted?.Invoke(); return; }

            currentObjectiveIndex++;
            if (currentObjectiveIndex >= objectives.Length) { onStageCompleted?.Invoke(); return; }

            var o = objectives[currentObjectiveIndex];
            if (!o) { Advance(); return; }

            o.Completed -= HandleCompleted;
            o.Completed += HandleCompleted;
            o.ResetObjective();
            o.BeginObjective();
        }

        void HandleCompleted(TutorialObjective o)
        {
            o.Completed -= HandleCompleted;
            o.EndObjective();
            Advance();
        }
    }

    [Header("Flow")]
    [SerializeField] private List<TutorialStage> stages = new();
    [SerializeField] private bool autoStart = true;

    [Header("Balloon (World)")]
    [SerializeField] private TutorialBalloonWorld balloonWorld;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip successClip;
    [SerializeField] private AudioClip wizardLaughClip;
    [SerializeField] private float betweenClipsDelay = 0.1f;

    Coroutine runningRoutine;
    int currentStageIndex = -1;

    void Awake()
    {
        for (int i = 0; i < stages.Count; i++)
            stages[i]?.Configure(this);
    }

    void OnEnable()
    {
        if (autoStart) StartTutorial();
    }

    public void StartTutorial()
    {
        if (runningRoutine != null) StopCoroutine(runningRoutine);
        currentStageIndex = -1;
        runningRoutine = StartCoroutine(RunTutorial());
    }

    public void SkipToStage(string stageId)
    {
        int idx = stages.FindIndex(s => s != null && s.StageId == stageId);
        if (idx < 0) return;
        StopTutorial();
        currentStageIndex = Mathf.Clamp(idx - 1, -1, stages.Count - 1);
        runningRoutine = StartCoroutine(RunTutorial());
    }

    public void StopTutorial()
    {
        if (runningRoutine != null)
        {
            StopCoroutine(runningRoutine);
            runningRoutine = null;
        }

        if (currentStageIndex >= 0 && currentStageIndex < stages.Count)
            stages[currentStageIndex]?.Exit();

        HideBalloon();
        currentStageIndex = -1;
    }

    IEnumerator RunTutorial()
    {
        currentStageIndex = Mathf.Clamp(currentStageIndex, -1, stages.Count - 1);

        while (true)
        {
            currentStageIndex++;
            if (currentStageIndex >= stages.Count) { runningRoutine = null; yield break; }

            var stage = stages[currentStageIndex];
            if (stage == null) continue;

            bool done = false;
            stage.Enter(() => done = true);
            while (!done) yield return null;

            stage.Exit();

            bool hasMore = currentStageIndex < stages.Count - 1;
            if (hasMore) yield return PlayTransitionSequence();
        }
    }

    IEnumerator PlayTransitionSequence()
    {
        if (audioSource && successClip)
        {
            audioSource.PlayOneShot(successClip);
            yield return new WaitForSeconds(successClip.length);
        }

        if (betweenClipsDelay > 0f) yield return new WaitForSeconds(betweenClipsDelay);

        if (audioSource && wizardLaughClip)
        {
            audioSource.PlayOneShot(wizardLaughClip);
            yield return new WaitForSeconds(wizardLaughClip.length);
        }
    }

    public void ShowBalloon(Sprite s)
    {
        if (balloonWorld && s) balloonWorld.Show(s);
    }

    public void HideBalloon()
    {
        if (balloonWorld) balloonWorld.Hide();
    }
}
