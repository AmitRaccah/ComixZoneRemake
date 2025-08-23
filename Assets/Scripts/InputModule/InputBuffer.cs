using System.Collections.Generic;
using UnityEngine;

public class InputBuffer : MonoBehaviour
{
    private int bufferSize = 12;

    private bool log = false;

    public static InputBuffer Instance { get; private set; }

    private readonly List<FrameInput> buffer = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[InputBuffer] Duplicate found, disabling this one.", this);
            enabled = false;
            return;
        }
        Instance = this;
    }

    private void OnDisable()
    {
        if (Instance == this) Instance = null;
    }

    public void Add(InputType input)
    {
        if (bufferSize == 0) return;

        int frame = Time.frameCount;
        if (buffer.Count > 0)
        {
            FrameInput last = buffer[buffer.Count - 1];
            if (last.inputType == input && last.frame == frame)
                return; 
        }

        buffer.Add(new FrameInput(input, frame));
    }

    public List<FrameInput> GetBuffer() => buffer;

    public void Clear() => buffer.Clear();

    private void Update()
    {
        if (bufferSize == 0)
        {
            if (buffer.Count > 0) buffer.Clear();
            return;
        }

        int now = Time.frameCount;
        for (int i = buffer.Count - 1; i >= 0; i--)
        {
            int age = now - buffer[i].frame;
            if (age >= bufferSize)
                buffer.RemoveAt(i);
        }

        if (log && (now % 30 == 0)) // 0.5 sec
            Debug.Log($"[InputBuffer] count={buffer.Count}");
    }
}
