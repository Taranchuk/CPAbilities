using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CPAbilities
{
    public class Command_CPAbility : Command_Action
    {
		private CompCPAbilities compAbility;
		private CPAbilityDef abilityDef;
		public new static readonly Texture2D BGTex = ContentFinder<Texture2D>.Get("UI/Widgets/AbilityButBG");
		public new static readonly Texture2D BGTexShrunk = ContentFinder<Texture2D>.Get("UI/Widgets/AbilityButBGShrunk");
		private static readonly Texture2D cooldownBarTex = SolidColorMaterials.NewSolidColorTexture(new Color32(9, 203, 4, 64));

		public Command_CPAbility(CompCPAbilities compAbility, CPAbilityDef abilityDef)
		{
			this.compAbility = compAbility;
			this.abilityDef = abilityDef;
			order = 5f;
			defaultLabel = abilityDef.LabelCap;
		}
        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth)
        {
            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth);
            var cooldownTicks = 0;
            if (compAbility.lastAbilityCountDown.TryGetValue(abilityDef, out int value))
            {
                cooldownTicks = Find.TickManager.TicksAbs - value;
            }
            if (cooldownTicks > 0)
            {
                float num = Mathf.InverseLerp(abilityDef.abilityCooldownTicks, 0f, cooldownTicks);
                Widgets.FillableBar(rect, Mathf.Clamp01(num), cooldownBarTex, null, doBorder: false);
            }
            if (result.State == GizmoState.Interacted)
            {
                return result;
            }
            return new GizmoResult(result.State);
        }
    }
}
