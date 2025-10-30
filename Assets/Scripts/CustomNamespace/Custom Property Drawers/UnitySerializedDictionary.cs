using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using CustomNamespace.Editor;
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

    // Staging entry for new items before they're added to the list
    [SerializeField]
    private SerializableKeyValuePair<TKey, TValue> m_stagingEntry;

    [NonSerialized]
    private Dictionary<TKey, TValue> m_dictionary;
    
    [NonSerialized]
    private bool m_initialized;
    
    public Dictionary<TKey, TValue> Dictionary 
    { 
        get 
        {
            EnsureInitialized();
            return m_dictionary;
        }
    }

    private void EnsureInitialized()
    {
        if (m_initialized && m_dictionary != null) return;
        
        if (m_dictionary == null)
            m_dictionary = new Dictionary<TKey, TValue>();
        else
            m_dictionary.Clear();
        
        // Build dictionary from list - last duplicate wins
        foreach (SerializableKeyValuePair<TKey, TValue> kvp in m_list.Where(kvp => kvp.Key != null))
        {
            m_dictionary[kvp.Key] = kvp.Value;
        }
        
        m_initialized = true;
    }

    public TValue this[TKey key]
    {
        get => Dictionary[key];
        set 
        { 
            Dictionary[key] = value;
            m_initialized = true;
        }
    }

    public void Add(TKey key, TValue value) 
    { 
        Dictionary.Add(key, value);
        m_initialized = true;
    }
    
    public bool Remove(TKey key) 
    { 
        bool result = Dictionary.Remove(key);
        if (result) m_initialized = true;
        return result;
    }
    
    public bool ContainsKey(TKey key) => Dictionary.ContainsKey(key);
    public bool TryGetValue(TKey key, out TValue value) => Dictionary.TryGetValue(key, out value);
    
    public void Clear() 
    { 
        Dictionary.Clear();
        m_initialized = true;
    }
    
    public int Count => Dictionary.Count;

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Dictionary.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Dictionary.GetEnumerator();

    public void OnBeforeSerialize()
    {
        // Only write back to list if dictionary was initialized and potentially modified
        if (!m_initialized || m_dictionary is not { Count: > 0 }) return;
        m_list.Clear();
        foreach (KeyValuePair<TKey, TValue> kvp in m_dictionary)
        {
            m_list.Add(new SerializableKeyValuePair<TKey, TValue>(kvp.Key, kvp.Value));
        }
    }

    public void OnAfterDeserialize()
    {
        // Force re-initialization on next access to get updated values
        m_initialized = false;
    }
    
    public static implicit operator Dictionary<TKey, TValue>(SerializableDictionary<TKey, TValue> dictionary)
    {
        return dictionary.Dictionary;
    }
    
    public static implicit operator SerializableDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
    {
        return new SerializableDictionary<TKey, TValue> { m_dictionary = new Dictionary<TKey, TValue>(dictionary), m_initialized = true }; 
    }
}

#if UNITY_EDITOR

// Custom PropertyDrawer using UIElements
[CustomPropertyDrawer(typeof(SerializableDictionary<,>), true)]
public class SerializableDictionaryDrawer : PropertyDrawer
{
    static readonly Dictionary<Type, BuildUIForType> s_UIBuilderCache = new();
    delegate void BuildUIForType(SerializedProperty property, VisualElement container);
    
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        // Get the generic type arguments from fieldInfo
        Type fieldType = fieldInfo.FieldType;
        Type[] genericArgs = fieldInfo.FieldType.GetGenericArguments();
        
        if (genericArgs.Length != 2)
        {
            return new Label("Error: Invalid SerializableDictionary type");
        }
        
        // These types are now available to pass to ClearPropertyValue
        Type keyType = genericArgs[0];
        Type valueType = genericArgs[1];
        
        VisualElement container = new VisualElement();
        SerializedProperty listProperty = property.FindPropertyRelative("m_list");
        SerializedProperty stagingProperty = property.FindPropertyRelative("m_stagingEntry");

        // Create a foldout for the dictionary
        Foldout foldout = new Foldout
        {
            text = $"{property.displayName} ({listProperty.arraySize} entries)",
            value = property.isExpanded
        };
        foldout.RegisterValueChangedCallback(evt => property.isExpanded = evt.newValue);

        // Container for list items
        VisualElement listContainer = new VisualElement();

