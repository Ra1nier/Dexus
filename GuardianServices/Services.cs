// <copyright file="Services.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace GuardianServices;

using System.Text.Json;
using MudBlazor;

/// <summary>
/// Contains the logic for the snackbar service.
/// </summary>
public class SnackbarService
{
    // Contains all the queued messages.
    private readonly Queue<string> messages;
    private readonly Queue<string> severity;
    private readonly Queue<Action<SnackbarOptions>> config;

    private readonly List<string> levels = ["Normal", "Info", "Success", "Warning", "Error"];

    /// <summary>
    /// Initializes a new instance of the <see cref="SnackbarService"/> class.
    /// </summary>
    public SnackbarService()
    {
        this.messages = new Queue<string>();
        this.severity = new Queue<string>();
        this.config = new Queue<Action<SnackbarOptions>>();
    }

    /// <summary>
    /// Gets the next message in the snackbar queue and dequeues it.
    /// </summary>
    /// <param name="message">The next message in the queue.</param>
    /// <param name="requestedSeverity">The associated severity.</param>
    /// <param name="messageConfig">The config to use for the message.</param>
    /// <returns>True if a message can be returned. False otherwise.</returns>
    public bool NextMessage(out string message, out string requestedSeverity, out Action<SnackbarOptions> messageConfig)
    {
        if (this.severity.Count > 0 && this.messages.Count > 0)
        {
            message = this.messages.Dequeue();
            requestedSeverity = this.severity.Dequeue();
            messageConfig = this.config.Dequeue();

            return true;
        }

        message = string.Empty;
        requestedSeverity = string.Empty;
        messageConfig = (options) => { };

        return false;
    }

    /// <summary>
    /// Adds a message to the snackbar queue.
    /// </summary>
    /// <param name="message">The message to add to the queue.</param>
    public void AddMessage(string message)
    {
        this.messages.Enqueue(message);
        this.severity.Enqueue("Info");
        this.config.Enqueue((options) => { });
    }

    /// <summary>
    /// Adds a message to the snackbar queue.
    /// </summary>
    /// <param name="message">The message to add to the queue.</param>
    /// <param name="requestedSeverity">The associated severity for the message.</param>
    public void AddMessage(string message, string requestedSeverity)
    {
        this.messages.Enqueue(message);
        this.config.Enqueue((options) => { });
        this.ExtractSeverity(requestedSeverity);
    }

    /// <summary>
    /// Adds a message to the snackbar queue.
    /// </summary>
    /// <param name="message">The message to add to the queue.</param>
    /// <param name="requestedSeverity">The associated severity for the message.</param>
    /// <param name="messageConfig">The config params to be used for the message.</param>
    public void AddMessage(string message, string requestedSeverity, Action<SnackbarOptions> messageConfig)
    {
        this.messages.Enqueue(message);
        this.config.Enqueue(messageConfig);
        this.ExtractSeverity(requestedSeverity);
    }

    /// <summary>
    /// Extracts and adds the severity to the queue. Normal will be queued instead if extract failed.
    /// </summary>
    /// <param name="messageSeverity">The severity level.</param>
    private void ExtractSeverity(string messageSeverity)
    {
        messageSeverity = char.ToUpper(messageSeverity[0]) + messageSeverity[1..];
        this.severity.Enqueue(this.levels.Contains(messageSeverity) ? messageSeverity : "Normal");
    }
}

/// <summary>
/// Contains the logic for the loading bar service. TODO: NOT FUNCTIONAL YET.
/// </summary>
public class LoadingbarService
{
    private readonly Dictionary<int, Loadingbar> loadingbars;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadingbarService"/> class.
    /// </summary>
    public LoadingbarService()
    {
        this.loadingbars = new Dictionary<int, Loadingbar>();
    }

    /// <summary>
    /// Generates a new loading bar with the given name.
    /// </summary>
    /// <param name="name">The name of the new loadingbar.</param>
    /// <returns>True if the loadingbar was created. False if it either already exists for failed to create.</returns>
    public bool GenerateLoadingbar(string name)
    {
        int id = name.GetHashCode();
        if (this.loadingbars.ContainsKey(id))
        {
            return false;
        }

        this.loadingbars.Add(id, new Loadingbar());
        return true;
    }

    /// <summary>
    /// Removes the loadingbar with the given name.
    /// </summary>
    /// <param name="name">Name of the loadingbar to remove.</param>
    public void RemoveLoadingbar(string name)
    {
        int id = name.GetHashCode();
        this.loadingbars.Remove(id);
    }

