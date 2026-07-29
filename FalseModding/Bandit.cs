using RoR2;
using RoR2.Skills;
using UnityEngine.AddressableAssets;
using R2API;
using EntityStates.Bandit2.Weapon;

namespace ForgottenSkillsTweaks
{
    public static class Bandit
    {
        public static bool isBleeding = false;
        public static void init()
        {
            On.EntityStates.Bandit2.Weapon.BaseFireSidearmRevolverState.OnEnter += (orig, self) =>
            {
                if (self is FireSidearmResetRevolver)
                {
                    isBleeding = true;
                    On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
                }
                orig(self);
             
            };

            

        }

        private static void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (isBleeding)
            {
                if (self.body && self.body.HasBuff(RoR2Content.Buffs.SuperBleed))
                {
                    // Multiply damage (e.g., 50% extra damage against Hemorrhaged targets)
                    damageInfo.damage *= 2.0f;
                    isBleeding = false;
                }

                orig(self, damageInfo);

                GlobalEventManager.instance.OnHitEnemy(damageInfo, self.gameObject);
                GlobalEventManager.instance.OnHitAll(damageInfo, self.gameObject);
            }
            else
            {
                orig(self, damageInfo);
            }
           
           
        }
       
    
    }
}
 
