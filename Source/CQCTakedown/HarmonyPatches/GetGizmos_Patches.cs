using CPAbilities;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CPAbilities
{
    [StaticConstructorOnStartup]
    internal static class HarmonyInit
    {
        static HarmonyInit()
        {
            new Harmony("CP.Abilities").PatchAll();
        }
    }

	[StaticConstructorOnStartup]
	public static class AssignAbilityCompToHumanlikes
	{
		static AssignAbilityCompToHumanlikes()
		{
			foreach (var thingDef in DefDatabase<ThingDef>.AllDefs.Where(x => x.race?.Humanlike ?? false))
            {
				thingDef.comps.Add(new CompProperties_CPAbilities());
			}
		}
	}

	[HarmonyPatch(typeof(Pawn_DraftController), "GetGizmos")]
	public class Pawn_DraftController_GetGizmos_Patch
	{
		public static void Postfix(ref IEnumerable<Gizmo> __result, ref Pawn_DraftController __instance)
		{
			if (!__instance.Drafted && Find.CurrentMap == null || Find.World == null || Find.World.renderer == null || Find.World.renderer.wantedMode == WorldRenderMode.Planet)
			{
				return;
			}
			if (__result == null || !__result.Any<Gizmo>())
			{
				return;
			}
			Pawn pawn = __instance.pawn;
			var comp = pawn.TryGetComp<CompCPAbilities>();
			if (comp == null)
            {
				return;
            }
			List<Gizmo> list = __result.ToList<Gizmo>();
			foreach (var abilityDef in DefDatabase<CPAbilityDef>.AllDefs)
            {
				bool check = true;
				foreach (var skillRequirement in abilityDef.skillRequirements)
                {
					if (skillRequirement.minLevel > pawn.skills.GetSkill(skillRequirement.skill).Level)
                    {
						check = false;
						break;
                    }
                }
				if (check)
                {
					Command_CPAbility item = new Command_CPAbility(comp, abilityDef)
					{
						defaultLabel = abilityDef.label,
						icon = ContentFinder<Texture2D>.Get(abilityDef.gizmoTexPath),
						disabled = comp.CanUse(abilityDef),
						action = delegate
                        {
							comp.Use(abilityDef);
                        }
					};
					list.Add(item);
				}
			}
			__result = list;
		}
	}
}
