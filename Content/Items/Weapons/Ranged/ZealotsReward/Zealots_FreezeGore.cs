using Luminance.Core.Graphics;
using Terraria.GameContent;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.ZealotsReward
{
    internal class Zealots_FreezeGore : ModSystem
    {

        public sealed class FrozenGoreData
        {
            public bool Frozen;
            public bool SuppressVanillaDraw;
            public float FreezeInterpolant;
            public int TimeAtCreation;
            public int TimeUntilShader = 60 * 6;
        }
        public sealed class FreezeRectZone
        {
            public Rectangle Area;
            public int ExpireTime;
            public bool DebugDraw;
            public int TimeUntilShader = 60 * 6;
            public bool IsExpired => Main.GameUpdateCount >= ExpireTime;
        }

        public static Dictionary<Gore, FrozenGoreData> FrozenGores;
        public static HashSet<Gore> GoresToShaderDraw;

        public static List<FreezeRectZone> FreezeZones;
        // im going to leave this in, but commented out for now. just because, i want to eventually port this out.
        //public static MiscShaderData FreezeShader;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            FrozenGores = new();
            GoresToShaderDraw = new();
            FreezeZones = new();

            On_Main.DrawGore += DrawFrozenGoresAfterVanilla;
            On_Gore.GetAlpha += On_Gore_GetAlpha;
            On_Gore.Update += TrackFrozenGoreState;
        }

        public override void Unload()
        {
            FrozenGores = null;
            GoresToShaderDraw = null;
            //FreezeShader = null;
        }

        public override void PostSetupContent()
        {
            if (Main.dedServ)
                return;

            //FreezeShader = GameShaders.Misc["HeavenlyArsenal:FreezeGore"];
        }

        public override void PostUpdateGores()
        {

            CleanupExpiredZones();
        }
        private static void DrawDebugFreezeZones()
        {
            if (FreezeZones is null)
                return;

            foreach (FreezeRectZone zone in FreezeZones)
            {
                if (!zone.DebugDraw || zone.IsExpired)
                    continue;
                Utils.DrawRect(Main.spriteBatch, zone.Area, Color.White);
            }
        }

        private static int IsGoreInsideAnyFreezeZone(Gore gore)
        {
            Rectangle goreRect = GetGoreWorldRect(gore);
            

            foreach (FreezeRectZone zone in FreezeZones)
            {
                if (zone.IsExpired)
                    continue;

                if (zone.Area.Intersects(goreRect))
                    return zone.TimeUntilShader;
            }

            return 0;
        }

        private static Rectangle GetGoreWorldRect(Gore gore)
        {
            Texture2D texture = TextureAssets.Gore[gore.type].Value;
            if (texture is null)
                return new Rectangle((int)gore.position.X, (int)gore.position.Y, 2, 2);

            Rectangle frame = gore.Frame.GetSourceRectangle(texture);

            return new Rectangle(
                (int)gore.position.X,
                (int)gore.position.Y,
                (int)(frame.Width * gore.scale),
                (int)(frame.Height * gore.scale));
        }

        private static void CleanupExpiredZones()
        {
            for (int i = FreezeZones.Count - 1; i >= 0; i--)
            {
                if (FreezeZones[i].IsExpired)
                    FreezeZones.RemoveAt(i);
            }
        }

        public static void AddFreezeZone(Rectangle area, int lifetime, int TimeUntilShader = 60 * 6, bool debugDraw = false)
        {
            
            if (FreezeZones is null)
                return;

            FreezeZones.Add(new FreezeRectZone
            {
                Area = area,
                ExpireTime = (int)Main.GameUpdateCount + lifetime,
                DebugDraw = debugDraw,
                TimeUntilShader = Math.Abs(TimeUntilShader)+1

            });
        }

        public static void AddFreezeZone(Vector2 center, int width, int height, int lifetime, int TimeUntilShader = 60 * 6, bool debugDraw = false)
        {
            Rectangle rect = Utils.CenteredRectangle(center, new Vector2(width, height));
            AddFreezeZone(rect, lifetime, TimeUntilShader, debugDraw);
        }

        private Color On_Gore_GetAlpha(On_Gore.orig_GetAlpha orig, Gore self, Color newColor)
        {
            Color result = orig(self, newColor);

            if (FrozenGores is null)
                return result;

            if (!FrozenGores.TryGetValue(self, out FrozenGoreData data))
                return result;

            if (!data.SuppressVanillaDraw)
                return result;

            GoresToShaderDraw?.Add(self);
            return Color.Transparent;
        }
        private void TrackFrozenGoreState(On_Gore.orig_Update orig, Gore self)
        {
            orig(self);

            if (FrozenGores is null)
                return;

            int a = IsGoreInsideAnyFreezeZone(self);
            if (a > 0)
            {

                if(!FrozenGores.ContainsKey(self))
                MarkFrozen(self, 0, a);
                else
                {
                    if (FrozenGores.TryGetValue(self, out var val))
                    {
                        val.TimeUntilShader = a;
                    }
                }
            }


            if (!self.active)
            {
                FrozenGores.Remove(self);
                GoresToShaderDraw?.Remove(self);
                return;
            }

            if (FrozenGores.TryGetValue(self, out FrozenGoreData data))
            {
                if (Main.GameUpdateCount - data.TimeAtCreation < data.TimeUntilShader)
                    return;

                float target = data.Frozen ? 2f : 0f;
                data.FreezeInterpolant = MathHelper.Lerp(data.FreezeInterpolant, target, 0.01f);
               
                data.SuppressVanillaDraw = true;
                if (!data.Frozen && data.FreezeInterpolant <= 0.01f)
                {
                    FrozenGores.Remove(self);

                    GoresToShaderDraw?.Remove(self);
                }
            }
        }



        private void DrawFrozenGoresAfterVanilla(On_Main.orig_DrawGore orig, Main self)
        {


            orig(self);

            if (Main.dedServ)
                return;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(
                SpriteSortMode.Immediate,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                Main.Rasterizer,
                null,
                Main.GameViewMatrix.TransformationMatrix
            );

            foreach (Gore gore in GoresToShaderDraw)
            {
                if (gore is null || !gore.active)
                    continue;

                if (!FrozenGores.TryGetValue(gore, out FrozenGoreData data))
                    continue;

                if (data.FreezeInterpolant <= 0f)
                    continue;

                DrawFrozenGore(gore, data);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                Main.Rasterizer,
                null,
                Main.GameViewMatrix.TransformationMatrix
            );


            DrawDebugFreezeZones();
        }

        private static void DrawFrozenGore(Gore gore, FrozenGoreData data)
        {
            Texture2D texture = TextureAssets.Gore[gore.type].Value;
            if (texture is null)
                return;

            Rectangle frame = GetFrame(gore, texture);
            Vector2 origin = frame.Size() * 0.5f;
            Vector2 drawPosition = gore.position - Main.screenPosition + origin + gore.drawOffset;

            Color drawColor = Lighting.GetColor(gore.position.ToTileCoordinates());


            float progress = data.FreezeInterpolant;

            Color edgeColor = Color.Lerp(Color.CadetBlue, Color.White, progress);
            //Utils.DrawBorderString(Main.spriteBatch, data.FreezeInterpolant.ToString(), drawPosition, drawColor);
            var Dissolve = ShaderManager.GetShader("HeavenlyArsenal.DissolveShader");
            Dissolve.SetTexture(texture, 0);
            Dissolve.SetTexture(GennedAssets.Textures.Noise.CrackedNoiseA, 1);
            Dissolve.TrySetParameter("dissolveProgress", progress);
            Dissolve.TrySetParameter("edgeWidth", 0.2f);
            Dissolve.TrySetParameter("opacity", 1);
            Dissolve.TrySetParameter("edgeColor", edgeColor.ToVector4());
            Dissolve.Apply();

            Main.EntitySpriteDraw(
                texture,
                drawPosition,
                frame,
                drawColor,
                gore.rotation,
                origin,
                gore.scale,
                SpriteEffects.None,
                0f
            );
        }

        private static Rectangle GetFrame(Gore gore, Texture2D texture)
        {
            if (gore.numFrames <= 1)
                return texture.Bounds;

            int frameHeight = texture.Height / gore.numFrames;
            return gore.Frame.GetSourceRectangle(texture);
        }


        public static void MarkFrozen(Gore gore, float initialInterpolant = 1f, int TimeUntilShader = 6 * 60)
        {
            if (gore is null || !gore.active || FrozenGores is null)
                return;

            //put the gore in the bag
            if (!FrozenGores.TryGetValue(gore, out FrozenGoreData data))
            {
                data = new FrozenGoreData();
                FrozenGores[gore] = data;
            }
            data.TimeAtCreation = (int)Main.GameUpdateCount;
            data.TimeUntilShader = data.TimeAtCreation + TimeUntilShader;
            data.Frozen = true;
            data.FreezeInterpolant = initialInterpolant;
        }

        public static void UnmarkFrozen(Gore gore)
        {
            if (gore is null || FrozenGores is null)
                return;

            if (FrozenGores.TryGetValue(gore, out FrozenGoreData data))
                data.Frozen = false;
        }


    }
}