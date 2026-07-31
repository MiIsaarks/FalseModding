
using RoR2;
using RoR2.Skills;
using UnityEngine.AddressableAssets;
using R2API;


namespace ForgottenSkillsTweaks
{
    public static class BlightTweak
    {

        public static float BlightProcCoefficient = 0.5f;

        [SystemInitializer(typeof(DotController))]
        public static void init()
        {
           

            SkillDef Blightdef = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Croco/CrocoPassiveBlight.asset").WaitForCompletion();

            LanguageAPI.Add("NewBlightDesc", "Attacks that apply Poison apply stacking <style=cIsdamage>Blight</style> instead, dealing <style=cIsdamage>100% damage</style> per second. New stacks extends all stacks durations. Can activate items");

            Blightdef.skillDescriptionToken = "NewBlightDesc";

            On.RoR2.DotController.InflictDot_refInflictDotInfo += DotController_InflictDot_refInflictDotInfo;

            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        private static void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if(damageInfo.dotIndex == DotController.DotIndex.Blight && damageInfo.inflictor.GetComponent<DotController>() != null)
            {
               
                damageInfo.procCoefficient = 0.65f;
               
                damageInfo.procChainMask = default;

                CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();

                if (attackerBody)
                { 
                    damageInfo.crit = attackerBody.RollCrit();
                }


                orig(self, damageInfo);

                GlobalEventManager.instance.OnHitEnemy(damageInfo, self.gameObject);
                GlobalEventManager.instance.OnHitAll(damageInfo , self.gameObject);
            }
            else
            {
                orig(self, damageInfo);
            }
           
        }

        static void DotController_InflictDot_refInflictDotInfo(On.RoR2.DotController.orig_InflictDot_refInflictDotInfo orig, ref InflictDotInfo inflictDotInfo)
        {

            if (inflictDotInfo.dotIndex == DotController.DotIndex.Blight)
            {

                inflictDotInfo.damageMultiplier += 0.67f;
                inflictDotInfo.duration = 6f;


                if (inflictDotInfo.victimObject)
                {
                    DotController dotController = DotController.FindDotController(inflictDotInfo.victimObject);
                    if (dotController && dotController.dotStackList != null)
                    {
                        foreach (DotController.DotStack stack in dotController.dotStackList)
                        {
                            if (stack.dotIndex == DotController.DotIndex.Blight)
                            {
                                stack.timer += 2f;
                            }

                        }
                    }
                }

            }

            orig(ref inflictDotInfo);
        }
    }
}
