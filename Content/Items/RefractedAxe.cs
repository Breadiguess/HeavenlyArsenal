using CalamityMod.Projectiles.Summon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using static CalamityMod.Projectiles.Summon.CelestialAxeMinion;

namespace HeavenlyArsenal.Content.Items
{
    public class RefractedAxe : ModProjectile
    {

        public ref Player Owner => ref Main.player[Projectile.owner];
        public int Index;
        public int Time
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public int CosmeticTimer
        {
            get => (int)Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }
        public override void SetStaticDefaults()
        {

            Main.projPet[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Type] = false;
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.ignoreWater = true;

        }
        public override void OnSpawn(IEntitySource source)
        {
            if(Owner.TryGetModPlayer<CelestialAxeMinion.CelestialAxePlayer>(out var player))
            {   
                player.Axes.Add(Projectile);
                Index = player.Axes.Count - 1;
            }
        }

        public CelestialAxeMinion.Behavior CurrentState
        {
            get
            {
                if (Owner.ownedProjectileCounts[ModContent.ProjectileType<CelestialAxeMinion>()] > 0)
                {
                    Projectile a = Main.projectile.FirstOrDefault(p => p.active && p.owner == Projectile.owner && p.type == ModContent.ProjectileType<CelestialAxeMinion>());
                    if (a != null)
                    {
                        return (CelestialAxeMinion.Behavior)a.ai[1];
                    }
                    else return 0;
                }
                else return 0;
            }

        }

        public override void AI()
        {
            StateMachine();
        }

        void StateMachine()
        {
            switch (CurrentState)
            {
                case Behavior.HoverAroundPlayer:
                    HoverAroundPlayer();
                    break;
                case Behavior.Attacking:
                    Attacking();
                    break;
            }
        }

       

        private void HoverAroundPlayer()
        {
            int distance = 80;
            float yOF = Index % 2 == 0 ? 1 : -1;

            float t = Index / (float)(CelestialAxeMinion.CelestialAxePlayer.MAX_AXE_CAP);
            float thing = 0.7f;
            float angle = MathHelper.Lerp(-MathHelper.PiOver2 * thing, MathHelper.PiOver2 * thing, t);

            Vector2 offset = new Vector2(0, -distance + 2 * MathF.Cos(Main.GameUpdateCount / 20.1f) * yOF);
            offset = offset.RotatedBy(angle);

            Projectile.rotation = Projectile.AngleTo(Owner.Center);
            Projectile.Center = Vector2.Lerp(Projectile.Center, Owner.Center + offset, 0.6f);//Owner.Center + new Vector2(Controller.BloodPhantoms.IndexOf(Projectile.whoAmI) * 10, -60);


        }
        private void Attacking()
        {
            NPC a = Projectile.FindTargetWithinRange(1000, true);
            if(a!= null)
            Projectile.velocity = Projectile.DirectionTo(a.Center);
        }
        public NPC FindTargetWithinRange(float maxRange, bool checkCanHit = false)
        {
            NPC result = null;
            float num = maxRange;
            foreach(var npc in Main.ActiveNPCs)
            {
                if(npc.CanBeChasedBy(Projectile)
                    && Projectile.localNPCImmunity[npc.whoAmI] == 0)
                {


                    if(npc.TryGetGlobalNPC<CelestialAxeGlobalNPC>(out var axeGlobalNPC))
                    {
                        if (!axeGlobalNPC.MarkedByAxe)
                        {

                        }
                    }
                    float num2 = Projectile.Distance(npc.Center);
                    if (!(num <= num2))
                    {
                        num = num2;
                        result = npc;
                    }
                }



                
            }
           

            return result;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.FinalDamage *= 0.7f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Utils.DrawBorderString(Main.spriteBatch, CurrentState.ToString(), Projectile.Center - Main.screenPosition, Color.White);



            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 DrawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;
            Main.EntitySpriteDraw(tex, DrawPos, null, Color.White, Projectile.rotation, origin, 1, 0);

            return false;// base.PreDraw(ref lightColor);
        }
    }
}
