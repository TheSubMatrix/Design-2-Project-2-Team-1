using System.Reflection;
using UnityEngine;

namespace CustomNamespace.Extensions
{
    /// <summary>
    /// Provides extension methods that utilize reflection.
    /// </summary>
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Gets a private value from an object using reflection
        /// This is usually bad but unity is forcing my hand
        /// </summary>
        /// <remarks>
        /// Using reflection to access private members can have performance implications and may break if the internal structure of the target object changes.
        /// Consider alternative approaches if possible.
        /// </remarks>
        /// <typeparam name="T">The type of variable that you are retrieving</typeparam>
        /// <param name="obj">object to get values from</param>
        /// <param name="name">The name of the variable you wish to retrieve</param>
        /// <returns>the value of the variable by the name passed into the method</returns>
        public static T GetFieldValue<T>(this object obj, string name)
        {
            // Set the flags so that private and public fields from instances will be found
            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            FieldInfo field = obj.GetType().GetField(name, bindingFlags);
            return (T)field?.GetValue(obj);
        }

        /// <summary>
        /// Sets the value of a private or public field on an object using reflection.
        /// This is usually bad practice as it breaks encapsulation, but may be necessary for
        /// interacting with internal Unity or third-party APIs.
        /// </summary>
        /// <remarks>
        /// Using reflection to modify private members can have performance implications and may break if the internal structure of the target object changes.
        /// Use with caution and consider alternative approaches if possible.
        /// </remarks>
        /// <typeparam name="T">The type of the value being set.</typeparam>
        /// <param name="obj">The object whose field value will be modified.</param>
        /// <param name="name">The name of the field to set.</param>
        /// <param name="value">The new value to assign to the field.</param>
        public static void SetFieldValue<T>(this object obj, string name, T value)
        {
            // Set the flags so that private and public fields from instances will be found
            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            FieldInfo field = obj.GetType().GetField(name, bindingFlags);
            if (field != null)
            {
                field.SetValue(obj, value);
            }
            else
            {
                Debug.LogError($"Field '{name}' not found on object of type '{obj.GetType().Name}'. Cannot set value using reflection.");
            }
        }
    }
}