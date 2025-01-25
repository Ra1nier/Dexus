// <copyright file="Services.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace GuardianServices;

/// <summary>
/// Contains the logic for the snackbar service.
/// </summary>
public class SnackbarService
{
    // Contains all of the queued messages.
    private Queue<string> messages;
    private Queue<string> severity;

    private List<string> levels = new List<string> { "Normal", "Info", "Success", "Warning", "Error" };

    /// <summary>
    /// Initializes a new instance of the <see cref="SnackbarService"/> class.
    /// </summary>
    public SnackbarService()
    {
        this.messages = new Queue<string>();
        this.severity = new Queue<string>();
    }

    /// <summary>
    /// Gets the next message in the snakebar queue and dequeues it.
    /// </summary>
    /// <param name="message">The next message in the queue.</param>
    /// <param name="severity">The assosiated severity.</param>
    /// <returns>True if a message can be returned. False otherwise.</returns>
    public bool NextMessage(out string message, out string severity)
    {
        if (this.severity.Count > 0 && this.messages.Count > 0)
        {
            message = this.messages.Count > 0 ? this.messages.Dequeue() : string.Empty;
            severity = this.severity.Count > 0 ? this.severity.Dequeue() : string.Empty;

            return true;
        }

        message = string.Empty;
        severity = string.Empty;
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
    }

    /// <summary>
    /// Adds a message to the snackbar queue.
    /// </summary>
    /// <param name="message">The message to add to the queue.</param>
    /// <param name="severity">The assosiated severity for the message.</param>
    public void AddMessage(string message, string severity)
    {
        this.messages.Enqueue(message);
        severity = char.ToUpper(severity[0]) + severity.Substring(1);

        if (this.levels.Contains(severity))
        {
            this.severity.Enqueue(severity);
        }
        else
        {
            this.severity.Enqueue("Normal");
        }
    }
}

/// <summary>
/// Contains the logic for the loading bar service. NOT FUNCTIONAL YET.
/// </summary>
public class LoadingbarService
{
    private Dictionary<int, Loadingbar> loadingbars;

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

        this.loadingbars.Add(id, new Loadingbar(name));
        return true;
    }

    /// <summary>
    /// Removes the loadingbar with the given name.
    /// </summary>
    /// <param name="name">Name of the loadingbar to remove.</param>
    public void RemoveLoadingbar(string name)
    {
        int id = name.GetHashCode();
        if (this.loadingbars.ContainsKey(id))
        {
            this.loadingbars.Remove(id);
        }
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
    /// Gets the progress value of the loadingbar with the given name.
    /// </summary>
    /// <param name="name">The name of the loadingbar.</param>
    /// <returns>The current progress value.</returns>
    public int GetProgressValue(string name)
    {
        int id = name.GetHashCode();
        if (this.loadingbars.ContainsKey(id))
        {
            return this.loadingbars[id].ProgressValue;
        }

        return 0;
    }

    /// <summary>
    /// Gets the buffer value of the loadingbar with the given name.
    /// </summary>
    /// <param name="name">The name of the loadingbar.</param>
    /// <returns>The current buffer value.</returns>
    public int GetBufferValue(string name)
    {
        int id = name.GetHashCode();
        if (this.loadingbars.ContainsKey(id))
        {
            return this.loadingbars[id].BufferValue;
        }

        return 0;
    }

    /// <summary>
    /// Gets the progress text of the loadingbar with the given name.
    /// </summary>
    /// <param name="name">The name of the loadingbar.</param>
    /// <returns>The current loadingbar text.</returns>
    public string GetProgressText(string name)
    {
        int id = name.GetHashCode();
        if (this.loadingbars.ContainsKey(id))
        {
            return this.loadingbars[id].ProgressText;
        }

        return string.Empty;
    }

    /// <summary>
    /// Defines a loadingbar.
    /// </summary>
    private class Loadingbar
    {
        private string name;

        // Values for the progress bar.
        private int progressValue;
        private int bufferValue;
        private string progressText = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="Loadingbar"/> class.
        /// </summary>
        public Loadingbar(string name)
        {
            this.progressValue = 0;
            this.bufferValue = 0;
            this.name = name;
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
                    this.ProgressValue = value;
                }
                else
                {
                    this.ProgressValue = 0;
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
/// Contains the logic for the settings service.
/// </summary>
public class SettingsService
{
    private Dictionary<int, bool> viewSettings = new Dictionary<int, bool>();

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsService"/> class.
    /// </summary>
    public SettingsService() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsService"/> class. The settings are initialized with the given names as true.
    /// </summary>
    /// <param name="initalSettings">Initail settings to add.</param>
    public SettingsService(string[] initalSettings)
    {
        foreach (string setting in initalSettings)
        {
            this.SetSetting(setting, true);
        }
    }

    /// <summary>
    /// Sets the setting to the corrosponding value. If the setting does not exist, it will be created.
    /// </summary>
    /// <param name="setting">Name of the setting.</param>
    /// <param name="value">Value of the setting.</param>
    public void SetSetting(string setting, bool value)
    {
        int settingHash = setting.GetHashCode();
        if (this.viewSettings.ContainsKey(settingHash))
        {
            this.viewSettings[settingHash] = value;
        }
        else
        {
            this.viewSettings.Add(settingHash, value);
        }
    }

    /// <summary>
    /// Removes the selected setting.
    /// </summary>
    /// <param name="setting">Setting to remove.</param>
    public void RemoveSetting(string setting)
    {
        int settingHash = setting.GetHashCode();
        if (this.viewSettings.ContainsKey(settingHash))
        {
            this.viewSettings.Remove(settingHash);
        }
    }

    /// <summary>
    /// Returns the value of the setting.
    /// </summary>
    /// <param name="setting">Setting to check.</param>
    /// <returns>The boolean state of the setting.</returns>
    public bool Check(string setting)
    {
        int settingHash = setting.GetHashCode();
        this.viewSettings.TryGetValue(settingHash, out bool value);
        return value;
    }
}