
using R2API;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace FalseModding
{
    public static class ArrowRainTweak
    {
        public static void init()
        {
            float newRadius = 15f;

            SkillDef HuntressRain = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Huntress/HuntressBodyArrowRain.asset").WaitForCompletion();

            SkillDef HuntressBlink = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Huntress/HuntressBodyBlink.asset").WaitForCompletion();

            HuntressBlink.baseRechargeInterval = 4.5f;

            HuntressRain.baseRechargeInterval = 10f;

            LanguageAPI.Add("HuntressArrowRainDescription",
                "<style=cIsUtility>Teleport</style> into the sky. Target an area to rain arrows, <style=cIsUtility>slowing</style> all enemies and dealing <style=cIsDamage>660% damage</style> per second.");

            HuntressRain.skillDescriptionToken = "HuntressArrowRainDescription";

            On.EntityStates.Huntress.ArrowRain.OnEnter += (orig, self) =>
            {
                orig(self);
                self.areaIndicatorInstance.transform.localScale = new Vector3(newRadius, 150, newRadius);

                EntityStates.Huntress.ArrowRain.projectilePrefab.GetComponent<ProjectileDotZone>().lifetime = 7f;

                EntityStates.Huntress.ArrowRain.projectilePrefab.GetComponent<ProjectileDotZone>().resetFrequency = 4.0f;

                EntityStates.Huntress.ArrowRain.projectilePrefab.GetComponent<ProjectileDotZone>().transform.localScale = new Vector3(newRadius, 150, newRadius)*2f;

                EntityStates.Huntress.ArrowRain.projectilePrefab.GetComponent<ProjectileDotZone>().overlapProcCoefficient = 0.7f;

                EntityStates.Huntress.ArrowRain.projectilePrefab.GetComponent<ProjectileDotZone>().damageCoefficient = 0.75f;


            };

        
        }
    }
}
