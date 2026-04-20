using CalamityMod;
using CalamityMod.NPCs;
using Luminance.Assets;
using Luminance.Core.Graphics;
using NoxusBoss.Core.Graphics.Automators;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.ZealotsReward
{
    [PierceResistException]
    internal class Zealots_StasisBurst : ModProjectile, IDrawSubtractive
    {
        public ref Player Owner => ref Main.player[Projectile.owner];
        private static Asset<Texture2D> Subtractive;
        public override void Load()
        {
            string path = this.GetPath();

            Subtractive = ModContent.Request<Texture2D>(path + "_Subtractive");
        }
        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.Size = new(600, 600);
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ArmorPenetration += 40;
            Projectile.timeLeft = 90;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            if (Projectile.ai[0]<1)
            {
                Zealots_FreezeGore.AddFreezeZone(Projectile.Hitbox, 60, 0);
                Projectile.ai[0]++;
            }
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if(target.TryGetGlobalNPC<Zealots_Stasis_NPC>(out var npc))
            {
                npc.AddStacks(target, 1, Owner.GetSource_OnHit(target) as Terraria.DataStructures.IEntitySource_OnHit);

                npc.Freeze(target, 60 * 5);
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            
        }



        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            var Tex = TextureAssets.Projectile[Type].Value;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float interp = 1-LumUtils.InverseLerp(0, 90, Projectile.timeLeft);

            var scale = new Vector2(4) * QuadInOut(interp);

            Main.spriteBatch.PrepareForShaders(BlendState.Additive);
            var Star = GennedAssets.Textures.GreyscaleTextures.HollowCircleSoftEdge;

            float progress = QuadInOut(interp);



            progress = MathHelper.SmoothStep(1f, 0, progress);

            Vector2 direction = new Vector2(0);
            if (direction != Vector2.Zero)
                direction.Normalize();


            
            //float noiseStrength;
            //float gradientStrength;
            var FrostBuildup = ShaderManager.GetShader("HeavenlyArsenal.StasisBurstShader");
            FrostBuildup.SetTexture(Star, 0);
            FrostBuildup.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 1);
            FrostBuildup.TrySetParameter("fragmentProgress", interp);
            FrostBuildup.TrySetParameter("fragmentStrength", 0);
            FrostBuildup.TrySetParameter("edgeWidth", 0.1f);
            FrostBuildup.TrySetParameter("noiseScale", 0.9f);
            FrostBuildup.TrySetParameter("edgeColor", Color.Black.ToVector4());
            FrostBuildup.Apply();
            for(int i = 0; i< 2; i++)
            Main.EntitySpriteDraw(Star, drawPos, null, Color.White * (1-interp), MathHelper.Pi * i, Star.Size() / 2, MathF.Pow(scale.LengthSquared(), 0.1f*(1-i)), 0);



            Main.spriteBatch.UseBlendState(BlendState.Additive);
            const float thing = 2;
            for(int i =  0; i < thing; i++)
            {

                Main.EntitySpriteDraw(Tex, drawPos, null, Color.CadetBlue *(1-interp), i*MathHelper.Pi, Tex.Size() / 2, scale+ new Vector2(1), 0);
            }

            var Thing = ModContent.Request<Texture2D>(this.GetPath() + "_Thing").Value;



            Main.EntitySpriteDraw(Thing, drawPos, null, Color.AliceBlue * QuadInOut(1-interp), 0, Thing.Size() / 2, scale+new Vector2(2*(1+interp), 2), 0);

            Main.spriteBatch.ResetToDefault();
            return false;
        }

        public void DrawSubtractive(SpriteBatch spriteBatch)
        {
            var tex = Subtractive.Value;

            Vector2 DrawPos = Projectile.Center - Main.screenPosition;

            var scale = new Vector2(0.6f) * LumUtils.InverseLerpBump(0, 5, 50, 60, Projectile.timeLeft);


           // Main.EntitySpriteDraw(tex, DrawPos, null, Color.White, 0, tex.Size() / 2, scale, 0);

        }

        private static float QuadInOut(float x)
        {
            if (x < 0.5f)
                return 2f * x * x;
            else
                return 1f - 2f * (1f - x) * (1f - x);
        }
    }
}
