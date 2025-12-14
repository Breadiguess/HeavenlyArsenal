using NoxusBoss.Assets;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Jellyfish;

internal partial class BloodJelly
{
    public float OpenInterpolant;

    private float warningPulseSpeed;

    private void RenderRailgunTelegraph(SpriteBatch spriteBatch, Vector2 screenPos)
    {
        if (CurrentState != Behavior.Railgun)
        {
            return;
        }

        const int lockOnDuration = 45;
        const int chargeDuration = 15;
        const int fireInterval = 60;

        var attackCycleTime = Time % fireInterval;

        // Only show during charge-up phase
        if (attackCycleTime < lockOnDuration || attackCycleTime >= lockOnDuration + chargeDuration)
        {
            return;
        }

        Texture2D pixel = GennedAssets.Textures.GreyscaleTextures.BloomLine2;
        var start = NPC.Center - screenPos;
        var dir = (NPC.rotation - MathHelper.PiOver2).ToRotationVector2();

        var Origin = new Vector2(pixel.Width / 2, 0);

        var chargeFactor = Math.Clamp((attackCycleTime - lockOnDuration) / (float)chargeDuration, 0f, 1f);
        var beamLength = 10000f;
        var beamThickness = 1f + 3f * chargeFactor;

        var color = Color.Lerp
        (
            Color.Transparent,
            Color.Crimson with
            {
                A = 0
            },
            chargeFactor
        );

        spriteBatch.Draw
        (
            pixel,
            start,
            null,
            color,
            NPC.rotation + float.Pi,
            Origin,
            new Vector2(beamThickness, beamLength / pixel.Height),
            0,
            0
        );
    }

    private void RenderRailgun(float Rotation, Vector2 screenPos)
    {
        Texture2D placeholder = GennedAssets.Textures.GreyscaleTextures.WhitePixel;

        // Main.EntitySpriteDraw(placeholder, NPC.Center - screenPos, null, Color.Aqua, NPC.rotation, placeholder.Size() * 0.5f, 5, 0);
        var pos = NPC.Center - screenPos;
        var Origin = placeholder.Size() * 0.5f + new Vector2(0, 0.5f);
        var thing = Math.Clamp(1 - recoilInterp, 0, 1);
        var Scale = new Vector2(12, 150 * OpenInterpolant * thing);
        Main.EntitySpriteDraw(placeholder, pos, null, Color.AntiqueWhite, NPC.rotation, Origin, Scale, 0);
    }

    //todo: maybe primitive cap? that would be cool.
    private void RenderCap(Vector2 screenPos, Color drawColor, float rot)
    {
        var cap = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Jellyfish/JellyFish_Cap").Value;
        var DrawPos = NPC.Center - new Vector2(0, 10).RotatedBy(rot) - screenPos;

        RenderRailgun(rot, screenPos);

        var Scale = new Vector2(1, 1);
        var remap = (int)Utils.Remap(OpenInterpolant, 0, 1, 0, 5);

        for (var i = 0; i < 2; i++)
        {
            var Frame = cap.Frame(2, 5, i, remap);
            var origin = new Vector2(i != 1 ? Frame.Width : 0, Frame.Height - 36);

            float offset = 0; //i != 1 ? -30 : 30;

            //offset *= OpenInterpolant;
            offset = MathHelper.ToRadians(offset);
            Main.EntitySpriteDraw(cap, DrawPos, Frame, drawColor * 0.8f, rot + offset, origin, Scale, 0);
        }
    }

    private void RenderExplosiveWarning(Vector2 pos)
    {
        Texture2D glow = GennedAssets.Textures.GreyscaleTextures.BloomCircleSmall;
        var pulse = warningPulseSpeed;

        // more intensity near explosion
        var progress = MathHelper.Clamp(Time / 300f, 0f, 1f);
        // float intensity = MathHelper.Lerp(0.4f, 1.4f, MathF.Pow(progress, 2f));

        Main.EntitySpriteDraw
        (
            glow,
            pos,
            null,
            Color.Purple with
            {
                A = 0
            } *
            pulse,
            0,
            glow.Size() * 0.5f,
            1f,
            0
        );
    }

