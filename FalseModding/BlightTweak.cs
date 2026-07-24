using HarmonyLib;
using RoR2;
using System;
using static RoR2.Console;

namespace ForgottenSkillsTweaks
{
    public static class BlightTweak
    {
        [SystemInitializer]
        public static void init()
        {
            On.RoR2.DotController.InflictDot_refInflictDotInfo += DotController_InflictDot_refInflictDotInfo;

        }

        static void DotController_InflictDot_refInflictDotInfo(On.RoR2.DotController.orig_InflictDot_refInflictDotInfo orig, ref InflictDotInfo inflictDotInfo)
        {
          
            if (inflictDotInfo.dotIndex == DotController.DotIndex.Blight)
            {
    
                inflictDotInfo.damageMultiplier += 0.7f;
                inflictDotInfo.duration = 6f;

                if (inflictDotInfo.victimObject)
                {
                    DotController dotController = DotController.FindDotController(inflictDotInfo.victimObject);
                    if(dotController && dotController.dotStackList != null)
                    {
                        foreach(DotController.DotStack stack in dotController.dotStackList)
                        {
                            if(stack.dotIndex == DotController.DotIndex.Blight)
                            {
                                stack.timer += 1.5f;
                            }
                            
                        }
                    }
                }
                
            }
            

            orig(ref inflictDotInfo);
        }
    }
}
