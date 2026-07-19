using BepInEx;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;
using R2API; 
    

namespace FalseSonTweak
{
    [BepInPlugin("com.YourName.FalseSonDamageScaling", "FalseSonDamageScaling", "1.0.0")]
    public class MainPlugin : BaseUnityPlugin
    {
        private static int hitsToLightning = 10;
        private static int hitcount = 0;
        private static float? originalDamage = null;

        private static bool light = false;
        public void Awake()
        {
            GameObject Lightning = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/FalseSon/LunarStakeLightningStrikeImpactEffect.prefab").WaitForCompletion();

            On.EntityStates.FalseSon.LaserFatherCharged.FireBullet += (orig, self, a, b, c, d) =>
            {
                light = true;
                orig(self, a, b, c, d);
                light = false;
            };

            On.RoR2.BulletAttack.DefaultHitCallbackImplementation += (orig, self,ref bullethit) =>
            {
    
             bool b =   orig(self, ref bullethit);
              

                if (!light)
                {
                    return b;
                }
                if (bullethit.entityObject.GetComponent<HealthComponent>() != null)
                {
                    hitcount++;
                    if (hitcount % hitsToLightning == 0)
                    {
                        GameObject obj = GameObject.Instantiate(Lightning, bullethit.entityObject.transform.position, Quaternion.identity);

                        bullethit.entityObject.GetComponent<HealthComponent>().TakeDamage(new DamageInfo
                        {
                            damage = self.damage * 3,
                            position = bullethit.point,
                            force = Vector3.zero,
                            attacker = self.owner,
                            inflictor = obj,
                            crit = self.isCrit,
                            damageColorIndex = DamageColorIndex.Default,
                            damageType = DamageType.Generic,
                            procCoefficient = 1f
                        });

                    }
                }
                return b;
            };

            On.EntityStates.FalseSon.LaserFather.OnEnter += (orig, self) =>
            {
                self.baseChargeDuration = 0.5f;
                orig(self);
            };

            SkillDef FalseSonLaserF = Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC2/FalseSon/FalseSonBodyLaser.asset").WaitForCompletion();

            LanguageAPI.Add("FalseSonLaserDescriptionNew", "My laser is goated and peak");

            FalseSonLaserF.skillDescriptionToken = "FalseSonLaserDescriptionNew";

            FalseSonLaserF.baseRechargeInterval = 10f;

            On.RoR2.FalseSonController.GetGrowthLaserBonusDuration += (orig, self) =>
            {
                return 0f;
            };

            On.RoR2.BulletAttack.Fire += (orig, self) =>
            {
                if (self.owner != null)
                {
                 
                    CharacterBody body = self.owner.GetComponent<CharacterBody>();
                    if (body != null && body.bodyIndex == BodyCatalog.FindBodyIndex("FalseSonBody"))
                    {
                       
                        EntityStateMachine stateMachine = EntityStateMachine.FindByCustomName(self.owner, "Weapon");
                        if (stateMachine != null && stateMachine.state is EntityStates.FalseSon.LaserFatherCharged)
                        {
                     
                            self.falloffModel = BulletAttack.FalloffModel.None;

                        }
                    }
                }
               
                orig(self);
            };

            On.EntityStates.FalseSon.LaserFatherCharged.OnEnter += (orig, self) =>
            {
                orig(self);

                hitcount = 1;

                EntityStates.FalseSon.LaserFatherCharged.procCoefficientPerTick = 0.7f;

                SkillLocator skillLocator = self.characterBody.skillLocator;
                self.spikeRefillAmountPerSecond = 0f;

                if (skillLocator != null)
                {
                    
                float num = skillLocator.GetSkill(SkillSlot.Secondary).maxStock;
                float num2 = skillLocator.GetSkill(SkillSlot.Secondary).stock;
                int num3 = (int)(num * 0.6f);
                self.skillLocator.GetSkill(SkillSlot.Secondary).stock = (int)Mathf.Clamp(num2 + (float)num3, num2, num);
            }


            if (originalDamage == null) 
                {
                    originalDamage = EntityStates.FalseSon.LaserFatherCharged.damageCoefficient*1.5f;
                }
           
                var growthController = self.characterBody.GetComponent<RoR2.FalseSonController>();
                if (growthController != null)
                {
                    int currentGrowth = growthController.growthLevel;

                   
                    float damageMultiplier = 1f + (currentGrowth * 0.07f);

                    
                    EntityStates.FalseSon.LaserFatherCharged.damageCoefficient =originalDamage.Value * damageMultiplier;
                }

               
               
            };

        }
    }
}