    /// <summary>
    /// Sets the progress value of the loadingbar with the given name.
    /// </summary>
    /// <param name="name">The name of the loadingbar.</param>
    /// <param name="value">The new progress value.</param>
    public void SetProgressValue(string name, int value)
    {
        int id = name.GetHashCode();
        if (this.loadingbars.ContainsKey(id))
        {
            this.loadingbars[id].ProgressValue = value;
        }
    }

    /// <summary>
    /// Sets the buffer value of the loadingbar with the given name.
    /// </summary>
    /// <param name="name">The name of the loadingbar.</param>
    /// <param name="value">The new buffer value.</param>
    public void SetBufferValue(string name, int value)
    {
        int id = name.GetHashCode();
        if (this.loadingbars.ContainsKey(id))
        {
            this.loadingbars[id].BufferValue = value;
        }
    }

    /// <summary>
    /// Sets the progress text of the loadingbar with the given name.
    /// </summary>
    /// <param name="name">The name of the loadingbar.</param>
    /// <param name="value">The new progress text.</param>
    public void SetProgressText(string name, string value)
    {
        int id = name.GetHashCode();
        if (this.loadingbars.ContainsKey(id))
        {
            this.loadingbars[id].ProgressText = value;
        }
    }

    /// <summary>
    /// Gets all values associated with the loadingbar.
    /// </summary>
    /// <param name="name">Loadingbar to get the values for.</param>
    /// <param name="progressValue">Progress value of the loadingbar.</param>
    /// <param name="bufferValue">Buffer value of the loadingbar.</param>
    /// <param name="progressText">Progress text of the loadingbar.</param>
    public void GetLoadingBar(string name, out int progressValue, out int bufferValue, out string progressText)
    {
        int id = name.GetHashCode();
        if (this.loadingbars.TryGetValue(id, out Loadingbar? loadingbar))
        {
            progressValue = loadingbar.ProgressValue;
            bufferValue = loadingbar.BufferValue;
            progressText = loadingbar.ProgressText;
        }

        progressValue = 0;
        bufferValue = 0;
        progressText = string.Empty;
    }

    /// <summary>
    /// Gets the progress value of the loadingbar with the given name.
    /// </summary>
    /// <param name="name">The name of the loadingbar.</param>
    /// <returns>The current progress value.</returns>
    public int GetProgressValue(string name)
    {
        int id = name.GetHashCode();
        return this.loadingbars.TryGetValue(id, out Loadingbar? loadingbar) ? loadingbar.ProgressValue : 0;
    }

    /// <summary>
    /// Gets the buffer value of the loadingbar with the given name.
    /// </summary>
    /// <param name="name">The name of the loadingbar.</param>
    /// <returns>The current buffer value.</returns>
    public int GetBufferValue(string name)
    {
        int id = name.GetHashCode();
        return this.loadingbars.TryGetValue(id, out Loadingbar? loadingbar) ? loadingbar.BufferValue : 0;
    }

    /// <summary>
    /// Gets the progress text of the loadingbar with the given name.
    /// </summary>
    /// <param name="name">The name of the loadingbar.</param>
    /// <returns>The current loadingbar text.</returns>
    public string GetProgressText(string name)
    {
        int id = name.GetHashCode();
        return this.loadingbars.TryGetValue(id, out Loadingbar? loadingbar) ? loadingbar.ProgressText : string.Empty;
    }

    /// <summary>
    /// Defines a loadingbar.
    /// </summary>
    private class Loadingbar
    {
        // Values for the progress bar.
        private int progressValue;
        private int bufferValue;
        private string progressText = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="Loadingbar"/> class.
        /// </summary>
        public Loadingbar()
        {
            this.progressValue = 0;
            this.bufferValue = 0;
        }

        /// <summary>
        /// Gets or sets the progress value of the main progressbar. To reset the progress bar, set this value to 0 or above 100.
        /// </summary>
        public int ProgressValue
        {
            get => this.progressValue;
            set
            {
                if (value <= 100 && value >= 0)
                {
                    this.progressValue = value;
                }
                else
                {
                    this.progressValue = 0;
                }
            }
        }

        /// <summary>
        /// Gets or sets the buffer value of the main progressbar.  To reset the progress bar, set this value to 0 or above 100.
        /// </summary>
        public int BufferValue
        {
            get => this.bufferValue;
            set
            {
                if (value <= 100 && value >= 0)
                {
                    this.bufferValue = value;
                }
                else
                {
                    this.bufferValue = 0;
                }
            }
        }

