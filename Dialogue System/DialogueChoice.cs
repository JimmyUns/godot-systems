using System;

public class DialogueChoice
{
    public string Text;
    public Action Callback;

    public DialogueChoice(string text, Action callback)
    {
        Text = text;
        Callback = callback;
    }
}