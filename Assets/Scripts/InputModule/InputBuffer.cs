using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class InputBuffer : MonoBehaviour
{
    public static InputBuffer Instance;

    [SerializeField] private int bufferSize = 12;
    [SerializeField] private bool log = false;

    private readonly List<FrameInput> buffer = new List<FrameInput>(16);

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
            {
                return;
            }
        }

        buffer.Add(new FrameInput(input, frame));
    }

    public List<FrameInput> GetBuffer()
    {
        return buffer;
    }

    public void Clear()
    {
        buffer.Clear();
    }

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
            {
                buffer.RemoveAt(i);
            }
        }


    }
}
