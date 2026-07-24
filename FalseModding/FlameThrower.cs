
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ForgottenSkillsTweaks
{
    public static class FlameThrowerTweak
    {
        private static float newRange = 45f;
        private static float jumpDuration = 0.5f;
        private static float baseSpeedMultiplier = 3.5f;
        private static float gravityDelay = 0.5f;

        private static readonly AnimationCurve speedCurve = new AnimationCurve(
             new Keyframe(0f, 1f),   
             new Keyframe(0.7f, 0.5f), 
             new Keyframe(1f, 0f)      
         );
        public static void init()
        { 

        SkillDef FlameThrowerr = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Mage/MageBodyFlamethrower.asset").WaitForCompletion();

            LanguageAPI.Add("MageFlamethrowerNewDesc", "<style=cIsdamage>Ignite</style>. Burn all enemies in front of you for <style=cIsdamage>5000% damage</style>.");
            FlameThrowerr.skillDescriptionToken = "MageFlamethrowerNewDesc";


            FlameThrowerr.keywordTokens = new string[]
            {
                "KEYWORD_IGNITE"
            };

            On.EntityStates.Mage.Weapon.Flamethrower.OnEnter += (orig, self) =>
            {
                orig(self);
                self.entryDuration = 0.3f;

                self.tickDamageCoefficient *= 2.5f;

                EntityStates.Mage.Weapon.Flamethrower.ignitePercentChance = 100f;

                self.maxDistance = newRange;

                float scaleRatio = newRange / 16f;

                if(self.characterMotor != null)
                {
                    self.characterMotor.Motor.ForceUnground(jumpDuration);

                    self.characterMotor.velocity.y = 0f;

                }

                ScaleFlamethrowerVisual(self.leftFlamethrowerEffectInstance, scaleRatio);
                ScaleFlamethrowerVisual(self.rightFlamethrowerEffectInstance, scaleRatio);

            };

            On.EntityStates.Mage.Weapon.Flamethrower.FixedUpdate += (orig, self) =>
            {
                orig(self);

             
                if (self.fixedAge <= jumpDuration && self.characterMotor != null && self.characterBody != null)
                {
                   
                    float normalizedTime = self.fixedAge / jumpDuration;

                   
                    float curveValue = speedCurve.Evaluate(normalizedTime);

                   
                    float currentSpeed = self.characterBody.moveSpeed * baseSpeedMultiplier * curveValue;

                  
                    self.characterMotor.rootMotion += Vector3.up * (currentSpeed * Time.fixedDeltaTime);

                   
                    self.characterMotor.velocity.y = 0f;
                }
                else if(self.fixedAge <= gravityDelay + jumpDuration)
                {
                    self.characterMotor.velocity.y = Mathf.Max(self.characterMotor.velocity.y, 0f);
                }
            };
        }


        private static void ScaleFlamethrowerVisual(GameObject effectInstance, float scaleRatio)
        {
            if (effectInstance == null) return;

            ParticleSystem[] particleSystems = effectInstance.GetComponentsInChildren<ParticleSystem>();

            foreach (ParticleSystem ps in particleSystems)
            {
                string psName = ps.gameObject.name;

                if (psName == "IcoCharge")
                {
                    continue;
                }

                var main = ps.main;

                main.startSpeedMultiplier *= scaleRatio;

                if (psName == "FireForward" || psName == "Billboard")
                {
                    main.startSizeMultiplier *= 1.5f; 
                }
            }

        }

      
    }
}
