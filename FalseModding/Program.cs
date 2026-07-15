using BepInEx;
using RoR2;
using EntityStates;
using UnityEngine;

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
                if(originalDamage == null) 
                {
                    originalDamage = EntityStates.FalseSon.LaserFatherCharged.damageCoefficient;
                }
           
                var growthController = self.characterBody.GetComponent<RoR2.FalseSonController>();
                if (growthController != null)
                {
                    int currentGrowth = growthController.growthLevel;

                   
                    float damageMultiplier = 1f + (currentGrowth * 0.1f);

                    
                    EntityStates.FalseSon.LaserFatherCharged.damageCoefficient =originalDamage.Value * damageMultiplier;
                }

               
                orig(self);
            };

           
        }
    }
}
