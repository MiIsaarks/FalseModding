using RoR2;
using RoR2.Skills;
using UnityEngine.AddressableAssets;
using R2API;
using EntityStates.Bandit2.Weapon;
using UnityEngine;

namespace ForgottenSkillsTweaks
{
    
    public static class Bandit
    {
        private static SkillLocator skillLocator = null;
        private static bool IsRevolver = false;
        public static void init()
        {
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            GlobalEventManager.onServerDamageDealt += ServerDamageDealt;

            On.EntityStates.Bandit2.Weapon.BaseFireSidearmRevolverState.OnEnter += (orig, self) =>
            {
                if (self is FireSidearmResetRevolver)
                {
                    IsRevolver = true;
                   
                    skillLocator = self.characterBody.skillLocator;
                }
                orig(self);
             
            };

        }


        

        private static void ServerDamageDealt(DamageReport damageReport)
        {
            DamageInfo damageinfo = damageReport.damageInfo;

            if (damageinfo.crit && IsRevolver)
            {
                skillLocator.GetSkill(SkillSlot.Primary).Reset();
                skillLocator.GetSkill(SkillSlot.Secondary).Reset();
                skillLocator.GetSkill(SkillSlot.Utility).Reset();
                

            }
        }

        private static void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
           
                if (self.body && self.body.HasBuff(RoR2Content.Buffs.SuperBleed))
                {
                    if (IsRevolver)
                    {
                        damageInfo.damage *= 2.5f;
                    IsRevolver = false;
                    }
                }

                orig(self, damageInfo);

                GlobalEventManager.instance.OnHitEnemy(damageInfo, self.gameObject);
                GlobalEventManager.instance.OnHitAll(damageInfo, self.gameObject);
          
        }
       
    
    }
}
 
