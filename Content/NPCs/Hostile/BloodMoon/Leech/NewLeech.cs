using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Leech
{
    #region StolenCode
    /*
                                             .:-:                             
                                           .--.--                             
                                          :=-:.==                             
                                 .:::.. .:-:-=:=-                             
                                 .:.::::....-=:::                             
                                 .:-::::::. =-.:-.                            
                                   ::.      ..:.:=-.                          
                                    .-:.      ..:..=:                         
                                      .. .:-+=::=+=.                          
                                       .-::-=-:..                             
                                      .:-:.                                   
                                 .:::.:+=:::.                                 
                          .:==++++-::-=+-::-+++==-:                           
                        .:=+=----::.....      ..:===:.                        
                    .::=++==+**+==:....           .:=+-::.                    
                    :-====**=:.:::-:. .:.   .:::..   .:=-.                    
                  .-+=-=++-... .:   :+##*-.   .. ...    :=:.                  
                 .-+---..:....::   -%%=.=#+:..    ...    .=:.                 
                 :+--........:. .-#%##+. :*%+::--:...     .-:                 
                .--:...:..:....-*#%%%%*:  .-*#+:..:--:     :..                
                .==. .:.::..:=##*#%%%#+:    .-+*+-:.:=:    :-.                
               .-=-  .::--+#%%%@@%#%%*=:       .-+**+*=.   :=-.               
              .::=-..:+%@####%%%##%##**+:         .-+%%-   =-::.              
                .-*:.:#@@##%%%%**+++++*-.            *%:..-+-.                
                 :%%@#*%@%%%#%#*+==++**+-    .....  .*%=#@%#-                 
                  -@%#%@@%%%@@%@@@@@%%+:--=@@@@@@@+..#@=-+%-                  
                  .*#%#@%%%**@@@@@@@%*=:...*@@@@@+. :#%=*%+...                
                ....+%%#%@%**#%%##*-=*=.::..:---:.  -#+*#=..                  
                    :-+@@@%#**###*-:+#=.    .  .... =%%+..                    
                    .-=#@@######**+#%#=.       .:. .*@*-.                     
                    .:=*%@%%##%%###%%#=..:.   ...  :#*==:.                    
                    .--##@@%#**%%%%%%@@%+:.   .:. :#@+==-.                    
                     .-*#@@@###@%%%%####=:....:: -%@%+*=.                     
                    .-:****%@%#%%%%#+:..:-:  .. =#*=+++-::.                   
                    ...-**+-+@@@##%###*+=:.  .=%@+.=+=:...                    
                       .-*++*%#@@@%#%%#-   :*%#=*=.--:.                       
                        -#+:+@##%@@@%#*++*#%#=..**:--..                       
                     .:=*@@%%@%%%%%#*+=+==:.   .*%#%%#=:.                     
    */
    #endregion


    public enum UmbralLeechAI
    {
        Idle,
        SeekTarget,
        FeedOnTarget,
        FlyAway,
        DeathAnim
    }
    class NewLeech :ModNPC
    {
        #region setup

        List<Vector2> SegmentPos = [];

        public UmbralLeechAI CurrentState;
        /// <summary>
        /// stores the npc ID of the head for the segments
        /// </summary>
        public ref float HeadID => ref NPC.ai[0]; 
        public ref float Time => ref NPC.ai[1];
       
        public ref float SegmentNum => ref NPC.ai[2];

        public ref float SegmentCount => ref NPC.localAI[0];

        public static readonly SoundStyle Bash = new SoundStyle("HeavenlyArsenal/Assets/Sounds/NPCs/Hostile/BloodMoon/UmbralLeech/UmbralLeech_Bash_", 3);
        public static readonly SoundStyle Explode = new SoundStyle("HeavenlyArsenal/Assets/Sounds/NPCs/Hostile/BloodMoon/UmbralLeech/GORE - Head_Crush_3");
        public static readonly SoundStyle Gore = new SoundStyle("HeavenlyArsenal/Assets/Sounds/NPCs/Hostile/BloodMoon/UmbralLeech/GORE - Giblet_Drop_3");
        public override string Texture => "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Leech/UmbralLeech";
        public override void SetDefaults()
        {
            NPC.width = NPC.height = 30;
            NPC.lifeMax = 50000;
            NPC.damage = 200;
            NPC.npcSlots = 0f;
            NPC.aiStyle = -1;
            NPC.defense = 330;
            NPC.noGravity = true;
        }

        public override void SetStaticDefaults()
        {
            NPCID.Sets.CantTakeLunchMoney[NPC.type] = true;
            NPCID.Sets.CannotDropSouls[NPC.type] = true;
            NPCID.Sets.DoesntDespawnToInactivityAndCountsNPCSlots[NPC.type] = true;
            

        }
        #endregion
        #region AI
        public override void AI()
        {

            NPC head = Main.npc[(int)HeadID];
            foreach (var npcScan in Main.ActiveNPCs)
            {
                if (npcScan.type == Type && npcScan.ai[0] == HeadID && npcScan.ai[1] == NPC.ai[1] - 1)
                {
                    SegmentPos[0] = head.Center;
                    for (int i = 1; i < SegmentPos.Count; i++)
                    {
                        SegmentPos[i] = SegmentPos[i - 1] + Vector2.Normalize(SegmentPos[i - 1].DirectionTo(SegmentPos[i]) * 9 - Vector2.Normalize(head.velocity)) * 26;
                        
                    }
                    if (SegmentNum < 2)
                    {
                        if (--Time < 1)
                        {
                            Time = 20;
                        }
                        //NPC.Center = SegmentPos[(int)NPC.ai[1] - 1] - Vector2.Normalize(head.velocity).RotatedBy(Math.Sin(Math.PI * Time / 10f) / 2f) * NPC.width / 2f;
                        NPC.Center = SegmentPos[(int)NPC.ai[1] - 1] - Vector2.Normalize(head.velocity).RotatedBy(Math.Sin(Math.PI * Time / 10f) / 2f) * 26;
                        NPC.Center = npcScan.Center + npcScan.DirectionTo(NPC.Center) * 26;
                        NPC.rotation = NPC.DirectionTo(npcScan.Center).ToRotation();
                    }
                    else
                    {
                        if (--Time < NPC.ai[1] * 4 + 1)
                        {
                            Time = 20 + NPC.ai[1] * 4;
                        }
                        //NPC.Center = SegmentPos[(int)NPC.ai[1] - 1] - Vector2.Normalize(head.velocity).RotatedBy(Math.Sin(Math.PI * NPC.ai[2] / 10f) / 2f) * NPC.width / 2f;
                        NPC.Center = SegmentPos[(int)NPC.ai[1] - 1] - Vector2.Normalize(head.velocity).RotatedBy(Math.Sin(Math.PI * Time / 10f) / 2f) * 26;
                        NPC.Center = npcScan.Center + npcScan.DirectionTo(NPC.Center) * 26;
                        NPC.rotation = NPC.DirectionTo(npcScan.Center).ToRotation();
                    }
                }
            }


            if (NPC.life > 0)
            {
                switch (CurrentState)
                {
                    case UmbralLeechAI.Idle:
                        
                        break;
                    case UmbralLeechAI.SeekTarget:  
                        
                        break;
                    case UmbralLeechAI.FeedOnTarget:
                        
                        break;
                    case UmbralLeechAI.FlyAway:
                        
                        break;
                   
                }
            }
            else
            {
               CurrentState = UmbralLeechAI.DeathAnim;
               DoDeathAnimation();
            }
        }
        public override bool CheckDead()
        {
            NPC.life = 1;
            CurrentState = UmbralLeechAI.DeathAnim;

            NPC.active = true;
            NPC.dontTakeDamage = true;

            NPC.netUpdate = true;

            // Prevent netUpdate from being blocked by the spam counter.
            if (NPC.netSpam >= 10)
                NPC.netSpam = 9;

            return false;
            
        }

        public float DeathAnimationTimer;
        public void DoDeathAnimation()
        {
            for (int i = 0; i < SegmentCount; i++)
            {
                NPC.velocity *= 0.1f;
            }




            if (DeathAnimationTimer == 1f)
            {
                if (Main.netMode != NetmodeID.Server && Main.LocalPlayer.WithinRange(NPC.Center, 4800f))
                    SoundEngine.PlaySound(Bash with { Volume = 1.65f });
            }
            // Begin fading out before the exploding sun animation happens.
            //if (DeathAnimationTimer >= 370f)
            //NPC.Opacity *= 0.97f;

            if (DeathAnimationTimer == 92f)
            {
                // SoundEngine.PlaySound(HolyBlast.ImpactSound, NPC.Center);
                // if (Main.netMode != NetmodeID.MultiplayerClient)
                //     Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<HolyExplosionBoom>(), 0, 0f);
            }

            if (Main.netMode == NetmodeID.Server && DeathAnimationTimer % 45f == 44f)
            {
                NPC.netUpdate = true;

                // Prevent netUpdate from being blocked by the spam counter.
                if (NPC.netSpam >= 10)
                    NPC.netSpam = 9;
            }

            // Die and create drops after the star is gone.
            if (DeathAnimationTimer >= 125f)
            {
                NPC.active = false;
                NPC.HitEffect();
                NPC.NPCLoot();

                NPC.netUpdate = true;

                // Prevent netUpdate from being blocked by the spam counter.
                if (NPC.netSpam >= 10)
                    NPC.netSpam = 9;
            }

            DeathAnimationTimer++;

        }

        public override void OnSpawn(IEntitySource source)
        {
            if(SegmentNum == 0) 
                for (int i = 1; i < Main.rand.Next(6, 16); i++)
                {
                NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X+i, (int)NPC.Center.Y + i, Type, default, NPC.whoAmI, i, i * 4);
                }
        }
        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            if (SegmentNum <= 1)
            {
                NPC.velocity -= NPC.DirectionTo(item.Center) * item.knockBack * 0.2f;
            }
            else
            {
                Main.npc[(int)HeadID].life -= damageDone;
                NPC.life = Main.npc[(int)HeadID].life;
            }
        }
        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            if (SegmentNum < 1)
            {
                NPC.velocity -= NPC.DirectionTo(projectile.Center) * projectile.knockBack * 0.2f;
            }
            else
            {
                Main.npc[(int)HeadID].life -= damageDone;
                NPC.life = Main.npc[(int)HeadID].life;
            }
        }
        #endregion

        #region drawcode
        private void DrawWhiskers(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D Whisker = AssetDirectory.Textures.UmbralLeechWhisker.Value;
          
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Leech/UmbralLeech2").Value;
            Rectangle sourceRect = texture.Frame(3, 8, NPC.ai[1] < 1 ? 2 : 1, 0);
            Main.EntitySpriteDraw(texture, NPC.Center - Main.screenPosition, sourceRect, Lighting.GetColor((NPC.Center / 16).ToPoint()), NPC.rotation, sourceRect.Size() / 2f, 1, SpriteEffects.None);
            /*
            if (SegmentNum % 2 == 0)
            {
                Utils.DrawBorderString(Main.spriteBatch, "| State: " + CurrentState.ToString(), NPC.Center - Vector2.UnitY * 160 - Main.screenPosition, Color.White);
                Utils.DrawBorderString(Main.spriteBatch, "| DeathTimer: " + DeathAnimationTimer.ToString(), NPC.Center - Vector2.UnitY * 140 - Main.screenPosition, Color.White);


            }
            if (SegmentNum != 0)
            {
                Utils.DrawBorderString(Main.spriteBatch, "| HeadID: " + HeadID.ToString(), NPC.Center - Vector2.UnitY * 120 - Main.screenPosition, Color.White);
                Utils.DrawBorderString(Main.spriteBatch, "| Segment: " + SegmentNum.ToString(), NPC.Center - Vector2.UnitY * 100 - Main.screenPosition, Color.White);

            }
            */
            return false;
        }
        #endregion
    }
}
