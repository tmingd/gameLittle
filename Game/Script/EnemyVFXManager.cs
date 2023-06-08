using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;
public class EnemyVFXManager : MonoBehaviour
{
    public VisualEffect FootStep; 
    public VisualEffect AttackVFX;
    public ParticleSystem BeingHitFVX;
    public VisualEffect BeingHitSplashVFX;
    public void BurstFootStep()
    {
        FootStep.SendEvent("OnPlay");
    }
    public void PlayAttackVFX()
    {
        AttackVFX.SendEvent("OnPlay");
    }
    public void PlayBeingHitVFX(Vector3 attackerPos)
    {
        Vector3 forceForward = transform.position - attackerPos;
        forceForward.Normalize();
        forceForward.y = 0;
        BeingHitFVX.transform.rotation = Quaternion.LookRotation(forceForward);
        BeingHitFVX.Play();

        Vector3 splashPos = transform.position;
        splashPos.y += 2f;
        VisualEffect newSplashVFX = Instantiate(BeingHitSplashVFX,splashPos,Quaternion.identity);
        newSplashVFX.SendEvent("onPlay");
        Destroy(newSplashVFX.gameObject,10f);
    }

}
