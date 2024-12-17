using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMonster
{
    public void Attack();

    public void TargetSet(Transform target);
}
