using HeavenlyArsenal.Common.utils;
using ReLogic.Content;
using Terraria.Audio;

namespace HeavenlyArsenal;

public static class AssetDirectory
{
    public static class Textures
    {
        public static class Particles
        {
            /// <summary>
            ///     rect: RuneParticle.Frame(2,6);
            /// </summary>
            public static readonly Asset<Texture2D> RuneParticle = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/Particles/BloodRunes");
        }

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

        public static readonly Asset<Texture2D> GlowCone = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/Extra/Glow_2");

        #region blockaroz stuff

        public static readonly Asset<Texture2D> BigGlowball = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/Extra/BigGlowball");

        public static readonly Asset<Texture2D> VoidLake = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/Extra/VoidLake");

        public static readonly Asset<Texture2D> VoidLakeShadowArm = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/Extra/VoidLakeShadowArm");

        public static readonly Asset<Texture2D> VoidLakeShadowHand = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/Extra/VoidLakeShadowHand");

        public static readonly Asset<Texture2D> HeatLightning = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/Particles/HeatLightning");

        #endregion

        #region UmbralLeech

        public static readonly Asset<Texture2D> UmbralLeechWhisker = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/NPCs/Hostile/BloodMoon/UmbralLeech/UmbralLeech_Whisker");

        public static readonly Asset<Texture2D> UmbralLeechTendril = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/NPCs/Hostile/BloodMoon/UmbralLeech/UmbralLeech_TailTendril");

        public static readonly Asset<Texture2D> UmbralLeechTelegraph = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/NPCs/Hostile/BloodMoon/UmbralLeech/LeechTelegraph");

        public static readonly Asset<Texture2D> UmbralLeech_Legs = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/NPCs/Hostile/BloodMoon/UmbralLeech/UmbralLeech_Legs");

        public static readonly Asset<Texture2D> Umbral_Sore = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/NPCs/Hostile/BloodMoon/UmbralLeech/Umbral_Sore");

        #endregion
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
                        public static readonly SoundStyle Bash = new("HeavenlyArsenal/Assets/Sounds/NPCs/Hostile/BloodMoon/UmbralLeech/UmbralLeech_Bash_", 3);

                        public static readonly SoundStyle Explode = new("HeavenlyArsenal/Assets/Sounds/NPCs/Hostile/BloodMoon/UmbralLeech/GORE - Head_Crush_3");

                        public static readonly SoundStyle GibletDrop = new("HeavenlyArsenal/Assets/Sounds/NPCs/Hostile/BloodMoon/UmbralLeech/GORE - Giblet_Drop_3");