        /// <summary>
        /// Gets or sets the progress text/status of the main progressbar.
        /// </summary>
        public string ProgressText
        {
            get => this.progressText;
            set => this.progressText = value;
        }
    }
}

/// <summary>
/// Handles data management for MudTimelines.
/// </summary>
public class TimelineService
{
    private Dictionary<string, Timeline> timelines;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimelineService"/> class.
    /// </summary>
    public TimelineService()
    {
        this.timelines = new Dictionary<string, Timeline>();
    }

    /// <summary>
    /// Gets all current information related to the requested timeline.
    /// </summary>
    /// <param name="name">Name of the timeline.</param>
    /// <param name="messages">All messages for the timeline.</param>
    /// <param name="color">Colors the messages should display.</param>
    /// <param name="alt">Booleans indicating whether the message should be alternated.</param>
    /// <returns>True if the timeline was found, False otherwise.</returns>
    public bool GetTimeline(string name, out List<string> messages, out List<Color> color, out List<bool> alt)
    {
        if (this.timelines.TryGetValue(name, out Timeline? timeline))
        {
            messages = timeline.Messages;
            color = timeline.Color;
            alt = timeline.Alt;
            return true;
        }

        messages = new List<string>();
        color = new List<Color>();
        alt = new List<bool>();
        return false;
    }

    /// <summary>
    /// Adds a timeline to the Timeline service with a pre-generated name.
    /// </summary>
    /// <returns>The name of the generated timeline.</returns>
    public string AddTimeline()
    {
        string name = Guid.NewGuid().ToString();
        if (this.timelines.ContainsKey(name))
        {
            return string.Empty;
        }

        this.timelines.Add(name, new Timeline());
        return name;
    }

    /// <summary>
    /// Adds a timeline to the Timeline service with the given name.
    /// </summary>
    /// <param name="name">Name of the timeline to add.</param>
    public void AddTimeline(string name)
    {
        if (this.timelines.ContainsKey(name))
        {
            return;
        }

        this.timelines.Add(name, new Timeline());
    }

    /// <summary>
    /// Removes the timeline with the given name from the timeline service.
    /// </summary>
    /// <param name="name">Name of the timeline to remove.</param>
    public void RemoveTimeline(string name)
    {
        this.timelines.Remove(name);
    }

    /// <summary>
    /// Adds a message to the timeline of a given name.
    /// </summary>
    /// <param name="name">Name of the timeline.</param>
    /// <param name="message">Message to add.</param>
    public void AddMessage(string name, string message)
    {
        if (!this.timelines.TryGetValue(name, out Timeline? timeline))
        {
            return;
        }

        timeline.Messages.Add(message);
        timeline.Color.Add(Color.Info);
        timeline.Alt.Add(false);
    }

    /// <summary>
    /// Adds a message to the timeline of a given name.
    /// </summary>
    /// <param name="name">Name of the timeline.</param>
    /// <param name="message">Message to add.</param>
    /// <param name="color">Color to display the message as.</param>
    public void AddMessage(string name, string message, Color color)
    {
        if (!this.timelines.TryGetValue(name, out Timeline? timeline))
        {
            return;
        }

        timeline.Messages.Add(message);
        timeline.Color.Add(color);
        timeline.Alt.Add(false);
    }

    /// <summary>
    /// Adds a message to the timeline of a given name.
    /// </summary>
    /// <param name="name">Name of the timeline.</param>
    /// <param name="message">Message to add.</param>
    /// <param name="color">Color to display the message as.</param>
    /// <param name="alt">Whether the message should display in the alternate manner.</param>
    public void AddMessage(string name, string message, Color color, bool alt)
    {
        if (!this.timelines.TryGetValue(name, out Timeline? timeline))
        {
            return;
        }

        timeline.Messages.Add(message);
        timeline.Color.Add(color);
        timeline.Alt.Add(alt);
    }

    /// <summary>
    /// Represents all the data for a Timeline.
    /// </summary>
    private class Timeline
    {
        private List<string> messages;
        private List<Color> color;
        private List<bool> alt;

        public Timeline()
        {
            this.messages = new List<string>();
            this.color = new List<Color>();
            this.alt = new List<bool>();
        }

        public List<string> Messages { get => this.messages; }

        public List<Color> Color { get => this.color; }

        public List<bool> Alt { get => this.alt; }
    }
}