using UnityEngine;

public class MonsterAttackAnim : MonoBehaviour
{
    public void Attack()
    {
        GetComponentInParent<IMonster>().Attack();
    }
}
