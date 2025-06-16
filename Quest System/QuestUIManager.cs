using Godot;
using System;

public partial class QuestUIManager : MarginContainer
{
    [Export] private Control questVboxContainer;
    public void ReloadQuests()
    {
        if (questVboxContainer.GetChildren().Count > 0)
        {
            foreach (Control child in questVboxContainer.GetChildren())
            {
                child.QueueFree();
            }
        }

        foreach (Quest quest in QuestManager.Instance.GetQuests())
        {
            RichTextLabel questLabel = new();
            questLabel.FitContent = true;
            questLabel.BbcodeEnabled = true;
            questLabel.Text = "-[b]" + quest.QuestName + "[/b]\n+" + quest.GetQuestStep().QuestStepName;
            questVboxContainer.AddChild(questLabel);
        }

    }
}
