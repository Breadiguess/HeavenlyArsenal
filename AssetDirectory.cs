using System.Collections.Generic;
using CalamityMod.Items.Accessories;
using HeavenlyArsenal.Common.utils;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace HeavenlyArsenal;

public static class AssetDirectory
{
    public static readonly string AssetPath = $"{nameof(HeavenlyArsenal)}/Assets/";

    public static class Textures
    {
        //public static readonly Asset<Texture2D> Sparkle = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/Extra/Sparkle");
        //public static readonly Asset<Texture2D> ShockRing = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/Extra/ShockRing");
        #region blockaroz stuff
        public static readonly Asset<Texture2D> BigGlowball = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/Extra/BigGlowball");
        public static readonly Asset<Texture2D> VoidLake = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/Extra/VoidLake");
        public static readonly Asset<Texture2D> VoidLakeShadowArm = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/Extra/VoidLakeShadowArm");
        public static readonly Asset<Texture2D> VoidLakeShadowHand = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/Extra/VoidLakeShadowHand");
        public static readonly Asset<Texture2D> HeatLightning = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/Particles/HeatLightning");
        #endregion
        #region UmbralLeech
        public static readonly Asset<Texture2D> UmbralLeechWhisker = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/NPCs/Hostile/BloodMoon/UmbralLeech/UmbralLeech_Whisker");
        #endregion
        public static class Inventors
        {
           
        }

        public static class Buffs
        {
         //   public static readonly Asset<Texture2D>[] SlimeCane = AssetUtilities.RequestArrayImmediate<Texture2D>(AssetPath + "Textures/Buffs/SlimeCane_", 2);
        }

        public static class Bars
        {
         public static readonly Asset<Texture2D>[] Bar = AssetUtilities.RequestArrayTotalImmediate<Texture2D>(AssetPath + "Textures/UI/Bars/BarBase_");
         public static readonly Asset<Texture2D>[] BarFill = AssetUtilities.RequestArrayTotalImmediate<Texture2D>(AssetPath + "Textures/UI/Bars/BarFill_");

        }


    }
    public static class PrimShaders 
    {
        /// <summary>
        /// Samples the provided 'sampleTexture' along the primitive (Texture must be horizontal)<br/>
        /// Sampling coordinates can be tiled or stretched using 'repeats', and scrolled with 'scroll' <br/>
        /// Sampled color is multiplied by the primitive's vertex colors
        /// thank you ibabbleplays
        /// <code>
        /// float4 scroll;
        /// float2 repeats;
        /// texture sampleTexture;
        /// </code>
        /// 
        /// Used by <see cref="VerletNet"/> when rendering non-framed verlet links, notably <see cref="Crabulon"/>'s numerous vines
        /// </summary>
        //public static Effect TextureMap => Scene["Primitive_TextureMap"].GetShader().Shader;

    }
    public static class Music
    {
      //  public static readonly int GoozmaPhase1 = MusicLoader.GetMusicSlot(AssetPath + "Music/GlutinousArbitration");
      //  public static readonly int GoozmaPhase2 = MusicLoader.GetMusicSlot(AssetPath + "Music/ViscousDesperation");

        //auric soul shorts
      //  public static readonly int ChromaticSoul = MusicLoader.GetMusicSlot(AssetPath + "Music/Souls/Iridescence");
      //  public static readonly int DraconicSoul = MusicLoader.GetMusicSlot(AssetPath + "Music/Souls/YharonAuricSoulMusic");
    }

    public static class Sounds
    {
    }

    public static class Effects
    {
        //Basic
        //public static readonly Asset<Effect> BasicTrail = AssetUtilities.RequestImmediate<Effect>(AssetPath + "Effects/BasicTrail");
        //public static readonly Asset<Effect> LightningBeam = AssetUtilities.RequestImmediate<Effect>(AssetPath + "Effects/LightningBeam");
        public static readonly Asset<Effect> FlameDissolve = AssetUtilities.RequestImmediate<Effect>(AssetPath + "AutoloadedEffects/Shaders/FlameDissolve");
        public static readonly Asset<Effect> GoomoireWindR = AssetUtilities.RequestImmediate<Effect>(AssetPath + "AutoloadedEffects/Shaders/GoomoireSuckEffect");
        public static readonly Asset<Effect> GoomoireWindL = AssetUtilities.RequestImmediate<Effect>(AssetPath + "AutoloadedEffects/Shaders/GoomoireSuckEffect2");
        public static readonly Asset<Effect> bloodShader = AssetUtilities.RequestImmediate<Effect>(AssetPath + "AutoloadedEffects/Shaders/BloodBlobShader");
        public static readonly Asset<Effect> FusionRifleCircle = AssetUtilities.RequestImmediate<Effect>(AssetPath + "AutoloadedEffects/Shaders/FusionRifle_Circle");
        public static class Dyes
        {
           // public static readonly Asset<Effect> Goop = AssetUtilities.RequestImmediate<Effect>(AssetPath + "Effects/GoopDye");
           // public static readonly Asset<Effect> Holograph = AssetUtilities.RequestImmediate<Effect>(AssetPath + "Effects/HolographDyeEffect");
        }
    }
}