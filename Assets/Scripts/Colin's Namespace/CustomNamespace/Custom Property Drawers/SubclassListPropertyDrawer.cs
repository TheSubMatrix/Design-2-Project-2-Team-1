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

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SubclassListAttribute))]
public class SubclassListPropertyDrawer : PropertyDrawer
{
    //TODO: Improve type caching, I feel like the efficiency of this is not great but it does function
    bool m_isInitialized;
    const string FilePathForUIBuilderTree = "Assets/Scripts/Colin's Namespace/CustomNamespace/Custom Property Drawers/SubclassList.uxml";
    VisualTreeAsset m_uiBuilderTree;
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    static Dictionary<Type, BuildUIForType> m_uiBuilderTreeCache = new Dictionary<Type, BuildUIForType>();
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    static Dictionary<Type, Type> m_customPropertyDrawerForType = new Dictionary<Type, Type>();
    SubclassListAttribute m_attributeData;
    
    delegate void BuildUIForType(SerializedProperty property, VisualElement typeDrawerContainer);
    
    BuildUIForType RetrieveOrCachePropertyDrawer(Type typeToDrawUIFor)
    {
        if (typeToDrawUIFor == null)
        {
            return null;
        }
        if (m_uiBuilderTreeCache.TryGetValue(typeToDrawUIFor, out BuildUIForType cachedDrawerDelegate))
        {
            return cachedDrawerDelegate;
        }

        Type customPropertyDrawerForType = GetCachedOrDeterminePropertyDrawer(typeToDrawUIFor);
        
        //Draw direct defined custom property drawer
        if (customPropertyDrawerForType is not null)
        {
            
            object customDrawerInstance = Activator.CreateInstance(customPropertyDrawerForType);
            m_uiBuilderTreeCache.Add(typeToDrawUIFor, TypeDrawerDelegate);
            return TypeDrawerDelegate;

            void TypeDrawerDelegate(SerializedProperty propertyToDrawFor, VisualElement typeDrawerContainer)
            {
                FieldInfo currentFieldInfo = propertyToDrawFor.GetFieldInfoAndStaticType(out Type _);
                customPropertyDrawerForType.GetField("m_FieldInfo", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.SetValue(customDrawerInstance, currentFieldInfo);
                typeDrawerContainer.Add((VisualElement)customPropertyDrawerForType.GetMethod("CreatePropertyGUI")
                    ?.Invoke(customDrawerInstance, new object[]
                    {
                        propertyToDrawFor
                    }));
            }
        }

        m_uiBuilderTreeCache.Add(typeToDrawUIFor, FallbackTypeDrawerDelegate);
        return FallbackTypeDrawerDelegate;
        void FallbackTypeDrawerDelegate(SerializedProperty propertyToDrawFor, VisualElement typeDrawerContainer)
        {
            foreach(SerializedProperty childProperty in propertyToDrawFor.GetChildren())
            {
                PropertyField fieldToAdd = new PropertyField();
                fieldToAdd.BindProperty(childProperty);
                typeDrawerContainer.Add(fieldToAdd);
            }
        } 
    }

    Type GetCachedOrDeterminePropertyDrawer(Type typeToDrawUIFor)
    {
        
        if (typeToDrawUIFor == null)
        {
            return null;
        }
        if (m_customPropertyDrawerForType.TryGetValue(typeToDrawUIFor, out Type propertyDrawerForType))
        {
            return propertyDrawerForType;
        }
        Type customPropertyDrawerForType = GetCustomPropertyDrawerFor(typeToDrawUIFor);
        if (customPropertyDrawerForType is not null)
        {
            m_customPropertyDrawerForType.Add(typeToDrawUIFor, customPropertyDrawerForType);
            return customPropertyDrawerForType;
        }
        for (Type parentType = typeToDrawUIFor; parentType is not null; parentType = parentType.BaseType)
        {
            Type parentTypeDrawer = GetCustomPropertyDrawerFor(parentType, out bool childrenUseDrawer);
            if (parentTypeDrawer != null && childrenUseDrawer)
            {
                m_customPropertyDrawerForType.Add(parentType, parentTypeDrawer);
                return parentTypeDrawer;
            }
        }
        m_customPropertyDrawerForType.Add(typeToDrawUIFor, null);
        return null;
    }
    
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        if (!m_isInitialized)
        {
            m_uiBuilderTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(FilePathForUIBuilderTree);
            m_isInitialized = true;
        }
        VisualElement root = new VisualElement();
        m_uiBuilderTree.CloneTree(root);
        m_attributeData = attribute as SubclassListAttribute;
        DropdownField dropdownMenu = root.Query<DropdownField>("TypeSelectionDropdown");
        VisualElement objectPropertiesContainer = root.Query<VisualElement>("ObjectProperties");
        dropdownMenu.choices.Clear();
        //Get all non-abstract subtypes and add them to a dictionary where they can be looked up by name
        if (m_attributeData == null)
            return root;
        Dictionary<string, Type> derivingTypes = GetDerivingTypes(m_attributeData.Type).ToDictionary(value => value.Name, value => value);
        //Add type names to dropdown
        derivingTypes.ToList().ForEach(entry =>
        {
            dropdownMenu.choices.Add(entry.Key);
            
        });
        //try to find the type and pass it to the selected type when the value is changed
        Type selectedType = null;
        
        dropdownMenu.RegisterValueChangedCallback(value =>
            {
                derivingTypes.TryGetValue(value.newValue, out selectedType);
                if (selectedType == null)
                    return;
                property.managedReferenceValue = Activator.CreateInstance(selectedType);
                objectPropertiesContainer.Clear();
                DrawUI(selectedType, property, objectPropertiesContainer);
                property.serializedObject.ApplyModifiedProperties();
            }
        );
        if (property.managedReferenceValue == null)
            return root;
        selectedType = property.managedReferenceValue.GetType();
        dropdownMenu.value = dropdownMenu.choices[dropdownMenu.choices.IndexOf(selectedType.Name)];
        DrawUI(selectedType, property, objectPropertiesContainer);
        return root;
    }

