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
        public static readonly Asset<Texture2D> ShockRing = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/Extra/ShockRing");
        //public static readonly Asset<Texture2D> Empty = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/Extra/Empty");
        //public static readonly Asset<Texture2D> Template = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/Extra/Template");


        //my first custom assetDirectory

        //public static readonly Asset<Texture2D> PerlinNoise = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/Extra/PerlinNoise");





        

        //public static Dictionary<int, Asset<Texture2D>> Particle = new Dictionary<int, Asset<Texture2D>>();
        //public static Dictionary<int, Asset<Texture2D>> Relic = new Dictionary<int, Asset<Texture2D>>();
        //public static Dictionary<int, Asset<Texture2D>> FlyingSlime = new Dictionary<int, Asset<Texture2D>>();

        //Jokes and Seasons
        //internal static readonly Asset<Texture2D> FrogParticle = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/Extra/FrogParticle");
       // internal static readonly Asset<Texture2D> WideMoto = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/Extra/WideMoto");
       // internal static readonly Asset<Texture2D> SantaHat = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/Extra/SantaHat");
       // internal static readonly Asset<Texture2D> ElfHat = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/Extra/ElfHat");

       
        public static class Inventors
        {
           // public static readonly string InventorsPrefix = AssetPath + "Textures/NPCs/Bosses/Inventors/";

            //lol
        }

        public static class Buffs
        {
         //   public static readonly Asset<Texture2D>[] SlimeCane = AssetUtilities.RequestArrayImmediate<Texture2D>(AssetPath + "Textures/Buffs/SlimeCane_", 2);
        }

        public static class Bars
        {
         //   public static readonly Asset<Texture2D>[] Bar = AssetUtilities.RequestArrayTotalImmediate<Texture2D>(AssetPath + "Textures/UI/Bars/BarBase_");
          //  public static readonly Asset<Texture2D>[] BarFill = AssetUtilities.RequestArrayTotalImmediate<Texture2D>(AssetPath + "Textures/UI/Bars/BarFill_");

         //   public static readonly Asset<Texture2D> Stress = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/UI/StressBar");
         //  public static readonly Asset<Texture2D> StressCharge = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/UI/StressBarFill");
         //   public static readonly Asset<Texture2D> StressTopped = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/UI/StressBarTopped");
        }


        //leftover code because i just copied it from blockaroz lmao
       // public static class SlimeMonsoon
       // {
       //     public static readonly Asset<Texture2D> MapBG = AssetUtilities.RequestImmediate<Texture2D>(AssetPath + "Textures/SlimeMonsoonBG");
       //     public static readonly Asset<Texture2D> Lightning = AssetUtilities.RequestImmediate<Texture2D>($"{nameof(HeavenlyArsenal)}/Common/Graphics/SlimeMonsoon/Lightning");
       //     public static readonly Asset<Texture2D> LightningGlow = AssetUtilities.RequestImmediate<Texture2D>($"{nameof(HeavenlyArsenal)}/Common/Graphics/SlimeMonsoon/LightningGlow");
       // }
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
       
        public static class GoozmaMinions
        {
            //public static readonly SoundStyle SlimeSlam = new(AssetPath + "Sounds/Goozma/Slimes/GoozmaSlimeSlam", 1, 3) { MaxInstances = 0 };

        }

    }

    public static class Effects
    {
        //Basic
        //public static readonly Asset<Effect> BasicTrail = AssetUtilities.RequestImmediate<Effect>(AssetPath + "Effects/BasicTrail");
        //public static readonly Asset<Effect> LightningBeam = AssetUtilities.RequestImmediate<Effect>(AssetPath + "Effects/LightningBeam");
        //public static readonly Asset<Effect> FlameDissolve = AssetUtilities.RequestImmediate<Effect>(AssetPath + "Effects/FlameDissolve");

        //Goozma related
        //public static readonly Asset<Effect> SlimeMonsoonOldCloudLayer = AssetUtilities.RequestImmediate<Effect>(AssetPath + "Effects/SlimeMonsoonOldCloudLayer");
        //public static readonly Asset<Effect> SlimeMonsoonSkyLayer = AssetUtilities.RequestImmediate<Effect>(AssetPath + "Effects/SlimeMonsoonSkyLayer");
        //public static readonly Asset<Effect> SlimeMonsoonDistortion = AssetUtilities.RequestImmediate<Effect>(AssetPath + "Effects/SlimeMonsoonDistortion");
        //public static readonly Asset<Effect> PluripotentDistortion = AssetUtilities.RequestImmediate<Effect>(AssetPath + "Effects/PluripotentDistortion");

        //public static readonly Asset<Effect> GoozmaCordMap = AssetUtilities.RequestImmediate<Effect>(AssetPath + "Effects/GoozmaCordMap");
        //public static readonly Asset<Effect> GooLightning = AssetUtilities.RequestImmediate<Effect>(AssetPath + "Effects/GooLightningEffect");
        //public static readonly Asset<Effect> Cosmos = AssetUtilities.RequestImmediate<Effect>(AssetPath + "Effects/CosmosEffect");
        //public static readonly Asset<Effect> StellarRing = AssetUtilities.RequestImmediate<Effect>(AssetPath + "Effects/StellarRing");
        //public static readonly Asset<Effect> RainbowGel = AssetUtilities.RequestImmediate<Effect>(AssetPath + "Effects/RainbowGel");
        //public static readonly Asset<Effect> HolographicGel = AssetUtilities.RequestImmediate<Effect>(AssetPath + "Effects/HolographEffect");
        //public static readonly Asset<Effect> BlackHole = AssetUtilities.RequestImmediate<Effect>(AssetPath + "Effects/SpaceHole");

        
        public static readonly Asset<Effect> GoomoireWindR = AssetUtilities.RequestImmediate<Effect>(AssetPath + "AutoloadedEffects/Shaders/GoomoireSuckEffect");
        public static readonly Asset<Effect> GoomoireWindL = AssetUtilities.RequestImmediate<Effect>(AssetPath + "AutoloadedEffects/Shaders/GoomoireSuckEffect2");
        public static readonly Asset<Effect> bloodShader = AssetUtilities.RequestImmediate<Effect>(AssetPath + "AutoloadedEffects/Shaders/BloodBlobShader");
        public static class Dyes
        {
           // public static readonly Asset<Effect> Goop = AssetUtilities.RequestImmediate<Effect>(AssetPath + "Effects/GoopDye");
           // public static readonly Asset<Effect> Holograph = AssetUtilities.RequestImmediate<Effect>(AssetPath + "Effects/HolographDyeEffect");
        }
    }
}