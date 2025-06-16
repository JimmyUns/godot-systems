using Godot;

[GlobalClass]
public partial class QuestStep : Resource
{
    [Export]
    public string QuestStepName;

    [Export]
    public bool IsComplete = false;

    // You can add virtual or abstract methods for logic here
    public virtual void ProcessEvent(string eventName, object data)
    {
        // Base implementation or empty
    }

}
