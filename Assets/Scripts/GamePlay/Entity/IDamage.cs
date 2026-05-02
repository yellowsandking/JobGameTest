using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IDamage
{
    void OnDamage(ActorBase from, float damage);
}
