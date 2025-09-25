using System;
using UnityEngine;

/// <summary>
/// An attribute to store the required information for <see cref="SubclassListPropertyDrawer"/>.
/// This custom selector will allow the user to select any given subclass from a dropdown
/// and render its serialized fields below the selector
/// </summary>
public class SubclassListAttribute : PropertyAttribute
{
    public Type Type { get; }

    public SubclassListAttribute(System.Type type)
    {
        this.Type = type;
    }
}
