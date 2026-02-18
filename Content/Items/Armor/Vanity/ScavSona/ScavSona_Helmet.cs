using CalamityMod.Items.Armor.Statigel;
using Luminance.Assets;
using NoxusBoss.Content.Rarities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeavenlyArsenal.Utilities.Extensions;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Armor.Vanity.ScavSona
{
    [AutoloadEquip(EquipType.Head)]
    public class ScavSona_Helmet : ModItem
    {
        public override string LocalizationCategory => "Items.Armor.ScavSona";
        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 14;
            Item.rare = ModContent.RarityType<AvatarRarity>();
            Item.vanity = true;
            Item.value = Item.sellPrice(0, 3, 0, 0);
            
        }

        public override void SetStaticDefaults()
        {

            ArmorIDs.Head.Sets.DrawHead[Item.headSlot] = true;
        }
        public override void UpdateEquip(Player player)
        {
            
        }
        public override void AddRecipes()
        {
            var recipe = CreateRecipe()
           .AddIngredient(ItemID.Silk, 6)
           .AddIngredient(ItemID.BlackThread)
           .AddTile(TileID.Loom);
        }
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D tex = ModContent.Request<Texture2D>(this.Texture).Value;
            for (int i = 0; i < 6; i++)
            {
                Main.EntitySpriteDraw(tex, position + new Vector2(1 + 0.4f * MathF.Cos(Main.GlobalTimeWrappedHourly), 0).RotatedBy(i / 6f * MathHelper.TwoPi + Main.GlobalTimeWrappedHourly), frame, Color.White with { A = 0 }, 0, origin, scale*1.1f, 0);

            }



            return base.PreDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            return base.PreDrawInWorld(spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
        }
    }

   
    public class Scavsona_Mask_Layer : PlayerDrawLayer
    {
        
        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            return drawInfo.drawPlayer.head == EquipLoader.GetEquipSlot(Mod, nameof(ScavSona_Helmet), EquipType.Head);
        }
        public override Position GetDefaultPosition()
        {

            return new AfterParent(PlayerDrawLayers.Head);
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Texture2D tex = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Armor/Vanity/ScavSona/ScavSona_Helmet_Mask").Value;

        
            Player Owner = drawInfo.drawPlayer;
            SpriteEffects b = Owner.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;



            DrawData a = new DrawData(tex, drawInfo.GetHeadDrawPosition() + new Vector2(0,-6.45f), Owner.legFrame, Color.White, Owner.headRotation, Owner.legFrame.Size() / 2, 1,b);
            a.shader = drawInfo.cHead;
            a.color = drawInfo.colorArmorHead;
            drawInfo.DrawDataCache.Add(a);



       
        }
    }


}
