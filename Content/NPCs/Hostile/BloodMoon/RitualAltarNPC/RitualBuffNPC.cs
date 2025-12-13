using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Common.utils;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC
{
    public class RitualBuffNPC : GlobalNPC
    {

        //todo: Arm buff???

        public static List<string> NameModifiers = new()
        {
            "",
            "Undying",
            "Furious",
            "Vigorful",
            "Regenerating",
            "Vampiric"
        };
        public enum RitualBuffType
        {
            None,
            Ressurection,
            Damagebuff,
            CooldownReduction,
            LifeRegen,
            Vampirism
        }
        public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        {
            if (entity.ModNPC != null && entity.ModNPC is BloodMoonBaseNPC)
                return lateInstantiation && true;
            else
                return false;

        }
        public override bool InstancePerEntity => true;
        public bool WasRessurectedRecently = false;
        public bool hasRitualBuff = false;

        public bool isBeingBuffed = false;
        public int ritualBuffTier;
        public int ritualBuffTimer = 0;
        public int ritualBuffDuration = 60 * 17;
        public RitualBuffType BuffType;
        public NPC BuffGranter = null;

        public Rope BuffString;

        public bool IsRessurecting = false;
        public int Ressurectiontimer = 0;


        public void PerformRitualOn(NPC npc)
        {
            if (!isBeingBuffed)
                return;
            if (BuffGranter != null)
            {
                RitualAltar altar = BuffGranter.ModNPC as RitualAltar;

                npc.velocity = Vector2.Zero;
                Vector2 endPos = BuffGranter.Top + new Vector2(0, -120).RotatedBy(BuffGranter.rotation + MathHelper.PiOver2);

                Lighting.AddLight(endPos, TorchID.Crimson);
                npc.Center = Vector2.Lerp(npc.Center, endPos, 0.5f);
                int cultistAmount = 1;
                if (CultistCoordinator.GetCultOfNPC(BuffGranter) != null)
                    foreach (NPC a in CultistCoordinator.GetCultOfNPC(BuffGranter).Cultists)
                    {
                        if (a.Distance(BuffGranter.Center) < 100)
                        {
                            FleshlingCultist.FleshlingCultist d = a.ModNPC as FleshlingCultist.FleshlingCultist;
                            d.CurrentState = FleshlingCultist.FleshlingCultist.Behaviors.Worship;
                            if (d.isWorshipping)
                            {
                                cultistAmount++;
                            }
                        }
                    }
                ritualBuffTimer++;

                if (ritualBuffTimer > 180 && isBeingBuffed)
                {
                    altar.blood -= altar.bloodBankMax / 5;

                    altar.NPCTarget = null;
                    isBeingBuffed = false;
                    ritualBuffTimer = 0;
                    ApplyRitualBuff(npc, cultistAmount);
                    npc.velocity = Vector2.Zero;
                    //altar.currentAIState = RitualAltar.AltarAI.lookForBuffTargets;
                }
                return;
            }
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (hasRitualBuff && BuffType == RitualBuffType.LifeRegen)
            {
                npc.lifeRegenCount += 13000;
            }
        }
        bool runningExtraAI = false;

        public override bool PreAI(NPC npc)
        {
            if (isBeingBuffed && !hasRitualBuff)
            {
                PerformRitualOn(npc);
                return false;
            }


            if (BuffGranter == null || !BuffGranter.active || BuffGranter.life <= 0)
            {
                DestroyBuffString();
                hasRitualBuff = false;

            }

            if (IsRessurecting && !WasRessurectedRecently)
            {
                ManageRessurection(npc);
                return false;
            }

            ApplyEffects(npc);
            if (hasRitualBuff && BuffType == RitualBuffType.CooldownReduction)
            {
                //AGAIN
                for (int i = 0; i < ritualBuffTier; i++)
                    npc.ModNPC.AI();
                return false;
            }
            return base.PreAI(npc);
        }
        private void ManageRessurection(NPC npc)
        {
            npc.velocity *= 0.98f;
            Ressurectiontimer++;

            if (Ressurectiontimer > 120 - ritualBuffTier * 20)
            {
                npc.life = npc.lifeMax;
                npc.immortal = false;
                ClearRitualBuff(npc);
                Ressurectiontimer = 0;
                IsRessurecting = false;
                WasRessurectedRecently = true;
                CombatText.NewText(npc.getRect(), CombatText.HealLife, npc.lifeMax, true);
            }



        }
        public override bool CheckDead(NPC npc)
        {
            if ((hasRitualBuff && BuffType == RitualBuffType.Ressurection) || IsRessurecting)
            {
                npc.life = 2;
                IsRessurecting = true;
                npc.immortal = true;
                return false;
            }
            return base.CheckDead(npc);
        }
        public void ApplyEffects(NPC npc)
        {
            if (BuffType == RitualBuffType.Damagebuff)
            {
                npc.damage = npc.defDamage * (1 + ritualBuffTier);
            }
            
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            if (hasRitualBuff && BuffType == RitualBuffType.Vampirism)
                npc.life = Math.Clamp(npc.life + (int)(hurtInfo.Damage * 1.2f), 0, npc.lifeMax);
        }
        public override void PostAI(NPC npc)
        {
            if (ritualBuffTimer > 0 && !isBeingBuffed)
                ritualBuffTimer--;

            ManageBuffstring(npc);
            if (Math.Abs(npc.velocity.Y) > 30 && isBeingBuffed)
            {
                Main.NewText("WTF, "+ npc.ToString() + ", " + npc.velocity.Y.ToString());
                npc.velocity.Y = 0;
            }

            if (ritualBuffTimer <= 0)
            {

                ClearRitualBuff(npc);

            }

        }

        public override void AI(NPC npc)
        {
            
        }
        private void ManageBuffstring(NPC npc)
        {
            if (!hasRitualBuff)
                return;
            Vector2 StringStart = BuffGranter.Top + new Vector2(0, 20);

            if (BuffString == null)
            {
                BuffString = new Rope(StringStart, npc.Center, 40, 10, Vector2.Zero);
            }

            if(npc.Opacity >0)
            for (int i = 1; i < BuffString.segments.Length - 1; i++)
            {

                if (Main.rand.NextBool(15) && i < BuffString.segments.Length - BuffString.segments.Length / 6)
                {
                    Dust blood = Dust.NewDustPerfect(BuffString.segments[i].position, DustID.CrimtaneWeapons, new Vector2(0, -3f), 10, Color.Crimson, 1);
                    blood.noGravity = true;
                    blood.rotation = Main.rand.NextFloat(-89, 89);

                }
                if (i == BuffString.segments.Length / 2)
                {
                    for (int u = -3; u < 6; u++)
                        BuffString.segments[i + u].position += new Vector2(0, -1f);
                }

            }
            BuffString.segments[0].position = npc.Center;
            BuffString.segments[^1].position = StringStart;
            BuffString.Update();
        }
        private void DestroyBuffString()
        {
            BuffString = null;
        }

        #region apply/remove buff

        public void ApplyRitualBuff(NPC npc, int Tier)
        {

            //CombatText.NewText(npc.Hitbox, Color.Red, "Buffed!", true);
            int buffType = Main.rand.Next(1, 6);
            hasRitualBuff = true;
            RitualSystem.AddNPC(npc);
            BuffType = (RitualBuffType)buffType;
            if(buffType == (int)RitualBuffType.CooldownReduction)
            {
                npc.life = npc.lifeMax;
                npc.netUpdate = true;
            }
            ritualBuffTier = Tier;
            ritualBuffTimer = ritualBuffDuration + 80 * Tier;
            SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.ArmJutOut with { Pitch = buffType / 6 }, npc.Center);
            BuffRune particle = BuffRune.pool.RequestParticle();
            particle.Prepare(BuffGranter.Top + new Vector2(120,0).RotatedBy(BuffGranter.rotation), buffType, 190);

            ParticleEngine.ShaderParticles.Add(particle);
        }

        public void ClearRitualBuff(NPC npc)
        {

            if (BuffType == RitualBuffType.Ressurection)
                WasRessurectedRecently = true;
            if (IsRessurecting)
                IsRessurecting = false;
            hasRitualBuff = false;
            BuffType = 0;
            BuffGranter = null;
            if (npc.immortal)
                npc.immortal = false;
        }
        #endregion


        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            //Utils.DrawBorderString(spriteBatch, BuffType.ToString() + $"\n" + ritualBuffTimer.ToString(), npc.Center - screenPos, Color.AntiqueWhite, anchory:-2);
            if (BuffString != null && hasRitualBuff)
            {
                Texture2D distortion = ModContent.Request<Texture2D>("HeavenlyArsenal/Assets/Textures/Extra/detail6").Value;
                Texture2D a = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
                Texture2D Glow = GennedAssets.Textures.GreyscaleTextures.BloomLine2;
                float theMagicFactorThatMakesEveryElectricShineEffectSoMuchBetter = MathF.Sin(Main.GlobalTimeWrappedHourly * 7 + npc.whoAmI);
                Color color = Color.Lerp(drawColor.MultiplyRGBA(Color.Red), Color.Red, theMagicFactorThatMakesEveryElectricShineEffectSoMuchBetter) * npc.Opacity;

                List<Vector2> points = new List<Vector2>();
                points.AddRange(BuffString.GetPoints());
                points.Add(npc.Center);
                for (int i = 1; i < points.Count - 2; i++)
                {
                    Vector2 DrawPos = points[i] - Main.screenPosition;
                    float rot = points[i].AngleTo(points[i - 1]);

                    Vector2 element = points[i];
                    Vector2 diff = points[i + 1] - element;
                    Vector2 scale = new Vector2(diff.Length() + 2f, 2f);
                    Main.EntitySpriteDraw(a, DrawPos, null, color, rot, a.Size() / 2, scale, SpriteEffects.None);
                    Main.EntitySpriteDraw(Glow, DrawPos, null, color with { A = 0 }, rot + MathHelper.PiOver2, Glow.Size() / 2, new Vector2(scale.X * 0.01f, diff.Length() * 0.00195f), SpriteEffects.None);
                    //Utils.DrawBorderString(spriteBatch, i.ToString(), DrawPos, Color.AntiqueWhite);
                }

                RitualBuffNPC Ba = npc.GetGlobalNPC<RitualBuffNPC>();


               // spriteBatch.End();
               // spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.AnisotropicWrap, default, RasterizerState.CullNone, default, Main.GameViewMatrix.ZoomMatrix);
                //spriteBatch.Draw(distortion, points[points.Count / 2] - screenPos, null, color, 0, distortion.Size() / 2, 1, 0,0);
                //spriteBatch.ResetToDefault();
                //String d = "";
                //d += $"{Ba.BuffType}\n";
                //d += $"tier: {Ba.ritualBuffTier}\n";
                //Utils.DrawBorderString(Main.spriteBatch, d, npc.Center - Main.screenPosition, Color.AntiqueWhite, 1, anchory: -2);

                /*
                string r = "null";
                if (BuffGranter != null)
                {
                    r = BuffGranter.FullName;
                }
                if (npc.type != ModContent.NPCType<RitualAltar>())
                {
                  
                      // Utils.DrawBorderString(spriteBatch, $"{hasRitualBuff}, Buffgranter: {r},\n Timer: {ritualBuffTimer},\n type:{BuffType.ToString()}", Vector2.UnitX * -60 + npc.Center - Main.screenPosition - Vector2.UnitY * -30, Color.Red);

                  
                }*/
            }
            return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
        }

    }
}
