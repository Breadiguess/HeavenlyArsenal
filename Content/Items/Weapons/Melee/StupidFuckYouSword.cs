using CalamityMod.Buffs.DamageOverTime;
using HeavenlyArsenal.Content.Items.Armor.ShintoArmor;
using Microsoft.Xna.Framework; // Using another one library
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.Buffs;
using NoxusBoss.Content.Rarities;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee
{
    [LegacyName("StupidFuckYouSword")] 
    public class EdgyDualSwords : ModItem
    {
        public override string Texture => "HeavenlyArsenal/Content/Items/Weapons/Melee/StupidFuckYouSword";
        public override void HoldItem(Player player)
        {
            player.GetModPlayer<EdgyKatanaPlayer>().WeebMode = true;
        }
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1; // How many items need for research in Journey Mode
        }

        public override void SetDefaults()
        {
            // Visual properties
            Item.width = 40; // Width of an item sprite
            Item.height = 40; // Height of an item sprite
            Item.scale = 0f; // Multiplicator of item size, for example is you set this to 2f our sword will be biger twice. IMPORTANT: If you are using numbers with floating point, write "f" in their end, like 1.5f, 3.14f, 2.1278495f etc.
            Item.rare = ItemRarityID.Blue; // The color of item's name in game. See https://terraria.wiki.gg/wiki/Rarity
                                           //Item
                                           // Combat properties
            Item.damage = 5090; // Item damage
            Item.DamageType = DamageClass.Melee; // What type of damage item is deals, Melee, Ranged, Magic, Summon, Generic (takes bonuses from all damage multipliers), Default (doesn't take bonuses from any damage multipliers)
                                                 // useTime and useAnimation often use the same value, but we'll see examples where they don't use the same values
            Item.useTime = 120; // How long the swing lasts in ticks (60 ticks = 1 second)
            Item.useAnimation = 120; // How long the swing animation lasts in ticks (60 ticks = 1 second)
            Item.knockBack = 6f; // How far the sword punches enemies, 20 is maximal value
            Item.autoReuse = true; // Can the item auto swing by holding the attack button
            Item.noUseGraphic = false;


            // Other properties
            Item.value = 10000; // Item sell price in copper coins
            Item.useStyle = ItemUseStyleID.Swing; // This is how you're holding the weapon, visit https://terraria.wiki.gg/wiki/Use_Style_IDs for list of possible use styles
            Item.UseSound = SoundID.Item1; // What sound is played when using the item, all sounds can be found here - https://terraria.wiki.gg/wiki/Sound_ID|

            Item.shoot = ModContent.ProjectileType<OdatchiProjectile>();
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.statLife--;
            player.Hurt(PlayerDeathReason.ByPlayerItem(type, Item), 300, 3);
            return base.Shoot(player, source, position, velocity, type, damage, knockback);
        }
       

        // What is happening on hitting living entity
        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextBool(1)) // 1/4 chance, or 25% in other words
            {
                target.AddBuff(ModContent.BuffType<MiracleBlight>(), // Adding Poisoned to target
                     600); // for 5 seconds (60 ticks = 1 second)
                           //target.AddBuff(ModContent.BuffType<AntimatterAnnihilation>(), // Adding Poisoned to target
                           //     6000); // for 5 seconds (60 ticks = 1 second)

            }
        }

        // Creating item craft
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            // We are using custom material for the craft, 7 Steel Shards
            recipe.AddIngredient(ItemID.Wood, 3); // Also, we are using vanilla material to craft, 3 Wood
            recipe.AddIngredient(ItemID.JungleSpores, 5); // I've added some Jungle Spores to craft
            recipe.AddTile(TileID.Anvils); // Crafting station we need for craft, WorkBenches, Anvils etc. You can find them here - https://terraria.wiki.gg/wiki/Tile_IDs
            recipe.Register();
        }
    }

    internal class EdgyKatanaPlayer : ModPlayer
    {
        /// <summary>
        /// Dictates whether this modplayer is active or not.
        /// </summary>
        public bool WeebMode;

        public EdgyKatanaAlts Alts = new EdgyKatanaAlts();


        public float OdatchiCharge;
        public float WakizashiCharge;


       
        public override void OnEnterWorld()
        {
            DictateAlt();
        }



        /// <summary>
        /// Decide the alt based on the player's name
        /// </summary>
        private void DictateAlt()
        {
            Alts.CurrentAlt = EdgyKatanaAlts.AltType.Default;
            
            string playername = Main.LocalPlayer.name.ToLowerInvariant();
            // Lucille variant
            if (playername == "lucille" || playername == "lucille karma")
            {
                Alts.CurrentAlt = EdgyKatanaAlts.AltType.Sylveon;
            }

            //spamton variant
            if (playername == "ink" || playername == "stardustink" || playername == "spamton" || playername == "calligrapher" || playername == "numberone rated salesman")
            {
                Alts.CurrentAlt = EdgyKatanaAlts.AltType.Freedom;
            }
            

           
            
        
        }

        private CurrentWeapon Chosen = CurrentWeapon.Wakizashi;
        /// <summary>
        ///  the current Variant that the weapon is using.
        /// </summary>
        private enum CurrentWeapon
        {
            Wakizashi,
            Odatchi
        }

        public override void ResetEffects()
        {
            // If we are not in weeb mode, reset the charges
            if (!WeebMode)
            {
                OdatchiCharge = 0f;
                WakizashiCharge = 0f;
                return;
            }
           
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!WeebMode)
                return;


        }

        public override void PostUpdateMiscEffects()
        {
            if(!WeebMode)
                return;

            if (Player.GetModPlayer<ShintoArmorPlayer>().Enraged)
            {
                WakizashiCharge += 0.1f;
                OdatchiCharge += 0.1f;
            }
        }
    }

    internal class EdgyKatanaAlts
    {
        public AltType CurrentAlt = AltType.Default;
        public enum AltType
        {
            Default,
            RGB,
            Freedom,
            Sylveon
        }

        public static Vector3[] Default = new Vector3[]
        {
            new Vector3(255, 255, 255), //white
            new Vector3(0, 0, 0) //black
        };
        public static Vector3[] RGB = new Vector3[]
        {
            new Color(0, 0, 0).ToVector3(),
            new Color(0, 0, 255).ToVector3(),
            new Color(255, 0, 0).ToVector3(),
            new Color(255, 0, 255).ToVector3(),
            new Color(0, 255, 0).ToVector3(),
            new Color(255, 255, 255).ToVector3(),
            new Color(255, 255, 0).ToVector3(),
            new Color(0, 255, 255).ToVector3(),
            new Color(0, 0, 0).ToVector3()
        };
        public static Vector3[] Freedom = new Vector3[]
        {
           new Vector3(255,242,0),//yellow
           new Vector3(255, 174, 201) //pink
        };

        public static Vector3[] Sylveon = new Vector3[]
        {
            Color.LightPink.ToVector3(), //light pink
            Color.White.ToVector3(), //white
            Color.LightBlue.ToVector3()
        };

    }



    internal class OdatchiProjectile : ModProjectile
    {
        public ref Player Owner => ref Main.player[Projectile.owner];
        public override string Texture => "HeavenlyArsenal/Assets/Textures/Items/EdgyWeebSword/StickSword";

        public float SwingProgress = 0;
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 30;
            Projectile.hostile = false;
            Projectile.friendly = true;
            
        }

        public override void AI()
        {
            Projectile.Center = Owner.Center;
            Projectile.rotation = -MathHelper.PiOver2 + MathHelper.ToRadians(SwingProgress);


            SwingProgress = 0;
        }


        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Vector2 origin = new Vector2(0, texture.Height / 2);
            float rotation = Projectile.rotation - MathHelper.PiOver2;
            Color color = Color.White;
            float scale = 0.5f; 

            SpriteEffects Direct = Owner.direction == 1 ?  SpriteEffects.FlipVertically:SpriteEffects.None ;
            Main.EntitySpriteDraw(texture, drawPosition, null, color, rotation, origin, scale, Direct   , 0);

            return false;
        }
    }
}


