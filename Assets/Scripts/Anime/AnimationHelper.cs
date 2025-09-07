using UnityEngine;

public class AnimationHelper : MonoBehaviour
{
    public static AnimationHelper Instance { get; private set; }

    private AnimationDriver animationDriver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
            animationDriver = playerGO.GetComponentInChildren<AnimationDriver>();
    }

    public void Trigger(string triggerName)
    {
        animationDriver?.Trigger(triggerName);
    }

    public void SetBool(string param, bool value)
    {
        animationDriver?.SetBool(param, value);
    }
}
