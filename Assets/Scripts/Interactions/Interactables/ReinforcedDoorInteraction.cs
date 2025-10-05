using UnityEngine;

public class ReinforcedDoorInteraction : MonoBehaviour, IDamageable
{
    private int hitCounter;

    public void Damage(uint damageToApply)
    {
        hitCounter++;
        Debug.Log(hitCounter);

        if (hitCounter == 4)
        {
            Destroy(gameObject);
        }
    }
}
