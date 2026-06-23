using Klei.AI;
using STRINGS;

namespace Database;

public class ImmunitySkillPerk : SkillPerk
{
	public ImmunitySkillPerk(string id, string nameOfEffectToBecomeImmuneTo)
		: base(id, "", null, null, delegate
		{
		})
	{
		Effect effect = Db.Get().effects.Get(nameOfEffectToBecomeImmuneTo);
		Name = GameUtil.SafeStringFormat(UI.ROLES_SCREEN.PERKS.IMMUNITY, effect.Name);
		base.OnApply = delegate(MinionResume identity)
		{
			Effects component = identity.GetComponent<Effects>();
			if (component != null)
			{
				component.AddImmunity(effect, id, shouldSave: false);
			}
		};
		base.OnRemove = delegate(MinionResume identity)
		{
			Effects component = identity.GetComponent<Effects>();
			if (component != null)
			{
				component.RemoveImmunity(effect, id);
			}
		};
	}
}
