using Godot;
using System;
using System.Collections.Generic;

public partial class DialogueSystem : Control
{
    public static DialogueSystem Instance;

    private Queue<(List<DialogueMessage> batch, List<DialogueChoice> choices, Action callback)> messageBatches = new();
    private Queue<DialogueMessage> currentQueue = new();

    [Export] private RichTextLabel speakerLabel;
    [Export] private RichTextLabel dialogueLabel;
    [Export] private Button nextButton;

    [Export] private VBoxContainer choicesContainer;

    private bool isDisplaying = false;
    private Action currentCallback = null;
    private List<DialogueChoice> currentChoices = null;

    public override void _Ready()
    {
        Instance = this;
        nextButton.Pressed += OnNextButton;
        choicesContainer.Visible = false;
        Hide();
    }

    public void QueueMessages(List<DialogueMessage> batch, List<DialogueChoice> choices = null, Action OnFinished = null)
    {
        messageBatches.Enqueue((batch, choices, OnFinished));

        if (!isDisplaying)
            ShowNextBatch();
    }

    private void ShowNextBatch()
    {
        if (messageBatches.Count == 0)
        {
            isDisplaying = false;
            Hide();
            return;
        }

        var (batch, choices, callback) = messageBatches.Dequeue();
        currentQueue = new Queue<DialogueMessage>(batch);
        currentCallback = callback;
        currentChoices = choices;
        isDisplaying = true;

        ShowNextMessage();
    }

    private void ShowNextMessage()
    {
        if (currentQueue.Count == 0)
        {
            // All messages shown
            if (currentChoices != null && currentChoices.Count > 0)
            {
                ShowChoices(currentChoices);
                currentChoices = null;
                return;
            }

            currentCallback?.Invoke();
            currentCallback = null;

            ShowNextBatch();
            return;
        }

        var msg = currentQueue.Dequeue();
        speakerLabel.Text = msg.Speaker;
        dialogueLabel.Text = msg.Text;
        Show();
    }

    private void OnNextButton()
    {
        ShowNextMessage();
    }

    private void ShowChoices(List<DialogueChoice> choices)
    {
        choicesContainer.Visible = true;
        if (choicesContainer.GetChildren().Count > 0)
        {
            foreach (var child in choicesContainer.GetChildren())
            {
                child.QueueFree();
            }
        }

        foreach (var choice in choices)
        {
            var btn = new Button();
            btn.Text = choice.Text;
            btn.Pressed += () =>
            {
                choice.Callback?.Invoke();
                choicesContainer.Visible = false;

                ShowNextBatch();
            };
            choicesContainer.AddChild(btn);
        }
    }
}
