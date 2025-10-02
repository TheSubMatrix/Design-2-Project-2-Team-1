using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Is Null", story: "[Object] is null", category: "Variable Conditions", id: "dcd3826def5c393915f150ac1f7104b9")]
public partial class IsNullCondition : Condition
{
    [SerializeReference] public BlackboardVariable Object;

    public override bool IsTrue()
    {
        if (Object == null)
        {
            return false;
        }
        return Object.ObjectValue is null;
    }
}
