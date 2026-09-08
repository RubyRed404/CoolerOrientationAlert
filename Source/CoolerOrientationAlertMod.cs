using HarmonyLib;
using RimWorld;
using Verse;

namespace CoolerOrientationAlert
{
    public class CoolerOrientationAlertMod : Mod
    {
        public CoolerOrientationAlertMod(ModContentPack content) : base(content)
        {
            new Harmony("saltgin.coolerorientationalert").PatchAll();
        }
    }

    [HarmonyPatch(typeof(Designator_Build), nameof(Designator_Build.DesignateSingleCell))]
    public static class Designator_Build_DesignateSingleCell_CoolerWarning
    {
        public static void Postfix(
            IntVec3 c,
            BuildableDef ___entDef,
            Rot4 ___placingRot)
        {
            ThingDef thingDef = ___entDef as ThingDef;
            if (!LooksLikeCooler(thingDef))
            {
                return;
            }

            Map map = Find.CurrentMap;
            if (map == null || !c.InBounds(map))
            {
                return;
            }

            IntVec3 coldCell = c + IntVec3.South.RotatedBy(___placingRot);

            // Treat out-of-bounds cold side as invalid orientation.
            if (!coldCell.InBounds(map) || coldCell.GetRoom(map)?.UsesOutdoorTemperature is not false)
            {
                Messages.Message(
                    "CoolerColdSideOutdoorWarning".Translate(thingDef.LabelCap),
                    new TargetInfo(c, map),
                    MessageTypeDefOf.CautionInput,
                    historical: false);
                
            }
        }

        private static bool LooksLikeCooler(ThingDef def)
        {
            if (def?.category != ThingCategory.Building)
            {
                return false;
            }

            if (def.thingClass != null && typeof(Building_Cooler).IsAssignableFrom(def.thingClass))
            {
                return true;
            }

            for (int i = 0; i < def.PlaceWorkers?.Count; i++)
            {
                if (def.PlaceWorkers[i] is PlaceWorker_Cooler)
                {
                    return true;
                }
            }

            if (def is {rotatable: true, building.canPlaceOverWall: true})
            {
                for (int i = 0; i < def.comps?.Count; i++)
                {
                    CompProperties_TempControl temp = def.comps[i] as CompProperties_TempControl;
                    if (temp?.energyPerSecond < 0f)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
