using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Goreca
{
    public enum Behaviours
    {
        goUp,
        flyOver,

        lineUp,
        charge,
        halt,

        chumming,

        diving,
        breaching,
        emerge,

        slack,
        rapidCharge,

        enrage,

        diving2,
        breaching2,
        sink,

        chumming2,

        thrash
    }
    public class Goreca : ModNPC
    {
        readonly int damage = 200;
        bool boner = false;
        bool drawHead;
        private const string bodyPath = "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Goreca/GorecaBody";
        private static Asset<Texture2D> bodyTexture;
        int tailFrames = 9;
        List<Vector2> posList = [];
        List<Vector2> oldVel = [];
        List<Vector2> rifts = [];
        List<bool> drawSeg = [];
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = bodyTexture.Value;
            for (int i = 0; i < tailFrames; i++)
            {
                if (drawSeg[i])
                {
                    Color color = Lighting.GetColor((posList[i] / 16).ToPoint());
                    if (myBalls == Behaviours.goUp)
                    {
                        color.A = (byte)(255 - NPC.alpha);
                    }
                    Rectangle sourceRect = texture.Frame(tailFrames, 1, 8 - i, 0);
                    float rot = i < 1 ? posList[i].DirectionTo(NPC.Center).ToRotation() : posList[i].DirectionTo(posList[i - 1]).ToRotation();
                    spriteBatch.Draw(texture, posList[i] - Main.screenPosition, sourceRect, color, rot, sourceRect.Size() / 2f, 1, NPC.rotation > MathHelper.PiOver2 || NPC.rotation < -MathHelper.PiOver2 ? SpriteEffects.FlipVertically : SpriteEffects.None, 0);
                }
            }
            return drawHead;
        }
        public Behaviours myBalls;
        int currentSide;
        int oldSide;
        public override void OnSpawn(IEntitySource source)
        {
            drawHead = true;
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest();
            }
            myBalls = Behaviours.goUp;
            NPC.alpha = 255;
            NPC.velocity = new Vector2(0, -1);
            currentSide = Main.npc[NPC.target].Center.X < NPC.Center.X ? -1 : 1;
            oldSide = -currentSide;
            for (int i = 0; i < tailFrames; i++)
            {
                drawSeg.Add(true);
                posList.Add(NPC.Center + new Vector2(0, (i + 1) * 28));
                oldVel.Add(new Vector2(0, -1));
                oldVel.Add(new Vector2(0, -1));
                oldVel.Add(new Vector2(0, -1));
            }
        }
        bool enraged = false;
        int timer = 0;
        bool flip = false;
        int charges = 0;
        public override void AI()
        {
            //Filler
            if (NPC.alpha > 0)
            {
                NPC.alpha -= 15;
            }
            if (rifts.Count > 0 && Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < rifts.Count; i++)
                {
                    Dust dust;
                    dust = Dust.NewDustPerfect(rifts[i] + new Vector2(Main.rand.NextFloat(-80, 80), 0), DustID.Blood, default, 0, default, 3);
                    dust.noGravity = true;
                    Dust dust2;
                    dust2 = Dust.NewDustPerfect(rifts[i] + new Vector2(Main.rand.NextFloat(-80, 80), 0), DustID.Blood, default, 0, default, 3);
                    dust2.noGravity = true;
                }
            }
            NPC.target = -1;
            float nearestPlayer = 16000; //1000 tiles
            foreach (Player playerScan in Main.ActivePlayers)
            {
                if (!playerScan.dead && playerScan.WithinRange(NPC.Center, nearestPlayer))
                {
                    nearestPlayer = NPC.Distance(playerScan.Center);
                    NPC.target = playerScan.whoAmI;
                }
            }
            if (NPC.target < 0)
            {
                NPC.EncourageDespawn(10);
                NPC.velocity.X /= 1.1f;
                NPC.velocity.Y++;
                NPC.active = false;
                return;
            }
            //Do shit
            else
            {
                Player target = Main.player[NPC.target];
                currentSide = target.Center.X > NPC.Center.X ? -1 : 1;
                float xDistance = (NPC.Center - target.Center).X * currentSide;
                switch (myBalls)
                {
                    case Behaviours.goUp:
                        {
                            if (target.Center.Y - NPC.Center.Y < 320)
                            {
                                NPC.velocity.X /= 1.1f;
                                NPC.velocity.Y -= 0.5f;
                                NPC.velocity.Y /= 1.03f;
                            }
                            else
                            {
                                myBalls = Behaviours.flyOver;
                            }
                        }
                        break;
                    case Behaviours.flyOver:
                        {
                            if (xDistance < 640 || currentSide == oldSide)
                            {
                                NPC.velocity.X -= oldSide * 0.25f;
                                NPC.velocity.X /= 1.05f;
                                NPC.velocity.Y -= NPC.Center.Y - MathHelper.Lerp(NPC.Center.Y, target.Center.Y - 320, 0.01f);
                                NPC.velocity.Y /= 1.03f;
                                if (++timer > 15  && xDistance < 320 && Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<ShadowHand>());
                                    timer = 0;
                                }
                            }
                            else
                            {
                                myBalls = NPC.life > NPC.lifeMax * 0.75f ? Behaviours.lineUp : Behaviours.enrage;
                                timer = 0;
                            }
                        }
                        break;
                    case Behaviours.lineUp:
                        {
                            if (NPC.Center.Y < target.Center.Y)
                            {
                                NPC.velocity.X /= 1.1f;
                                NPC.velocity.Y += 0.5f;
                                NPC.velocity.Y /= 1.03f;
                            }
                            else
                            {
                                myBalls = Behaviours.charge;
                            }
                        }
                        break;
                    case Behaviours.charge:
                        {
                            if (xDistance > 120)
                            {
                                NPC.velocity.X += oldSide;
                                NPC.velocity.Y -= NPC.Center.Y - MathHelper.Lerp(NPC.Center.Y, target.Center.Y, 0.01f);
                                NPC.velocity.Y /= 1.05f;
                            }
                            else
                            {
                                myBalls = Behaviours.halt;
                                oldSide = currentSide;
                            }
                        }
                        break;
                    case Behaviours.halt:
                        {
                            if (NPC.Distance(target.Center) < 480)
                            {
                                NPC.velocity.X -= oldSide * 2;
                                NPC.velocity.X /= 1.1f;
                                NPC.velocity.Y -= NPC.Center.Y - MathHelper.Lerp(NPC.Center.Y, target.Center.Y, 0.01f);
                                NPC.velocity.Y /= 1.05f;
                            }
                            else
                            {
                                myBalls = NPC.life > NPC.lifeMax * 0.75f ? Behaviours.chumming : Behaviours.enrage; //Varixianvii decided it was once, not thrice ;)
                            }
                        }
                        break;
                    case Behaviours.chumming:
                        {
                            if (++timer > 59 && timer % 15 == 0 && timer < 121)
                            {
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<Chum>(), 0, NPC.whoAmI);
                                }
                                if (Main.netMode != NetmodeID.Server)
                                {
                                    SoundEngine.PlaySound(SoundID.NPCDeath13, NPC.Center);
                                }
                            }
                            if (timer < 150)
                            {
                                NPC.velocity += NPC.DirectionTo(target.Center) * 0.1f;
                                NPC.velocity /= 1.1f;
                            }
                            else if (NPC.ai[0] < 1)
                            {
                                int chums = 0;
                                int chumToEat = -1;
                                Vector2 nearestChum = NPC.Center + new Vector2(9999);
                                foreach (NPC chum in Main.ActiveNPCs)
                                {
                                    if (chum.type == ModContent.NPCType<Chum>() && chum.ai[0] == NPC.whoAmI)
                                    {
                                        chums++;
                                        if (chum.Distance(NPC.Center) < NPC.Distance(nearestChum))
                                        {
                                            nearestChum = chum.Center;
                                            chumToEat = chum.whoAmI;
                                        }
                                    }
                                }
                                if (chums > 0)
                                {
                                    NPC.velocity += NPC.DirectionTo(nearestChum) * (float)Math.Sqrt(NPC.Distance(nearestChum)) / 16f;
                                    NPC.velocity /= 1.2f;
                                }
                                else
                                {
                                    myBalls = NPC.life > NPC.lifeMax * 0.75f ? Behaviours.diving : Behaviours.enrage;
                                    timer = 0;
                                }
                            }
                            else
                            {
                                NPC.ai[0]--;
                                myBalls = Behaviours.thrash;
                            }
                        }
                        break;
                    case Behaviours.diving:
                        {
                            if (rifts.Count < 1)
                            {
                                rifts.Add(NPC.Center + new Vector2(NPC.velocity.X < 0 ? -160 : 160, 480));
                            }
                            if (NPC.Distance(rifts[0]) > 21 && !NPC.dontTakeDamage)
                            {
                                NPC.velocity += NPC.DirectionTo(rifts[0]) * 5;
                                NPC.velocity /= 1.2f;
                            }
                            else
                            {
                                drawHead = false;
                                if (drawSeg[tailFrames - 1])
                                {
                                    for (int i = 0; i < tailFrames; i++)
                                    {
                                        if (posList[i].Y > rifts[0].Y)
                                        {
                                            drawSeg[i] = false;
                                        }
                                    }
                                }
                                else
                                {
                                    myBalls = Behaviours.breaching;
                                    NPC.Center = target.Center + new Vector2(currentSide * 320, 320);
                                    NPC.velocity = Vector2.Normalize(NPC.velocity);
                                    rifts.Clear();
                                }
                                NPC.damage = 0;
                                NPC.dontTakeDamage = true;
                            }
                        }
                        break;
                    case Behaviours.breaching:
                        {
                            if (rifts.Count < 1)
                            {
                                boner = true;
                                rifts.Add(target.Center + new Vector2(currentSide * 240, 80));
                                rifts.Add(target.Center + new Vector2(currentSide * 240, 80));
                            }
                            if (NPC.Center.Y > rifts[0].Y && NPC.dontTakeDamage)
                            {
                                NPC.velocity += NPC.DirectionTo(rifts[0]) * 2;
                                NPC.velocity /= 1.1f;
                            }
                            else
                            {
                                if (NPC.Center.Y < rifts[0].Y)
                                {
                                    if (Main.netMode != NetmodeID.MultiplayerClient && !drawHead)
                                    {
                                        for (int i = 0; i < Main.rand.Next(5, 10); i++)
                                        {
                                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.velocity.RotatedByRandom(0.5f), ModContent.ProjectileType<BloodSplash>(), damage / 4, 0, NPC.target);
                                        }
                                    }
                                    drawHead = true;
                                    NPC.damage = damage;
                                    NPC.dontTakeDamage = false;
                                    NPC.velocity.X /= 1.01f;
                                    NPC.velocity.Y += 0.3f;
                                    rifts[1] = new Vector2(NPC.Center.X, rifts[0].Y);
                                }
                                else
                                {
                                    if (Main.netMode != NetmodeID.MultiplayerClient && drawHead)
                                    {
                                        for (int i = 0; i < Main.rand.Next(5, 10); i++)
                                        {
                                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, -NPC.velocity.RotatedByRandom(0.5f), ModContent.ProjectileType<BloodSplash>(), damage / 4, 0, NPC.target);
                                        }
                                    }
                                    drawHead = false;
                                    if (!drawSeg[tailFrames - 1])
                                    {
                                        myBalls = Behaviours.emerge;
                                        NPC.damage = 0;
                                        NPC.dontTakeDamage = true;
                                        rifts.Clear();
                                    }
                                }
                                for (int i = 0; i < tailFrames; i++)
                                {
                                    if (rifts.Count > 0 && posList[i].Y < rifts[0].Y)
                                    {
                                        drawSeg[i] = true;
                                    }
                                    else
                                    {
                                        drawSeg[i] = false;
                                    }
                                }
                            }
                        }
                        break;
                    case Behaviours.emerge:
                        {
                            if (rifts.Count < 1)
                            {
                                boner = false;
                                rifts.Add(target.Center + new Vector2(currentSide * 480, 80));
                            }
                            if (NPC.Distance(rifts[0]) > 21 && NPC.dontTakeDamage)
                            {
                                NPC.velocity += NPC.DirectionTo(rifts[0]) * 2;
                                NPC.velocity /= 1.1f;
                            }
                            else
                            {
                                if (drawSeg[tailFrames - 1])
                                {
                                    myBalls = NPC.life > NPC.lifeMax * 0.75f ? Behaviours.goUp : Behaviours.enrage;
                                    rifts.Clear();
                                }
                                else
                                {
                                    for (int i = 0; i < tailFrames; i++)
                                    {
                                        if (posList[i].Y < rifts[0].Y)
                                        {
                                            drawSeg[i] = true;
                                        }
                                    }
                                }
                                drawHead = true;
                                NPC.damage = damage;
                                NPC.dontTakeDamage = false;
                            }
                        }
                        break;
                    case Behaviours.enrage:
                        {
                            if (++timer < 60)
                            {
                                NPC.velocity += NPC.DirectionTo(target.Center) * 0.2f;
                                NPC.velocity /= 1.05f;
                            }
                            else
                            {
                                enraged = true;
                                if (Main.netMode != NetmodeID.Server)
                                {
                                    SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
                                }
                                myBalls = Behaviours.slack;
                                timer = 0;
                            }
                        }
                        break;
                    case Behaviours.slack:
                        {
                            if (NPC.Distance(target.Center) < 1200)
                            {
                                NPC.velocity += Vector2.Normalize(NPC.velocity);
                                NPC.velocity /= 1.02f;
                            }
                            else
                            {
                                myBalls = Behaviours.rapidCharge;
                            }
                        }
                        break;
                    case Behaviours.rapidCharge:
                        {
                            if (NPC.Distance(target.Center) > 480)
                            {
                                NPC.velocity += NPC.DirectionTo(target.Center) * 8;
                                NPC.velocity /= 1.1f;
                            }
                            else
                            {
                                if (++charges > 6)
                                {
                                    charges = 0;
                                    myBalls = Behaviours.diving2;
                                    NPC.velocity /= 5;
                                    oldSide = currentSide;
                                }
                                else
                                {
                                    myBalls = Behaviours.slack;
                                }
                            }
                        }
                        break;
                    case Behaviours.diving2:
                        {
                            if (rifts.Count < 1)
                            {
                                rifts.Add(target.Center + new Vector2(oldSide * 320, 80));
                            }
                            if (NPC.Distance(rifts[0]) > 21 && !NPC.dontTakeDamage)
                            {
                                NPC.velocity += NPC.DirectionTo(rifts[0]) * 5;
                                NPC.velocity /= 1.2f;
                            }
                            else
                            {
                                drawHead = false;
                                if (drawSeg[tailFrames - 1])
                                {
                                    for (int i = 0; i < tailFrames; i++)
                                    {
                                        if (posList[i].Y > rifts[0].Y)
                                        {
                                            drawSeg[i] = false;
                                        }
                                    }
                                }
                                else
                                {
                                    myBalls = Behaviours.breaching2;
                                    NPC.Center = target.Center + new Vector2(-oldSide * 1760, 320);
                                    NPC.velocity = Vector2.Normalize(NPC.velocity);
                                    rifts.Clear();
                                }
                                NPC.damage = 0;
                                NPC.dontTakeDamage = true;
                            }
                        }
                        break;
                    case Behaviours.breaching2:
                        {
                            if (rifts.Count < 1)
                            {
                                rifts.Add(target.Center + new Vector2(charges < 5 ? -oldSide * 1600 + oldSide * 640 * charges : -oldSide * 320, 80));
                            }
                            if (NPC.Center.Y > rifts[0].Y && NPC.dontTakeDamage)
                            {
                                NPC.velocity += NPC.DirectionTo(rifts[0]) * 4;
                                NPC.velocity /= 1.1f;
                            }
                            else
                            {
                                rifts[0] = new Vector2(NPC.Center.X, rifts[0].Y);
                                if (NPC.Center.Y < rifts[0].Y)
                                {
                                    if (Main.netMode != NetmodeID.MultiplayerClient && !drawHead)
                                    {
                                        for (int i = 0; i < Main.rand.Next(3, 7); i++)
                                        {
                                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.velocity.RotatedByRandom(0.5f) * 0.5f, ModContent.ProjectileType<BloodSplash>(), 50, 0, NPC.target);
                                        }
                                    }
                                    drawHead = true;
                                    NPC.damage = damage;
                                    NPC.dontTakeDamage = false;
                                    NPC.velocity.X /= 1.01f;
                                    NPC.velocity.Y += 0.9f;
                                }
                                for (int i = 0; i < tailFrames; i++)
                                {
                                    if (rifts.Count > 0 && posList[i].Y < rifts[0].Y)
                                    {
                                        drawSeg[i] = true;
                                    }
                                }
                                if (NPC.velocity.Y > 0)
                                {
                                    if (++charges > 5)
                                    {
                                        charges = 0;
                                        myBalls = Behaviours.chumming2;
                                        rifts.Clear();
                                    }
                                    else
                                    {
                                        myBalls = Behaviours.sink;
                                    }
                                }
                            }
                        }
                        break;
                    case Behaviours.sink:
                        {
                            if (NPC.Center.Y < rifts[0].Y && !NPC.dontTakeDamage)
                            {
                                NPC.velocity.X /= 1.01f;
                                NPC.velocity.Y += 0.9f;
                                rifts[0] = new Vector2(NPC.Center.X, rifts[0].Y);
                            }
                            else
                            {
                                if (Main.netMode != NetmodeID.MultiplayerClient && drawHead)
                                {
                                    for (int i = 0; i < Main.rand.Next(3, 8); i++)
                                    {
                                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, -NPC.velocity.RotatedByRandom(0.5f) * 0.5f, ModContent.ProjectileType<BloodSplash>(), 50, 0, NPC.target);
                                    }
                                }
                                drawHead = false;
                                if (drawSeg[tailFrames - 1])
                                {
                                    for (int i = 0; i < tailFrames; i++)
                                    {
                                        if (posList[i].Y > rifts[0].Y)
                                        {
                                            drawSeg[i] = false;
                                        }
                                    }
                                }
                                else
                                {
                                    myBalls = Behaviours.breaching2;
                                    NPC.Center = target.Center + new Vector2(charges < 5 ? -oldSide * 1760 + oldSide * charges * 640 : -oldSide * 480, 320);
                                    NPC.velocity = Vector2.Normalize(NPC.velocity);
                                    rifts.Clear();
                                }
                                NPC.damage = 0;
                                NPC.dontTakeDamage = true;
                            }
                        }
                        break;
                    case Behaviours.chumming2:
                        {
                            if (++timer > 59 && timer % 6 == 0 && timer < 91 && Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Point pos = target.Center.ToPoint() + new Point(Main.rand.Next(-480, 481), Main.rand.Next(-640, 1));
                                NPC.NewNPC(NPC.GetSource_FromAI(), pos.X, pos.Y, ModContent.NPCType<Chum>(), 0, NPC.whoAmI, 1);
                            }
                            if (timer < 120)
                            {
                                NPC.velocity += NPC.DirectionTo(target.Center) * 0.1f;
                                NPC.velocity /= 1.05f;
                            }
                            else if (NPC.ai[0] < 1)
                            {
                                int chums = 0;
                                int chumToEat = -1;
                                Vector2 nearestChum = NPC.Center + new Vector2(9999);
                                foreach (NPC chum in Main.ActiveNPCs)
                                {
                                    if (chum.type == ModContent.NPCType<Chum>() && chum.ai[0] == NPC.whoAmI)
                                    {
                                        chums++;
                                        if (chum.Distance(NPC.Center) < NPC.Distance(nearestChum))
                                        {
                                            nearestChum = chum.Center;
                                            chumToEat = chum.whoAmI;
                                        }
                                    }
                                }
                                if (chums > 0)
                                {
                                    NPC.velocity += NPC.DirectionTo(nearestChum) * (float)Math.Sqrt(NPC.Distance(nearestChum)) / 16f;
                                    NPC.velocity /= 1.2f;
                                }
                                else
                                {
                                    myBalls = Behaviours.slack;
                                    timer = 0;
                                }
                            }
                            else
                            {
                                NPC.ai[0]--;
                                myBalls = Behaviours.thrash;
                            }
                        }
                        break;
                    case Behaviours.thrash:
                        {
                            if (--NPC.ai[0] > 0)
                            {
                                NPC.velocity = Vector2.Normalize(NPC.velocity).RotatedBy(Math.Sin(NPC.ai[0] / 3f) / 2.5f) * 8;
                                //NPC.velocity /= 1.2f;
                            }
                            else
                            {
                                myBalls = enraged ? Behaviours.chumming2 : Behaviours.chumming;
                            }
                        }
                        break;
                }
            }
            //Look the right way
            if (flip)
            {
                flip = false;
                if (NPC.rotation > 0)
                {
                    NPC.rotation -= MathHelper.Pi;
                }
                else
                {
                    NPC.rotation += MathHelper.Pi;
                }
            }
            if (rifts.Count > 0 && (myBalls == Behaviours.diving || myBalls == Behaviours.breaching))
            {
                if (myBalls == Behaviours.diving)
                {
                    NPC.rotation = Utils.AngleLerp(NPC.rotation, NPC.DirectionTo(rifts[0]).ToRotation(), 0.15f);
                }
                else if (myBalls == Behaviours.breaching)
                {
                    NPC.rotation = Utils.AngleLerp(NPC.rotation, (-NPC.DirectionTo(rifts[0])).ToRotation(), 0.15f);
                }
            }
            else
            {
                float agility = 0.15f;
                if (myBalls == Behaviours.thrash)
                {
                    agility = 0.45f;
                }
                NPC.rotation = Utils.AngleLerp(NPC.rotation, NPC.velocity.ToRotation(), agility);
            }
            if (NPC.rotation > MathHelper.PiOver2 || NPC.rotation < -MathHelper.PiOver2)
            {
                flip = true;
                if (NPC.rotation > 0)
                {
                    NPC.rotation += MathHelper.Pi;
                }
                else
                {
                    NPC.rotation -= MathHelper.Pi;
                }
            }
            NPC.spriteDirection = flip ? 1 : -1;
            //Tail stuff
            oldVel.Insert(0, NPC.velocity);
            oldVel.RemoveAt(tailFrames * 3 - 1);
            for (int i = 0; i < tailFrames; i++)
            {
                if (boner)
                {
                    posList[i] = i < 1 ? NPC.Center - NPC.rotation.ToRotationVector2() * 70 : posList[i - 1] - NPC.rotation.ToRotationVector2() * 28;
                }
                else
                {
                    posList[i] = i < 1 ? NPC.Center - Vector2.Normalize(oldVel[i]) * 70 : posList[i - 1] - Vector2.Normalize(oldVel[i]) * 28;
                }
            }
        }
        public override void OnKill()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 25; i++)
                {
                    //Dust.NewDust(NPC.position, NPC.width, NPC.height, ModContent.DustType<Dusts.NobleDust>(), 0, 0, 1, new Color(140, 255, 60));
                }
            }
        }
        public override void Load()
        {
            if (!Main.dedServ)
            {
                bodyTexture = ModContent.Request<Texture2D>(bodyPath);
            }
        }
        public override bool CheckActive()
        {
            return false;
        }
        public override void SetDefaults()
        {
            NPC.damage = damage;
            NPC.aiStyle = -1;
            NPC.DeathSound = SoundID.NPCHit57;
            NPC.defense = 150;
            NPC.friendly = false;
            NPC.height = 98;
            NPC.lavaImmune = true;
            NPC.HitSound = SoundID.NPCHit18;
            NPC.knockBackResist = 0;
            NPC.lifeMax = 320000;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.takenDamageMultiplier = 0.6f;
            NPC.value = 250000;
            NPC.width = 126;
        }
        public override void SetStaticDefaults()
        {
            /*NPCID.Sets.MPAllowedEnemies[Type] = true; 
            NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                Hide = true
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, value);
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            Main.npcFrameCount[Type] = 4;*/
            NPCID.Sets.TrailCacheLength[Type] = 15;
            NPCID.Sets.TrailingMode[Type] = 2;
        }
    }
    public class Chum : ModNPC
    {
        int timer = 0;
        public override void AI()
        {
            NPC owner = Main.npc[(int)NPC.ai[0]];
            if (owner.active)
            {
                if (++timer > 60)
                {
                    if (owner.ai[0] < 1 && NPC.Distance(owner.Center) < 64)
                    {
                        NPC.active = false;
                        owner.ai[0] = 60;
                        owner.HealEffect(owner.lifeMax / 40);
                        owner.life += owner.lifeMax / 40;
                    }
                    foreach (Player player in Main.ActivePlayers)
                    {
                        if (player.Distance(NPC.Center) < 32)
                        {
                            if (Main.netMode != NetmodeID.Server)
                            {
                                SoundEngine.PlaySound(SoundID.Item2, player.Center);
                            }
                            NPC.active = false;
                            player.statLife += player.statLifeMax2 / 40;
                            player.HealEffect(player.statLifeMax2 / 40);
                        }
                    }
                }
            }
            else
            {
                NPC.active = false;
            }
            if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(5))
            {
                Dust.NewDustPerfect(NPC.Center + new Vector2(Main.rand.NextFloat(0, NPC.width / 2f), 0).RotatedByRandom(MathHelper.Pi), DustID.Blood);
            }
            NPC.velocity /= 1.05f;
            NPC.rotation += 0.03f + NPC.velocity.Length() * 0.1f;
        }
        //Because knockback normally fucking 100% negates velocity. This grants 80% negation of the effects of knockback THE RIGHT WAY
        public override void ModifyHitByItem(Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            NPC.velocity -= NPC.DirectionTo(Main.npc[(int)NPC.ai[0]].Center) * item.knockBack * 0.2f;
        }
        public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            NPC.velocity -= NPC.DirectionTo(Main.npc[(int)NPC.ai[0]].Center) * projectile.knockBack * 0.2f;
        }
        public override void OnSpawn(IEntitySource source)
        {
            if (NPC.ai[1] < 1)
            {
                NPC.velocity = Vector2.Normalize(Main.npc[(int)NPC.ai[0]].velocity).RotatedByRandom(MathHelper.PiOver4) * Main.rand.NextFloat(20, 40);
            }
            else if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.netUpdate = true;
                NPC.velocity = new Vector2(0, Main.rand.Next(10, 20));
            }

        }
        public override void SetDefaults()
        {
            NPC.color = Color.Red; //Remove once sprited
            NPC.defense = 999999;
            NPC.friendly = false;
            NPC.height = 58;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.knockBackResist = 0;
            NPC.lifeMax = 100;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.width = 62;
            for (int i = 0; i < BuffLoader.BuffCount; i++)
            {
                NPC.buffImmune[i] = true;
            }
        }
    }
    public class ShadowHand : ModNPC
    {
        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest();
            }
            Player target = Main.player[NPC.target];
            if (target.dead)
            {
                NPC.active = false;
                if (Main.netMode != NetmodeID.Server)
                {
                    SoundEngine.PlaySound(SoundID.NPCDeath39, NPC.Center);
                }
            }
            else
            {
                NPC.velocity += NPC.DirectionTo(target.Center);
                NPC.velocity /= 1.05f;
                NPC.rotation = NPC.velocity.ToRotation();
            }
        }
        //Because knockback normally fucking 100% negates velocity. This grants 80% negation of the effects of knockback THE RIGHT WAY
        public override void ModifyHitByItem(Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            NPC.velocity -= NPC.DirectionTo(item.Center) * item.knockBack * 0.2f;
        }
        public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            NPC.velocity -= NPC.DirectionTo(projectile.Center) * projectile.knockBack * 0.2f;
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            NPC.active = false;
            if (Main.netMode != NetmodeID.Server)
            {
                SoundEngine.PlaySound(SoundID.NPCDeath39, NPC.Center);
            }
        }
        public override void SetDefaults()
        {
            NPC.color = Color.Black; //Remove once sprited and affected
            NPC.damage = 100;
            NPC.DeathSound = SoundID.NPCDeath39;
            NPC.defense = 999999;
            NPC.friendly = false;
            NPC.height = 26;
            NPC.HitSound = SoundID.NPCHit36;
            NPC.knockBackResist = 0;
            NPC.lifeMax = 10;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.width = 48;
        }
    }
    public class BloodSplash : ModProjectile
    {
        public override void AI()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                Dust.NewDustPerfect(Projectile.Center + new Vector2(Main.rand.NextFloat(0, Projectile.width / 2f), 0).RotatedByRandom(MathHelper.Pi), DustID.Blood);
            }
            if (Projectile.timeLeft > 545)
            {
                Projectile.velocity.X /= 1.01f;
                Projectile.velocity.Y += 0.3f;
            }
            else if (Projectile.timeLeft > 510)
            {
                Projectile.velocity /= 1.05f;
            }
            else if (Projectile.timeLeft > 500)
            {
                Projectile.velocity += Projectile.DirectionTo(Main.player[Projectile.owner].Center) * 5;
                Projectile.velocity /= 1.2f;
            }
        }
        public override void SetDefaults()
        {
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.timeLeft = 600;
            Projectile.width = 16;
        }
    }
}