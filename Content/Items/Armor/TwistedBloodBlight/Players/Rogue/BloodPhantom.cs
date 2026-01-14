using Luminance.Assets;
using NoxusBoss.Assets;

namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Rogue
{
    public class BloodPhantom : ModProjectile
    {

        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        public RogueBloodController Controller
        {
            get
            {
                return Owner.GetModPlayer<BloodBlightParasite_Player>().ConstructController as RogueBloodController;
            }
        }

        public Player Owner => Main.player[Projectile.owner];

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.Size = new Vector2(30);

            Projectile.tileCollide = false;
            Projectile.timeLeft = 120;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            int distance = 80;
            int index = (Controller.BloodPhantoms.IndexOf(Projectile.whoAmI));
            var d = Controller.BloodPhantoms;
            float yOF = index % 2 == 0 ? 1 : -1;

            float t = index / (float)(Controller.BloodPhantoms.Capacity - 1);
            float thing = 0.7f;
            float angle = MathHelper.Lerp(-MathHelper.PiOver2 * thing, MathHelper.PiOver2 * thing, t);

            Vector2 offset = new Vector2(0, -distance + 2 * MathF.Cos(Main.GameUpdateCount / 20.1f) * yOF);
            offset = offset.RotatedBy(angle);

            Projectile.rotation = Projectile.AngleTo(Owner.Center);
            Projectile.Center = Vector2.Lerp(Projectile.Center, Owner.Center + offset, 0.6f);//Owner.Center + new Vector2(Controller.BloodPhantoms.IndexOf(Projectile.whoAmI) * 10, -60);
            Projectile.timeLeft = 2;


        }
        public override bool PreDraw(ref Color lightColor)
        {

            float rotation = Projectile.rotation - MathHelper.PiOver2;
            Texture2D tex = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Armor/TwistedBloodBlight/Players/Rogue/BloodEcho").Value;
            Texture2D glow = GennedAssets.Textures.GreyscaleTextures.BloomCircleSmall;
            float LoopCount = 6;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, default, default, null, Main.GameViewMatrix.ZoomMatrix);
            for (int i = 0; i < LoopCount; i++)
            {
                Color color = Color.Lerp(Color.Crimson, Color.PaleVioletRed, i / LoopCount);
                Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition + new Vector2(2, 0).RotatedBy(i / LoopCount * MathHelper.TwoPi + (Main.GlobalTimeWrappedHourly)), null, color, rotation, tex.Size() / 2, 1, 0);

            }

            Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, null, Color.Crimson, rotation, glow.Size() / 2, new Vector2(0.18f, 0.5f), 0);
            Main.spriteBatch.ResetToDefault();
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.Black, rotation, tex.Size() / 2, 1, 0);

            return base.PreDraw(ref lightColor);
        }
    }
}
