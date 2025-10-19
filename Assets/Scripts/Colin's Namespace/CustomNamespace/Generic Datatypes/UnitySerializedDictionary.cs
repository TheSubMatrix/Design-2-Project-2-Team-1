using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

// Serializable key-value pair
[Serializable]
public struct SerializableKeyValuePair<TKey, TValue>
{
    public TKey Key;
    public TValue Value;

    public SerializableKeyValuePair(TKey key, TValue value)
    {
        Key = key;
        Value = value;
    }
}

// Base class for serializable dictionary
[Serializable]
public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver, IEnumerable
{
    [FormerlySerializedAs("list")] [SerializeField]
    private List<SerializableKeyValuePair<TKey, TValue>> m_list = new List<SerializableKeyValuePair<TKey, TValue>>();

    public Dictionary<TKey, TValue> Dictionary { get; } = new Dictionary<TKey, TValue>();

    public TValue this[TKey key]
    {
        get => Dictionary[key];
        set => Dictionary[key] = value;
    }

    public void Add(TKey key, TValue value) => Dictionary.Add(key, value);
    public bool Remove(TKey key) => Dictionary.Remove(key);
    public bool ContainsKey(TKey key) => Dictionary.ContainsKey(key);
    public bool TryGetValue(TKey key, out TValue value) => Dictionary.TryGetValue(key, out value);
    public void Clear() => Dictionary.Clear();
    public int Count => Dictionary.Count;

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Dictionary.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Dictionary.GetEnumerator();

    public void OnBeforeSerialize()
    {
        m_list.Clear();
        foreach (KeyValuePair<TKey, TValue> kvp in Dictionary)
        {
            m_list.Add(new SerializableKeyValuePair<TKey, TValue>(kvp.Key, kvp.Value));
        }
    }

    public void OnAfterDeserialize()
    {
        Dictionary.Clear();
        foreach (SerializableKeyValuePair<TKey, TValue> kvp in m_list.Where(kvp => !Dictionary.ContainsKey(kvp.Key)))
        {
            Dictionary.Add(kvp.Key, kvp.Value);
        }
    }
}

// Concrete implementations for common types
[Serializable]
public class StringIntDictionary : SerializableDictionary<string, int> { }

[Serializable]
public class StringStringDictionary : SerializableDictionary<string, string> { }

[Serializable]
public class StringGameObjectDictionary : SerializableDictionary<string, GameObject> { }

#if UNITY_EDITOR
// Custom PropertyDrawer using UIElements
[CustomPropertyDrawer(typeof(SerializableDictionary<,>), true)]
public class SerializableDictionaryDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        VisualElement container = new VisualElement();
        SerializedProperty listProperty = property.FindPropertyRelative("m_list");

        // Create foldout
        Foldout foldout = new Foldout
        {
            text = $"{property.displayName} (Count: {listProperty.arraySize})",
            value = property.isExpanded
        };
        
        foldout.RegisterValueChangedCallback(evt => property.isExpanded = evt.newValue);

        // Content container
        VisualElement contentContainer = new()
        {
            style =
            {
                paddingLeft = 15
            }
        };

        // Create the list view
        VisualElement listContainer = new VisualElement();

        // Add button
        Button addButton = new Button(() =>
        {
            listProperty.arraySize++;
            property.serializedObject.ApplyModifiedProperties();
            RebuildList();
        })
        {
            text = "Add New Entry",
            style =
            {
                marginTop = 5
            }
        };

        // Build initial list
        RebuildList();

        // Assemble UI
        contentContainer.Add(listContainer);
        contentContainer.Add(addButton);
        foldout.Add(contentContainer);
        container.Add(foldout);

        // Track property changes to update count
        container.TrackPropertyValue(listProperty, prop =>
        {
            foldout.text = $"{property.displayName} (Count: {prop.arraySize})";
        });

        return container;

        void RebuildList()
        {
            listContainer.Clear();
            foldout.text = $"{property.displayName} (Count: {listProperty.arraySize})";

            for (int i = 0; i < listProperty.arraySize; i++)
            {
                int index = i; // Capture for closure
                SerializedProperty elementProperty = listProperty.GetArrayElementAtIndex(index);
                
                // Container for each entry
                VisualElement entryContainer = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        marginBottom = 2
                    }
                };

                // Property field for the key-value pair
                PropertyField propertyField = new PropertyField(elementProperty, $"Element {index}")
                {
                    style =
                    {
                        flexGrow = 1
                    }
                };
                propertyField.BindProperty(elementProperty);
                
                // Remove button
                Button removeButton = new Button(() =>
                {
                    listProperty.DeleteArrayElementAtIndex(index);
                    property.serializedObject.ApplyModifiedProperties();
                    RebuildList();
                })
                {
                    text = "Remove",
                    style =
                    {
                        width = 60,
                        marginLeft = 5
                    }
                };

                entryContainer.Add(propertyField);
                entryContainer.Add(removeButton);
                listContainer.Add(entryContainer);
            }
        }
    }
}
#endif
