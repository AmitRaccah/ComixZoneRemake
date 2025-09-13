using UnityEngine;
using System.Collections.Generic;

[DefaultExecutionOrder(-100)]
public class InputBuffer : MonoBehaviour
{
    public static InputBuffer Instance;

    private readonly List<FrameInput> _buffer = new List<FrameInput>(16);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public List<FrameInput> GetBuffer()
    {
        return _buffer;
    }

    public void Add(InputType t)
    {
        _buffer.Add(new FrameInput { inputType = t });
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureExists()
    {
        if (Instance == null)
        {
            var go = new GameObject("InputBuffer");
            go.AddComponent<InputBuffer>();
        }
    }
}
