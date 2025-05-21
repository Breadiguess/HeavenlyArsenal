using CalamityMod;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.NPCs.CalamityAIs.CalamityRegularEnemyAIs;
using CalamityMod.World;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.siphonophore
{
    public class Cryonophore : ModNPC
    {
        #region setup
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 6;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.damage = 42;
            NPC.width = 50;
            NPC.height = 64;
            NPC.defense = 10;
          
            NPC.lifeMax = 300;
            NPC.knockBackResist = 0.4f;
            NPC.value = Item.buyPrice(0, 0, 5, 0);
            NPC.HitSound = SoundID.NPCHit5;
            NPC.DeathSound = SoundID.NPCDeath7;
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<CryonBanner>();
            NPC.coldDamage = true;
            NPC.Calamity().VulnerableToHeat = true;
            NPC.Calamity().VulnerableToCold = false;
            NPC.Calamity().VulnerableToSickness = false;

          
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.BloodMoon,
                
                new FlavorTextBestiaryInfoElement("Mods.HeavenylArsenal.Bestiary.Cryonophore_Basic")
            });
        }

        public enum CryonophorAI
        {
            Idle,
            SummonBlizzard,
            Attack,
            Retreat
        }
        public CryonophorAI CurrentState;
        #endregion
        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);
        }
        public override void AI()
        {
            switch (CurrentState)
            {
                case CryonophorAI.Idle:
                    NPC.TargetClosest();
                    if (NPC.HasValidTarget)
                    {
                        NPC.velocity.X = 0f;
                        NPC.velocity.Y = 0f;
                        CurrentState = CryonophorAI.Attack;
                    }
                    break;
                case CryonophorAI.Attack:
                    break;
                case CryonophorAI.Retreat:
                    break;
            }
        }
        public override void OnKill()
        {
            base.OnKill(); 
        }
        public override bool SpecialOnKill() => true;

        public override void FindFrame(int frameHeight)
        {
            if ((NPC.velocity.Y > 0f || NPC.velocity.Y < 0f) && !NPC.IsABestiaryIconDummy)
            {
                NPC.spriteDirection = NPC.direction;
                NPC.frame.Y = frameHeight * 5;
                NPC.frameCounter = 0.0;
            }
            else
            {
                if (NPC.IsABestiaryIconDummy)
                {
                    NPC.frameCounter += 2;
                }
                else
                {
                    NPC.frameCounter += (double)(NPC.velocity.Length() / 2f);
                }
                NPC.spriteDirection = NPC.direction;
                if (NPC.frameCounter > 12.0)
                {
                    NPC.frame.Y = NPC.frame.Y + frameHeight;
                    NPC.frameCounter = 0.0;
                }
                if (NPC.frame.Y >= frameHeight * 4)
                {
                    NPC.frame.Y = 0;
                }
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            /*
            return spawnInfo.Player.ZoneSnow &&
                !spawnInfo.Player.PillarZone() &&
                !spawnInfo.Player.ZoneDungeon &&
                !spawnInfo.Player.InSunkenSea() &&
                Main.hardMode && !spawnInfo.PlayerInTown && !spawnInfo.Player.ZoneOldOneArmy && !Main.snowMoon && !Main.pumpkinMoon ? 0.045f : 0f;
       */
            if(Main.bloodMoon && DownedBossSystem.downedYharon)
                return 0.045f;
            else
                return 0f;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            if (hurtInfo.Damage > 0)
            {
                target.AddBuff(BuffID.Frostburn, 120, true);
                target.AddBuff(BuffID.Chilled, 90, true);
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 3; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Frost, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                for (int k = 0; k < 15; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Frost, hit.HitDirection, -1f, 0, default, 1f);
                }
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot) => npcLoot.Add(ModContent.ItemType<EssenceofEleum>());
    }
}