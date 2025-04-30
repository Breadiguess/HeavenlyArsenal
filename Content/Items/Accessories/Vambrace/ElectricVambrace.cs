using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Accessories.Vambrace
{
   
    public class ElectricVambrace : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";

        public override void SetDefaults()
        {
            Item.width = 54;
            Item.height = 56;
            Item.value = CalamityGlobalItem.RarityLightRedBuyPrice;
            Item.rare = ModContent.RarityType<HotPink>();
            Item.accessory = true;
        }
        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(6, 8));
            ItemID.Sets.AnimatesAsSoul[Type] = true;
        }


        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<ElectricVambracePlayer>().ElectricVambrace = true;
            var modPlayer = player.Calamity();
            modPlayer.transformer = true;
            modPlayer.aSpark = true;

            //modPlayer.DashID = ElectricVambraceDash.ID;
        }


        public override void AddRecipes()
        {
            CreateRecipe().
            AddIngredient<SlagsplitterPauldron>(1).
            AddIngredient<LeviathanAmbergris>(1).
            AddIngredient<AscendantSpiritEssence>(8).
            AddTile<CosmicAnvil>().
            Register();
        }
    }
    public class ElectricVambracePlayer : ModPlayer
    {
        internal bool ElectricVambrace;
        public bool HasReducedDashFirstFrame 
        { 
            get; 
            private set; 
        }
        public bool isVambraceDashing
        {
            get;
            set;
        }

        
        public override void Load()
        { 

        }
        public override void PostUpdate()
        {
            if (ElectricVambrace)
            {
                if (Player.miscCounter % 8 == 7 && Player.dashDelay > 0) // Reduced dash cooldown by 38%
                    Player.dashDelay--;

                //Console.WriteLine(Player.dashDelay);

                if (Player.dashDelay == -1)
                {
                    if (Player.miscCounter % 6 == 0 && Player.velocity != Vector2.Zero)
                    {
                        int damage = Player.ApplyArmorAccDamageBonusesTo(Player.GetBestClassDamage().ApplyTo(170));
                        Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center + Player.velocity * 1.5f, Vector2.Zero, ModContent.ProjectileType<VambraceDash>(), damage, 10f, Player.whoAmI);
                    }
                    Player.endurance += 0.20f;
                    if (!isVambraceDashing) // Dash isn't reduced, this is used to determine the first frame of dashing
                    {
                        SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Volume = 0.4f, PitchVariance = 0.4f }, Player.Center);

                        int damage = Player.ApplyArmorAccDamageBonusesTo(Player.GetBestClassDamage().ApplyTo(750));
                        isVambraceDashing = true;


                    }

                    else
                        isVambraceDashing = false;
                }
            }

            //there should be a better way to setup the lightning
            
            }


        public override void PostUpdateMiscEffects()
        {
            if (ElectricVambrace)
            {

            }
        }
        public override void ResetEffects()
        {
            ElectricVambrace = false;

        }
    }

    public class VambraceLightningNPC : GlobalNPC
    {

        public static List<int> HitNPC = new List<int>();
        public override bool InstancePerEntity => true;
        
        
        public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            if (projectile.type == ModContent.ProjectileType<VambraceLightning>() && !HitNPC.Contains(npc.whoAmI))
            {
                // Append the NPC's ID to the HitNPC list
                HitNPC.Add(npc.whoAmI);
            }

            base.OnHitByProjectile(npc, projectile, hit, damageDone);
        }

        public override void PostAI(NPC npc)
        {
         

            if (!Main.projectile.Any(proj => proj.active && proj.type == ModContent.ProjectileType<VambraceLightning>()) && HitNPC.Contains(npc.whoAmI))
            {
                // Remove the NPC's ID from the HitNPC list as soon as lightning is gone
                HitNPC.Remove(npc.whoAmI);
            }

            base.PostAI(npc);
        }
        public override void ResetEffects(NPC npc)
        {
            
        }
    }

    public class VambraceLightning : ModProjectile
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        private const int MaxJumps = 5;
        private const float JumpRange = 400f;

        // Tracks which NPCs have been hit by this instance
        public ref List<int> hitNPCs => ref VambraceLightningNPC.HitNPC;

        // Tracks the projectile index of the parent that spawned this instance
        private int ParentProjID;
        public Vector2 ParentCenter => Main.projectile[ParentProjID].Center;
        public ref float Time => ref Projectile.ai[0];
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.AllowsContactDamageFromJellyfish[Type] = true;
            ProjectileID.Sets.TrailingMode[Type] = 1;
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.DontApplyParryDamageBuff[Type] = true;
            ProjectileID.Sets.NoMeleeSpeedVelocityScaling[Type] = true;

        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 64;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 500;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            //todo: check global npc VambraceLightningNPC for a list of npcs that have been hit already.
            //then, run a check find the nearst npc to hit.
            // this projectile should then be teleported instantly to the npc.
        }


        public override void OnSpawn(IEntitySource source)
        {
            //get the projectile that created this one.it should be another vambrace lightning projectile.
        }
        public override void OnKill(int timeLeft)
        {
            base.OnKill(timeLeft);
        }


        public override bool? CanCutTiles() => false;

        public override bool? CanHitNPC(NPC target)
        {
            if (hitNPCs.Contains(target.whoAmI))
                return false;
            return true;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            int currentJumps = (int)Projectile.ai[0];

            // Record hit
            if (!hitNPCs.Contains(target.whoAmI))
                hitNPCs.Add(target.whoAmI);
            if (currentJumps >= MaxJumps)
            {
                //Projectile.Kill();
                return;
            }

            // Find next target
            NPC nextTarget = null;
            float closestDist = JumpRange;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.life > 0)
                {
                    float dist = Vector2.Distance(npc.Center, Projectile.Center);
                    if (dist < closestDist && !hitNPCs.Contains(npc.whoAmI))
                    {
                        closestDist = dist;
                        nextTarget = npc;
                    }
                }
            }
            if (nextTarget == null)
            {
                //Projectile.Kill();
                return;
            }
            Projectile.ai[0] = currentJumps + 1;

            Vector2 direction = nextTarget.Center - Projectile.Center;
            direction.Normalize();
            direction *= 12f;
            int newProj = Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                direction,
                Type,
                Projectile.damage,
                Projectile.knockBack,
                Projectile.owner,
                Projectile.ai[0],
                Projectile.whoAmI
            );
            if (Main.projectile[newProj].ModProjectile is VambraceLightning vd)
            {
                vd.hitNPCs = new List<int>(hitNPCs);
            }
            Projectile.Kill();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            //todo: draw chaining lightning between vambrace lightning projecitles.

           return false;
        }
    }

}
