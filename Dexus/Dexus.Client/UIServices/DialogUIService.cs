namespace Dexus.Client.UIServices;

using MudBlazor;
using Flairs.DialogBoxes;

/// <summary>
/// Provides a service for displaying Dialogs to the UI.
/// </summary>
public class DialogUIService
{
    // Contains all the queued dialogs.
    private readonly Queue<string> titles;
    private readonly Queue<string> messages;
    private readonly Queue<DialogType> type;
    private readonly Queue<Action> action;

    /// <summary>
    /// Initializes a new instance of the <see cref="DialogUIService"/> class.
    /// </summary>
    public DialogUIService()
    {
        this.titles = new Queue<string>();
        this.messages = new Queue<string>();
        this.type = new Queue<DialogType>();
        this.action = new Queue<Action>();
    }

    /// <summary>
    /// Dequeues the next Dialog information.
    /// </summary>
    /// <param name="title">Title of the Dialog.</param>
    /// <param name="message">Message for the Dialog.</param>
    /// <param name="action">Config for the Dialog.</param>
    /// <param name="type">Type of the Dialog to display.</param>
    /// <returns>True if there is a Dialog to display, False otherwise.</returns>
    public bool NextDialog(out string title, out string message, out Action action, out int type)
    {
        if (this.type.TryDequeue(out DialogType dialogType))
        {
            int nextType = (int)dialogType;
            switch (nextType)
            {
                case 0:
                    title = this.titles.Dequeue();
                    message = this.messages.Dequeue();
                    action = () => { };
                    type = nextType;
                    return true;
                case 1:
                    title = this.titles.Dequeue();
                    message = this.messages.Dequeue();
                    action = this.action.Dequeue();
                    type = nextType;
                    return true;
            }
        }

        title = string.Empty;
        message = string.Empty;
        action = () => { };
        type = -1;
        return false;
    }

    /// <summary>
    /// Adds a new Dialog to the queue.
    /// </summary>
    /// <param name="title">Title for the Dialog.</param>
    /// <param name="message">Message for the Dialog to display.</param>
    /// <param name="action">Action for the Dialog to invoke.</param>
    public void SendDialog(string title, string message, Action action)
    {
        this.titles.Enqueue(title);
        this.messages.Enqueue(message);
        this.action.Enqueue(action);
        this.type.Enqueue(DialogType.Confirmation);
    }

    /// <summary>
    /// Adds a new Dialog to the queue.
    /// </summary>
    /// <param name="title">Title for the Dialog.</param>
    /// <param name="message">Message for the Dialog to display.</param>
    /// <param name="type">Type of the Dialog to display.</param>
    public void SendDialog(string title, string message, int type)
    {
        this.titles.Enqueue(title);
        this.messages.Enqueue(message);
        this.type.Enqueue((DialogType)type);
    }

    /// <summary>
    /// Adds a new Dialog to the queue.
    /// </summary>
    /// <param name="title">Title for the Dialog.</param>
    /// <param name="message">Message for the Dialog to display.</param>
    public void SendDialog(string title, string message)
    {
        this.titles.Enqueue(title);
        this.messages.Enqueue(message);
        this.type.Enqueue(DialogType.Message);
    }
    
    /// <summary>
    /// Defines the type of Dialog to display.
    /// </summary>
    enum DialogType
    {
        Message,        // 0
        Confirmation,   // 1
    }
}