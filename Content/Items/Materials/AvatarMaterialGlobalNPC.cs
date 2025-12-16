using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using Terraria.GameContent.ItemDropRules;

namespace HeavenlyArsenal.Content.Items.Materials;

public sealed class AvatarMaterialGlobalNPC : GlobalNPC
{
    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
    {
        return entity.type == ModContent.NPCType<AvatarOfEmptiness>();
    }

    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        base.ModifyNPCLoot(npc, npcLoot);

        var rule = new LeadingConditionRule(new Conditions.NotExpert());

        rule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<AvatarMaterial>(), 3, 3));

        npcLoot.Add(rule);
    }
}