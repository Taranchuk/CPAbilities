using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CPAbilities
{
    public class Stun : Hediff
    {
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            pawn.stances.stunner.StunFor(12 * 60, null);
			pawn.health.RemoveHediff(this);
		}
    }
}