                        public static readonly SoundStyle DyingNoise = new("HeavenlyArsenal/Assets/Sounds/NPCs/Hostile/BloodMoon/UmbralLeech/Dying1");
                    }
                }
            }
        }

        public static class Nightfall
        {
            public static readonly SoundStyle Nightfall_Burst = new("HeavenlyArsenal/Assets/Sounds/Items/Accessories/Nightfall/Nightfall_Burst");

            public static readonly SoundStyle Nightfall_Burst_Hard = new("HeavenlyArsenal/Assets/Sounds/Items/Accessories/Nightfall/Nightfall_Burst_Hard");

            public static readonly SoundStyle Nightfall_Burst_Heavy = new("HeavenlyArsenal/Assets/Sounds/Items/Accessories/Nightfall/Nightfall_Burst_Heavy");

            public static readonly SoundStyle Nightfall_Windup = new("HeavenlyArsenal/Assets/Sounds/Items/Accessories/Nightfall/Nightfall_Windup");

            public static readonly SoundStyle Nightfall_3 = new(AssetPath + "Sounds/Items/Accessories/Nightfall/Nightfall_3");

            public static readonly SoundStyle Hit = new(AssetPath + "Sounds/Items/Accessories/Nightfall/Nightfall_Hit");
        }

        public static class Items
        {
            public static class Armor
            {
                public static readonly SoundStyle Antishield_Regen = new(AssetPath + "Sounds/Items/Armor/HAvatarShieldRegenerate");

                public static readonly SoundStyle Antishield_Break = new(AssetPath + "Sounds/Items/Armor/HAvatarShieldBreak");

                public static readonly SoundStyle Antishield_Hit = new(AssetPath + "Sounds/Items/Armor/HAvatarShieldHit", 3);
            }

            public static class CombatStim
            {
                public static readonly SoundStyle PsychosisWhisper = new(AssetPath + "Sounds/Items/CombatStim/PsychosisWhisper_", 5);
            }

            public static class Weapons
            {
                public static class AvatarRifle
                {
                    public static readonly SoundStyle FireSoundNormal = new("HeavenlyArsenal/Assets/Sounds/Items/Ranged/AvatarRifle/New/avatar rifle shot normal ", 3);

                    public static readonly SoundStyle FireSoundStrong = new("HeavenlyArsenal/Assets/Sounds/Items/Ranged/AvatarRifle/New/avatar rifle shot strong ", 3);

                    public static readonly SoundStyle FireSoundSuper = new("HeavenlyArsenal/Assets/Sounds/Items/Ranged/AvatarRifle/New/avatar rifle shot super ", 2);

                    public static readonly SoundStyle ReloadSound = new("HeavenlyArsenal/Assets/Sounds/Items/Ranged/AvatarRifle/New/avatar rifle reload ", 2);

                    public static readonly SoundStyle CycleSound = new("HeavenlyArsenal/Assets/Sounds/Items/Ranged/AvatarRifle/AvatarRifle_Cycle_Dronnor1");

                    public static readonly SoundStyle CycleEmptySound = new("HeavenlyArsenal/Assets/Sounds/Items/Ranged/AvatarRifle/AvatarRifle_Cycle");

                    public static readonly SoundStyle MagEmptySound = new("HeavenlyArsenal/Assets/Sounds/Items/Ranged/AvatarRifle/AvatarRifle_ClipEject");
                }

                public static class Rapture
                {
                    public static readonly SoundStyle Swing = new(AssetPath + "Sounds/Items/Melee/rapture_swing_", 3);

                    public static readonly SoundStyle Collapse = new(AssetPath + "Sounds/Items/Melee/Collapse_1");

                    public static readonly SoundStyle CollapseImpact = new(AssetPath + "Sounds/Items/Melee/Collapse_Impact");
                }
            }
        }
    }

    public static class Effects
    {
        public static class Dyes
        {
            // public static readonly Asset<Effect> Goop = AssetUtilities.RequestImmediate<Effect>(AssetPath + "Effects/GoopDye");
            // public static readonly Asset<Effect> Holograph = AssetUtilities.RequestImmediate<Effect>(AssetPath + "Effects/HolographDyeEffect");
        }

        //Basic
        //public static readonly Asset<Effect> BasicTrail = AssetUtilities.RequestImmediate<Effect>(AssetPath + "Effects/BasicTrail");
        //public static readonly Asset<Effect> LightningBeam = AssetUtilities.RequestImmediate<Effect>(AssetPath + "Effects/LightningBeam");
        public static readonly Asset<Effect> FlameDissolve = AssetUtilities.RequestImmediate<Effect>(AssetPath + "AutoloadedEffects/Shaders/FlameDissolve");

        public static readonly Asset<Effect> GoomoireWindR = AssetUtilities.RequestImmediate<Effect>(AssetPath + "AutoloadedEffects/Shaders/GoomoireSuckEffect");

        public static readonly Asset<Effect> GoomoireWindL = AssetUtilities.RequestImmediate<Effect>(AssetPath + "AutoloadedEffects/Shaders/GoomoireSuckEffect2");

        public static readonly Asset<Effect> bloodShader = AssetUtilities.RequestImmediate<Effect>(AssetPath + "AutoloadedEffects/Shaders/BloodBlobShader");

        public static readonly Asset<Effect> FusionRifleCircle = AssetUtilities.RequestImmediate<Effect>(AssetPath + "AutoloadedEffects/Shaders/FusionRifle_Circle");
    }

    public static readonly string AssetPath = $"{nameof(HeavenlyArsenal)}/Assets/";
}