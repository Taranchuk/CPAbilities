using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CPAbilities
{
    public class RipAndTear : Hediff
    {
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
			List<BodyPartRecord> list = (from x in pawn.RaceProps.body.AllParts where !pawn.health.hediffSet.PartIsMissing(x) 
										 && x.depth == BodyPartDepth.Outside
										 && x.coverage > 0.1f
										 select x).ToList<BodyPartRecord>();
			if (list.Count == 0)
			{
				return;
			}
			BodyPartRecord bodyPartRecord;
			if (GenCollection.TryRandomElement<BodyPartRecord>(list, out bodyPartRecord))
			{
				var missingBodyPart = HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, pawn, bodyPartRecord);
				pawn.health.AddHediff(missingBodyPart);
			}
			pawn.health.RemoveHediff(this);
		}
    }
}
