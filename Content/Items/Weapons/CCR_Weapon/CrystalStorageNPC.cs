using System.Collections.Generic;

namespace HeavenlyArsenal.Content.Items.Weapons.CCR_Weapon;

internal class CrystalStorageNPC : GlobalNPC
{
    public List<Projectile> storedProjectiles = new(Main.maxProjectiles);

    public override bool InstancePerEntity => true;

    public override void PostAI(NPC npc)
    {
        base.PostAI(npc);
        CheckStoredProjectiles(npc);
        // string msg = "";

        ///foreach(var kvd in storedProjectiles)
        {
            //    msg += kvd.Value.ToString()+ $"\n";
        }
        // //Main.NewText(msg);
    }

    public void AttachtoNPC(NPC npc, Projectile proj)
    {
        if (!storedProjectiles.Contains(proj))
        {
            storedProjectiles.Add(proj);
        }
    }

    public void CheckStoredProjectiles(NPC npc)
    {
        storedProjectiles.RemoveAll(proj => proj == null || !proj.active || proj.type != ModContent.ProjectileType<EntropicCrystal>());
    }

    public override void OnKill(NPC npc)
    {
        storedProjectiles.Clear();
        base.OnKill(npc);
    }

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        //Utils.DrawBorderString(spriteBatch, $"Stored Crystals: {storedProjectiles.Count}", npc.Center - screenPos + new Vector2(0, -40), Color.White);
    }
}