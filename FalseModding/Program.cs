using BepInEx;
using RoR2;
using EntityStates;
using EntityStates.FalseSon;
using UnityEngine;
using RoR2.Projectile;
using RoR2.Skills;
using RoR2.Networking;

namespace FalseSonTweak
{
    [BepInPlugin("com.YourName.FalseSonDamageScaling", "FalseSonDamageScaling", "1.0.0")]
    public class MainPlugin : BaseUnityPlugin
    {
        private static float? originalDamage = null;
        public void Awake()
        {
          
            On.RoR2.FalseSonController.GetGrowthLaserBonusDuration += (orig, self) =>
            {
                return 0f;
            };

            On.EntityStates.FalseSon.LaserFatherCharged.OnEnter += (orig, self) =>
            {
                orig(self);

                EntityStates.FalseSon.LaserFatherCharged.procCoefficientPerTick = 0.7f;

                SkillLocator skillLocator = self.characterBody.skillLocator;
                self.spikeRefillAmountPerSecond = 0f;

                if (skillLocator != null)
                {
                    
                float num = skillLocator.GetSkill(SkillSlot.Secondary).maxStock;
                float num2 = skillLocator.GetSkill(SkillSlot.Secondary).stock;
                int num3 = (int)(num * 0.5f);
                self.skillLocator.GetSkill(SkillSlot.Secondary).stock = (int)Mathf.Clamp(num2 + (float)num3, num2, num);
            }


            if (originalDamage == null) 
                {
                    originalDamage = EntityStates.FalseSon.LaserFatherCharged.damageCoefficient;
                }
           
                var growthController = self.characterBody.GetComponent<RoR2.FalseSonController>();
                if (growthController != null)
                {
                    int currentGrowth = growthController.growthLevel;

                   
                    float damageMultiplier = 1f + (currentGrowth * 0.05f);

                    
                    EntityStates.FalseSon.LaserFatherCharged.damageCoefficient =originalDamage.Value * damageMultiplier;
                }

               
               
            };

           
        }
    }
}
