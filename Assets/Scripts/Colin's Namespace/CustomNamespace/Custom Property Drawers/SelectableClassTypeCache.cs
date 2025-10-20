using System;
using System.Collections.Generic;
using System.Linq;
using CustomNamespace.Extensions;
using UnityEditor;
using UnityEditor.Compilation;

public static class SelectableClassTypeCache
{
    static Dictionary<Type, List<Type>> derivedTypesCache;
    static Dictionary<Type, Type> propertyDrawerCache;
    static Dictionary<Type, bool> drawerUseForChildrenCache;
    
    [InitializeOnLoadMethod]
    static void Initialize()
    {
        CompilationPipeline.compilationFinished += OnCompilationFinished;
        RebuildCache();
    }
    
    static void OnCompilationFinished(object obj)
    {
        RebuildCache();
    }
    
    static void RebuildCache()
    {
        derivedTypesCache = new Dictionary<Type, List<Type>>();
        propertyDrawerCache = new Dictionary<Type, Type>();
        drawerUseForChildrenCache = new Dictionary<Type, bool>();
        
        // Cache all property drawers
        IEnumerable<Type> allDrawers = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(ass => ass.GetTypes())
            .Where(t => t.IsSubclassOf(typeof(PropertyDrawer)));
            
        foreach (Type drawer in allDrawers)
        {
            foreach (Attribute attr in Attribute.GetCustomAttributes(drawer))
            {
                if (attr is not CustomPropertyDrawer cpd) continue;
                Type targetType = cpd.GetFieldValue<Type>("m_Type");
                if (targetType == null || !propertyDrawerCache.TryAdd(targetType, drawer)) continue;
                drawerUseForChildrenCache[targetType] = cpd.GetFieldValue<bool>("m_UseForChildren");
            }
        }
    }
    
    public static List<Type> GetDerivedTypes(Type baseType, bool includeBaseType = false)
    {
        if (derivedTypesCache == null)
            RebuildCache();

        if (derivedTypesCache != null && derivedTypesCache.TryGetValue(baseType, out List<Type> types)) 
        {
            // If we need to include the base type, and it's not abstract, add it
            if (includeBaseType && !baseType.IsAbstract && !types.Contains(baseType))
            {
                types = new List<Type>(types) { baseType };
            }
            return types;
        }
        
        // Support both classes and interfaces
        if (baseType.IsInterface)
        {
            // For interfaces, find all non-abstract classes that implement the interface
            types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(ass => ass.GetTypes())
                .Where(t => baseType.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface && t != baseType)
                .ToList();
        }
        else
        {
            // For classes, find all non-abstract derived types
            types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(ass => ass.GetTypes())
                .Where(t => baseType.IsAssignableFrom(t) && !t.IsAbstract && t != baseType)
                .ToList();
                
            // Include the base type if it's not abstract and includeBaseType is true
            if (includeBaseType && !baseType.IsAbstract)
            {
                types.Add(baseType);
            }
        }
        
        if (derivedTypesCache != null) 
            derivedTypesCache[baseType] = types;
        
        return types;
    }
    
    public static Type GetPropertyDrawerForType(Type targetType)
    {
        if (propertyDrawerCache == null)
            RebuildCache();
    
        // Direct match
        if (propertyDrawerCache != null && propertyDrawerCache.TryGetValue(targetType, out Type drawer))
        {
            return drawer;
        }
    
        // Check parent types (for class inheritance)
        for (Type parentType = targetType.BaseType; parentType != null; parentType = parentType.BaseType)
        {
            if (propertyDrawerCache == null || !propertyDrawerCache.TryGetValue(parentType, out drawer))
                continue;

            if (drawerUseForChildrenCache.TryGetValue(parentType, out bool parentUseForChildren) && parentUseForChildren)
            {
                return drawer;
            }
        }
        
        // Check interfaces (for interface implementation)
        Type[] interfaces = targetType.GetInterfaces();
        foreach (Type interfaceType in interfaces)
        {
            if (propertyDrawerCache == null || !propertyDrawerCache.TryGetValue(interfaceType, out drawer))
                continue;

            if (drawerUseForChildrenCache.TryGetValue(interfaceType, out bool interfaceUseForChildren) && interfaceUseForChildren)
            {
                return drawer;
            }
        }
    
        return null;
    }
    
    public static Type GetDrawerTargetType(Type drawerType)
    {
        if (drawerType == null) return null;
        
        foreach (Attribute attr in Attribute.GetCustomAttributes(drawerType))
        {
            if (attr is CustomPropertyDrawer cpd)
            {
                return cpd.GetFieldValue<Type>("m_Type");
            }
        }
        return null;
    }
}