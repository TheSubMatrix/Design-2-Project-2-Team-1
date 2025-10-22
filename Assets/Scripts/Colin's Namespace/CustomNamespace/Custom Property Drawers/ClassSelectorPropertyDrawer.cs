using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
#endif
using CustomNamespace.Extensions;
using UnityEngine;

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(ClassSelectorAttribute))]
public class ClassSelectorPropertyDrawer : PropertyDrawer
{
    static readonly Dictionary<Type, BuildUIForType> UIBuilderCache = new();
    ClassSelectorAttribute m_attributeData;
    
    delegate void BuildUIForType(SerializedProperty property, VisualElement container);
    
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        m_attributeData = attribute as ClassSelectorAttribute;
        
        // Infer the base type from the field if not explicitly provided
        Type baseType = m_attributeData?.Type;
        if (baseType == null)
        {
            property.GetFieldInfoAndStaticType(out Type staticType);
            baseType = staticType;
        }
        
        if (baseType == null)
        {
            Debug.LogError($"Could not determine base type for property {property.propertyPath}");
            return new Label("Error: Could not determine base type");
        }
        VisualElement root = new()
        {
            style =
            {
                marginTop = 2,
                marginBottom = 2
            }
        };

        // Create foldout for collapsing
        Foldout foldout = new()
        {
            text = property.displayName,
            value = property.isExpanded
        };
        
        // Save the expanded state
        foldout.RegisterValueChangedCallback(evt =>
        {
            property.isExpanded = evt.newValue;
            property.serializedObject.ApplyModifiedProperties();
        });
        DropdownField dropdown = new()
        {
            name = "TypeSelectionDropdown",
            style =
            {
                marginBottom = 4,
                marginLeft = 0  // Remove any default left margin
            }
        };

        // Container for object properties
        VisualElement propertiesContainer = new()
        {
            name = "ObjectProperties",
            style =
            {
                paddingLeft = 15,
                marginTop = 4
            }
        };

        foldout.Add(dropdown);
        foldout.Add(propertiesContainer);
        root.Add(foldout);
        
        // Get derived types and populate dropdown (include base type if not abstract)
        List<Type> derivedTypes = SelectableClassTypeCache.GetDerivedTypes(baseType, includeBaseType: true);
        Dictionary<string, Type> typesByName = derivedTypes.ToDictionary(t => t.Name, t => t);
        
        List<string> choices = new() { "None" };
        choices.AddRange(typesByName.Keys.OrderBy(name => name));
        dropdown.choices = choices;
        dropdown.SetValueWithoutNotify("None");
        
        // Handle type selection changes
        Type selectedType = null;
        dropdown.RegisterValueChangedCallback(evt =>
        {
            // Handle "None" selection
            if (evt.newValue == "None")
            {
                property.managedReferenceValue = null;
                property.serializedObject.ApplyModifiedProperties();
                propertiesContainer.Clear();
                return;
            }
            
            if (!typesByName.TryGetValue(evt.newValue, out selectedType)) return;
            property.managedReferenceValue = Activator.CreateInstance(selectedType);
            property.serializedObject.ApplyModifiedProperties();
                
            propertiesContainer.Clear();
            DrawUIForType(selectedType, property, propertiesContainer);
        });
        
        // Handle [field: SerializeField] by checking the backing field
        object currentValue = property.managedReferenceValue;
        if (currentValue == null)
        {
            // Try to get the value from the backing field if it exists
            SerializedProperty backingField = property.serializedObject.FindProperty($"<{property.name}>k__BackingField");
            if (backingField != null)
            {
                currentValue = backingField.managedReferenceValue;
                if (currentValue != null)
                {
                    property.managedReferenceValue = currentValue;
                }
            }
        }
        
        // Set the initial value and draw UI
        if (currentValue == null)
        {
            dropdown.SetValueWithoutNotify("None");
            return root;
        }
        selectedType = currentValue.GetType();
        int index = dropdown.choices.IndexOf(selectedType.Name);
        if (index < 0) return root;
        dropdown.SetValueWithoutNotify(dropdown.choices[index]);
        DrawUIForType(selectedType, property, propertiesContainer);

