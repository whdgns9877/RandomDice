using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : Enemy
{
    public bool isDead;

    private void OnEnable()
    {
        isDead = false;
        health = 2000;
    }

    private new void OnDisable()
    {
        base.OnDisable();
        isDead = true;
    }
}