    private void bestiaryTendrils()
    {
        var BodyRot = (NPC.rotation + MathHelper.PiOver2).ToRotationVector2();
        var waveSpeed = 01f;
        var waveStrength = 100f;
        var segmentLength = 3f;

        if (Tendrils.Count == 0)
        {
            for (var i = 0; i < tendrilCount; i++)
            {
                if (i < tendrilCount - 1)
                {
                    if (i % 2 != 0)
                    {
                        Tendrils.Add(i, (new Vector2[23], new Vector2[23]));
                    }

                    else
                    {
                        Tendrils.Add(i, (new Vector2[17], new Vector2[17]));
                    }
                }
                else
                {
                    Tendrils.Add(i, (new Vector2[30], new Vector2[30]));
                }
            }
        }

        for (var j = 0; j < Tendrils.Count; j++)
        {
            var _tendrilPos = Tendrils[j].Item1;
            var _tendrilVel = Tendrils[j].Item2;

            _tendrilPos[0] = NPC.Center;

            for (var i = 1; i < _tendrilPos.Length; i++)
            {
                if (_tendrilPos[i] == Vector2.Zero)
                {
                    _tendrilPos[i] = NPC.Center;
                }

                var offset = MathHelper.ToRadians(-45.74f);

                //if (j % 2 == 0)
                //offset = 24.75f;
                var wave = MathHelper.ToRadians((float)Math.Sin(Main.GlobalTimeWrappedHourly * waveSpeed) * (1 + waveStrength));
                offset = float.Lerp(offset, wave, 0.2f);
                float RotationOffset = 0;
                RotationOffset = j % 3 == 0 ? offset : -offset;
                //RotationOffset = j % 2 == 0 ? offset:offset ;

                if (j == tendrilCount - 1)
                {
                    if (CurrentState != Behavior.StickAndExplode)
                    {
                        RotationOffset = 0;
                    }
                    else
                    {
                        var thing = MathHelper.ToRadians(MathF.Sin(Main.GlobalTimeWrappedHourly) * 46.75f);
                        RotationOffset = float.Lerp(RotationOffset, thing, 1f);
                    }
                    //Main.NewText(RotationOffset);
                }

                //Vector2 perp = BodyRot.RotatedBy(MathHelper.PiOver2 * wave * waveStrength / 20f);

                var targetPos = _tendrilPos[i - 1] + BodyRot.RotatedBy(RotationOffset) * segmentLength;

                var alignVel = (targetPos - _tendrilPos[i]) * 0.5f;

                if (j != tendrilCount - 1)
                {
                    _tendrilVel[i] = Vector2.Lerp(_tendrilVel[i], alignVel, 1f);
                }
                else
                {
                    _tendrilVel[i] = Vector2.Lerp(_tendrilVel[i], alignVel, 1f);
                }

                _tendrilPos[i] += _tendrilVel[i];

                Utils.DrawBorderString(Main.spriteBatch, _tendrilVel[i].ToString(), _tendrilPos[i] - Main.screenPosition, Color.AntiqueWhite);

                if (_tendrilPos[i] == Vector2.Zero)
                {
                    _tendrilPos[i] = NPC.Center;
                }
            }
        }
    }

    private void RenderTendrils(Vector2 screenPos, Color drawColor)
    {
        if (NPC.IsABestiaryIconDummy)
        {
            bestiaryTendrils();
        }

        var tex = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Jellyfish/JellyTentacle").Value;

        for (var j = 0; j < Tendrils.Count; j++)
        {
            var _tendrilPos = Tendrils[j].Item1;

            for (var i = 0; i < _tendrilPos.Length - 1; i++)
            {
                var DrawPos = _tendrilPos[i] - tendrilOffsets[j].RotatedBy(NPC.rotation) - screenPos;

                var style = 0;

                if (i == _tendrilPos.Length - 3)
                {
                    style = 0;
                }

                if (i > _tendrilPos.Length - 3)
                {
                    style = 1;
                }

                var frame = tex.Frame(1, 2, 0, style);

                var rotation = _tendrilPos[i].AngleTo(_tendrilPos[i + 1]);

                var segmentCount = _tendrilPos.Length;
                var t = 0f;

                if (segmentCount > 1)
                {
                    t = i / (float)(segmentCount - 1); // 0 at base, 1 at tip
                }

                var baseWidth = 1.9f; // thicker near body
                var tipWidth = 0.65f; // thinner near tip

                // Horizontal thickness (X) tapers from baseWidth to tipWidth
                var width = MathHelper.Lerp(baseWidth, tipWidth, t);

                // Vertical stretch based on actual distance to next segment and texture height
                var segmentDistance = _tendrilPos[i].Distance(_tendrilPos[i + 1]);
                var lengthFactor = 1f;
                float denom = Math.Max(1, frame.Height - 5);
                lengthFactor = segmentDistance / denom * 1.2f;

                // Combine into final stretch vector and apply a small global multiplier for visual tuning
                var stretch = new Vector2(width, lengthFactor) * 1.2f;
                var Origin = frame.Size() * 0.5f;

                if (i == _tendrilPos.Length - 2)
                {
                    stretch = Vector2.One;
                    Origin = new Vector2(frame.Width / 2, 2);
                }

                Main.EntitySpriteDraw(tex, DrawPos, frame, drawColor, rotation - MathHelper.PiOver2, Origin, stretch, 0);
                var bomb = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Jellyfish/JellyBomb").Value;
                var bombGlow = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Jellyfish/JellyBomb_Glow").Value;

                if (i == _tendrilPos.Length - 2 && j == tendrilCount - 1)
                {
                    Main.EntitySpriteDraw(bomb, DrawPos, null, drawColor, rotation - MathHelper.PiOver2, new Vector2(bomb.Width / 2, 0), 1, 0);
                    Main.EntitySpriteDraw(bombGlow, DrawPos, null, Color.White, rotation - MathHelper.PiOver2, new Vector2(bomb.Width / 2, 0), 1, 0);
                    RenderExplosiveWarning(DrawPos + new Vector2(bomb.Width / 2, 0).RotatedBy(rotation));
                }
            }
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        RenderRailgunTelegraph(spriteBatch, screenPos);

        RenderTendrils(screenPos, drawColor);
        var debug = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Jellyfish/JellyFish_Core").Value;
        var glow = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Jellyfish/JellyFish_Core_Glow").Value;
        var Scale = new Vector2(1 + 0.1f * MathF.Sin(CosmeticTime / 10.1f + 10), 1);
        Main.EntitySpriteDraw(debug, NPC.Center - screenPos, null, drawColor, NPC.rotation, debug.Size() * 0.5f, Scale, 0);

        Main.EntitySpriteDraw(glow, NPC.Center - screenPos, null, Color.AntiqueWhite, NPC.rotation, debug.Size() * 0.5f, Scale, 0);

        RenderCap(screenPos, drawColor, NPC.rotation);

        //Utils.DrawBorderString(spriteBatch, CurrentState.ToString() + $"\n {Time}", NPC.Center - screenPos, Color.AntiqueWhite);
        return false; // base.PreDraw(spriteBatch, screenPos, drawColor);
    }
}