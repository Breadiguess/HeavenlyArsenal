using HeavenlyArsenal.Content.Projectiles.Weapons.Summon;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Buffs
{
    class SolynWhip_Onhit_Buff : ModBuff
    {
        public static readonly int TagDamage = 500;
        public override void SetStaticDefaults()
        {

        }
        public override void Update(Player player, ref int buffIndex)
        {
            // Ensure this is tagged as a summon buff
            BuffID.Sets.IsATagBuff[Type] = true;

            // Logic to summon the minion (if not already summoned)
            if (player.ownedProjectileCounts[ModContent.ProjectileType<SolynWhip_BattleSolyn>()] <= 0)
            {
                Vector2 spawnPosition = player.Center; // Set spawn position near the player
                Projectile.NewProjectile(player.GetSource_FromThis(), spawnPosition, Vector2.Zero,
                    ModContent.ProjectileType<SolynWhip_BattleSolyn>(), 50, 0f, player.whoAmI);
            }
        }

        //public override void Update(Player player, ref int buffIndex)
        //{
        //    BuffID.Sets.IsATagBuff[Type] = true;
        //}

    }
    public class Solynel : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (npc.HasBuff<SolynWhip_Onhit_Buff>())
            {
                modifiers.FlatBonusDamage += SolynWhip_Onhit_Buff.TagDamage * ProjectileID.Sets.SummonTagDamageMultiplier[projectile.type];
            }
        }
    }
}
