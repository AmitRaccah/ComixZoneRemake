using UnityEngine;
using System.Collections.Generic;


public class InputBuffer : MonoBehaviour
{
    [SerializeField] private int bufferSize = 12;

    public static InputBuffer Instance;

    private List<FrameInput> buffer = new List<FrameInput>();

    private int currentFrame;

    private void Awake()
    {
   //     Instance = this;
    }

    private void OnEnable()
    {
        Instance = this;
    }

    private void OnDisable()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    public void Add(InputType input)
    {
        if (bufferSize == 0)                    
        {
            return;                            
        }

        if (buffer.Count > 0)
        {
            FrameInput last = buffer[buffer.Count - 1];
            if (last.inputType == input && last.frame == currentFrame)
            {
                return;                         
            }
        }

        buffer.Add(new FrameInput(input, currentFrame));
    }


    public List<FrameInput> GetBuffer()
    {
        return buffer;
    }


    void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        currentFrame++;
        Debug.Log($"BUF count={buffer.Count}");


        if (bufferSize == 0)
        {
            buffer.Clear();
            return;
        }

        for (int i = buffer.Count - 1; i >= 0; i--)
        {
            int age = currentFrame - buffer[i].frame;
            if (age >= bufferSize)        
            {
                buffer.RemoveAt(i);
            }
        }
    }
}