        // New entry section
        VisualElement newEntrySection = new()
        {
            style = 
            { 
                backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 0.2f)),
                paddingTop = 5,
                paddingBottom = 5,
                paddingLeft = 5,
                paddingRight = 5,
                marginBottom = new StyleLength(5),
                borderBottomWidth = 1,
                borderBottomColor = new StyleColor(new Color(0.1f, 0.1f, 0.1f, 0.5f))
            }
        };

        Label newEntryLabel = new Label("Add/Update Entry")
        {
            style = { unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 }
        };
        newEntrySection.Add(newEntryLabel);

        // Staging entry fields
        SerializedProperty stagingKey = stagingProperty.FindPropertyRelative("Key");
        SerializedProperty stagingValue = stagingProperty.FindPropertyRelative("Value");

        VisualElement stagingRow = new()
        {
            style = { flexDirection = FlexDirection.Column, marginBottom = 2 }
        };

        // Use PropertyDrawerCache for staging key
        VisualElement keyContainer = new() { style = { marginBottom = 2 } };
        DrawUIForType(keyType, stagingKey, keyContainer);
        
        // Use PropertyDrawerCache for staging value
        VisualElement valueContainer = new() { style = { marginBottom = 5 } };
        DrawUIForType(valueType, stagingValue, valueContainer);

        Button addButton = new Button(() =>
        {
            // Check if key already exists in the list
            int existingIndex = -1;
            for (int i = 0; i < listProperty.arraySize; i++)
            {
                SerializedProperty element = listProperty.GetArrayElementAtIndex(i);
                SerializedProperty existingKey = element.FindPropertyRelative("Key");

                if (!SerializedProperty.DataEquals(existingKey, stagingKey)) continue;
                existingIndex = i;
                break;
            }
            
            SerializedProperty targetElement;
            
            if (existingIndex >= 0)
            {
                // Update existing entry
                targetElement = listProperty.GetArrayElementAtIndex(existingIndex);
            }
            else
            {
                // Add new entry
                int newIndex = listProperty.arraySize;
                listProperty.arraySize++;
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
                targetElement = listProperty.GetArrayElementAtIndex(newIndex);
            }
            
            // Copy values from staging to target element
            SerializedProperty targetKey = targetElement.FindPropertyRelative("Key");
            SerializedProperty targetValue = targetElement.FindPropertyRelative("Value");
            
            CopyPropertyValue(stagingKey, targetKey);
            CopyPropertyValue(stagingValue, targetValue);
            
            property.serializedObject.ApplyModifiedProperties();
            
            // Clear staging entry using the robust reflection approach
            ClearPropertyValue(stagingKey, keyType);
            ClearPropertyValue(stagingValue, valueType);
            
            property.serializedObject.ApplyModifiedProperties();
        })
        {
            text = "Add/Update",
            style = { alignSelf = Align.FlexEnd, width = 80 }
        };

        stagingRow.Add(keyContainer);
        stagingRow.Add(valueContainer);
        stagingRow.Add(addButton);
        newEntrySection.Add(stagingRow);

        // Build the existing entries list
        RebuildList();

        foldout.Add(newEntrySection);
        foldout.Add(listContainer);
        container.Add(foldout);

        // Track changes (Fixes the Undo/Redo error)
        container.TrackPropertyValue(listProperty, (prop) => 
        {
            foldout.text = $"{property.displayName} ({prop.arraySize} entries)";
            RebuildList(); 
        });

        return container;

        void RebuildList()
        {
            listContainer.Clear();
            
            for (int i = 0; i < listProperty.arraySize; i++)
            {
                int index = i;
                SerializedProperty element = listProperty.GetArrayElementAtIndex(i);
                SerializedProperty keyProp = element.FindPropertyRelative("Key");
                SerializedProperty valueProp = element.FindPropertyRelative("Value");

                VisualElement entryRow = new()
                {
                    style = 
                    { 
                        flexDirection = FlexDirection.Column, 
                        marginBottom = 5,
                        paddingTop = 5, // Corrected padding
                        paddingBottom = 5, // Corrected padding
                        paddingLeft = 5, // Corrected padding
                        paddingRight = 5, // Corrected padding
                        
                        borderBottomWidth = 1,
                        borderBottomColor = new StyleColor(new Color(0.1f, 0.1f, 0.1f, 0.5f))
                    }
                };

                // Use PropertyDrawerCache for key
                VisualElement keyEntryContainer = new() { style = { marginBottom = 2 } };
                DrawUIForType(keyType, keyProp, keyEntryContainer);
                keyEntryContainer.SetEnabled(false); // Make key read-only
                
                // Use PropertyDrawerCache for value
                VisualElement valueEntryContainer = new() { style = { marginBottom = 2 } };
                DrawUIForType(valueType, valueProp, valueEntryContainer);
                valueEntryContainer.SetEnabled(false); // Make value read-only

                Button removeButton = new(() =>
                {
                    if (index < listProperty.arraySize)
                    {
                        listProperty.DeleteArrayElementAtIndex(index);
                        property.serializedObject.ApplyModifiedProperties();
                    }
                })
                {
                    text = "Remove",
                    style = { alignSelf = Align.FlexEnd }
                };

                entryRow.Add(keyEntryContainer);
                entryRow.Add(valueEntryContainer);
                entryRow.Add(removeButton);
                listContainer.Add(entryRow);
            }
        }
    }
    
    // --- Helper Methods ---
    
    static void DrawUIForType(Type typeToDrawUIFor, SerializedProperty property, VisualElement container)
    {
        BuildUIForType builderDelegate = GetOrCacheUIBuilder(typeToDrawUIFor);
        builderDelegate?.Invoke(property, container);
    }

    static BuildUIForType GetOrCacheUIBuilder(Type typeToDrawUIFor)
    {
        if (typeToDrawUIFor == null)
            return null;
    
        if (s_UIBuilderCache.TryGetValue(typeToDrawUIFor, out BuildUIForType cached))
            return cached;
        
        BuildUIForType builder = CreateBuilderForType();
        s_UIBuilderCache[typeToDrawUIFor] = builder;
        return builder;
    }

    static BuildUIForType CreateBuilderForType()
    {
        return (prop, typeContainer) =>
        {
            // Try to get a custom drawer first
            // NOTE: PropertyDrawerCache is assumed to exist in CustomNamespace.Editor based on original code
            PropertyDrawer drawer = CustomNamespace.Editor.PropertyDrawerCache.CreateDrawerForProperty(prop, typeof(SerializableDictionaryDrawer));

            // Use the custom drawer's UI
            VisualElement customUI = drawer?.CreatePropertyGUI(prop);
            if (customUI != null)
            {
                typeContainer.Add(customUI);
                return;
            }

            // Fallback to simple PropertyField
            PropertyField field = new(prop, "");
            field.BindProperty(prop);
            typeContainer.Add(field);
        };
    }
    
    static void CopyPropertyValue(SerializedProperty source, SerializedProperty dest)
    {
        if (source == null || dest == null) return;
        dest.boxedValue = source.boxedValue;
    }
    
    // --- GENERIC REFLECTION-BASED CLEARING LOGIC ---
    
    // Helper to get the actual Type of a SerializedProperty using reflection on the path.
    static Type GetFieldType(SerializedProperty prop)
    {
        string path = prop.propertyPath.Replace(".Array.data[", "[");
        string[] elements = path.Split('.');

        Type currentType = prop.serializedObject.targetObject.GetType();

        for (int i = 0; i < elements.Length; i++)
        {
            string element = elements[i];
            
            // Handle array elements like "myArray[0]"
            if (element.EndsWith("]"))
            {
                currentType = currentType.IsArray ? currentType.GetElementType() : currentType.GetGenericArguments()[0];
                
                // Advance past the array part
                element = element.Substring(0, element.IndexOf('['));
            }

            FieldInfo field = currentType.GetField(element, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            
            if (field == null)
            {
                // Fallback attempt: if field is null, we can't reliably continue.
                return currentType;
            }

            currentType = field.FieldType;
        }

        return currentType;
    }

    // Top-level clearing function for a generic property
    static void ClearPropertyValue(SerializedProperty prop, Type targetType)
    {
        if (prop == null || targetType == null) return;

        // Clear Reference Types (Classes)
        if (!targetType.IsValueType || prop.propertyType == SerializedPropertyType.ManagedReference)
        {
            try
            {
                prop.boxedValue = null;
            }
            catch
            {
                // If setting null fails for a complex reference type, clear fields as a fallback.
                ClearStructChildren(prop);
            }
        }
        // Clear Value Types (Structs) by iterating through children
        else 
        {
            ClearStructChildren(prop);
        }
    }

    // Recursive helper to clear all fields within a struct.
    static void ClearStructChildren(SerializedProperty prop)
    {
        SerializedProperty iterator = prop.Copy();
        SerializedProperty end = prop.GetEndProperty();

        if (iterator.Next(true)) // Enter the first child
        {
            do
            {
                if (SerializedProperty.EqualContents(iterator, end))
                    break;

                // Get the actual C# Type for the current field being iterated
                Type fieldType = GetFieldType(iterator);

                if (iterator.propertyType == SerializedPropertyType.Generic)
                {
                    // If the child is another struct, recurse
                    ClearStructChildren(iterator); 
                }
                else
                {
                    // For all other types (primitives, vectors, colors, object refs, etc.), 
                    // use Activator.CreateInstance to get the generic default value.
                    try
                    {
                        object defaultValue = fieldType.IsValueType && fieldType != typeof(void) ? Activator.CreateInstance(fieldType) : null;
                        iterator.boxedValue = defaultValue;
                    }
                    catch (Exception e)
                    {
                        // Fallback for types that may not have a clear default value
                        Debug.LogWarning($"Failed to reset field {iterator.name} of type {fieldType.Name}. Error: {e.Message}");
                    }
                }
            }
            while (iterator.Next(false)); // Move to the next sibling
        }
    }
}
#endif