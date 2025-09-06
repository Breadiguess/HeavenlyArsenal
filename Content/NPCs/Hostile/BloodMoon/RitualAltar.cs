using CalamityMod;
using CalamityMod.Projectiles;
using HeavenlyArsenal.Common.utils;
using HeavenlyArsenal.Content.Items.Weapons.Summon.SolynButterfly;
using Luminance.Assets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon
{
    public class RitualAltar : BloodmoonBaseNPC
    {
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        public override float buffPrio => 0;
        public override bool canBeSacrificed => false;
        
        public override int bloodBankMax => 500;

        public bool isSacrificing = false;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 1;
            NPCID.Sets.CantTakeLunchMoney[NPC.type] = true;
        }
        public override void SetDefaults()
        {
            NPC.friendly = false;

            //blood = bloodBankMax;
            NPC.lifeMax = 300_000;
            NPC.damage = 60;
            NPC.defense = 70;


            NPC.width = 34;
            NPC.height = 60;
        }

        public enum AltarAI
        {
            LookingForSacrifice,
            Sacrificing,

            lookForBuffTargets,
            Buffing
        }
        public AltarAI currentAIState = AltarAI.LookingForSacrifice;
        public override void AI()
        {

            StateMachine();


           
        }

        private void StateMachine()
        {
            switch (currentAIState)
            {
                case AltarAI.LookingForSacrifice:
                    if(blood < bloodBankMax)
                    {
                        SacrificeNPC();
                    }
                    else
                    {
                        currentAIState = AltarAI.lookForBuffTargets;
                    }
                    break;
                case AltarAI.Sacrificing:
                    SacrificeNPC();
                    if(blood >= bloodBankMax)
                    {
                        currentAIState = AltarAI.lookForBuffTargets;
                    }
                    break;
                case AltarAI.lookForBuffTargets:
                    BuffOtherEnemies();
                    if(blood < bloodBankMax / 5)
                    {
                        currentAIState = AltarAI.LookingForSacrifice;
                    }
                    break;
                case AltarAI.Buffing:
                    BuffOtherEnemies();
                    if(blood < bloodBankMax / 5)
                    {
                        currentAIState = AltarAI.LookingForSacrifice;
                    }
                    break;
            }
        }

        private void BuffOtherEnemies()
        {
            List<NPC> nearbyNpcs = new List<NPC>();


            if (blood > bloodBankMax / 5)
            {
                blood = (int)Math.Sqrt(blood);

                foreach (NPC npc in Main.npc)
                {
                    if (BlackListProjectileNPCs.BlackListedNPCs.Contains(npc.type))
                        continue;
                    if (npc.type == NPC.type) //don't buff yourself or other ritual altars
                        continue;

                    if (npc.Distance(NPC.Center) > 300)
                        continue;

                    if (npc.GetGlobalNPC<RitualBuffNPC>().hasRitualBuff)
                        continue;



                    nearbyNpcs.Add(npc);

                }

            }

            if (nearbyNpcs.Count > 0)
            {
                // Sort by buffPrio if the npc is a BloodmoonBaseNPC
                nearbyNpcs.Sort((a, b) =>
                {
                    float aPrio = (a.ModNPC is BloodmoonBaseNPC aBloodmoon) ? aBloodmoon.buffPrio : 0f;
                    float bPrio = (b.ModNPC is BloodmoonBaseNPC bBloodmoon) ? bBloodmoon.buffPrio : 0f;
                    return bPrio.CompareTo(aPrio); // descending order, highest prio first
                });
                
                NPC target = nearbyNpcs[0];

                if (!target.GetGlobalNPC<RitualBuffNPC>().hasRitualBuff)
                {
                    target.GetGlobalNPC<RitualBuffNPC>().BuffGranter = NPC;
                    target.GetGlobalNPC<RitualBuffNPC>().ApplyRitualBuff();
                    CombatText.NewText(target.Hitbox, Color.Red, "Buffed!");
                }
            }


        }

        private void SacrificeNPC()
        {
            foreach(NPC npc in Main.npc)
            {
                if (BlackListProjectileNPCs.BlackListedNPCs.Contains(npc.type))
                    continue;

                if(npc.type == NPC.type) //don't sacrifice yourself or other ritual altars
                    continue;

                if(npc.immortal || npc.dontTakeDamage)
                    continue;

                // ✅ Check if this NPC is a BloodmoonBaseNPC
                if (npc.ModNPC is BloodmoonBaseNPC bloodmoonNpc)
                {
                    if (!bloodmoonNpc.canBeSacrificed)
                        continue;
                }

                if (Vector2.Distance(npc.Center, NPC.Center) < 100f && npc.active && !npc.friendly && !npc.boss)
                {
                    SacrificeNPC a = npc.GetGlobalNPC<SacrificeNPC>();
                    if (!a.isSacrificed)
                    {
                        a.isSacrificed = true;
                        a.Priest = this;
                        isSacrificing = true;
                        SoundEngine.PlaySound(SoundID.Item3, NPC.position);
                        break; //only sacrifice one at a time
                    }
                }

            }
        }

        private void LookForSacrifice()
        {
            if(blood < bloodBankMax)
            {
                foreach(Player player in Main.player)
                {
                    if(player.active && !player.dead && Vector2.Distance(player.Center, NPC.Center) < 100f)
                    {

                        if (player.HasIFrames())
                            continue;
                        //sacrifice the player
                        int bloodToSacrifice = 50; //amount of blood to sacrifice
                        blood += bloodToSacrifice;
                        if(blood > bloodBankMax)
                        {
                            blood = bloodBankMax;
                        }
                        player.Hurt(Terraria.DataStructures.PlayerDeathReason.ByCustomReason($"{player.name} was sacrificed at the Ritual Altar."), 20, 0);
                        //play sound
                        SoundEngine.PlaySound(SoundID.Item3, NPC.position);
                        break; 
                    }
                }
            }
        }
    }

    public class SacrificeNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public bool isSacrificed = false;
        public int SacrificeTimer = 0;
        public int SacrificeDuration = 60*3;
        public Vector2 OriginalPosition;
        public RitualAltar Priest;
        public override bool PreAI(NPC npc)
        {
            if(isSacrificed)
            {


                npc.noGravity = false;
                
                
                npc.Center = Vector2.Lerp(OriginalPosition, OriginalPosition + new Vector2(0, -75), SacrificeTimer/(float)SacrificeDuration);

                if (SacrificeTimer >= SacrificeDuration)
                {
                    SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.BloodCry with { MaxInstances = 0});
                    npc.StrikeInstantKill();
                    Priest.blood += npc.lifeMax;
                    Priest.isSacrificing = false;
                    SacrificeTimer = 0;
                    isSacrificed = false;
                }

                SacrificeTimer++;
                return false; //lambs don't fight the slaughter
            }
            else
                OriginalPosition = npc.Center;

            return base.PreAI(npc);
        }

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D Outline = GennedAssets.Textures.GreyscaleTextures.Corona;
            Vector2 drawPos = npc.Center - screenPos;

            if (isSacrificed)
            {
                float scale =  (float)SacrificeTimer / SacrificeDuration;
                float alpha = 1f - (float)SacrificeTimer / SacrificeDuration;
                spriteBatch.Draw(Outline, drawPos, null, Color.Red with { A = 0 } * alpha, 0f, Outline.Size() / 2, scale, SpriteEffects.None, 0f);
            }

            //if(npc.type != ModContent.NPCType<RitualAltar>())
             //   Utils.DrawBorderString(spriteBatch, $"{SacrificeTimer}/{SacrificeDuration}",drawPos +Vector2.UnitY*40, Color.Red,1);

            return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
        }
    }
    public class RitualBuffNPC : GlobalNPC
    {
        public enum RitualBuffType
        {
            None,
            Ressurection,
            Damagebuff,
            CooldownReduction,
            LifeRegen,
            Vampirism
        }
        public override bool InstancePerEntity => true;
        public bool hasRitualBuff = false;
        public int ritualBuffTimer = 0;
        public int ritualBuffDuration = 600; //10 seconds
        public RitualBuffType BuffType;
        public NPC BuffGranter = null;

        public Rope BuffString;

        public bool IsRessurecting = false;
        public int Ressurectiontimer = 0;
        private void ManageRessurection(NPC npc)
        {
            Ressurectiontimer++;

            if (Ressurectiontimer > 120)
            {
                npc.immortal = false;
                ClearRitualBuff(npc);
                Ressurectiontimer = 0;
                IsRessurecting = false;

            }
            
        }
        public override bool CheckDead(NPC npc)
        {
            if(hasRitualBuff && BuffType == RitualBuffType.Ressurection)
            {
                 IsRessurecting = true;
                return false;//prevent death because FUCK YOU
            }
            return base.CheckDead(npc);
        }
       
        public override bool PreAI(NPC npc)
        {
            if(BuffGranter ==  null)
            {
                DestroyBuffString();
                hasRitualBuff = false;

            }

            if (IsRessurecting)
            {
                ManageRessurection(npc);
            }


            if (hasRitualBuff)
            {
                ManageBuffstring(npc);
                if(ritualBuffTimer > 0)
                    ritualBuffTimer--;

                if (ritualBuffTimer <= 0)
                    hasRitualBuff = false;
            }
            return base.PreAI(npc);
        }

        private void ManageBuffstring(NPC npc)
        {
            if (BuffString == null)
            {
                BuffString = new Rope(BuffGranter.Center, npc.Center, 40, 10, Vector2.Zero);
            }

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
            BuffString.segments[^1].position = BuffGranter.Center;
            BuffString.Update();
        }
        private void DestroyBuffString()
        {
            BuffString = null;
        }

        #region apply/remove buff
        public void ApplyRitualBuff()
        {

            int buffType = Main.rand.Next(0, 6);
            hasRitualBuff = true;
            BuffType = (RitualBuffType)buffType;
            ritualBuffTimer = ritualBuffDuration;
        }

        public void ClearRitualBuff(NPC npc)
        {
            hasRitualBuff = false;
            BuffType = 0;
            BuffGranter = null;
        }
        #endregion


        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (BuffString != null && hasRitualBuff)
            {
                Texture2D a = GennedAssets.Textures.GreyscaleTextures.WhitePixel;

                List<Vector2> points = new List<Vector2>();
                points.AddRange(BuffString.GetPoints());
                points.Add(npc.Center);
                for (int i = 1; i < points.Count - 1; i++)
                {
                    Vector2 DrawPos =points[i] - Main.screenPosition;
                    float rot = points[i].AngleTo(points[i - 1]);
                    Main.EntitySpriteDraw(a, DrawPos, null, Color.Red, rot, a.Size() / 2, new Vector2(4, 1), SpriteEffects.None);
                }
            }
            string r = "null";
            if(BuffGranter != null)
            {
                r = BuffGranter.FullName;
            }
            if(npc.type != ModContent.NPCType<RitualAltar>())
            {
                if (npc.ModNPC is BloodmoonBaseNPC bloodmoonNpc)
                {
                    Utils.DrawBorderString(spriteBatch, $"{hasRitualBuff}, Buffgranter: {r}, Timer: {ritualBuffTimer}, type:{BuffType.ToString()}", Vector2.UnitX * -60 + npc.Center - Main.screenPosition - Vector2.UnitY * -30, Color.Red);

                }
            }
          
            return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
        }
    }
}
