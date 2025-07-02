using UnityEngine;
using System.Collections;

public class TowerAnimationResponder : MonoBehaviour
{
    Tower tower;

    public void SetParentTower(Tower parentTower)
    {
        tower = parentTower;
    }

    public void FireWeapon(int fireIndex)
    {
        if (tower == null)
            return;

        var weapon = tower.weapon;
        if (fireIndex < 0)
        {
            //fire all weapons at the same time. eg multiple lasers
            for (int i = 0; i < weapon.fireLocations.Count; ++i)
                weapon.OnFire(i);
        }
        else if (fireIndex < weapon.fireLocations.Count)
        {
            //fire specific weapon. eg staggering rockets
            weapon.OnFire(fireIndex);
        }
    }

    public void AnimEnd()
    {
        if (tower != null)
            tower.OnAnimEnd();
    }

    public void PlaySpawnPFX()
    {
        //TODO: i think this is legacy...
        if (tower != null)
            tower.pfx.Play(PFX.Tower_OnSpawn, true);
    }

    public void PlayEffect()
    {
        //suppress warnings. the ninja tower fires off the events but they arent needed
    }
}
