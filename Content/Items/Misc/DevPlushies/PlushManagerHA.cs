using CalamityMod;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor.Demonshade;
using CalamityMod.Items.Armor.Statigel;
using HeavenlyArsenal.Content.Items.Materials;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace HeavenlyArsenal.Content.Misc.DevPlushies
{
    public class PlushManagerHA : ModSystem
    {
        // Contains all of the plush items
        // To get a plush item, just insert the boss' name as per the wall of entries below
        public static Dictionary<string, int> PlushItems = new Dictionary<string, int>();

        // Load all of the plushies
        public override void Load()
        {
            LoadPlush("Shadow", 3, sound: AssetDirectory.Sounds.Items.Misc.PlayMyMod);
            LoadPlush("Ink", 3,sound: GennedAssets.Sounds.NPCKilled.DeltaruneExplosion.WithPitchOffset(-1));
            LoadPlush("Scav", 3, sound: GennedAssets.Sounds.Avatar.Chirp);
            LoadPlush("Multi", 3, sound: GennedAssets.Sounds.Avatar.AngryDistant);
            LoadPlush("Origami", 3, sound: AssetDirectory.Sounds.Items.Misc.OrigamiPlush);
            LoadPlush("Pil", 3, sound: AssetDirectory.Sounds.NPCs.Hostile.BloodMoon.UmbralLeech.Bash);
            LoadPlush("Sumii", 3, sound: GennedAssets.Sounds.NamelessDeity.Chuckle);
            LoadPlush("Vertis", 3, sound: AssetDirectory.Sounds.NPCs.Hostile.BloodMoon.UmbralLeech.Explode);
            LoadPlush("Bobulous",3);
        }

    

        /// <summary>
        /// Adds a plush 
        /// </summary>
        /// <param name="name">The name of the entity a plush is being made of. The sprites should follow the same naming convention with XPlush and XPlushPlaced</param>
        /// <param name="rarity">The rarity, same as the boss drops.</param>
        /// <param name="loadLegacy">Whether or not a legacy plush should be loaded for refunding. Set this to false for all future plushies.</param>
        /// <param name="width">The plush tile width</param>
        /// <param name="height">The plush tile height</param>
        public static void LoadPlush(string name, int rarity, bool loadLegacy = true, int width = 2, int height = 2, SoundStyle sound = default, string moddedRarity = "")
        {
            PlushItem item = new PlushItem(name, rarity, moddedRarity);
            PlushTile tile = new PlushTile(name, sound);
            PlushProj proj = new PlushProj(name);
            ModContent.GetInstance<HeavenlyArsenal>().AddContent(item);
            ModContent.GetInstance<HeavenlyArsenal>().AddContent(tile);
            ModContent.GetInstance<HeavenlyArsenal>().AddContent(proj);
            
            // Set the item's projectile and tile types, as well as the projectile's item drop type
            item.ProjectileType = proj.Type;
            item.TileType = tile.Type;
            proj.ItemType = item.Type;
            tile.Width = width;
            tile.Height = height;
            tile.ItemType = item.Type;
            // Add the item to the plush list
            PlushItems.Add(name, item.Type);
        }
    }
    [Autoload(false)]
    public class PlushItem : ModItem
    {
        public override string LocalizationCategory => "Items.Misc";
        public override string Texture => TexturePath;
        public override string Name => InternalName;

        public int ProjectileType;
        public int TileType;
        public int Rarity;
        public string ModdedRarity;
        public string TexturePath;
        public string InternalName;
        public string PlushName;
        protected override bool CloneNewInstances => true;

        public PlushItem(string name, int rarity, string moddedRarity = "")
        {
            PlushName = name;
            InternalName = name + "Plush";
            TexturePath = $"HeavenlyArsenal/Content/Items/Misc/DevPlushies/" + name + "Plush";
            Rarity = rarity;
            ModdedRarity = moddedRarity;
        }
        public override void SetStaticDefaults()
        {
            if (Name.Contains("EX"))
            {
                string withoutEX = PlushName.Replace("EX", "");
                ItemID.Sets.ShimmerTransformToItem[Type] = PlushManagerHA.PlushItems[withoutEX];
                ItemID.Sets.ShimmerTransformToItem[PlushManagerHA.PlushItems[withoutEX]] = Type;
            }
        }

        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Swing;
            Item.width = 44;
            Item.height = 44;
            Item.consumable = true;
            Item.UseSound = SoundID.Item1;
            //Item.rare = CVUtils.SetRarity(Rarity, ModdedRarity);
            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.value = 20;
            Item.createTile = TileType;
            Item.maxStack = 9999;
            Item.shoot = ProjectileType;
            Item.Calamity().devItem = true;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override void UseAnimation(Player player)
        {
            if (player.altFunctionUse == 2f)
            {
                Item.shoot = ProjectileType;
                Item.shootSpeed = 6f;
                Item.createTile = -1;
                // Calamitas plush has a custom projectile
                //if (Item.type == PlushManagerHA.PlushItems["Calamitas"])
                //{
                //    Item.shoot = ModContent.ProjectileType<CalaFumoSpeen>();
               // }
            }
            else
            {
                Item.shoot = ProjectileID.None;
                Item.shootSpeed = 0f;
                Item.createTile = TileType;
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            /*
            // Calamitas plush can randomly throw out very concerning alt variants
            if (Item.type == PlushManagerHA.PlushItems["Calamitas"])
            {
                if (Main.rand.NextFloat() < 0.01f)
                {
                    type = ModContent.ProjectileType<ItsReal>();
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.NPCHit49, player.position);
                    Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
                    return false;
                }
                else if (Main.rand.NextFloat() < 0.1f && CalValEX.month == 6 && CalValEX.day == 22)
                {
                    type = ModContent.ProjectileType<ItsReal>();
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.NPCHit49, player.position);
                    Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
                    return false;
                }
                else if (Main.rand.NextFloat() < 0.002f)
                {
                    type = ModContent.ProjectileType<ItsRealAlt>();
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.NPCHit49, player.position);
                    Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
                    return false;
                }
            }*/
            return true;
        }
        public override void AddRecipes()
        {
            var recipe = CreateRecipe()
           .AddIngredient(ItemID.Silk)
           .AddTile(TileID.Loom);

            
            recipe.Register();
        }
    }

    [Autoload(false)]
    public class PlushTile : ModTile
    {
        public override string Texture => TexturePath;
        public override string Name => InternalName;

        public string InternalName;
        public string TexturePath;

        public int Height = 2;
        public int Width = 2;

        public SoundStyle ClickSound;

        public int ItemType;

        public PlushTile(string name, SoundStyle sound)
        {
            InternalName = name + "PlushPlaced";
            TexturePath = $"HeavenlyArsenal/Content/Items/Misc/DevPlushies/" + name + "PlushPlaced";
            ClickSound = sound with { Pitch = 0.5f, MaxInstances = 0 };
        }

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Terraria.ID.TileID.Sets.DisableSmartCursor[Type] = true;
            Main.tileObsidianKill[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.Width = Width;
            TileObjectData.newTile.Height = Height;
            TileObjectData.newTile.AnchorBottom = new AnchorData(Terraria.Enums.AnchorType.Table | Terraria.Enums.AnchorType.SolidTile | Terraria.Enums.AnchorType.SolidWithTop | Terraria.Enums.AnchorType.AlternateTile, TileObjectData.newTile.Width, 0);
            List<int> heightArray = new List<int>(0);
            for (int i = 0; i < Height; i++)
            {
                heightArray.Add(16);
            }
            TileObjectData.newTile.CoordinateHeights = [.. heightArray];
            TileObjectData.addTile(Type);
            LocalizedText name = CreateMapEntryName();
            AddMapEntry(new Color(144, 148, 144), name);
            DustType = 11;
        }

        public override void PostSetupTileMerge()
        {
            // Allow plushies to be placed on any sleepable bed
            List<int> beds = new List<int>();
            for (int i = 0; i < TileLoader.TileCount; i++)
            {
                if (TileID.Sets.CanBeSleptIn[i])
                {
                    beds.Add(i);
                }
            }
            int[] bedArray = [.. beds];
            TileObjectData data = TileObjectData.GetTileData(Type, 0);
            data.AnchorAlternateTiles = bedArray;
        }

        public override void MouseOver(int i, int j)
        {
            Player localPlayer = Main.LocalPlayer;
            localPlayer.noThrow = 2;
            localPlayer.cursorItemIconEnabled = true;
            localPlayer.cursorItemIconID = ItemType;
        }

        public override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;
            SoundEngine.PlaySound(ClickSound, new Vector2(i * 16, j * 16));
            /*
            if (CalValEX.CalamityActive)
            {
                if (ItemType == PlushManagerHA.PlushItems["DevourerofGods"] || ItemType == PlushManagerHA.PlushItems["DevourerofGodsEX"])
                {
                    List<string> dialogue =
                    [
                        Language.GetTextValue("Mods.CalamityMod.Status.Boss.EdgyBossText"),
                        Language.GetTextValue("Mods.CalamityMod.Status.Boss.EdgyBossText2"),
                        Language.GetTextValue("Mods.CalamityMod.Status.Boss.EdgyBossText3"),
                        Language.GetTextValue("Mods.CalamityMod.Status.Boss.EdgyBossText4"),
                        Language.GetTextValue("Mods.CalamityMod.Status.Boss.EdgyBossText5"),
                        Language.GetTextValue("Mods.CalamityMod.Status.Boss.EdgyBossText6"),
                        Language.GetTextValue("Mods.CalamityMod.Status.Boss.EdgyBossText7"),
                        Language.GetTextValue("Mods.CalamityMod.Status.Boss.EdgyBossText8"),
                    ];
                    CombatText.NewText(new Rectangle(i * 16, j * 16, 16 * Width, 16 * Height), Color.LightBlue, dialogue[Main.rand.Next(0, dialogue.Count - 1)]);
                }
            }
            */
            return true;
        }

        public override void HitWire(int i, int j)
        {
            int x = i - Main.tile[i, j].TileFrameX / 18 % Width;
            int y = j - Main.tile[i, j].TileFrameY / 18 % Height;
            if (Wiring.running)
            {
                Wiring.SkipWire(x, y);
                Wiring.SkipWire(x + 1, y);
                if (Height >= 2)
                {
                    Wiring.SkipWire(x, y + 1);
                    Wiring.SkipWire(x + 1, y + 1);
                }
                if (Width >= 3)
                {
                    Wiring.SkipWire(x, y + 2);
                    Wiring.SkipWire(x + 1, y + 2);
                    Wiring.SkipWire(x + 2, y);
                    Wiring.SkipWire(x + 2, y + 1);
                    Wiring.SkipWire(x + 2, y + 2);
                }
            }
            NetMessage.SendTileSquare(-1, x + 1, y + 1, Width);
            SoundEngine.PlaySound(ClickSound, new Vector2(i * 16, j * 16));
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile t = Framing.GetTileSafely(i, j);
            // Get the top left tile
            int tileX = t.TileFrameX / 18;
            while (tileX >= 4)
            {
                tileX -= 4;
            }
            tileX = i - tileX;
            int tileY = t.TileFrameY / 18;
            while (tileY >= 4)
            {
                tileY -= 4;
            }
            tileY = j - tileY;

            Texture2D tex = TextureAssets.Item[ItemType].Value;
            // If the tile beneath can be slept in (aka a bed most of the time), run
            if (Main.tile[tileX, tileY + Height].HasTile && TileID.Sets.CanBeSleptIn[Main.tile[tileX, tileY + Height].TileType])
            {
                // Don't draw for tiles besides the top left
                if (t.TileFrameX == 0 && t.TileFrameY == 0)
                {
                    spriteBatch.Draw(tex, new Vector2(i, j) * 16 - Main.screenPosition + new Vector2(16, 16 * Height + 16) + (Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange)), null, Lighting.GetColor(i, j), 0, new Vector2(tex.Width / 2, tex.Height), 1, 0, 0);
                }
                // Regardless of the above, cancel drawing
                return false;
            }
            else
                if (t.TileFrameX == 0 && t.TileFrameY == 0)
            {
                spriteBatch.Draw(tex, new Vector2(i, j) * 16 - Main.screenPosition + new Vector2(tex.Width/5,34) + (Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange)), null, Lighting.GetColor(i, j), 0, new Vector2(0   , tex.Height), 1, 0, 0);
            }

                // If there's no bed, draw normally
                return false;
        }
    }

    [Autoload(false)]
    public class PlushProj : ModProjectile
    {
        public override string Texture => "HeavenlyArsenal/Content/Items/Misc/DevPlushies/" + PlushName + "Plush";
        public override string Name => PlushName + "Plush";

        protected readonly string PlushName;
        public int ItemType;
        protected override bool CloneNewInstances => true;

        public PlushProj(string name)
        {
            PlushName = name;
        }
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 44;
            Projectile.height = 44;
            Projectile.aiStyle = ProjAIStyleID.BeachBall;
            Projectile.friendly = true;
        }

        public override void OnKill(int timeLeft)
        {
            Item.NewItem(Projectile.GetSource_DropAsItem(), Projectile.getRect(), ItemType);
        }
    }
}