    #region Get custom property drawer for type
    /// <summary>
    /// Get all property drawers in the project
    /// </summary>
    /// <returns>All property drawers in the projects a types</returns>
    IEnumerable<Type> AllPropertyDrawers()
    {
        //List<System.Type> drawers = new List<Type>();
        return from ass in AppDomain.CurrentDomain.GetAssemblies() from t in ass.GetTypes() where t.IsSubclassOf(typeof(PropertyDrawer)) select t;
    }
    /// <summary>
    /// Gets a custom property drawer given a type
    /// </summary>
    /// <param name="target">The type to find the property drawer for</param>
    /// <returns>The type of the property drawer found if there is one. Otherwise null</returns>
    Type GetCustomPropertyDrawerFor(Type target) 
    {
        foreach (Type drawer in AllPropertyDrawers())
        {
            foreach (Attribute propertyAttribute in Attribute.GetCustomAttributes(drawer))
            {
                if (propertyAttribute is CustomPropertyDrawer cpd && cpd.GetFieldValue<Type>("m_Type") == target)
                {
                    return drawer;
                }
            }
        }

        return null;
    }
    /// <summary>
    /// Gets a custom property drawer given a type and returns out saying whether child classes should use this drawer
    /// </summary>
    /// <param name="target">The type to find the property drawer for</param>
    /// <param name="childrenUseDrawer">whether child classes should use this drawer</param>
    /// <returns>The type of the property drawer found if there is one. Otherwise null</returns>
    Type GetCustomPropertyDrawerFor(Type target, out bool childrenUseDrawer)
    {
        foreach (Type drawer in AllPropertyDrawers())
        {
            foreach (Attribute propertyAttribute in Attribute.GetCustomAttributes(drawer))
            {
                if (propertyAttribute is not CustomPropertyDrawer cpd || cpd.GetFieldValue<Type>("m_Type") != target)
                    continue;
                childrenUseDrawer = cpd.GetFieldValue<bool>("m_UseForChildren");
                return drawer;
            }
        }
        childrenUseDrawer = false;
        return null;
    }
    #endregion
    /// <summary>
    /// Gets all types that derive from a given type
    /// </summary>
    /// <param name="typeToFindDerivingTypesFrom">The type that all items returned should derive from</param>
    /// <returns>All types that derive from the given type</returns>
    IEnumerable<Type> GetDerivingTypes(Type typeToFindDerivingTypesFrom)
    {
        return (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                from type in domainAssembly.GetTypes()
                where typeToFindDerivingTypesFrom.IsAssignableFrom(type) && !type.IsAbstract
                select type);
    }
    /// <summary>
    /// Tries to draw from a custom property drawer and defaults to a standard setup otherwise
    /// </summary>
    /// <param name="typeToDrawUIFor">The type of object you desire to draw a UI sor</param>
    /// <param name="property">The SerializedProperty for handling data</param>
    /// <param name="objectPropertiesContainer">The VisualElement to put the drawn properties into</param>
    void DrawUI(Type typeToDrawUIFor, SerializedProperty property, VisualElement objectPropertiesContainer)
    {
        RetrieveOrCachePropertyDrawer(typeToDrawUIFor).Invoke(property, objectPropertiesContainer);
    }
}
#endif