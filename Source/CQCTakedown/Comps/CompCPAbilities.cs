using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Sound;

namespace CPAbilities
{
	public class CompProperties_CPAbilities : CompProperties
	{
		public CompProperties_CPAbilities()
		{
			this.compClass = typeof(CompCPAbilities);
		}
	}
	public class CompCPAbilities : ThingComp
	{
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
			if (lastAbilityCountDown is null)
            {
				lastAbilityCountDown = new Dictionary<CPAbilityDef, int>();
            }
		}

		public bool CanUse(CPAbilityDef abilityDef)
        {
			if (this.lastAbilityCountDown.TryGetValue(abilityDef, out int value))
			{
				return abilityDef.abilityCooldownTicks + value >= Find.TickManager.TicksAbs;
			}
			return false;
		}

		public void Use(CPAbilityDef abilityDef)
        {
			var target = GetMeleeEnemy(abilityDef);
			if (target != null)
            {
				var chance = GetChance(target, abilityDef);
				if (Rand.Chance(chance))
                {
					if (abilityDef.abilityEffect.hediffToApplyOnTarget != null)
					{
						target.health.AddHediff(abilityDef.abilityEffect.hediffToApplyOnTarget);
					}
					if (abilityDef.abilityEffect.damageToApplyOnTarget != null)
                    {
						var damageInfo = new DamageInfo(abilityDef.abilityEffect.damageToApplyOnTarget, abilityDef.abilityEffect.damageAmount, 0, -1, Pawn);
						target.TakeDamage(damageInfo);
                    }
					this.lastAbilityCountDown[abilityDef] = Find.TickManager.TicksAbs;
					abilityDef.soundDefCast?.PlayOneShotOnCamera();
					if (!abilityDef.abilityEffect?.moteTextOnSuccess.NullOrEmpty() ?? false)
                    {
						MoteMaker.ThrowText(Pawn.TrueCenter(), Pawn.Map, abilityDef.abilityEffect.moteTextOnSuccess);
                    }
				}
				else
                {
					if (!abilityDef.abilityEffect?.moteTextOnFail.NullOrEmpty() ?? false)
					{
						MoteMaker.ThrowText(Pawn.TrueCenter(), Pawn.Map, abilityDef.abilityEffect.moteTextOnFail);
					}
				}
			}
		}

		private float GetChance(Pawn enemyTarget, CPAbilityDef abilityDef)
        {
			var chance = 0.5f;
			if (abilityDef.abilityEffect.enemyMeleeSkillCounterChance)
            {
				var level = (Pawn.skills.GetSkill(SkillDefOf.Melee).Level - enemyTarget.skills.GetSkill(SkillDefOf.Melee).Level);
				chance += level / 10f;
            }
			return chance;
        }

		private Pawn GetMeleeEnemy(CPAbilityDef abilityDef)
        {
			if (Pawn.mindState.meleeThreat != null && abilityDef.meleeDistRange >= Pawn.mindState.meleeThreat.Position.DistanceTo(Pawn.Position))
            {
				return Pawn.mindState.meleeThreat;
            }
			if (Pawn.mindState.enemyTarget is Pawn enemyPawn && abilityDef.meleeDistRange >= enemyPawn.Position.DistanceTo(Pawn.Position))
			{
				return enemyPawn;
			}
			var enemies = Pawn.Map.attackTargetsCache.GetPotentialTargetsFor(Pawn).Where(x => x.Thing is Pawn pawn && abilityDef.meleeDistRange >= x.Thing.Position.DistanceTo(Pawn.Position)).Select(x => x.Thing as Pawn);
			if (enemies.Any())
            {
				return enemies.RandomElement();
            }
			return null;
		}

        public CompProperties_CPAbilities Props => this.props as CompProperties_CPAbilities;
		public Pawn Pawn => this.parent as Pawn;
		public Dictionary<CPAbilityDef, int> lastAbilityCountDown = new Dictionary<CPAbilityDef, int>();
        public override void PostExposeData()
        {
            base.PostExposeData();
			Scribe_Collections.Look(ref lastAbilityCountDown, "lastAbilityCountDown", LookMode.Def, LookMode.Value, ref cPAbilityDefKeys, ref intValues);
		}

		private List<CPAbilityDef> cPAbilityDefKeys;
		private List<int> intValues;
	}
}
