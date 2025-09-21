public class Word
{
    public string Text { get; }
    private bool _hidden;

    public Word(string text)
    {
        Text = text;
        _hidden = false;
    }

    public void Hide() => _hidden = true;
    public bool IsHidden() => _hidden;

    public string GetDisplayText()
    {
        return _hidden ? new string('_', Text.Length) : Text;
    }
}

