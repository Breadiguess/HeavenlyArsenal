using CalamityMod;
using CalamityMod.Particles;
using Luminance.Common.Easings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;

namespace HeavenlyArsenal.Content.Items.Accessories.Nightfall
{
    internal class NightfallNPC : GlobalNPC
    {
       
        /// <summary>
        /// the current amonunts of stacks 
        /// </summary>
        public int Stack;
        
        /// <summary>
        /// the timer that counts down to losing a stack
        /// </summary>
        public int StackTimer;

        /// <summary>
        /// the cooldown before the npc can start gaining stacks again.
        /// </summary>
        public int BurstCooldown;
        
        /// <summary>
        /// stores damage done to this npc.
        /// </summary>
        public int DamageBucketNPC;

        /// <summary>
        /// when this is below 0, drain damage done     
        /// </summary>
        public int BucketLossTimer;

        /// <summary>
        /// The windup interpolation, from 0 -> 1
        /// controls the process of the orbs going from circling around the npc to crciling above it
        /// </summary>
        public float WindupInterp = 0;
        
        /// <summary>
        /// 1 -> 0
        /// </summary>
        /// <remarks> this is weird. it both controls the distance from the center of the npc, and also the interpolant for the orb's slam down.</remarks>
        public float OrbitInterp = 1;
        public float t = 0;


        //todo: simulate a z axis for the orbs, so that they can go behind the npc when they are at the back of the orbit.
        // this'll probably require some fuckery by drawing in predraw and postdraw.


        private PiecewiseCurve Interpolant;

