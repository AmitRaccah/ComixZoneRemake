public struct FrameInput
{
    public InputType inputType;
    public int frame;

    public FrameInput(InputType type, int frame)
    {
        inputType = type;
        this.frame = frame;
    }
}
