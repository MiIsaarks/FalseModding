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
        public static void init()
        {
            SkillDef SlowMine = Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC1/Railgunner/RailgunnerBodyFireMineBlinding.asset").WaitForCompletion();

            SkillDef HH44 = Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC1/Railgunner/RailgunnerBodyScopeLight.asset").WaitForCompletion();

            SlowMine.baseRechargeInterval = 7f;
            SlowMine.baseMaxStock = 2;

            LanguageAPI.Add("HH44NewDescription",
               "Activate your <style=cIsUtility>short-range scope</style>, highlighting <style=cIsdamage>Weak Points</style> and transforming your weapon into a quick <style=cIsdamage>500% damage</style> railgun. Consecutive <style=cIsdamage>Weak Point</style> hits increases damage");

            HH44.skillDescriptionToken = "HH44NewDescription";

            LanguageAPI.Add("KEYWORD_CONSECUTIVE", "<style=cKeywordName>Sniper's Flow</style><style=cSub>Consecutive <style=cIsDamage>Weak Point</style> hits boost the damage of your next shot by <style=cIsdamage>100%</style> per consecutive hit up to 10 times.</style>");


            HH44.keywordTokens = new string[]
            {
                "KEYWORD_WEAKPOINT",
                "KEYWORD_CONSECUTIVE"
            };
            On.EntityStates.Railgunner.Weapon.BaseFireSnipe.OnEnter += (orig, self) =>
            {
             

                if(self is FireSnipeLight)
                {
                    self.damageCoefficient = 5f + consecutiveWeakSpotHit;

                   
                }

                if(self is FireSnipeCryo)
                {
                    self.damageCoefficient += consecutiveWeakSpotHit;
                }

                if(self is FireSnipeSuper)
                {
                    self.damageCoefficient += consecutiveWeakSpotHit;
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
    }
}