        public Player StackOwner;
        public override bool InstancePerEntity => true;

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            binaryWriter.Write(StackOwner.whoAmI);
            binaryWriter.Write(DamageBucketNPC);
            binaryWriter.Write(BurstCooldown);
        }
        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            StackOwner = Main.player[binaryReader.Read()];
            DamageBucketNPC = binaryReader.Read();
            BurstCooldown = binaryReader.Read();
        }
        public override bool PreAI(NPC npc)
        {
            if(BucketLossTimer > 0)
                if(t == 0)
                {
                    BucketLossTimer--;
                }
            if(BucketLossTimer <= 0)
            {

               
                if(DamageBucketNPC > 0)
                    DamageBucketNPC -= 10;
                else
                    DamageBucketNPC = 0;
            }
           
            if (Stack > 0)
            {
                if (Interpolant == null)
                    Interpolant = new PiecewiseCurve()
                    .Add(EasingCurves.Exp, EasingType.In, 0f, 1f ,1f);



                if (StackOwner == null || !StackOwner.active || StackOwner.dead || !StackOwner.GetModPlayer<NightfallPlayer>().NightfallActive)
                {
                    Stack = 0;
                    return base.PreAI(npc);
                }
                if (StackTimer < 0 && Stack < 9)
                {
                    if (Stack > 0)
                    {
                        Stack--;
                        StackTimer = 300;
                    }
                        
                }
                else if (Stack >= 9 && BurstCooldown <= 0)
                {
                    if (OrbitInterp == 0)
                    {
                        Vector2 SpawnPos = new Vector2(npc.Center.X, npc.Center.Y + npc.height / 2);

                        for(int i = 0; i < 9; i++)
                        {
                            Vector2 AdjustedSpawn = SpawnPos + new Vector2(i*9 - (9*9)/2,0);

                            Particle DarkTrail = new SparkParticle(AdjustedSpawn, Vector2.Zero, false, 35, 1.2f, Color.DarkRed);
                            Particle Trail = new SparkParticle(AdjustedSpawn, Vector2.Zero, false, 35, 1.2f, Color.Red);

                            GeneralParticleHandler.SpawnParticle(DarkTrail);

                            GeneralParticleHandler.SpawnParticle(Trail);
                        }

                      

                        Particle a = new CalamityMod.Particles.PlasmaExplosion(npc.Center, Vector2.Zero, Color.Red, Vector2.One, 0, 0, 0.1f, 60);
                        GeneralParticleHandler.SpawnParticle(a);

                        //Particle d = new CustomPulse(npc.Center, npc.velocity, Color.AntiqueWhite, "HeavenlyArsenal/Content/Items/Accessories/Vambrace/ElectricVambrace", new Vector2(1), 0, 1, 4, 60);
                        //GeneralParticleHandler.SpawnParticle(d);
                        //Particle A = new CalamityMod.Particles.StrongBloom(npc.Center, new Vector2(0, 0), Color.DarkRed, 1, 10);
                        //GeneralParticleHandler.SpawnParticle(A);
                        for(int i = 0; i< 9; i++)
                        {
                            Vector2 particleVelocity = new Vector2(0, (float)Math.Sin(float.Pi/(i+1)))*10;
                            Particle d = new CalamityMod.Particles.MediumMistParticleAlphaBlend(npc.Center, particleVelocity, Color.Red, Color.Black, 1, 255);
                            GeneralParticleHandler.SpawnParticle(d);
                        }
                        float DamageReduction = 0.75f;

                        int damage = (int) (StackOwner.GetModPlayer<NightfallPlayer>().DamageBucketTotal * DamageReduction);
                        bool crit = true;

                       

                        //SoundEngine.PlaySound(GennedAssets.Sounds.Enemies.DismalLanternExplode with { PitchRange = (-1f, 0), Volume = 0.85f, PitchVariance = 0.4f });
                        SoundEngine.PlaySound(AssetDirectory.Sounds.Nightfall.Nightfall_Burst);

                        if (damage > (NightfallPlayer.DamageBucketMax*DamageReduction)/2)
                        {
                            SoundEngine.PlaySound(AssetDirectory.Sounds.Nightfall.Nightfall_Burst_Heavy with { Volume = 2});

                        }
                        else if (damage > 500 && damage < (NightfallPlayer.DamageBucketMax * DamageReduction) / 2)
                        {
                            SoundEngine.PlaySound(AssetDirectory.Sounds.Nightfall.Nightfall_Burst_Hard with { Volume = 2 });

                        }

                        StackOwner.ApplyDamageToNPC(npc, damage, 0, 0, crit, DamageClass.Generic);
                        DamageBucketNPC = 0;
                        WindupInterp = 0;
                        BurstCooldown = NightfallPlayer.CooldownMax;
                        Stack = 0;
                        OrbitInterp = 1;
                        t = 0;
                       
                    }
                    else
                    {
                        if (WindupInterp == 0)
                        {

                            SoundEngine.PlaySound(AssetDirectory.Sounds.Nightfall.Nightfall_Windup with { Volume = 4f, PitchVariance = 0.2f });
                            //SoundEngine.PlaySound(GennedAssets.Sounds.Common.Twinkle with { PitchVariance = 0.2f , Pitch = -2f});
                           
                        }
                        if (WindupInterp > 0.9f)
                        {
                            //Main.NewText($"Before: {OrbitInterp}, t: {t}");

                            t = MathHelper.Clamp(t + 0.02f, 0f, 1f);
                            //ain.NewText(t);
                            OrbitInterp = Interpolant.Evaluate(t);

                            //Main.NewText($"{t}, orbit: {OrbitInterp}");
                            //Main.NewText($"{OrbitInterp}, t: {t}");

                        }
                        float AdjustedValue = WindupInterp < 0.5f ? 0.02f : 0.05f; 
                        WindupInterp = float.Lerp(WindupInterp, 1, AdjustedValue);



                       
                    }
                    
                   
                }
                StackTimer--;
            }

            if(BurstCooldown > 0)
            {
                BurstCooldown--;
            }


            return base.PreAI(npc);
        }

        private void DrawOrbs(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor, bool drawBehind)
        {
            Texture2D Orb = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Particles/Metaballs/BasicCircle").Value;
            Texture2D WoodenBall = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Accessories/Nightfall/WoodenOrbs").Value;
            Texture2D Spire = GennedAssets.Textures.GreyscaleTextures.ChromaticSpires;
            Texture2D Glow = GennedAssets.Textures.GreyscaleTextures.BloomCirclePinpoint;

            Vector2 BasePos = npc.Center - Main.screenPosition;
            Vector2 Origin = Orb.Size() * 0.5f;
            Vector2 GlowOrigin = Glow.Size() * 0.5f;
            Vector2 SpireOrigin = Spire.Size() * 0.5f;
            Vector2 WoodenOrigin = new Vector2(WoodenBall.Width / 2, WoodenBall.Height / 9 / 2);

            if (Stack > 0)
            {
                for (int i = 0; i < Stack; i++)
                {
                    Rectangle ballframe = WoodenBall.Frame(1, 10, 0, i + 1);

                    float offset = npc.width / 1.1f;
                    Vector2 RotationOffset = new Vector2(0, offset + npc.width * OrbitInterp).RotatedBy(
                        (MathHelper.TwoPi * (i + 1) / Stack) +
                        MathHelper.ToRadians((Main.GlobalTimeWrappedHourly * 20) * (1 + WindupInterp / 5))
                    );

                    float xScale = 1 + 1.25f * WindupInterp;
                    float yScale = 1 - 0.85f * WindupInterp;
                    RotationOffset *= new Vector2(xScale, yScale);

                    bool isBehind = RotationOffset.Y*2 < 2f;
                    if (isBehind != drawBehind)
                        continue;

                    float value = -npc.height * 1.25f * WindupInterp;
                    if (OrbitInterp < 0.999f)
                        value = -npc.height + (npc.height * 1.5f) * (1 - OrbitInterp);

                    Vector2 HaloOffset = new Vector2(0, value * WindupInterp);
                    Vector2 DrawPos = BasePos + RotationOffset + HaloOffset;


                    Main.EntitySpriteDraw(Glow, DrawPos, null, Color.Red with { A = 1 } * npc.Opacity, 0, GlowOrigin, 0.17f * (1+WindupInterp), SpriteEffects.None);
                    Main.EntitySpriteDraw(Orb, DrawPos, null, Color.Red * npc.Opacity, 0, Origin, 0.11f, SpriteEffects.None, 0);
                    Main.EntitySpriteDraw(Orb, DrawPos, null, Color.White * 0.85f * npc.Opacity, 0, Origin, 0.1f, SpriteEffects.None);
                    Main.EntitySpriteDraw(WoodenBall, DrawPos, ballframe, drawColor, 0, WoodenOrigin, 1, SpriteEffects.None);
                    //Utils.DrawBorderString(spriteBatch, RotationOffset.Y.ToString(), DrawPos, Color.AliceBlue);
                }
            }
        }


     
        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            DrawOrbs(npc, spriteBatch, screenPos, drawColor, false);  
        }
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            DrawOrbs(npc, spriteBatch, screenPos, drawColor, true);  
            return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
           
        }
    }
}
