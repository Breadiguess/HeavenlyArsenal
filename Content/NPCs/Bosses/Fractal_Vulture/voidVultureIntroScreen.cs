using InfernumMode.Assets.Fonts;
using InfernumMode.Content.BossIntroScreens;
using InfernumMode.Content.BossIntroScreens.InfernumScreens;
using NoxusBoss.Assets;
using ReLogic.Content;
using ReLogic.Graphics;
using System.Globalization;
using System.Linq;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.UI.Chat;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture
{
    [JITWhenModsEnabled("InfernumMode")]
    [ExtendsFromMod("InfernumMode")]
    public class voidVultureIntroScreen : BaseIntroScreen
    {
        internal Vector2 CalculateOffsetOfCharacter(string character)
        {
            float extraOffset = character.ToLower(CultureInfo.InvariantCulture) == "i" ? 9f : 0f;
            return Vector2.UnitX * (InfernumFontRegistry.BossIntroScreensFont.MeasureString(character).X + extraOffset + 10f) * AspectRatioFactor * TextScale;
        }

        public override void DrawText(SpriteBatch sb)
        {
            float opacity =
                Utils.GetLerpValue(TextDelayInterpolant, TextDelayInterpolant + 0.05f, AnimationCompletion, true) *
                Utils.GetLerpValue(1f, 0.97f, AnimationCompletion, true);

            if (CanPlaySound && SoundToPlayWithTextCreation != null && !HasPlayedMainSound)
            {
                SoundEngine.PlaySound(SoundToPlayWithTextCreation.Value);
                HasPlayedMainSound = true;
            }

            int absoluteLetterCounter = 0;
            bool playedNewLetterSound = false;

            string[] splitTextInstances = CachedText.Split('\n');

            for (int i = 0; i < splitTextInstances.Length; i++)
            {
                string splitText = splitTextInstances[i];

                // ------------------------------------
                // FONT SELECTION PER LINE
                // ------------------------------------
                bool isBossNameLine = i == splitTextInstances.Length - 1;

                DynamicSpriteFont font =
                    isBossNameLine
                        ? Mod.Assets.Request<DynamicSpriteFont>("Assets/Fonts/WINDLISTENERGRAPHIC", AssetRequestMode.ImmediateLoad).Value   // BIG TITLE FONT
                        : InfernumFontRegistry.BossIntroScreensFont;
                // SUBTITLE FONT

                float lineScaleMultiplier = isBossNameLine ? BottomTextScale : 1f;

                Vector2 offset =
                    -Vector2.UnitX *
                    splitText.Sum(c => CalculateOffsetOfCharacter(c.ToString()).X * lineScaleMultiplier) *
                    0.5f;

                Vector2 textScale = Vector2.One * TextScale * AspectRatioFactor * lineScaleMultiplier;

                if (i > 0)
                    offset.Y += TextScale * AspectRatioFactor * i * 24f * lineScaleMultiplier;

                for (int j = 0; j < splitText.Length; j++)
                {
                    float individualLineLetterCompletionRatio = j / (float)(splitText.Length - 1f);
                    float absoluteLineLetterCompletionRatio = absoluteLetterCounter / (float)(CachedText.Length - 1f);

                    int previousTotalLettersToDisplay =
                        (int)(CachedText.Length * LetterDisplayCompletionRatio(AnimationTimer - 1));

                    int totalLettersToDisplay =
                        (int)(CachedText.Length * LetterDisplayCompletionRatio(AnimationTimer));

                    if (totalLettersToDisplay > previousTotalLettersToDisplay &&
                        SoundToPlayWithLetterAddition != null &&
                        !playedNewLetterSound)
                    {
                        SoundEngine.PlaySound(SoundToPlayWithLetterAddition.Value);
                        playedNewLetterSound = true;
                    }

                    if (absoluteLineLetterCompletionRatio >= LetterDisplayCompletionRatio(AnimationTimer))
                        break;

                    string character = splitText[j].ToString();
                    
                    Vector2 charOffset = CalculateOffsetOfCharacter(character) * lineScaleMultiplier;
                    offset += charOffset;

                    if (character == " ")
                    {
                        absoluteLetterCounter++;
                        continue;
                    }

                    if (ShaderToApplyToLetters != null)
                    {
                        ShaderToApplyToLetters.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
                        ShaderToApplyToLetters.Parameters["uLetterCompletionRatio"]?.SetValue(individualLineLetterCompletionRatio);
                        PrepareShader(ShaderToApplyToLetters);
                        ShaderToApplyToLetters.CurrentTechnique.Passes[0].Apply();
                    }

                    Color textColor = TextColor.Calculate(individualLineLetterCompletionRatio) * opacity;
                    Vector2 origin = Vector2.UnitX * font.MeasureString(character);

                    // ------------------------------------
                    // AFTERIMAGES
                    // ------------------------------------
                    for (int k = 0; k < 4; k++)
                    {
                        float afterimageOpacityInterpolant =
                            Utils.GetLerpValue(1f, TextDelayInterpolant + 0.05f, AnimationCompletion, true);

                        float afterimageOpacity = MathF.Pow(afterimageOpacityInterpolant, 2f) * 0.3f;
                        Color afterimageColor = textColor * afterimageOpacity;

                        Vector2 drawOffset =
                            (MathHelper.TwoPi * k / 4f).ToRotationVector2() *
                            (1f - afterimageOpacityInterpolant) *
                            30f;

                        ChatManager.DrawColorCodedStringShadow(
                            sb,
                            font,
                            character,
                            DrawPosition + drawOffset + offset,
                            Color.Black * afterimageOpacity * opacity,
                            0f,
                            origin,
                            textScale,
                            -1,
                            1.5f
                        );

                        ChatManager.DrawColorCodedString(
                            sb,
                            font,
                            character,
                            DrawPosition + drawOffset + offset,
                            afterimageColor,
                            0f,
                            origin,
                            textScale
                        );
                    }

                    // ------------------------------------
                    // BASE GLYPH
                    // ------------------------------------
                    ChatManager.DrawColorCodedStringShadow(
                        sb,
                        font,
                        character,
                        DrawPosition + offset,
                        Color.Black * opacity,
                        0f,
                        origin,
                        textScale,
                        -1,
                        1.5f
                    );

                    ChatManager.DrawColorCodedString(
                        sb,
                        font,
                        character,
                        DrawPosition + offset,
                        textColor,
                        0f,
                        origin,
                        textScale
                    );

                    absoluteLetterCounter++;
                }
            }
        }


        public override int AnimationTime => 660;
        public override float TextDelayInterpolant => 0.1f;
        public override float TextScale => 1.2f;
        public override LocalizedText TextToDisplay =>
            Language.GetText("Mods.HeavenlyArsenal.InfernumIntros.test");
        public override TextColorData TextColor => new(_ =>
        {
            float colorFadeInterpolant = MathF.Sin(AnimationCompletion * MathHelper.Pi * 3f) * 0.5f + 0.5f;
            return Color.Lerp(Color.White, Color.Crimson, colorFadeInterpolant);
        });

        public override bool TextShouldBeCentered => true;

        public override bool ShouldCoverScreen => false;

        public override bool ShouldBeActive()
        {
            int voidVultureIndex = NPC.FindFirstNPC(ModContent.NPCType<voidVulture>());

            if (voidVultureIndex == -1)
                return false;


            return voidVulture.Myself.As<voidVulture>().currentState == voidVulture.Behavior.Roar && InfernumMode.InfernumMode.CanUseCustomAIs;
        }
        public override SoundStyle? SoundToPlayWithTextCreation => GennedAssets.Sounds.Avatar.Angry;
        public override SoundStyle? SoundToPlayWithLetterAddition => GennedAssets.Sounds.Common.TwinkleMuffled.WithPitchOffset(-2f);

        public override float LetterDisplayCompletionRatio(int animationTimer)
        {
            string text = TextToDisplay.Value;
            int newlineIndex = text.IndexOf('\n');
            int totalLength = text.Length;

            // Safety
            if (newlineIndex <= 0)
                return 1f;

            // Base Infernum typing progress (respects TextDelayInterpolant)
            float typingProgress01 = Utils.GetLerpValue(
                TextDelayInterpolant,
                0.6f,
                animationTimer / (float)AnimationTime,
                true
            );

            // Timeline (percentages of typingProgress01)
            const float firstLineEnd = 0.28f; // end of typing
            const float revealSecondLine = 0.65f; // snap full text

            // Phase 1: type FIRST line only
            if (typingProgress01 <= firstLineEnd)
            {
                float t = Utils.GetLerpValue(0f, firstLineEnd, typingProgress01, true);
                int visibleChars = (int)MathHelper.Lerp(0, newlineIndex, t);
                return visibleChars / (float)totalLength;
            }

            // Phase 2: pause (hold first line)
            if (typingProgress01 <= revealSecondLine)
            {
                return newlineIndex / (float)totalLength;
            }

            // Phase 3: snap FULL second line instantly
            return 1f;
        }

    }
}
