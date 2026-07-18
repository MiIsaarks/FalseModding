using BepInEx;
using EntityStates;
using EntityStates.FalseSon;
using R2API.Utils;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.UIElements;

namespace FalseSonTweak
{
    [BepInPlugin("com.YourName.FalseSonDamageScaling", "FalseSonDamageScaling", "1.0.0")]
    public class MainPlugin : BaseUnityPlugin
    {
        private static float? originalDamage = null;
        public void Awake()
        {
            GameObject Lightning = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/FalseSon/LunarStakeLightningStrikeImpactEffect.prefab").WaitForCompletion();

            On.RoR2.BulletAttack.InitBulletHitFromRaycastHit += (orig, self, ref bullethit, ray, ref raycasthit) =>
            {
               

                orig(self, ref bullethit, ray, ref raycasthit);

                if (BulletAttack.IgnoreAlliesFilter(self, ref bullethit))
                {
                    GameObject obj = GameObject.Instantiate(Lightning, bullethit.entityObject.transform.position, raycasthit.transform.rotation);
                   
                    NetworkServer.Spawn(obj);
                }
            };

            On.EntityStates.FalseSon.LaserFather.OnEnter += (orig, self) =>
            {
                self.baseChargeDuration = 0.5f;
                orig(self);
            };

            SkillDef FalseSonLaserF = Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC2/FalseSon/FalseSonBodyLaser.asset").WaitForCompletion();

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
