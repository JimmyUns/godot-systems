using Godot;
using System;

[GlobalClass]
public partial class DebuggerQS : QuestStep
{
    [Export] public string debugMessage = "Debug step completed";

    public override void ProcessEvent(string eventName, object data)
    {
        GD.Print(debugMessage);
        IsComplete = true;
    }
}