        return root;
    }

    static void DrawUIForType(Type typeToDrawUIFor, SerializedProperty property, VisualElement container)
    {
        BuildUIForType builderDelegate = GetOrCachePropertyDrawer(typeToDrawUIFor);
        builderDelegate?.Invoke(property, container);
    }

    static BuildUIForType GetOrCachePropertyDrawer(Type typeToDrawUIFor)
    {
        if (typeToDrawUIFor == null)
            return null;
    
        if (UIBuilderCache.TryGetValue(typeToDrawUIFor, out BuildUIForType cached))
            return cached;

        Type drawerType = SelectableClassTypeCache.GetPropertyDrawerForType(typeToDrawUIFor);

        // The custom property drawer exists (either direct match or inherited with useForChildren)
        if (drawerType != null)
        {
            UIBuilderCache[typeToDrawUIFor] = HybridDrawerDelegate;
            return HybridDrawerDelegate;
    
            void HybridDrawerDelegate(SerializedProperty prop, VisualElement typeContainer)
            {
                // Get the type that the drawer was designed for
                Type drawerTargetType = SelectableClassTypeCache.GetDrawerTargetType(drawerType);
                
                // Get all serialized fields from the actual type
                HashSet<string> fieldsHandledByDrawer = new();
                
                // If the drawer is for a parent class or interface, get fields from that type
                if (drawerTargetType != null && drawerTargetType != typeToDrawUIFor)
                {
                    // Collect field names from the drawer's target type
                    // For interfaces, this will be empty, which is correct
                    FieldInfo[] targetFields = drawerTargetType.GetFields(
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    
                    foreach (FieldInfo field in targetFields)
                    {
                        if (field.IsPublic || field.GetCustomAttribute<SerializeField>() != null)
                        {
                            fieldsHandledByDrawer.Add(field.Name);
                        }
                    }
                }
                else
                {
                    // Drawer is for the exact type, so it handles all fields
                    FieldInfo[] allFields = typeToDrawUIFor.GetFields(
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    
                    foreach (FieldInfo field in allFields)
                    {
                        if (field.IsPublic || field.GetCustomAttribute<SerializeField>() != null)
                        {
                            fieldsHandledByDrawer.Add(field.Name);
                        }
                    }
                }
                
                // Draw the custom drawer
                object drawerInstance = Activator.CreateInstance(drawerType);
                FieldInfo fieldInfoAndStaticType = prop.GetFieldInfoAndStaticType(out Type _);
                drawerType.GetField("m_FieldInfo", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    ?.SetValue(drawerInstance, fieldInfoAndStaticType);
        
                VisualElement customUI = (VisualElement)drawerType.GetMethod("CreatePropertyGUI")
                    ?.Invoke(drawerInstance, new object[] { prop });
            
                if (customUI != null)
                    typeContainer.Add(customUI);
                
                // Now draw any additional fields that are not handled by the drawer
                if (drawerTargetType == null || drawerTargetType == typeToDrawUIFor) return;
                
                // Create a section for derived class fields or interface implementation fields
                bool hasAdditionalFields = false;
                VisualElement additionalFieldsContainer = new()
                {
                    style = { marginTop = 8 }
                };
                
                foreach (SerializedProperty child in prop.GetChildren())
                {
                    // Skip fields that are handled by the parent drawer
                    if (fieldsHandledByDrawer.Contains(child.name))
                        continue;
                    
                    hasAdditionalFields = true;
                    PropertyField field = new(child);
                    additionalFieldsContainer.Add(field);
                }
                
                if (hasAdditionalFields)
                {
                    typeContainer.Add(additionalFieldsContainer);
                }
                
            }
        }

        // Fallback to default property fields
        UIBuilderCache[typeToDrawUIFor] = DefaultDrawerDelegate;
        return DefaultDrawerDelegate;

        void DefaultDrawerDelegate(SerializedProperty prop, VisualElement typeContainer)
        {
            foreach (SerializedProperty child in prop.GetChildren())
            {
                PropertyField field = new(child);
                typeContainer.Add(field);
            }
        }
    }
}
#endif