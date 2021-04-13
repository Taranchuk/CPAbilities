using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CPAbilities
{
    public class AbilityEffect
    {
        public HediffDef hediffToApplyOnTarget;
        public DamageDef damageToApplyOnTarget;
        public int damageAmount;
        public string moteTextOnFail;
        public string moteTextOnSuccess;
        public bool enemyMeleeSkillCounterChance;
    }
    public class CPAbilityDef : Def
    {
        public List<SkillRequirement> skillRequirements;
        public string gizmoTexPath;
        public SoundDef soundDefCast;
        public int abilityCooldownTicks;
        public float meleeDistRange;
        public IntRange enemyTargetCount;
        public AbilityEffect abilityEffect;
        public JobDef CP_UseAbilityOnThing;
    }
}
