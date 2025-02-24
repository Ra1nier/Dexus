// <copyright file="DataStructures.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace GuardianServices;

/// <summary>
/// Holds a dictionary of objects that are removed after a certain amount of time.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TValue">The type of the value.</typeparam>
public class TimedDictionary<TKey, TValue>
{
    private readonly Dictionary<TKey, (TValue Value, DateTime Expiry)> storage = new Dictionary<TKey, (TValue Value, DateTime Expiry)>();
    private readonly TimeSpan expirationTime;
    private Timer? cleanupTimer;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimedDictionary{TKey, TValue}"/> class.
    /// </summary>
    /// <param name="expirationTime">The time that items stay in the dictionary before they are removed.</param>
    public TimedDictionary(TimeSpan expirationTime)
    {
        this.expirationTime = expirationTime;
    }

    /// <summary>
    /// Adds the KeyValuePair to the TimedDictionary.
    /// </summary>
    /// <param name="key">The identifier for the item.</param>
    /// <param name="value">The item itself.</param>
    public void Add(TKey key, TValue value)
    {
        lock (this.storage)
        {
            this.storage[key] = (value, DateTime.UtcNow.Add(this.expirationTime));
        }
    }

    /// <summary>
    /// Tries to get the value associated with the key.
    /// </summary>
    /// <param name="key">Key to find.</param>
    /// <param name="value">Value that the key is assosiated with.</param>
    /// <returns>True of a value was assosiated with a key. False otherwise.</returns>
    public bool TryGetValue(TKey key, out TValue value)
    {
        lock (this.storage)
        {
            if (this.storage.TryGetValue(key, out var entry))
            {
                if (entry.Expiry > DateTime.UtcNow)
                {
                    value = entry.Value;
                    return true;
                }
                else
                {
                    this.storage.Remove(key);
                }
            }
        }

        value = default!;
        return false;
    }

    /// <summary>
    /// Cleans the dictionary for expired items.
    /// </summary>
    public void Cleanup()
    {
        lock (this.storage)
        {
            var now = DateTime.UtcNow;
            var expiredKeys = this.storage.Where(kvp => kvp.Value.Expiry <= now).Select(kvp => kvp.Key).ToList();
            foreach (var key in expiredKeys)
            {
                this.storage.Remove(key);
            }
        }
    }

    /// <summary>
    /// Starts a Timer to auto clean the dictionary every so often.
    /// </summary>
    /// <param name="interval">How often to clean the dictionary.</param>
    public void StartAutoCleanup(TimeSpan interval)
    {
        this.cleanupTimer = new Timer(_ => this.Cleanup(), null, TimeSpan.Zero, interval);
    }

    /// <summary>
    /// Stops the auto cleanup timer.
    /// </summary>
    public void StopAutoCleanup()
    {
        this.cleanupTimer?.Dispose();
    }

    /// <summary>
    /// Removes all objects currently in the Timed Dictionary.
    /// </summary>
    public void Clear()
    {
        lock (this.storage)
        {
            this.storage.Clear();
        }
    }
}
