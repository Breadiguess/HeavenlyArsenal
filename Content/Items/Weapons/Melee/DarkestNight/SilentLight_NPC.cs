namespace HeavenlyArsenal.Content.Items.Weapons.Melee.DarkestNight;

internal class SilentLight_NPC : GlobalNPC
{
    public Player Sun;

    public int Heat = 0;

    public int CooldownMAX = 40;

    public int Cooldown;

    public bool active => Heat > 0 && Sun != null;

    public override bool InstancePerEntity => true;

    public override void PostAI(NPC npc)
    {
        if (active)
        {
            if (Heat % 6 == 0 && Cooldown <= 0)
            {
                var a = npc.CalculateHitInfo(Heat * 10_000, 0, damageType: DamageClass.Melee, luck: 40 + Sun.luck);
                Sun.StrikeNPCDirect(npc, a);
                Cooldown = CooldownMAX;
            }

            if (Cooldown > 0)
            {
                Cooldown--;
            }
        }
    }

    public override void UpdateLifeRegen(NPC npc, ref int damage) { }

    public override void DrawEffects(NPC npc, ref Color drawColor) { }

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) { }
}