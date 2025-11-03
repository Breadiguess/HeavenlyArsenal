using System.Collections.Generic;
using CalamityMod.Items.Accessories;
using HeavenlyArsenal.Common.utils;
using Luminance.Common.Utilities;
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

        public static class Items
        {
            public static class Accessories
            {
                public static class VoidCrestOath
                {
                    public static readonly Asset<Texture2D> VoidSigil = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/Items/Accessories/Temp");
                    public static readonly Asset<Texture2D> VoidSigil2 = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/Items/Accessories/Alt");

                }
                /*
                public static class Nightfall
                {
                    public static readonly Asset<Texture2D> Nightfall = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/Items/Accessories/Nightfall/Nightfall");
                    public static readonly Asset<Texture2D> Nightfall_Glow = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/Items/Accessories/Nightfall/Nightfall_Glow");
                    public static readonly Asset<Texture2D> Nightfall_Orb = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/Items/Accessories/Nightfall/Nightfall_Orb");
                }
                public static class CombatStim
                {
                    public static readonly Asset<Texture2D> CombatStim = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/Items/Accessories/CombatStim/CombatStim");
                    public static readonly Asset<Texture2D> CombatStim_Glow = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/Items/Accessories/CombatStim/CombatStim_Glow");
                }
                */
            }

            public static class Weapons
            {
                public static class Rogue
                {
                    public static readonly Asset<Texture2D> BolaBall = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/Items/Weapons/BolaBall");

                }
            }
        }



        #region blockaroz stuff
        public static readonly Asset<Texture2D> BigGlowball = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/Extra/BigGlowball");
        public static readonly Asset<Texture2D> VoidLake = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/Extra/VoidLake");
        public static readonly Asset<Texture2D> VoidLakeShadowArm = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/Extra/VoidLakeShadowArm");
        public static readonly Asset<Texture2D> VoidLakeShadowHand = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/Extra/VoidLakeShadowHand");
        public static readonly Asset<Texture2D> HeatLightning = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/Particles/HeatLightning");
        #endregion
        public static readonly Asset<Texture2D> GlowCone = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/Extra/Glow_2");


        #region UmbralLeech
        public static readonly Asset<Texture2D> UmbralLeechWhisker = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/NPCs/Hostile/BloodMoon/UmbralLeech/UmbralLeech_Whisker");
        public static readonly Asset<Texture2D> UmbralLeechTendril = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/NPCs/Hostile/BloodMoon/UmbralLeech/UmbralLeech_TailTendril");
        public static readonly Asset<Texture2D> UmbralLeechTelegraph = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/NPCs/Hostile/BloodMoon/UmbralLeech/LeechTelegraph");
        public static readonly Asset<Texture2D> UmbralLeech_Legs = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/NPCs/Hostile/BloodMoon/UmbralLeech/UmbralLeech_Legs");

        public static readonly Asset<Texture2D> Umbral_Sore = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/NPCs/Hostile/BloodMoon/UmbralLeech/Umbral_Sore");
        #endregion

        public static class Bars
        {
            public static readonly Asset<Texture2D>[] Bar = AssetUtilities.RequestArrayTotalImmediate<Texture2D>(AssetPath + "Textures/UI/Bars/BarBase_");
            public static readonly Asset<Texture2D>[] BarFill = AssetUtilities.RequestArrayTotalImmediate<Texture2D>(AssetPath + "Textures/UI/Bars/BarFill_");

        }

        public static class BadSun
        {
            public static readonly Asset<Texture2D> GlowOutline = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/Items/BadSun/Glow_3");
            public static readonly Asset<Texture2D> Eye = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/Items/BadSun/BadSunEye_1");

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


    public static class Sounds
    {
        internal static class Projectiles
        {
            internal static class BloodNeedle
            {
                public static readonly SoundStyle NeedleStrike = new("HeavenlyArsenal/Assets/Sounds/Projectiles/BloodNeedle/NeedleStrike_", 2);
            }
        }
        internal static class NPCs
            {
                internal static class Hostile
                {
                    internal static class BloodMoon
                    {
                        internal static class UmbralLeech
                        {
                            public static readonly SoundStyle Bash = new SoundStyle("HeavenlyArsenal/Assets/Sounds/NPCs/Hostile/BloodMoon/UmbralLeech/UmbralLeech_Bash_", 3);

                            public static readonly SoundStyle Explode = new SoundStyle("HeavenlyArsenal/Assets/Sounds/NPCs/Hostile/BloodMoon/UmbralLeech/GORE - Head_Crush_3");

                            public static readonly SoundStyle GibletDrop = new SoundStyle("HeavenlyArsenal/Assets/Sounds/NPCs/Hostile/BloodMoon/UmbralLeech/GORE - Giblet_Drop_3");
                            public static readonly SoundStyle DyingNoise = new SoundStyle("HeavenlyArsenal/Assets/Sounds/NPCs/Hostile/BloodMoon/UmbralLeech/Dying1");
                        }
                    }
                }
            }
        public static class Nightfall
            {
                public static readonly SoundStyle Nightfall_Burst = new SoundStyle("HeavenlyArsenal/Assets/Sounds/Items/Accessories/Nightfall/Nightfall_Burst");
                public static readonly SoundStyle Nightfall_Burst_Hard = new SoundStyle("HeavenlyArsenal/Assets/Sounds/Items/Accessories/Nightfall/Nightfall_Burst_Hard");
                public static readonly SoundStyle Nightfall_Burst_Heavy = new SoundStyle("HeavenlyArsenal/Assets/Sounds/Items/Accessories/Nightfall/Nightfall_Burst_Heavy");

                public static readonly SoundStyle Nightfall_Windup = new SoundStyle("HeavenlyArsenal/Assets/Sounds/Items/Accessories/Nightfall/Nightfall_Windup");
                public static readonly SoundStyle Nightfall_3 = new SoundStyle(AssetPath + "Sounds/Items/Accessories/Nightfall/Nightfall_3");

                public static readonly SoundStyle Hit = new SoundStyle(AssetPath + "Sounds/Items/Accessories/Nightfall/Nightfall_Hit");

            }

        public static class Items
        {
            public static class CombatStim
            {
                    public static readonly SoundStyle PsychosisWhisper = new SoundStyle(AssetPath + "Sounds/Items/CombatStim/PsychosisWhisper_", 5);
            }
            public static class Weapons
            {
                public static class Rapture
                {
                    public static readonly SoundStyle Swing = new SoundStyle(AssetPath + "Sounds/Items/Melee/Swing1");
                    public static readonly SoundStyle Swing2 = new SoundStyle(AssetPath + "Sounds/Items/Melee/Swing2");
                    public static readonly SoundStyle Collapse = new SoundStyle(AssetPath + "Sounds/Items/Melee/Collapse_1");

                    public static readonly SoundStyle CollapseImpact = new SoundStyle(AssetPath + "Sounds/Items/Melee/Collapse_Impact");
                }
            }
        }
        

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