using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Armor.Haemsong
{
	[AutoloadEquip(EquipType.Body)]
	public class BloodArmorBody : ModItem
	{
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
			Texture2D which = TextureAssets.Item[Type].Value;
            Rectangle sourceRect = which.Frame(1, 2, 0, Main.LocalPlayer.GetModPlayer<BloodPlayer>().offenseMode ? 0 : 1);
            spriteBatch.Draw(which, position, sourceRect, drawColor, 0, origin, scale * 2, SpriteEffects.None, 0);
            return false;
        }
		public override void SetDefaults()
		{
			Item.defense = 64;
			Item.height = 46;
			Item.rare = ItemRarityID.Red;
			Item.value = 200000;
			Item.width = 30;
		}
    }
}
