using BepInEx;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;
using R2API;




namespace ForgottenSkillsTweaks
{
    [BepInPlugin("com.MiIsaarks.ForgottenSkillsTweaks", "ForgottenSkillsTweaks", "0.4.0")]
    [BepInDependency(R2API.LanguageAPI.PluginGUID)]
    [BepInDependency(R2API.DotAPI.PluginGUID)]
    [BepInDependency("com.rune580.riskofoptions")]
    public class MainPlugin : BaseUnityPlugin
    {
        private static int hitsToLightning = 10;
        private static int hitcount = 0;
        private static float? originalDamage = null;
        private static SkillDef FalseSonLaserF;
        private static string OriginalFalseSonLaserDesc;
        private static string[] OriginalFalseSonLaserDesc2;

        private static bool light = false;
        public void Awake()
        {
            config.init(Config);
            FlameThrowerTweak.init();
            ArrowRainTweak.init();
            BlightTweak.init();
            RailGunner.init();
            Bandit.init();

            GameObject Lightning = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/FalseSon/LunarStakeLightningStrikeImpactEffect.prefab").WaitForCompletion();

            On.EntityStates.FalseSon.LaserFatherCharged.FireBullet += (orig, self, a, b, c, d) =>
            {
                light = true;
                orig(self, a, b, c, d);
                light = false;
            };

            On.RoR2.BulletAttack.DefaultHitCallbackImplementation += (orig, self, ref bullethit) =>
            {

                bool b = orig(self, ref bullethit);


                if (!light)
                {
                    return b;
                }
                if (bullethit.entityObject.GetComponent<HealthComponent>() != null && config.LaserFather.Value)
                {
                    hitcount++;
                    if (hitcount % hitsToLightning == 0)
                    {
                        GameObject obj = GameObject.Instantiate(Lightning, bullethit.entityObject.transform.position, Quaternion.identity);

                        BlastAttack blastAttack = new BlastAttack
                        {
                            attacker = self.owner,
                            position = bullethit.point,
                            inflictor = obj,
                            baseDamage = self.damage * 3.5f,
                            crit = self.isCrit,
                            damageColorIndex = DamageColorIndex.Default,
                            damageType = DamageTypeExtended.Electrical,
                            radius = 6f,
                            procCoefficient = 1f,
                            falloffModel = BlastAttack.FalloffModel.None,
                            teamIndex = TeamComponent.GetObjectTeam(self.owner)
                        };
                        blastAttack.Fire();
                    }
                }
                return b;
            };

            On.EntityStates.FalseSon.LaserFather.OnEnter += (orig, self) =>
            {
                if (config.LaserFather.Value)
                {
                    self.baseChargeDuration = 0.5f;
                }
                orig(self);
            };

            FalseSonLaserF = Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC2/FalseSon/FalseSonBodyLaser.asset").WaitForCompletion();

            OriginalFalseSonLaserDesc = FalseSonLaserF.skillDescriptionToken;

            OriginalFalseSonLaserDesc2 = FalseSonLaserF.keywordTokens;

            LanguageAPI.Add("FalseSonLaserDescriptionNew",
                "Charge a rapidly hitting laser dealing <style=cIsDamage>320%-1890% damage</style> for <style=cIsUtility>4s</style> and refill <style=cIsUtility>Lunar Spikes</style>. " +
                "Increases in damage through <style=cIsHealing>Growth</style>. Summons <style=cIsDamage>Lightning</style> on repeated hits.");

           


            LanguageAPI.Add("KEYWORD_LASER_LIGHTNING",
                "<style=cKeywordName>Brother's Lightning</style><style=cSub>Every <style=cIsUtility>10 hits</style> with the laser, call down a lightning strike dealing <style=cIsDamage>350% </style>of the laser's damage.</style>");



            config.OnConfigChanged += UpdateSkills;

            UpdateSkills();

            On.RoR2.FalseSonController.GetGrowthLaserBonusDuration += (orig, self) =>
            {
                if (config.LaserFather.Value)
                {
                    return 0f;
                }
                return orig(self);
            };

            On.RoR2.BulletAttack.Fire += (orig, self) =>
            {
                if (self.owner != null && config.LaserFather.Value)
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

                    if (config.LaserFather.Value)
                    {
                        EntityStates.FalseSon.LaserFatherCharged.procCoefficientPerTick = 0.7f;

                        self.spikeRefillAmountPerSecond = 0f;
                    }
                    else
                    {
                        EntityStates.FalseSon.LaserFatherCharged.procCoefficientPerTick = 0.45f;
                       
                    }
                    float recalc = (self.characterBody.attackSpeed - self.characterBody.baseAttackSpeed) * 0.3f;
                    self.fireFrequency = EntityStates.FalseSon.LaserFatherCharged.baseFireFrequency * (1 + recalc);

                    hitcount = 1;

                    SkillLocator skillLocator = self.characterBody.skillLocator;
                   
                    if (skillLocator != null && config.LaserFather.Value)
                    {

                        float num = skillLocator.GetSkill(SkillSlot.Secondary).maxStock;
                        float num2 = skillLocator.GetSkill(SkillSlot.Secondary).stock;
                        int num3 = (int)(num * 0.5f);
                        self.skillLocator.GetSkill(SkillSlot.Secondary).stock = (int)Mathf.Clamp(num2 + (float)num3, num2, num);
                    }


                    if (originalDamage == null)
                    {
                        originalDamage = EntityStates.FalseSon.LaserFatherCharged.damageCoefficient * 1.35f;
                    }

                    var growthController = self.characterBody.GetComponent<RoR2.FalseSonController>();
                    if (growthController != null && config.LaserFather.Value)
                    {
                        int currentGrowth = growthController.growthLevel;


                        float damageMultiplier = 1f + (currentGrowth * 0.05f);


                        EntityStates.FalseSon.LaserFatherCharged.damageCoefficient = originalDamage.Value * damageMultiplier;
                    }
                    else
                    {
                        EntityStates.FalseSon.LaserFatherCharged.damageCoefficient = originalDamage.Value/1.35f;
                    }



                };
            }
            

        

        private static void UpdateSkills()
        {
            if (config.LaserFather.Value)
            {
                FalseSonLaserF.baseRechargeInterval = 10f;

                FalseSonLaserF.skillDescriptionToken = "FalseSonLaserDescriptionNew";

                FalseSonLaserF.keywordTokens = new string[]
                {
                    "KEYWORD_GROWTH",
                    "KEYWORD_LASER_LIGHTNING"
                };
            }
            else
            {
                FalseSonLaserF.baseRechargeInterval = 15f;

                FalseSonLaserF.skillDescriptionToken = OriginalFalseSonLaserDesc;

                FalseSonLaserF.keywordTokens = OriginalFalseSonLaserDesc2;
            }
        }
    }
}