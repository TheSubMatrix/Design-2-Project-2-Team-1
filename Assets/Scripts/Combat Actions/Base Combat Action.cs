using System;
using System.Collections;
using UnityEngine;
[Serializable]
public abstract class BaseCombatAction
{
    public bool IsExecuting { get; protected set; }
    protected Coroutine CombatActionCoroutine;
    [SerializeField] protected float Duration;
    [SerializeField] protected float Cooldown;

    public abstract void InitializeCombatAction();
    public void StartCombatAction(MonoBehaviour owner)
    {
        if (IsExecuting) return;
        CombatActionCoroutine = owner.StartCoroutine(ExecuteCombatActionAsync());
    }
    IEnumerator ExecuteCombatActionAsync()
    {
        IsExecuting = true;
        yield return ExecuteCombatActionAsyncImplementation();
        yield return new WaitForSeconds(Cooldown);
        IsExecuting = false;
    }

    protected abstract IEnumerator ExecuteCombatActionAsyncImplementation();

    public void CancelCombatAction(MonoBehaviour owner)
    {
        owner.StopCoroutine(CombatActionCoroutine);
        CombatActionCoroutine = null;
        CancelCombatActionImplementation();
    }

    public virtual void CancelCombatActionImplementation()
    {
        
    }
}
