using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Armor.Haemsong
{
	[AutoloadEquip(EquipType.Legs)]
	public class BloodArmorLegs : ModItem
	{
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D which = TextureAssets.Item[Type].Value;
            Rectangle sourceRect = which.Frame(1, 2, 0, Main.LocalPlayer.GetModPlayer<BloodPlayer>().offenseMode ? 0 : 1);
            Vector2 whichOrigin = new Vector2(sourceRect.Width / 2, sourceRect.Height / 2);
            spriteBatch.Draw(which, position, sourceRect, drawColor, 0, whichOrigin, scale*1.25f, SpriteEffects.None, 0);
            return false;
        }
		public override void SetDefaults()
        {
            Item.defense = 54;
            Item.height = 18;
            Item.rare = ItemRarityID.Red;
            Item.value = 200000;
			Item.width = 22;
        }
    }
}
