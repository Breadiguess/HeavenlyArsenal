using CalamityMod;
using HeavenlyArsenal.Common.utils;
using Luminance.Assets;
using NoxusBoss.Assets;
using NoxusBoss.Core.Graphics.Automators;
using NoxusBoss.Core.Graphics.SpecificEffectManagers;

namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Mage
{
    internal class BloodEcho_Projectile : ModProjectile, IDrawSubtractive
    {
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        public Player Owner => Main.player[Projectile.owner];

        public Player Clone = new Player();
        public Rope rope;
        public int Index;

        public int Time
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.Size = Owner.Size;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 600;
            Projectile.extraUpdates = 1;

            rope = new Rope(Projectile.Center, Projectile.Center, 40, 1, Vector2.Zero);
        }

        public override bool? CanDamage() => false;
        public override void PostAI()
        {

            CheckDespawnConditions();

            Clone = (Player)Owner.Clone();
            Main.NewText(Clone.HeldItem);
            LocalPlayerDrawManager.StopCondition = () => !Projectile.active;
            LocalPlayerDrawManager.ShaderDrawAction = () => { };
            Projectile.Center = Vector2.Lerp(Projectile.Center, Owner.Center + new Vector2(100 * (Index % 2 == 0 ? -1 : 1), 0), 0.5f);//Owner.Center + new Vector2(100 * (Index % 2 == 0 ? -1 : 1), 0);
            Clone.Center = Projectile.Center;

            if (Clone.HeldItem.ModItem != null)
            {
                Clone.HeldItem.ModItem.UseItem(Clone);
                Clone.Calamity().mouseWorld = Owner.Calamity().mouseWorld;
                
                Clone.ItemCheck();
            }
                
            Clone.HeldItem.channel = Owner.HeldItem.channel;
            if (Owner.controlUseItem && (Owner.itemTime == Owner.itemTimeMax) || Owner.channel)
            {
                Vector2 direction = Projectile.Center.DirectionTo(Main.MouseWorld);
                Projectile.direction = Math.Sign(direction.X);
                if (Projectile.owner == Main.myPlayer)
                {

                    Clone.controlUseItem = true;
                }
            }

            rope.segments[0].position = Projectile.Center;
            rope.segments[^1].position = Owner.Center;
            rope.damping = 0.3f;
            rope.Update();

            Time++;
        }
        void CheckDespawnConditions()
        {
            BloodBlightParasite_Player symbiote = Owner.GetModPlayer<BloodBlightParasite_Player>();

            if (Owner.dead || !symbiote.Ascended)
            {
                Projectile.active = false;
            }
            Projectile.timeLeft++;
        }

        void RenderClone()
        {
            //lmao. who could've guessed that the best way around it, was to just use both?

            Matrix transformation = Projectile.isAPreviewDummy ? Main.UIScaleMatrix : Main.GameViewMatrix.TransformationMatrix;
            Vector2 scale = Vector2.One / new Vector2(Main.GameViewMatrix.TransformationMatrix.M11, Main.GameViewMatrix.TransformationMatrix.M22);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, transformation);

            SpriteEffects direction = SpriteEffects.None;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition - Vector2.UnitY * Main.LocalPlayer.gfxOffY + Vector2.UnitY * Projectile.gfxOffY;

            float rotation = Projectile.rotation;
            Texture2D texture = LocalPlayerDrawManager.PlayerTarget;
            Main.spriteBatch.Draw(texture, drawPosition, null, Color.White * Projectile.Opacity, rotation, texture.Size() * 0.5f, scale, direction, 0f);
            //Main.spriteBatch.Draw(texture, drawPosition, null, Color.White * Projectile.Opacity * MathF.Pow(1f - pulse, 2f), rotation, texture.Size() * 0.5f, scale * (1f + pulse * 0.9f), direction, 0f);

            Main.spriteBatch.ResetToDefault();


            Clone.position = Projectile.position;
            Clone.CopyVisuals(Owner);
            Color red = Color.Crimson;
            Clone.skinColor = red;
            Clone.shirtColor = red;
            Clone.underShirtColor = red;
            Clone.pantsColor = red;
            Clone.shoeColor = red;
            Clone.hairColor = red;
            Clone.eyeColor = Color.Red;

            // update everything for our little dummy player
            Clone.ResetEffects();
            Clone.ResetVisibleAccessories();
            Clone.DisplayDollUpdate();
            Clone.UpdateSocialShadow();
            Clone.UpdateDyes();
            Clone.PlayerFrame();
            // copy the player's arm movements while swinging, otherwise idle
            if (Owner.ItemAnimationActive && Owner.altFunctionUse != 2)
                Clone.bodyFrame = Owner.bodyFrame;

            Main.PlayerRenderer.DrawPlayer(Main.Camera, Clone, Clone.position, 0f, Clone.fullRotationOrigin, 0f, 1f);


        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D glow = GennedAssets.Textures.GreyscaleTextures.BloomCirclePinpoint;


            Vector2 DrawPos = Projectile.Center - Main.screenPosition;
            Vector2 Origin = glow.Size() / 2f;
            Main.EntitySpriteDraw(glow, DrawPos, null, Color.Crimson with { A = 0 }, 0f, Origin, new Vector2(0.3f, 0.465f), 0);
            if (rope != null)
            {
                for (int i = 0; i < rope.segments.Length - 1; i++)
                {
                    Color color = Color.Lerp(Color.Red, Color.Crimson, i / (float)rope.segments.Length);
                    Utils.DrawLine(Main.spriteBatch, rope.segments[i].position, rope.segments[i + 1].position, color, color, 3);
                }
            }
            RenderClone();


            return false;
        }

        void IDrawSubtractive.DrawSubtractive(SpriteBatch spriteBatch)
        {

            Texture2D glow = GennedAssets.Textures.GreyscaleTextures.BloomCirclePinpoint;


            Vector2 DrawPos = Projectile.Center - Main.screenPosition;
            Vector2 Origin = glow.Size() / 2f;
            Main.EntitySpriteDraw(glow, DrawPos, null, Color.White, 0f, Origin, new Vector2(0.3f, 0.465f), 0);
        }
    }
}
