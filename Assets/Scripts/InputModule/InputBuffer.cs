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
        Instance = this;
    }

    public void Add(InputType input)
    {
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
    void Update()
    {
        currentFrame++;

        buffer.RemoveAll(i => currentFrame - i.frame > bufferSize);


    }
}
