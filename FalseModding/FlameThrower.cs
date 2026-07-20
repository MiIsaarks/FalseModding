using EntityStates;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static System.Net.Mime.MediaTypeNames;

namespace FalseModding
{
    public static class FlameThrowerTweak
    {
        private static float newRange = 45f;
        public static void init()
        {

        SkillDef FlameThrowerr = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Mage/MageBodyFlamethrower.asset").WaitForCompletion();

            LanguageAPI.Add("MageFlamethrowerNewDesc", "<style=cIsdamage>Ignite</style>. Burn all enemies in front of you for <style=cIsdamage>4000% damage</style>.");
            FlameThrowerr.skillDescriptionToken = "MageFlamethrowerNewDesc";


            FlameThrowerr.keywordTokens = new string[]
            {
                "KEYWORD_IGNITE"
            };

            On.EntityStates.Mage.Weapon.Flamethrower.OnEnter += (orig, self) =>
            {
                orig(self);
                self.entryDuration = 0;

                self.tickDamageCoefficient *= 2f;

                EntityStates.Mage.Weapon.Flamethrower.ignitePercentChance = 100f;

                self.maxDistance = newRange;

                float scaleRatio = newRange / 16f;

                ScaleFlamethrowerVisual(self.leftFlamethrowerEffectInstance, scaleRatio);
                ScaleFlamethrowerVisual(self.rightFlamethrowerEffectInstance, scaleRatio);

            };
        }


        private static void ScaleFlamethrowerVisual(GameObject effectInstance, float scaleRatio)
        {
            if (effectInstance == null) return;

            ParticleSystem[] particleSystems = effectInstance.GetComponentsInChildren<ParticleSystem>();

            foreach (ParticleSystem ps in particleSystems)
            {
                string psName = ps.gameObject.name;

                // 1. SKIP the hand orb/charge effect entirely
                if (psName == "IcoCharge")
                {
                    continue;
                }

                var main = ps.main;

                // 2. Extend the travel distance/speed of the flame particles
                main.startSpeedMultiplier *= scaleRatio;

                // 3. Scale up the particle size for the actual flame stream
                if (psName == "FireForward" || psName == "Billboard")
                {
                    main.startSizeMultiplier *= 1.5f; // Adjust this multiplier until thickness looks right
                }
            }

        }

      
    }
}
