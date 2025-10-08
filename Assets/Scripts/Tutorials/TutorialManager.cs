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

        private TutorialManager owner;
        private int currentObjectiveIndex;
        private System.Action onStageCompleted;

        public string StageId => stageId;

        public void Configure(TutorialManager tutorialManager)
        {
            owner = tutorialManager;
            if (objectives == null) return;
            for (int i = 0; i < objectives.Length; i++)
            {
                if (objectives[i] == null) continue;
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
                var objective = objectives[i];
                if (objective == null) continue;
                objective.Completed -= HandleObjectiveCompleted;
                objective.EndObjective();
            }
        }

        private void Advance()
        {
            if (objectives == null || objectives.Length == 0)
            {
                onStageCompleted?.Invoke();
                return;
            }

            currentObjectiveIndex++;
            if (currentObjectiveIndex >= objectives.Length)
            {
                onStageCompleted?.Invoke();
                return;
            }

            var objective = objectives[currentObjectiveIndex];
            if (objective == null)
            {
                Advance();
                return;
            }

            objective.Completed -= HandleObjectiveCompleted;
            objective.Completed += HandleObjectiveCompleted;
            objective.ResetObjective();
            objective.BeginObjective();
        }

        private void HandleObjectiveCompleted(TutorialObjective objective)
        {
            objective.Completed -= HandleObjectiveCompleted;
            objective.EndObjective();
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

    private Coroutine runningRoutine;
    private int currentStageIndex = -1;

    void Awake()
    {
        for (int i = 0; i < stages.Count; i++)
        {
            var stage = stages[i];
            if (stage == null) continue;
            stage.Configure(this);
        }
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
        int targetIndex = stages.FindIndex(s => s != null && s.StageId == stageId);
        if (targetIndex < 0) return;

        StopTutorial();
        currentStageIndex = Mathf.Clamp(targetIndex - 1, -1, stages.Count - 1);
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

        currentStageIndex = -1;
        HideBalloon();
    }

    private IEnumerator RunTutorial()
    {
        currentStageIndex = Mathf.Clamp(currentStageIndex, -1, stages.Count - 1);

        while (true)
        {
            currentStageIndex++;
            if (currentStageIndex >= stages.Count)
            {
                runningRoutine = null;
                yield break;
            }

            var stage = stages[currentStageIndex];
            if (stage == null) continue;

            bool stageCompleted = false;
            stage.Enter(() => stageCompleted = true);

            while (!stageCompleted)
                yield return null;

            stage.Exit();

            // צלילי מעבר בין סטייג'ים
            yield return PlayTransitionAudio();

            HideBalloon();
        }
    }

    private IEnumerator PlayTransitionAudio()
    {
        if (successClip && audioSource)
        {
            audioSource.PlayOneShot(successClip);
            yield return new WaitForSeconds(successClip.length);
        }

        if (betweenClipsDelay > 0f)
            yield return new WaitForSeconds(betweenClipsDelay);

        if (wizardLaughClip && audioSource)
        {
            audioSource.PlayOneShot(wizardLaughClip);
            yield return new WaitForSeconds(wizardLaughClip.length);
        }
    }

    public void ShowBalloon(Sprite s)
    {
        if (balloonWorld) balloonWorld.Show(s);
    }

    public void HideBalloon()
    {
        if (balloonWorld) balloonWorld.Hide();
    }
}
