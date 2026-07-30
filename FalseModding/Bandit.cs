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
            SkillDef revolver = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Bandit2/ResetRevolver.asset").WaitForCompletion();

            SkillDef rifle = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Bandit2/Bandit2Blast.asset").WaitForCompletion();
            LanguageAPI.Add("NewRevolverDesc",
               "<style=cIsdamage>Slayer</style>. <style=cIsHealth>Death Marked</style>. Fire a revolver shot for <style=cIsdamage>600% damage</style>. Kills <style=cIsUtility>reset all your cooldowns</style>. Critical strikes <style=cIsUtility>reset the other skills cooldowns</style>"
               );

            LanguageAPI.Add("KEYWORD_DEATH",
                "<style=cKeywordName>Death Marked</style><style=cSub>Enemies <style=cIsHealth>hemorrhaging</style> take 2.5x more <style=cIsDamage>damage</style>, hitting a <style=cIsHealth>hemorrhaging</style> enemy guarantees <style=cIsDamage>critical strikes</style> for 2 seconds.</style>");

            LanguageAPI.Add("NewRifleDesc",
              "Fire a rifle blast for <style=cIsDamage>380% damage</style>. Can hold up to 4 bullets."
              );

            revolver.skillDescriptionToken = "NewRevolverDesc";

            rifle.skillDescriptionToken = "NewRifleDesc";

            revolver.keywordTokens = new string[]
           {
                "KEYWORD_SLAYER",
                "KEYWORD_DEATH"
           };

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
                self.maxSpread = 0.3f;
                self.spreadBloomValue = 0.3f;
                self.damageCoefficient = 3.8f;
                orig(self, bulletAttack);
            }; 

        }

        private static void ServerDamageDealt(DamageReport damageReport)
        {
           
            DamageInfo damageinfo = damageReport.damageInfo;

            bool isResetRevolver = (damageinfo.damageType.damageType & DamageType.ResetCooldownsOnKill) != DamageType.Generic;

            if (damageinfo.crit && IsRevolver && isResetRevolver)
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
            CharacterBody body = damageInfo.attacker ? damageInfo.attacker.GetComponent<CharacterBody>() : null;

            bool isResetRevolver = (damageInfo.damageType.damageType & DamageType.ResetCooldownsOnKill) != DamageType.Generic;

            if (self.body && self.body.HasBuff(RoR2Content.Buffs.SuperBleed))
            {
                if (isResetRevolver)
                {
                    damageInfo.damage *= 2.5f;
                  
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
 
