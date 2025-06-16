using System.Collections.Generic;
using Godot;

public partial class QuestManager : Node
{
    private List<Quest> activeQuests = new List<Quest>();

    public static QuestManager Instance;

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("Debug"))
        {
            PlayerCore.Instance.cc.SetState(CharacterStates.Interaction);
            DialogueSystem.Instance.QueueMessages(
               batch: new List<DialogueMessage>
               {
                    new DialogueMessage("Debug", "If youre reading this HOW"),
                    new DialogueMessage("Debug", "Contact me and please report this as a bug ty :3"),
                    new DialogueMessage("Debug", "OR ELSE"),
               },

               OnFinished: () =>
               {
                   PlayerCore.Instance.cc.SetState(CharacterStates.Locomotion);
               }
           );
        }
    }


    public void AddQuest(Quest quest)
    {
        if (activeQuests.Exists(q => q.QuestId == quest.QuestId))
        {
            GD.Print($"QUEST: Quest {quest.QuestId} already exists in active quests, trying to add it again failed.");
            return;
        }
        activeQuests.Add(quest);
        GD.Print($"QUEST: Quest {quest.QuestId} added.");
    }

    public void NotifyEvent(string eventName, object data)
    {
        foreach (var quest in activeQuests)
        {
            quest.ProcessEvent(eventName, data);
            if (quest.IsComplete)
            {
                GD.Print($"QUEST: Quest {quest.QuestId} completed!");
                activeQuests.Remove(quest);
                if (activeQuests.Count <= 0) break;
            }
        }
    }

    public List<Quest> GetQuests()
    {
        return activeQuests;
    }
}
