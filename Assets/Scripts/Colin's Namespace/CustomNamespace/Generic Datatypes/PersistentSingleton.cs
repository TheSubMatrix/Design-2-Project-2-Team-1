using CustomNamespace.GenericDatatypes;
using UnityEngine;

public class PersistentSingleton<T> : Singleton<T> where T : Component
{
    protected override void InitializeSingleton()
    {
        base.InitializeSingleton();
        DontDestroyOnLoad(gameObject);
    }
}
