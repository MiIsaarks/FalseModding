using EntityStates;
using RoR2;
using UnityEngine;


namespace FalseModding
{
    public static class FlameThrowerTweak
    {
        private static float newRange = 45f;
        public static void init()
        {
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
                var main = ps.main;
                main.startSpeedMultiplier *= scaleRatio;
            }
           
        }
    }
}
