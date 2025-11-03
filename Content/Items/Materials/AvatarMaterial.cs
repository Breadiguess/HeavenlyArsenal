using HeavenlyArsenal.Core.Globals;
using Luminance.Assets;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using NoxusBoss.Content.Rarities;
using NoxusBoss.Core.GlobalInstances;
using NoxusBoss.Core.World.GameScenes.AvatarUniverseExploration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Materials
{
    internal class AvatarMaterial : ModItem, ILocalizedModType
    {
        public override string LocalizationCategory => "Items.Misc";
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemNoGravity[Type] = true;
            GlobalNPCEventHandlers.ModifyNPCLootEvent += (NPC npc, NPCLoot npcLoot) =>
            {
                if (npc.type == ModContent.NPCType<AvatarOfEmptiness>())
                {
                    LeadingConditionRule normalOnly = new LeadingConditionRule(new Conditions.NotExpert());
                    {
                        normalOnly.OnSuccess(ItemDropRule.Common(Type, minimumDropped:3, maximumDropped:3));
                    }
                    npcLoot.Add(normalOnly);
                }
            };
            ArsenalGlobalItem.ModifyItemLootEvent += (Item item, ItemLoot loot) =>
            {
                if (item.type == AvatarOfEmptiness.TreasureBagID)
                    loot.Add(ItemDropRule.Common(Type,minimumDropped:3, maximumDropped: 3));
            };
        }
    
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 999;
            Item.value = Terraria.Item.buyPrice(platinum: 0, gold: 0, silver: 0, copper: 3);
            Item.rare = ModContent.RarityType<AvatarRarity>();
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            return base.PreDrawInWorld(spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
        }
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
            
            Vector2 particleDrawCenter = position + new Vector2(0f);
            Texture2D glow = AssetDirectory.Textures.BigGlowball.Value;

            //Main.EntitySpriteDraw(glow, particleDrawCenter - Main.screenPosition, glow.Frame(), Color.Red with { A = 200 }, 0, glow.Size() * 0.5f, new Vector2(0.12f, 0.25f), 0, 0);
            Texture2D BaseE = GennedAssets.Textures.GreyscaleTextures.WhitePixel;

            ManagedShader AOEMaterial = ShaderManager.GetShader("HeavenlyArsenal.avatarMaterial");

            var s = AOEMaterial;
            s.TrySetParameter("Time", Main.GlobalTimeWrappedHourly);
            s.TrySetParameter("Color", Color.Red.ToVector4());
            s.TrySetParameter("MorphSpeed", 0.5f);   // try 0.15–0.5
            s.TrySetParameter("Threshold", 0.20f);
            s.TrySetParameter("EdgeWidth", 0.08f);   // try 0.06–0.12 for smoother edges
            s.TrySetParameter("NoiseScale", new Vector2(2f, 2f));
            s.TrySetParameter("WarpStrength", 0.05f); // keep small to avoid clamping
            s.TrySetParameter("NoiseSpeed", 0.15f);
            AOEMaterial.SetTexture(GennedAssets.Textures.FirstPhaseForm.AvatarRift, 0);
            AOEMaterial.SetTexture(GennedAssets.Textures.Noise.DendriticNoiseZoomedOut, 1, SamplerState.AnisotropicWrap);
            AOEMaterial.SetTexture(GennedAssets.Textures.GoodAppleHearts.BarsLifeOverlay_Fill, 2);


            AOEMaterial.SetTexture(GennedAssets.Textures.SecondPhaseForm.Beads2, 3);
            AOEMaterial.Apply();

        

            Main.spriteBatch.Draw(BaseE, particleDrawCenter, null, Color.White, 0, BaseE.Size() * 0.5f, new Vector2(20f), 0, 0);

            Main.spriteBatch.End();

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

            //spriteBatch.Draw(itemTexture, position, itemFrame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);



            return base.PreDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
        }
    }
}
