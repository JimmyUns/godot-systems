using Godot;
using System.Collections.Generic;
using System.Diagnostics;

[GlobalClass]
public partial class Quest : Resource
{
    [Export] public string QuestId;
    [Export] public string QuestName;
    [Export] public QuestStep[] Steps;
    [Export] public int CurrentStepIndex = 0;

    public void ProcessEvent(string eventName, object data)
    {
        if (CurrentStepIndex >= Steps.Length) return;


        var currentStep = Steps[CurrentStepIndex];
        currentStep.ProcessEvent(eventName, data);

        if (currentStep.IsComplete)
            CurrentStepIndex++;
    }

    public QuestStep GetQuestStep()
    {
        return Steps[CurrentStepIndex];
    }

    public bool IsComplete => CurrentStepIndex >= Steps.Length;
}
