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

		private HashSet<DamageDef> randomInjuries = new HashSet<DamageDef>
		{
			DamageDefOf.Crush,
			DamageDefOf.Blunt,
			DamageDefOf.Stab,
			DamageDefOf.Scratch,
			DamageDefOf.Bite
		};
		public void UseOn(CPAbilityDef abilityDef, Pawn target)
        {
			if (target != null)
            {
				var chance = GetChance(target, abilityDef);
				if (Rand.Chance(chance))
                {
					if (abilityDef.abilityEffect != null)
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

						if (abilityDef.abilityEffect.targetBodyPartsToDestroy.HasValue)
                        {
							var destroyCount = abilityDef.abilityEffect.targetBodyPartsToDestroy.Value.RandomInRange;
							for (var i = 0; i < destroyCount; i++)
                            {
								List<BodyPartRecord> list = (from x in target.RaceProps.body.AllParts where !target.health.hediffSet.PartIsMissing(x) 
															 && x.coverage > 0.1f select x).ToList<BodyPartRecord>();
								if (list.Count == 0)
								{
									continue;
								}
								BodyPartRecord bodyPartRecord;
								if (GenCollection.TryRandomElement<BodyPartRecord>(list, out bodyPartRecord))
								{
									var missingBodyPart = HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, target, bodyPartRecord);
									target.health.AddHediff(missingBodyPart);
								}
							}
                        }

						if (abilityDef.abilityEffect.targetBodyPartsToDamage.HasValue)
						{
							var damageCount = abilityDef.abilityEffect.targetBodyPartsToDamage.Value.RandomInRange;
							for (var i = 0; i < damageCount; i++)
							{
								List<BodyPartRecord> list = (from x in target.RaceProps.body.AllParts
															 where !target.health.hediffSet.PartIsMissing(x) && x.coverage > 0.1f
															 select x).ToList<BodyPartRecord>();
								if (list.Count == 0)
								{
									continue;
								}
								BodyPartRecord bodyPartRecord;
								if (GenCollection.TryRandomElement<BodyPartRecord>(list, out bodyPartRecord))
								{
									var injury = new DamageInfo(randomInjuries.RandomElement(), abilityDef.abilityEffect.targetBodyPartsDamageRange.RandomInRange, 0, -1f, this.parent, bodyPartRecord);
									target.TakeDamage(injury);
								}
							}
						}
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
			this.lastAbilityToUse = null;
		}

		private float GetChance(Pawn enemyTarget, CPAbilityDef abilityDef)
        {
			var chance = 0.5f;
			if (abilityDef.abilityEffect != null && Pawn.skills != null && enemyTarget.skills != null && abilityDef.abilityEffect.enemyMeleeSkillCounterChance)
            {
				var level = (Pawn.skills.GetSkill(SkillDefOf.Melee).Level - enemyTarget.skills.GetSkill(SkillDefOf.Melee).Level);
				chance += level / 10f;
            }
			return chance;
        }
        public CompProperties_CPAbilities Props => this.props as CompProperties_CPAbilities;
		public Pawn Pawn => this.parent as Pawn;
		public Dictionary<CPAbilityDef, int> lastAbilityCountDown = new Dictionary<CPAbilityDef, int>();
		public CPAbilityDef lastAbilityToUse;
        public override void PostExposeData()
        {
            base.PostExposeData();
			Scribe_Collections.Look(ref lastAbilityCountDown, "lastAbilityCountDown", LookMode.Def, LookMode.Value, ref cPAbilityDefKeys, ref intValues);
			Scribe_Defs.Look(ref lastAbilityToUse, "lastAbilityToUse");
		}

		private List<CPAbilityDef> cPAbilityDefKeys;
		private List<int> intValues;
	}
}
