using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A custom dictionary class that is serializable by Unity.
/// This allows Dictionaries to be used directly in MonoBehaviours or ScriptableObjects
/// and have their data persisted in the Unity Editor and builds.
/// </summary>
/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
[Serializable]
public class UnitySerializedDictionary<TKey, TValue> : ISerializationCallbackReceiver
{
    private Dictionary<TKey, TValue> m_internalDictionary = new();

    [SerializeField]
    private List<SerializedKeyValuePair> m_serializedDictionaryHelper = new();

    /// <summary>
    /// Gets the underlying <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/>
    /// instance that holds the runtime data.
    /// </summary>
    public Dictionary<TKey, TValue> Dictionary { get => m_internalDictionary; }

    /// <summary>
    /// Callback method invoked by Unity before serialization.
    /// This method populates the serializable list from the internal dictionary.
    /// </summary>
    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        m_serializedDictionaryHelper.Clear();
        //to set the list to the updated state if it is modified via code (so we can track it :) )

        foreach (var pair in m_internalDictionary)
        {//checks up top, setting below.

            // Check if the key is null (only relevant for reference types)
            if (pair.Key == null)
            {
                Debug.LogWarning($"UnitySerializedDictionary: Skipping serialization of entry with a null key. Value: {pair.Value}");
                continue; // Skip this key-value pair
            }
            m_serializedDictionaryHelper.Add(new SerializedKeyValuePair(pair.Key, pair.Value));
        }
    }

    /// <summary>
    /// Callback method invoked by Unity after deserialization.
    /// This method reconstructs the internal dictionary from the serializable list.
    /// Handles null keys and duplicate keys by logging errors and skipping entries.
    /// </summary>
    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        m_internalDictionary = new Dictionary<TKey, TValue>();

        foreach (SerializedKeyValuePair pair in m_serializedDictionaryHelper)
        {
            // Check for null keys AND empty string keys (if TKey is string)
            // This makes the deserialization robust for string keys.
            if (typeof(TKey) == typeof(string))
            {
                // Use string.IsNullOrEmpty to catch both null and empty strings
                // This is safer than just 'pair.Key is null' for string keys.
                if (string.IsNullOrEmpty(pair.Key as string))
                {
                    Debug.LogError($"Key '{pair.Key}' (type: {typeof(TKey).Name}) is null or empty, this set will be skipped.");
                    continue; // Skip this pair if the string key is null or empty
                }
            }
            else // For other reference types (classes) or value types, the original null check is appropriate
            {
                if (pair.Key == null) // This will catch actual nulls for class-based TKey
                {
                    Debug.LogError($"Key '{pair}' (type: {typeof(TKey).Name}) is null, this set will be skipped.");
                    continue; // Skip this pair if the key is null
                }
            }

            // Standard duplicate key check (this remains important)
            if (m_internalDictionary.ContainsKey(pair.Key))
            {
                Debug.LogError($"{this.Dictionary} already contains '{pair.Key}', this set will be skipped.");
                continue;
            }

            m_internalDictionary[pair.Key] = pair.Value;
        }
    }

    /// <summary>
    /// A serializable struct used to store key-value pairs for Unity's serialization system.
    /// This is an internal helper for <see cref="UnitySerializedDictionary{TKey, TValue}"/>.
    /// </summary>
    [Serializable]
    public struct SerializedKeyValuePair
    {
        /// <summary>
        /// The key of the key-value pair.
        /// </summary>
        [SerializeField]
        public TKey Key;

        /// <summary>
        /// The value associated with the key.
        /// </summary>
        [SerializeField]
        public TValue Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializedKeyValuePair"/> struct.
        /// </summary>
        /// <param name="key">The key for the pair.</param>
        /// <param name="value">The value for the pair.</param>
        public SerializedKeyValuePair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }

    /// <summary>
    /// Gets or sets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get or set.</param>
    /// <returns>The value associated with the specified key.</returns>
    /// <exception cref="KeyNotFoundException">The property is retrieved and <paramref name="key"/> does not exist in the collection.</exception>
    public TValue this[TKey key]
    {
        get { return m_internalDictionary[key]; }
        set { m_internalDictionary[key] = value; }
    }

    /// <summary>
    /// Adds the specified key and value to the dictionary.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">The value of the element to add.</param>
    /// <exception cref="ArgumentException">An element with the same key already exists in the dictionary.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
    public void Add(TKey key, TValue value)
    {
        m_internalDictionary.Add(key, value);
    }

    /// <summary>
    /// Removes the key-value pair with the specified key from the <see cref="UnitySerializedDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <param name="key">The key of the element to remove.</param>
    /// <returns><c>true</c> if the element is successfully found and removed; otherwise, <c>false</c>.
    /// This method returns <c>false</c> if <paramref name="key"/> is not found in the dictionary.</returns>
    public void Remove(TKey key)
    {
        m_internalDictionary.Remove(key);
    }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="value">When this method returns, contains the value associated with the specified key,
    /// if the key is found; otherwise, the default value for the type of the <paramref name="value"/> parameter.
    /// This parameter is passed uninitialized.</param>
    /// <returns><c>true</c> if the <see cref="UnitySerializedDictionary{TKey, TValue}"/> contains an element with the specified key;
    /// otherwise, <c>false</c>.</returns>
    public bool TryGetValue(TKey key, out TValue value)
    {
        return m_internalDictionary.TryGetValue(key, out value);
    }

    /// <summary>
    /// Determines whether the <see cref="UnitySerializedDictionary{TKey, TValue}"/> contains the specified key.
    /// </summary>
    /// <param name="key">The key to locate in the <see cref="UnitySerializedDictionary{TKey, TValue}"/>.</param>
    /// <returns><c>true</c> if the <see cref="UnitySerializedDictionary{TKey, TValue}"/> contains an element with the specified key;
    /// otherwise, <c>false</c>.</returns>
    public bool ContainsKey(TKey key)
    {
        return m_internalDictionary.ContainsKey(key);
    }

    /// <summary>
    /// Gets the number of key/value pairs contained in the <see cref="UnitySerializedDictionary{TKey, TValue}"/>.
    /// </summary>
    public int Count
    {
        get { return m_internalDictionary.Count; }
    }

    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="UnitySerializedDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <returns>An enumerator for the <see cref="UnitySerializedDictionary{TKey, TValue}"/>.</returns>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return m_internalDictionary.GetEnumerator();
    }
}