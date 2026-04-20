namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.ZealotsReward
{
    internal class Zealots_FireParticle : CalamityMod.Particles.Particle
    {

        public Vector2 Position;
        public int MaxTime;
        public int TimeLeft;
        public float Rotation;
        public Color color;

        public bool Super = false;
        public Zealots_FireParticle(Vector2 Position, float Rotation, int MaxTime, bool Super = false)
        {
            Prepare(Position, Rotation, MaxTime);
            this.Super = Super;
        }

        public void Prepare(Vector2 Position, float Rotation, int MaxTime)
        {
            this.Position = Position;
            this.MaxTime = MaxTime;
            TimeLeft = MaxTime;
            this.Rotation = Rotation;
            color = Color.PowderBlue;
        }

        public override void Update()
        {

            if (TimeLeft-- < 0)
                Kill();
        }


        public override bool UseAdditiveBlend => true;

        public override bool UseCustomDraw => true;
        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            var MuzzleFlash = ModContent.Request<Texture2D>(this.GetPath() + (!Super ? "" : "_Strong")).Value;
            var tex = GennedAssets.Textures.GreyscaleTextures.HollowCircleSoftEdge;

            var GlowTex = GennedAssets.Textures.GreyscaleTextures.BloomCircle;
            var Corona = GennedAssets.Textures.GreyscaleTextures.Corona;
            float progress = LumUtils.InverseLerp(0, MaxTime, TimeLeft);

            Vector2 drawPos = Position - Main.screenPosition;
            Vector2 Scale = new Vector2(0.1f, 0.4f) * 0.2f * progress;



            Main.EntitySpriteDraw(GlowTex, drawPos + new Vector2(-2, 0).RotatedBy(Rotation), null, Color.White * MathF.Pow(progress, 2f), Rotation, GlowTex.Size() / 2, Scale * 2, 0);

            Main.EntitySpriteDraw(tex, drawPos + new Vector2(4, 0).RotatedBy(Rotation), null, color * MathF.Pow(progress, 4f) * 2, Rotation, tex.Size() / 2, Scale, 0);
            //Main.EntitySpriteDraw(tex, drawPos + new Vector2(-2, 0).RotatedBy(Rotation), null, color.HueShift(-0.1f) * MathF.Pow(progress, 4.5f) * 2, Rotation, tex.Size() / 2, Scale*0.8f, 0);

            Main.EntitySpriteDraw(Corona, drawPos + new Vector2(-5, 0).RotatedBy(Rotation), null, color * MathF.Pow(progress, 4.5f) * 2, Rotation, Corona.Size() / 2, Scale * 1.1f, 0);



            Vector2 Origin;
            Vector2 scale;
            if (!Super)
            {

                Origin = new Vector2(MuzzleFlash.Width / 2, MuzzleFlash.Height * 0.87f);
                scale = new Vector2(1) * MathF.Pow(progress, 4) * 0.2f;
                Main.EntitySpriteDraw(MuzzleFlash, drawPos, null, color, Rotation + MathHelper.PiOver2, Origin, scale, 0);
            }
            else
            {
                Origin = new Vector2(MuzzleFlash.Width / 2, MuzzleFlash.Height*0.86f);
                scale = new Vector2(0.2f) * QuintInOut(progress);

                spriteBatch.UseBlendState(BlendState.Additive);
                for(int i = 0; i< 6; i++)
                {
                    Main.EntitySpriteDraw(MuzzleFlash, drawPos, null, color, Rotation + MathHelper.PiOver2, Origin, scale, 0);
                }
            }
           
        }

        private static float QuintInOut(float x)
        {
            if (x < 0.5f)
                return 16 * x * x * x * x * x;
            else
                return 1 - MathF.Pow(-2 * x + 2, 5) / 2;
        }


    }
}
