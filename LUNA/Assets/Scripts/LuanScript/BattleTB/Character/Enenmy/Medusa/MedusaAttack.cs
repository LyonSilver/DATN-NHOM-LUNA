using UnityEngine;

public class MedusaAttack : EnemyAttackBase
{
    public override void AttackWithoutTiming()
    {
        base.AttackWithoutTiming();

        Debug.Log("Medusa sử dụng đòn tấn công!");
    }
}