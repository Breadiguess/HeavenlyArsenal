using CalamityMod.Rarities;
using NoxusBoss.Content.Rarities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.Items.Armor.Vanity.ScavSona
{
    [AutoloadEquip(EquipType.Body)]
    internal class ScavSona_Dress : ModItem
    {
        public override string LocalizationCategory => "Items.Armor.ScavSona";
        public override void SetStaticDefaults()
        {

            ArmorIDs.Body.Sets.HidesArms[Item.bodySlot] = true;
        }
        public override void Load()
        {
            if (Main.netMode == NetmodeID.Server)
                return;



            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Legs}", EquipType.Legs, this);

            On_Main.CheckMonoliths += ShowTheLegsGoddamnYou;
            On_Player.PlayerFrame += ShowtheLegsPLEASE;
        }

        private void ShowtheLegsPLEASE(On_Player.orig_PlayerFrame orig, Player self)
        {
            Player local = Main.LocalPlayer;
            if (local == null) return;

            if (local.body == Item.bodySlot)
            {
                local.legs = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs);
                local.cLegs = local.cBody;
            }

            orig(self);
        }

        private void ShowTheLegsGoddamnYou(On_Main.orig_CheckMonoliths orig)
        {

            orig();
        }

        public override void EquipFrameEffects(Player player, EquipType type)
        {
            if (player.body == Item.bodySlot)
            {
                player.legs = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs);
                player.cLegs = player.cBody;
            }
        }
        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 14;
            Item.rare = ModContent.RarityType<AvatarRarity>();
            Item.vanity = true;
            Item.value = Item.sellPrice(0, 3, 0, 0);
        }


        public override void UpdateVanity(Player player)
        {
            player.GetModPlayer<ScavSona_ArmManager>().Active = true;
        }
        public override void UpdateEquip(Player player)
        {
            player.GetModPlayer<ScavSona_ArmManager>().Active = true;
        }


        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D tex = ModContent.Request<Texture2D>(this.Texture).Value;
            for (int i = 0; i < 6; i++)
            {
                Main.EntitySpriteDraw(tex, position + new Vector2(1 + 0.4f*MathF.Cos(Main.GlobalTimeWrappedHourly), 0).RotatedBy(i / 6f * MathHelper.TwoPi + Main.GlobalTimeWrappedHourly), frame, Color.White with {  A = 0 }, 0, origin, scale*1.1f, 0);
                
            }


            return base.PreDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            return base.PreDrawInWorld(spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
        }
    }
    
}
