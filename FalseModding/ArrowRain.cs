
using R2API;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ForgottenSkillsTweaks
{
    public static class ArrowRainTweak
    {
        private static SkillDef Rain;
        private static SkillDef Blink;

        private static string OriginalRainDesc;
        public static void init()
        {
            float newRadius = 13f;

            Rain = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Huntress/HuntressBodyArrowRain.asset").WaitForCompletion();

            Blink = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Huntress/HuntressBodyBlink.asset").WaitForCompletion();

            OriginalRainDesc = Rain.skillDescriptionToken;

            LanguageAPI.Add("HuntressArrowRainDescription",
                "<style=cIsUtility>Teleport</style> into the sky. Target an area to rain arrows, <style=cIsUtility>slowing</style> all enemies and dealing <style=cIsDamage>660% damage</style> per second.");

            config.OnConfigChanged += UpdateSkills;

            UpdateSkills();



            On.EntityStates.Huntress.ArrowRain.OnEnter += (orig, self) =>
            {
                orig(self);

                if (config.ArrowRain.Value)
                {
                    self.areaIndicatorInstance.transform.localScale = new Vector3(newRadius, 150, newRadius);

                    EntityStates.Huntress.ArrowRain.projectilePrefab.GetComponent<ProjectileDotZone>().lifetime = 7f;

                    float recalc = (self.characterBody.attackSpeed - self.characterBody.baseAttackSpeed) * 0.5f;

                    EntityStates.Huntress.ArrowRain.projectilePrefab.GetComponent<ProjectileDotZone>().resetFrequency = 4f * (1 + recalc);

                    EntityStates.Huntress.ArrowRain.projectilePrefab.GetComponent<ProjectileDotZone>().transform.localScale = new Vector3(newRadius, 150, newRadius) * 2f;

                    EntityStates.Huntress.ArrowRain.projectilePrefab.GetComponent<ProjectileDotZone>().overlapProcCoefficient = 0.7f;

                    EntityStates.Huntress.ArrowRain.projectilePrefab.GetComponent<ProjectileDotZone>().damageCoefficient = 0.75f;


                }
            };
        }

        private static void UpdateSkills()
        {
            if (config.ArrowRain.Value)
            {
                Rain.baseRechargeInterval = 11f;

                Rain.skillDescriptionToken = "HuntressArrowRainDescription";
            }
            else
            {
                Rain.baseRechargeInterval = 12f;

                Rain.skillDescriptionToken = OriginalRainDesc;
            }
            if (config.Blink.Value)
            {
                Blink.baseRechargeInterval = 4.5f;
            }
            else
            {
                Blink.baseRechargeInterval = 7f;
            }
        }
    }
}
