using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_02_Shoot : MonoBehaviour
{
    public Transform Shootingpoint;
    public GameObject DamageOrb;
    private Character cc;

    private void Awake()
    {
        cc = GetComponent<Character>();
    }
    public void ShootTheDamageOrb()
    {
        Instantiate(DamageOrb,Shootingpoint.position,Quaternion.LookRotation(Shootingpoint.forward));
    }
    private void Update()
    {
        cc.RotateToTarget();
    }
}
