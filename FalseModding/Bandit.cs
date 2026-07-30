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

                }
                else
                {
                    IsRevolver = false;
                }
                orig(self);
             
            };

            On.EntityStates.Bandit2.Weapon.Bandit2FireRifle.ModifyBullet += (orig, self, bulletAttack) =>
            {
               
                self.minSpread = 0f;
                self.maxSpread = 0.2f;
                self.spreadBloomValue = 0.2f;
                self.damageCoefficient = 3.8f;
                orig(self, bulletAttack);
            }; 

        }

        private static void ServerDamageDealt(DamageReport damageReport)
        {
           
            DamageInfo damageinfo = damageReport.damageInfo;
           
            
            if (damageinfo.crit && IsRevolver && damageinfo.damageType.damageSource == DamageSource.Special)
            {
                CharacterBody body = damageReport.attackerBody;
                if(body && body.skillLocator)
                {
                   SkillLocator skillLocator = body.skillLocator;

                    skillLocator.GetSkill(SkillSlot.Primary).Reset();
                    skillLocator.GetSkill(SkillSlot.Secondary).Reset();
                    skillLocator.GetSkill(SkillSlot.Utility).Reset();
                } 
               
            }
        }

        private static void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            CharacterBody body = damageInfo.attacker.GetComponent<CharacterBody>();
            
            if (self.body && self.body.HasBuff(RoR2Content.Buffs.SuperBleed))
            {
                if (IsRevolver && damageInfo.damageType.damageSource == DamageSource.Special)
                {
                    damageInfo.damage *= 3f;
                  
                   if (body)
                   {
                       body.AddTimedBuff(RoR2Content.Buffs.FullCrit, 2f);
                   }
                }
            }

            orig(self, damageInfo);

        }
       
    
    }
}
 
