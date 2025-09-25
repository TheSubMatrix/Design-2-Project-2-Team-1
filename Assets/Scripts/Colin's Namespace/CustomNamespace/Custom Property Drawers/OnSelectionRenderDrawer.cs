using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;

[CustomPropertyDrawer(typeof(OnSelectionRenderAttribute))]
public class OnSelectionRenderDrawer : PropertyDrawer
{
    private bool m_shouldShow = true; // Cached state to avoid recalculating in OnGUI if not needed

    // This method calculates the height of the property based on visibility
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        OnSelectionRenderAttribute renderAttribute = (OnSelectionRenderAttribute)this.attribute;

        // Find the conditional property (boolean or enum)
        SerializedProperty conditionalProperty = property.serializedObject.FindProperty(renderAttribute.ConditionalFieldName);

        if (conditionalProperty == null)
        {
            // If the conditional field is not found, show a warning and the property.
            // Reserve space for the warning message + the property itself.
            m_shouldShow = true; // Ensure it draws if there's an error with the condition itself
            return EditorGUI.GetPropertyHeight(property, label, true) + EditorGUIUtility.singleLineHeight;
        }

        bool conditionMet = false;

        if (renderAttribute.IsEnumComparison)
        {
            // Handle Enum comparison
            if (conditionalProperty.propertyType == SerializedPropertyType.Enum)
            {
                // The actual value of the enum field
                int actualEnumValue = conditionalProperty.intValue; // SerializedProperty.intValue works for enums

                // The required value from the attribute, cast to its underlying int type
                // We need to ensure RequiredValue is indeed an enum and get its int value.
                if (renderAttribute.RequiredValue is Enum requiredEnum)
                {
                    int requiredInt = Convert.ToInt32(requiredEnum); // Convert enum value to its underlying int
                    conditionMet = (actualEnumValue == requiredInt);
                }
                else
                {
                    Debug.LogError($"OnSelectionRenderAttribute: RequiredValue for enum comparison is not a valid enum type for field '{renderAttribute.ConditionalFieldName}'.");
                }
            }
            else
            {
                // Conditional field is specified as enum but is not actually an enum type
                Debug.LogWarning($"OnSelectionRenderAttribute: Field '{renderAttribute.ConditionalFieldName}' is not an enum, but an enum comparison was specified for property '{property.name}'.");
            }
        }
        else // Boolean comparison
        {
            if (conditionalProperty.propertyType == SerializedPropertyType.Boolean)
            {
                bool actualBoolValue = conditionalProperty.boolValue;
                bool requiredBoolValue = (bool)renderAttribute.RequiredValue; // Cast the object to bool

                // Check if the actual value matches the required value
                conditionMet = (actualBoolValue == requiredBoolValue);
            }
            else
            {
                // Conditional field is specified as bool but is not actually a boolean type
                Debug.LogWarning($"OnSelectionRenderAttribute: Field '{renderAttribute.ConditionalFieldName}' is not a boolean, but a boolean comparison was specified for property '{property.name}'.");
            }
        }

        m_shouldShow = conditionMet; // Cache the result for OnGUI

        if (m_shouldShow)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
        else
        {
            // Return a minimal height (or 0) to hide the field
            return -EditorGUIUtility.standardVerticalSpacing; // Consumes no vertical space
        }
    }


    // This method draws the actual property
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        OnSelectionRenderAttribute renderAttribute = (OnSelectionRenderAttribute)this.attribute;
        SerializedProperty conditionalProperty = property.serializedObject.FindProperty(renderAttribute.ConditionalFieldName);

        // This check is mainly for the initial setup or if the field somehow becomes null.
        // The GetPropertyHeight method already handles the logic for shouldShow.
        if (conditionalProperty == null)
        {
            // Draw an error box and the field if the conditional field isn't found
            EditorGUI.HelpBox(position, $"Error: Conditional field '{renderAttribute.ConditionalFieldName}' not found for property '{property.name}'.", MessageType.Error);
            EditorGUI.PropertyField(new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUI.GetPropertyHeight(property, label, true)), property, label, true);
            return;
        }

        // Use the cached m_shouldShow value from GetPropertyHeight
        if (m_shouldShow)
        {
            EditorGUI.PropertyField(position, property, label, true);
        }
        // Else: Do nothing, the field remains hidden because its height is 0.
    }
}
#endif


