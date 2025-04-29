using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Armor.Haemsong
{
    public class BloodTentacle : ModProjectile
    {
        bool stuck = false;
        int hitCooldown = 0;
        int stuckOne = -1;
        int timer = 0;
        int toHeal = 0;
        private const string defSegPath = "HeavenlyArsenal/Content/Items/Armor/Haemsong/DefSeg";
        private const string defTentaclePath = "HeavenlyArsenal/Content/Items/Armor/Haemsong/DefTentacle";
        private const string regSegPath = "HeavenlyArsenal/Content/Items/Armor/Haemsong/RegSeg";
        private static Asset<Texture2D> defSegTexture;
        private static Asset<Texture2D> defTentacleTexture;
        private static Asset<Texture2D> regSegTexture;
        private Projectiles.globalHomingAI HomingAI => Projectile.GetGlobalProjectile<Projectiles.globalHomingAI>();
        Vector2 stuckPos = Vector2.Zero;
        public override bool PreDraw(ref Color lightColor)
        {
            int drawLimit = Projectile.ai[0] == 2 ? 240 : 120;
            Player player = Main.player[Projectile.owner];
            Texture2D texture = Projectile.ai[0] == 2 ? defSegTexture.Value : regSegTexture.Value;
            Rectangle sourceRect = texture.Frame();
            Vector2 oldPos;
            Vector2 segPos;
            Vector2 segTrail;
            oldPos = segPos = segTrail = Projectile.Center;
            Vector2 segVel = -Projectile.rotation.ToRotationVector2() * 4;
            while (drawLimit > 0 && oldPos.Distance(player.Center) > sourceRect.Width * 1.5f)
            {
                segVel += segTrail.DirectionTo(player.Center) * 0.05f;
                segVel /= 1.025f;
                segTrail += segVel;
                if (segPos.Distance(segTrail) > sourceRect.Width - 2)
                {
                    drawLimit--;
                    segTrail = segPos - segTrail.DirectionTo(segPos) * (sourceRect.Width - 2);
                    segPos = segTrail;
                    Main.EntitySpriteDraw(texture, segPos - Main.screenPosition, sourceRect, Lighting.GetColor((segPos / 16).ToPoint()), segPos.DirectionTo(oldPos).ToRotation(), sourceRect.Size() / 2f, 1, SpriteEffects.None);
                    oldPos = segPos;
                }
            }
            return true;
        }
		public override void AI()
		{
            //Filler
            if (++timer > 1200)
            {
                timer = 0;
            }
            Player player = Main.player[Projectile.owner];
            if (!player.dead && player.GetModPlayer<BloodPlayer>().fullBloodArmor)
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    HomingAI.rangeCenter = Main.MouseWorld;
                }
                if (Projectile.ai[0] != 2)
                {
                    Projectile.timeLeft = 2;
                }
            }
            int totalHeal = (int)(Projectile.ai[1] * player.statLifeMax2 / 100f);
            Projectile.damage = (int)player.GetTotalDamage(DamageClass.Generic).ApplyTo(Projectile.ai[0] < 2 ? 200 : Projectile.ai[0] < 3 ? totalHeal : 100);
            Projectile.localNPCHitCooldown = Projectile.ai[0] < 2 ? 24 : 120;
            Vector2 anchor = Vector2.Zero;
            if (Main.myPlayer == Projectile.owner)
            {
                anchor = player.Center + player.DirectionTo(Main.MouseWorld).RotatedBy(MathHelper.PiOver2 * Projectile.ai[0]) * 160 + new Vector2(16, 0).RotatedBy(MathHelper.TwoPi * (timer / 1200f));
            }
            //Don't let them get too far!
            if (Projectile.ai[0] != 2 && Projectile.Distance(player.Center) > 1600)
            {
                Projectile.Center = player.Center;
            }
            else if (Projectile.Distance(player.Center) > 4800)
            {
                Projectile.Kill();
                return;
            }
            //Do shit
            if (stuck)
            {
                Projectile.Center = Main.npc[stuckOne].Center + stuckPos;
                if (Projectile.timeLeft % (1800 / totalHeal) == 0)
                {
                    toHeal++;
                }
                if (!Main.npc[stuckOne].CanBeChasedBy())
                {
                    HomingAI.enabled = true;
                    stuck = false;
                }
            }
            else
            {
                if (Projectile.friendly)
                {
                    //Blood boost tentacles are slower
                    if (Projectile.ai[0] < 3)
                    {
                        HomingAI.agility = 4;
                        HomingAI.decel = 1.2f;
                        Projectile.penetrate = -1;
                    }
                    else
                    {
                        HomingAI.agility = 1;
                        HomingAI.decel = 1.05f;
                        Projectile.penetrate = 1;
                    }
                    //Idle
                    if (!HomingAI.hasTarget)
                    {
                        if (Projectile.ai[0] < 2)
                        {
                            Projectile.velocity += Projectile.DirectionTo(anchor) * Projectile.Distance(anchor) / 160f;
                            Projectile.velocity /= 1.1f;
                        }
                        else if (timer > 60)
                        {
                            if (Projectile.Distance(player.Center) > 24)
                            {
                                Projectile.Center += Projectile.DirectionTo(player.Center);
                                Projectile.velocity += Projectile.DirectionTo(player.Center);
                                Projectile.velocity /= 1.05f;
                            }
                            else
                            {
                                Projectile.Kill();
                            }
                        }
                    }
                }
                //Don't do shit for 0.2s after hitting an enemy. Keep looking at enemies if enemies there be, though
                else if (--hitCooldown < 1)
                {
                    Projectile.friendly = true;
                }
            }
            //Properly set projectile rotation
            if (stuck)
            {
                Projectile.rotation = Projectile.DirectionTo(Main.npc[stuckOne].Center).ToRotation();
            }
            else if (HomingAI.hasTarget)
            {
                Projectile.rotation = Utils.AngleLerp(Projectile.rotation, Projectile.DirectionTo(HomingAI.targetPos).ToRotation(), 0.2f);
            }
            else if (Main.myPlayer == Projectile.owner)
            {
                if (player.Distance(Main.MouseWorld) > 240)
                {
                    Projectile.rotation = Utils.AngleLerp(Projectile.rotation, Projectile.DirectionTo(Main.MouseWorld).ToRotation(), 0.1f);
                }
                else
                {
                    Projectile.rotation = Utils.AngleLerp(Projectile.rotation, Projectile.DirectionTo(player.Center + player.DirectionTo(Main.MouseWorld) * 240).ToRotation(), 0.1f);
                }
            }
            //No touching tips >:(
            foreach (Projectile proj in Main.ActiveProjectiles)
            {
                if (proj.type == Projectile.type && proj.owner == Projectile.owner && proj.Distance(Projectile.Center) < 16 && proj.whoAmI != Projectile.whoAmI && proj.Center != Projectile.Center && Projectile.ai[0] != 2)
                {
                    Projectile.velocity += proj.DirectionTo(Projectile.Center) * 5;
                }
            }
        }
        public override void Load()
        {
            if (!Main.dedServ)
            {
                defSegTexture = ModContent.Request<Texture2D>(defSegPath);
                defTentacleTexture = ModContent.Request<Texture2D>(defTentaclePath);
                regSegTexture = ModContent.Request<Texture2D>(regSegPath);
            }
        }
        public override void SetDefaults()
		{
			HomingAI.enabled = true;
            HomingAI.range = 20;
			HomingAI.wallHack = true;

			Projectile.DamageType = DamageClass.Generic;
            Projectile.extraUpdates = 1;
			Projectile.friendly = true;
			Projectile.height = 22;
			Projectile.tileCollide = false;
            Projectile.timeLeft = 1800;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.width = 34;
		}
        public override void SetStaticDefaults()
		{
			ProjectileID.Sets.CultistIsResistantTo[Type] = true;
		}
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.ai[0] < 2)
            {
                //"Neuter" homing AI, the projectile only tracks target position from this point on
                HomingAI.agility = 0;
                HomingAI.decel = 1;
                //Make a gross noise
                if (Main.netMode != NetmodeID.Server)
                {
                    SoundEngine.PlaySound(SoundID.Item17, Projectile.Center);
                }
                //Defensive blood boost tentacles reduce defense and DR on hit. This prevents repeated setting of the original stats (basically enemies would quickly be permenantly reduced to 0 defense)
                if (Projectile.ai[0] > 2)
                {
                    if (!target.HasBuff(ModContent.BuffType<ClotBuff>()))
                    {
                        target.GetGlobalNPC<ClotNPC>().oldDefense = target.defense;
                        target.GetGlobalNPC<ClotNPC>().oldDR = target.takenDamageMultiplier;
                    }
                    target.AddBuff(ModContent.BuffType<ClotBuff>(), 900);
                    return;
                }
                //Don't deal damage for 0.2s and bounce away from the target
                Projectile.friendly = false;
                if (Main.myPlayer == Projectile.owner)
                {
                    Projectile.netUpdate = true;
                    Projectile.velocity = -Projectile.DirectionTo(target.Center).RotatedByRandom(0.1f) * Projectile.velocity.Length() * Main.rand.NextFloat(0.4f, 0.6f);
                }
                hitCooldown = 24;
            }
            else if (Projectile.ai[0] < 3)
            {
                if (!stuck)
                {
                    HomingAI.enabled = false;
                    Projectile.rotation = Projectile.DirectionTo(target.Center).ToRotation();
                    stuck = true;
                    stuckOne = target.whoAmI;
                    stuckPos = Projectile.Center - target.Center;
                    if (!target.HasBuff(ModContent.BuffType<ClotBuff>()))
                    {
                        target.GetGlobalNPC<ClotNPC>().oldDefense = target.defense;
                        target.GetGlobalNPC<ClotNPC>().oldDR = target.takenDamageMultiplier;
                    }
                    target.AddBuff(ModContent.BuffType<ClotBuff>(), 900);
                }
                else if (target.whoAmI == stuckOne)
                {
                    Main.player[Projectile.owner].statLife += toHeal;
                    Main.player[Projectile.owner].HealEffect(toHeal);
                    toHeal = 0;
                }
            }
        }
    }
    public class ClotBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
        }
        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<ClotNPC>().clotted = true;
        }
    }
    public class ClotNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public bool clotted;
        public float oldDR;
        public int oldDefense;
        int clotTimer = 0;
        public override void ResetEffects(NPC npc)
        {
            clotted = false;
        }
        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (clotted)
            {
                npc.defense = (int)(clotTimer / 900f * oldDefense);
                if (npc.takenDamageMultiplier < 1)
                {
                    npc.takenDamageMultiplier = oldDR + ((1 - oldDR) * ((900 - clotTimer) / 900f));
                }
                clotTimer++;
            }
            else if (clotTimer != 0)
            {
                npc.defDamage = oldDefense;
                npc.takenDamageMultiplier *= oldDR;
                clotTimer = 0;
            }
        }
    }
}
