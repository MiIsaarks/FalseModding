using EntityStates.Railgunner.Weapon;
using R2API;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.AddressableAssets;

namespace ForgottenSkillsTweaks
{
    public static class RailGunner
    {
        private static float consecutiveWeakSpotHit = 0;
        private static SkillDef SlowMine;
        private static SkillDef HH44;

        private static string originalHH44Desc;
        private static string[] originalHH44Desc2;
        public static void init()
        {
             SlowMine = Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC1/Railgunner/RailgunnerBodyFireMineBlinding.asset").WaitForCompletion();

             HH44 = Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC1/Railgunner/RailgunnerBodyScopeLight.asset").WaitForCompletion();

            originalHH44Desc = HH44.skillDescriptionToken;
            originalHH44Desc2 = HH44.keywordTokens;

           

            LanguageAPI.Add("HH44NewDescription",
               "Activate your <style=cIsUtility>short-range scope</style>, highlighting <style=cIsdamage>Weak Points</style> and transforming your weapon into a quick <style=cIsdamage>500% damage</style> railgun. Consecutive <style=cIsdamage>Weak Point</style> hits increases damage");

            

            LanguageAPI.Add("KEYWORD_CONSECUTIVE", "<style=cKeywordName>Sniper's Flow</style><style=cSub>Consecutive <style=cIsDamage>Weak Point</style> hits boost the damage of your next shot by <style=cIsdamage>100%</style> per consecutive hit up to 10 times.</style>");

            config.OnConfigChanged += UpdateSkills;

          
            On.EntityStates.Railgunner.Weapon.BaseFireSnipe.OnEnter += (orig, self) =>
            {

                if (config.HH44.Value)
                {
                    if (self is FireSnipeLight)
                    {
                        self.damageCoefficient = 5f + consecutiveWeakSpotHit;


                    }

                    if (self is FireSnipeCryo)
                    {
                        self.damageCoefficient += consecutiveWeakSpotHit;
                    }

                    if (self is FireSnipeSuper)
                    {
                        self.damageCoefficient += consecutiveWeakSpotHit;
                    }
                }
       
                orig(self);
            };

            BaseFireSnipe.onWeakPointHit += (damageInfo) =>
            {
                
                if (consecutiveWeakSpotHit < 10)
                {
                    consecutiveWeakSpotHit++;
                }
            };

            BaseFireSnipe.onWeakPointMissed += () =>
            {
               
                consecutiveWeakSpotHit = 0;
            };

         
        }

        private static void UpdateSkills()
        {
            if (config.HH44.Value)
            {
                HH44.skillDescriptionToken = "HH44NewDescription";
                HH44.keywordTokens = new string[]
                {
                    "KEYWORD_WEAKPOINT",
                    "KEYWORD_CONSECUTIVE"
                };
            }
            else
            {
                HH44.skillDescriptionToken = originalHH44Desc;
                HH44.keywordTokens = originalHH44Desc2;
            }
            if (config.PolarMines.Value)
            {
                SlowMine.baseRechargeInterval = 8f;
                SlowMine.baseMaxStock = 2;

            }
            else
            {
                SlowMine.baseRechargeInterval = 12f;
                SlowMine.baseMaxStock = 1;
            }
        }
    }
}
