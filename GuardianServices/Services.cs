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
/// Contains the logic for the loading bar service. NOT FUNCTIONAL YET.
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
/// Contains the logic for the settings service.
/// </summary>
public class SettingsService
{
    // File Paths
    private readonly string dataFolderPath;
    private readonly string settingsPath;

    // Contains all the settings.
    private readonly Dictionary<int, bool> settings = new Dictionary<int, bool>();

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsService"/> class.
    /// </summary>
    public SettingsService()
    {
        this.dataFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Services", "settings");
        this.settingsPath = Path.Combine(this.dataFolderPath, "UserSettings.json");

        this.LoadSettings();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsService"/> class. The settings are initialized with the given names as true.
    /// </summary>
    /// <param name="initialSettings">Initial settings to add.</param>
    public SettingsService(string[] initialSettings)
        : this()
    {
        foreach (string setting in initialSettings)
        {
            this.SetSetting(setting, true, false);
        }
    }

    /// <summary>
    /// Sets the setting to the corresponding value. If the setting does not exist, it will be created.
    /// </summary>
    /// <param name="setting">Name of the setting.</param>
    /// <param name="value">Value of the setting.</param>
    /// <param name="overwrite">If true the value of the setting will be overwritten if the setting exists already.</param>
    public void SetSetting(string setting, bool value, bool overwrite)
    {
        setting = setting.ToUpper();
        int settingHash = setting.GetHashCode();
        if (this.settings.ContainsKey(settingHash))
        {
            if (overwrite)
            {
                this.settings[settingHash] = value;
            }
        }
        else
        {
            this.settings.Add(settingHash, value);
        }
    }

    /// <summary>
    /// Sets the setting to the corresponding value. If the setting does not exist, it will be created.
    /// </summary>
    /// <param name="settingsList">Dictionary of the settings to change or add.</param>
    /// <param name="overwrite">If true the value of the setting will be overwritten if the setting exists already.</param>
    public void SetSettings(Dictionary<string, bool> settingsList, bool overwrite)
    {
        foreach (KeyValuePair<string, bool> setting in settingsList)
        {
            this.SetSetting(setting.Key, setting.Value, overwrite);
        }
    }

    /// <summary>
    /// Removes the selected setting.
    /// </summary>
    /// <param name="setting">Setting to remove.</param>
    public void RemoveSetting(string setting)
    {
        setting = setting.ToUpper();
        int settingHash = setting.GetHashCode();
        this.settings.Remove(settingHash);
    }

    /// <summary>
    /// Returns the value of the setting. WARNING: If the setting does not exist, it will return false.
    /// </summary>
    /// <param name="setting">Setting to check.</param>
    /// <returns>The boolean state of the setting.</returns>
    public bool Check(string setting)
    {
        setting = setting.ToUpper();
        return this.settings.GetValueOrDefault(setting.GetHashCode(), false);
    }

    /// <summary>
    /// Saves the user settings to a json file.
    /// </summary>
    /// <returns>True if completed successfully. False Otherwise.</returns>
    public bool SaveSettings()
    {
        try
        {
            if (!Directory.Exists(this.dataFolderPath))
            {
                Directory.CreateDirectory(this.dataFolderPath);
            }

            string json = JsonSerializer.Serialize(this.settings);
            File.WriteAllText(this.settingsPath, json);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Could not save user settings: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Sets the setting to the corresponding value. If the setting does not exist, it will be created.
    /// </summary>
    /// <param name="settingHash">Hash of the name for the setting.</param>
    /// <param name="value">Value of the setting.</param>
    /// <param name="overwrite">If true the value of the setting will be overwritten if the setting exists already.</param>
    private void SetSetting(int settingHash, bool value, bool overwrite)
    {
        if (this.settings.TryAdd(settingHash, value))
        {
            return;
        }

        if (overwrite)
        {
            this.settings[settingHash] = value;
        }
    }

    /// <summary>
    /// Loads the user settings from a json file.
    /// </summary>
    /// <returns>True if completed successfully. False Otherwise.</returns>
    private bool LoadSettings()
    {
        try
        {
            if (!File.Exists(this.settingsPath))
            {
                return false;
            }

            string json = File.ReadAllText(this.settingsPath);
            Dictionary<int, bool> loadedSettings =
                JsonSerializer.Deserialize<Dictionary<int, bool>>(json) ?? new Dictionary<int, bool>();

            foreach (KeyValuePair<int, bool> setting in loadedSettings)
            {
                this.SetSetting(setting.Key, setting.Value, true);
            }

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Could not load user settings: {e.Message}");
            return false;
        }
    }
}