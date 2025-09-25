using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]

public class MethodButtonAttribute : PropertyAttribute
{
    public string ButtonText { get; private set; }
    public MethodButtonAttribute()
    {
        // Default constructor uses method name as button text
    }

    // Constructor to use [Button("Custom Text")]
    public MethodButtonAttribute(string buttonText)
    {
        ButtonText = buttonText;
    }
}
