using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace CPAbilities
{
	public class JobDriver_UseAbilityOnThing : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Goto.Goto(TargetIndex.A, PathEndMode.Touch);
			yield return new Toil
			{
				initAction = delegate ()
				{
					var comp = this.pawn.TryGetComp<CompCPAbilities>();
					comp.UseOn(comp.lastAbilityToUse, this.TargetA.Pawn);
				}
			};
		}
	}
}
