using Godot;
using System;

[GlobalClass]
public partial class KillEnemyQS : QuestStep
{
    [Export]
    public string EnemyId;

    [Export]
    public int RequiredKills = 1;

    private int killCount = 0;

    public override void ProcessEvent(string eventName, object data)
    {
        if (eventName == "EnemyKilled" && data is string killedId && killedId == EnemyId)
        {
            killCount++;
            if (killCount >= RequiredKills)
            {
                IsComplete = true;
                GD.Print("QUEST: killed enemies");
            }
        }
    }
}
