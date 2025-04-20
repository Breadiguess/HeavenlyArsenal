using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace HeavenlyArsenal.Content.Projectiles.Weapons.Ranged.AvatarRifleProj
{
    public class AvatarRifleGlobalNPC : GlobalNPC
    {
        private int shotcount = 0;
        private const int maxshotcount = 4;

        private float shredTimer = 0f;
        private const int MaxShredTime = 13 * 60; // 13 seconds in ticks
        private bool Shredding = false;

        private int originalDefense = -1; // store original defense

        public override bool InstancePerEntity => true; // make this data per-NPC

        public override bool PreAI(NPC npc)
        {
            //CombatText.NewText(npc.Hitbox, Color.Gray, $"Shotcount: {shotcount}");
            if (Shredding)
            {
                if (shredTimer > 0)
                {
                    shredTimer--;
                }
                else
                {
                    EndShredEffect(npc);
                }
            }

            return base.PreAI(npc);
        }

        public void TriggerShredEffect(NPC npc, int time)
        {
            if (!Shredding)
            {
                originalDefense = npc.defense;
                npc.defense = (int)(npc.defense * 0.55f); // reduce defense by 45%
                shredTimer = time;
                Shredding = true;

                // Radial damage to nearby NPCs (600px radius)
                float radius = 600f;
                Player player = Main.player[Player.FindClosest(npc.Center, npc.width, npc.height)];

                float damage = GetPlayerStrongestDamage(player) * 2f; // scale however you like

                foreach (NPC target in Main.npc)
                {
                    if (target.active && !target.friendly && !target.dontTakeDamage && target.whoAmI != npc.whoAmI)
                    {
                        if (Vector2.Distance(npc.Center, target.Center) <= radius)
                        {
                            target.SimpleStrikeNPC((int)damage, 0, true, 0, DamageClass.Generic, true, 50, false); // apply damage with no knockback
                        }
                    }
                }

                if (Main.netMode != NetmodeID.Server)
                    CombatText.NewText(npc.Hitbox, Color.Red, "SHREDDED!");

                Main.NewText($"{npc.FullName} is shredded! Defense reduced by 45% for {time / 60f:F1} seconds.", Color.Orange);
            }
        }

        public void EndShredEffect(NPC npc)
        {
            if (originalDefense != -1)
            {
                npc.defense = originalDefense;
                originalDefense = -1;
            }

            Shredding = false;

            if (Main.netMode != NetmodeID.Server)
                CombatText.NewText(npc.Hitbox, Color.Gray, "Shred faded");
        }

        public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            if (projectile.TryGetGlobalProjectile<AvatarRifleSuperBullet>(out var global) && global.empowerment >= 1f && !Shredding)
            {
                shotcount++;
                if (shotcount >= maxshotcount)
                {
                    shotcount = 0;
                    TriggerShredEffect(npc, MaxShredTime);
                }
            }

            base.OnHitByProjectile(npc, projectile, hit, damageDone);
        }

        private float GetPlayerStrongestDamage(Player player)
        {
            float maxDamage = player.GetTotalDamage(DamageClass.Magic).ApplyTo(1f);
            maxDamage = Math.Max(maxDamage, player.GetTotalDamage(DamageClass.Melee).ApplyTo(1f));
            maxDamage = Math.Max(maxDamage, player.GetTotalDamage(DamageClass.Ranged).ApplyTo(1f));
            maxDamage = Math.Max(maxDamage, player.GetTotalDamage(DamageClass.Summon).ApplyTo(1f));
            return maxDamage * 40; // base scaling factor for AoE damage
        }
    }
}
