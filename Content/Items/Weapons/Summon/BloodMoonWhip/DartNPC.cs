using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Summon.BloodMoonWhip
{
    internal class DartNPC : GlobalNPC
    {
        
        public HashSet<Projectile> dart = new HashSet<Projectile>(Main.maxProjectiles);
        public override bool InstancePerEntity => true;
        public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            if (projectile.type == ModContent.ProjectileType<BloodDart>())
            {
                BloodDart Dart = projectile.ModProjectile as BloodDart;
                if (Dart.CurrentState == BloodDart.NeedleState.StuckInEnemy)
                    dart.Add(projectile);
            }

            if (projectile.type == ModContent.ProjectileType<ViscousWhip_Proj>())
            {
                if (dart.Count > 3)
                {
                    Player Owner = Main.player[projectile.owner];
                    if (Owner == null)
                        return;

                    //todo: damage calculation with diminishing returns
                    NPC.HitInfo hitInfo = new NPC.HitInfo()
                    {
                        DamageType = DamageClass.Summon,
                        Damage = dart.Count * 1000,
                        Knockback = 0,
                        HitDirection = npc.Center.X < Owner.Center.X ? -1 : 1,
                    };
                    Owner.StrikeNPCDirect(npc, hitInfo);
                    //CombatText.NewText(npc.Hitbox, Color.Red, dart.Count * 4000);

                    foreach (Projectile proj in dart)
                    {
                        BloodDart Dart = proj.ModProjectile as BloodDart;
                        Dart.Time = 0;
                        Dart.CurrentState = BloodDart.NeedleState.Dislodge;
                        Dart.timeOffset = Main.rand.Next(0, 30);
                        proj.damage = (int)(proj.damage * 1.4f);
                    }


                    dart.Clear();
                }

                if (projectile.minion && npc.HasBuff(ModContent.BuffType<BloodwhipBuff>()))
                {
                    int extraDamage = BloodwhipBuff.TagDamage;
                    npc.SimpleStrikeNPC(extraDamage, 0, noPlayerInteraction: true);
                    CombatText.NewText(npc.Hitbox, Color.Red, extraDamage);
                }
                base.OnHitByProjectile(npc, projectile, hit, damageDone);
            }
        }

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            //if(dart!=null)
            //Utils.DrawBorderString(spriteBatch, "Darts stuck: " + dart.Count, npc.Center - screenPos + new Vector2(0, -40), Color.Red, 1f);
        }
    }
}
