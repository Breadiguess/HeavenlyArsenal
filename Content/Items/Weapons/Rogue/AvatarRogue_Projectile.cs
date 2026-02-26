using HeavenlyArsenal.Common.utils;
using Luminance.Assets;
using Luminance.Common.Easings;
using Luminance.Core.Graphics;
using System.IO;
using Terraria.DataStructures;
using static NoxusBoss.Core.Utilities.Utilities;

namespace HeavenlyArsenal.Content.Items.Weapons.Rogue
{
    public class AvatarRogue_Projectile : ModProjectile
    {
        public ref Player Owner => ref Main.player[Projectile.owner];
        public float RiftAngle;
        public Vector2 ChainEnd { get; set; }
        public Rope Chain;
        public PiecewiseCurve ChainCurve;
        public int Time
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

        public override void SendExtraAI(BinaryWriter writer)
        {
            base.SendExtraAI(writer);
            writer.WriteVector2(ChainEnd);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            base.ReceiveExtraAI(reader);
            ChainEnd = reader.ReadVector2();
        }

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.Size = new Vector2(60);
            Projectile.scale = 0;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }
        public Vector2 TargetLocation => Main.MouseWorld;
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.Center = TargetLocation + Main.rand.NextVector2CircularEdge(120, 120) * 3;
            if (Chain is null)
            {
                Chain = new Rope(Projectile.Center - Vector2.One, Projectile.Center, 20, 2, Vector2.Zero);

            }
            ChainCurve = new PiecewiseCurve().Add(EasingCurves.Exp, EasingType.InOut, 1, 1f);
            
            ChainEnd = Projectile.Center;
            Projectile.velocity = Projectile.DirectionTo(TargetLocation);
            RiftAngle = Projectile.velocity.ToRotation();       
        }
        public override void AI()
        {
            Projectile.rotation = ChainEnd.AngleTo(Projectile.Center);
            Chain.segments[0].position = ChainEnd;
            Chain.segments[^1].position = Projectile.Center;
            Chain.Update();

            Projectile.scale = LumUtils.InverseLerp(0, 20, Time);
            float interp = ChainCurve.Evaluate(LumUtils.InverseLerp(0, 40, Time));
            Projectile.Center = Vector2.Lerp(ChainEnd, Main.MouseWorld, interp);
            Time++;
        }


        public override bool PreDraw(ref Color lightColor)
        {
            DrawRift();

            DrawChain();
            DrawHead();

            return base.PreDraw(ref lightColor);
        }
        private void DrawHead()
        {
            Texture2D tex = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/Rogue/CorrodedLance").Value;
            float Rot = Projectile.rotation;
            Vector2 DrawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = new Vector2(tex.Width / 5f, tex.Height / 2);
            Main.EntitySpriteDraw(tex, DrawPos, null, Color.White, Rot, origin, 1, 0);
        }
        private VertexPositionColorTexture[] vertices;
        private void DrawChain()
        {
            Texture2D tex = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/Rogue/ChainLink").Value;

            if (Chain is null)
                return;

            for (int i = 0; i < Chain.segments.Length - 1; i++)
            {
                float Interp = i / (float)(Chain.segments.Length - 1);
                //NoxusBoss.Core.Utilities.Utilities.DrawLineBetter(Main.spriteBatch, Chain.segments[i].position, Chain.segments[i + 1].position, Color.Wheat, 2);
                Vector2 DrawPos = Chain.segments[i].position - Main.screenPosition;
                float Rot = Chain.segments[i].position.AngleTo(Chain.segments[i + 1].position);
                Rectangle Frame = tex.Frame(1, 2, 0, i % 2 == 0 ? 0 : 1);
                Vector2 origin = Frame.Size() * 0.5f;
                Main.EntitySpriteDraw(tex, DrawPos, Frame, Color.White * Interp, Rot, origin, 1, 0);
            }
        }
        private void DrawRift()
        {
            Main.spriteBatch.End();
            var Rot = RiftAngle + MathHelper.PiOver2;
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            var glow = AssetDirectory.Textures.BigGlowball.Value;

            Main.EntitySpriteDraw
            (
                glow,
                ChainEnd - Main.screenPosition,
                glow.Frame(),
                Color.Red with
                {
                    A = 200
                },
                Rot,
                glow.Size() * 0.5f,
                new Vector2(0.1f, 0.05f) * 0.275f,
                0
            );

            var innerRiftTexture = GennedAssets.Textures.FirstPhaseForm.RiftInnerTexture.Value;
            var riftShader = ShaderManager.GetShader("NoxusBoss.DarkPortalShader");
            riftShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly * 0.1f);
            riftShader.TrySetParameter("baseCutoffRadius", 0.24f);
            riftShader.TrySetParameter("swirlOutwardnessExponent", 0.151f);
            riftShader.TrySetParameter("swirlOutwardnessFactor", 6f);
            riftShader.TrySetParameter("vanishInterpolant", 0.001f);
            riftShader.TrySetParameter("edgeColor", new Vector4(1f, 0.08f, 0.08f, 1f));
            riftShader.TrySetParameter("edgeColorBias", 0.15f);
            riftShader.SetTexture(GennedAssets.Textures.Noise.WavyBlotchNoise, 1, SamplerState.AnisotropicWrap);
            riftShader.SetTexture(GennedAssets.Textures.Noise.WavyBlotchNoise, 2, SamplerState.AnisotropicWrap);
            riftShader.Apply();

            Main.spriteBatch.Draw
                (innerRiftTexture, ChainEnd - Main.screenPosition, null, new Color(77, 0, 2), Rot, innerRiftTexture.Size() * 0.5f, ViewportSize / innerRiftTexture.Size() * new Vector2(0.1f, 0.05f) * Projectile.scale * 0.6f, 0, 0f);
            Main.spriteBatch.ResetToDefault();
        }
    }
}
