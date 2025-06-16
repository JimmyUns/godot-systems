# Dialogue System (C#)

Currently in early stages, works using the default godot UI.
Will be adding a separate UI script to control the printing of the dialogue and storing them in the near future.

Note:
The button is created using `new button()`, replace this with an exported packed scene and instantiate it for a custom style.

## Installation

1. Place the folder in your Godot FileSystem.
2. Create a Control Node in your main game scene and attach the `DialogueSystem` script to it. Optionally rename the node to "Dialogue System".
3. Create the UI and attach the nodes to the `DialogueSystem`'s exported slots.

## Usage Functions

**Queue Messages**  
Adds the message (dialogue) into the queue.  
`QueueMessages(List<DialogueMessage> batch, List<DialogueChoice> choices = null, Action OnFinished = null)`

- `batch` sets the message the dialogue system will be reading.
- `choices` before OnFinished is called, used to give choices before the message is completed.
- `OnFinished` sets the action called when the message is over.

## Example Usage

**Talk Button Pressed**

```csharp
DialogueSystem.Instance.QueueMessages(
    batch: new List<DialogueMessage>
    {
        new DialogueMessage("Npc Name", "Line 1"),
        new DialogueMessage("Npc Name", "Line 2")
    },

    OnFinished: () =>
    {
        this.Show();
    }
);
```

**Quest Dialogue Button Pressed**

```csharp
this.Hide();

            DialogueSystem.Instance.QueueMessages(
                batch: new List<DialogueMessage>
                {
                    new DialogueMessage("Misu", "See those ugly bandits there? I really hate them"),
                    new DialogueMessage("Misu", "Can you [b]kill one[/b] of them for me?")
                },

                choices: new List<DialogueChoice>
                {
                    new DialogueChoice("Accept", () =>
                    {
                        DialogueSystem.Instance.QueueMessages(
                            batch: new List<DialogueMessage>
                            {
                                new DialogueMessage("Misu", "Great! The payment will be transfered into your account automatically when completed.")
                            },

                            OnFinished: () =>
                            {
                                Quest newQuest = (Quest)questIntroduction.Duplicate(true);
                                QuestManager.Instance.AddQuest(newQuest);

                                this.Show();
                            }
                        );
                    }),
                    new DialogueChoice("Refuse", () =>
                    {
                        DialogueSystem.Instance.QueueMessages(
                            batch: new List<DialogueMessage>
                            {
                                new DialogueMessage("Misu", "Bummer...")
                            },

                            OnFinished: () =>
                            {
                                this.Show();
                            }
                        );
                    }),
                },

                OnFinished: () =>
                {
                    Quest newQuest = (Quest)questIntroduction.Duplicate(true);
                    QuestManager.Instance.AddQuest(newQuest);

                    this.Show();
                }
            );
```
