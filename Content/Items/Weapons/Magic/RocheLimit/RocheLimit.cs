using HeavenlyArsenal.Core.Globals;
using Luminance.Assets;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity;
using NoxusBoss.Content.Rarities;
using NoxusBoss.Core.GlobalInstances;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Magic.RocheLimit;

// TODO -- Investigate bugs pertaining to rendering the black hole.
public class RocheLimit : ModItem
{
    /// <summary>
    /// The rate at which this weapon consumes mana.
    /// </summary>
    internal static int ManaConsumptionRate => LumUtils.SecondsToFrames(0.08f);

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetStaticDefaults()
    {
        GlobalNPCEventHandlers.ModifyNPCLootEvent += (npc, npcLoot) =>
        {
            if (npc.type == ModContent.NPCType<NamelessDeityBoss>())
            {
                LeadingConditionRule normalOnly = new LeadingConditionRule(new Conditions.NotExpert());
                {
                    normalOnly.OnSuccess(ItemDropRule.Common(Type));
                }
                npcLoot.Add(normalOnly);
            }
        };
        ArsenalGlobalItem.ModifyItemLootEvent += (item, loot) =>
        {
            if (item.type == NamelessDeityBoss.TreasureBagID)
                loot.Add(ItemDropRule.Common(Type));
        };


    }

    public override void SetDefaults()
    {
        Item.width = 12;
        Item.height = 12;
        Item.DamageType = DamageClass.Magic;
        Item.damage = 12000;
        Item.knockBack = 0f;
        Item.useTime = 25;
        Item.useAnimation = 25;
        Item.autoReuse = true;
        Item.mana = 32;
        Item.holdStyle = 0;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.channel = true;
        Item.noUseGraphic = true;
        Item.noMelee = true;

        Item.shoot = ModContent.ProjectileType<RocheLimitBlackHole>();
        Item.shootSpeed = 10f;
        Item.rare = ModContent.RarityType<NamelessDeityRarity>();
        Item.value = Item.buyPrice(gold: 2);
    }

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        Main.spriteBatch.PrepareForShaders(null, true);

        Vector3 mainColor = RocheLimitBlackHole.TemperatureGradient.SampleColor(0.37f).ToVector3();
        Vector3 coronaColor = Vector3.One;
        Vector2 drawPosition = position;

        // Supply information to the sun shader.
        ManagedShader sunShader = ShaderManager.GetShader("HeavenlyArsenal.RocheLimitSunShader");
        sunShader.TrySetParameter("coronaIntensityFactor", 0.23f);
        sunShader.TrySetParameter("mainColor", mainColor);
        sunShader.TrySetParameter("darkerColor", mainColor);
        sunShader.TrySetParameter("coronaColor", coronaColor);
        sunShader.TrySetParameter("subtractiveAccentFactor", Vector3.Zero);
        sunShader.TrySetParameter("sphereSpinTime", Main.GlobalTimeWrappedHourly * 0.21f);
        sunShader.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 1, SamplerState.LinearWrap);
        sunShader.SetTexture(GennedAssets.Textures.Extra.PsychedelicWingTextureOffsetMap, 2, SamplerState.LinearWrap);
        sunShader.Apply();

        // Draw the sun.
        Texture2D fireNoise = GennedAssets.Textures.Noise.FireNoiseA;
        Main.spriteBatch.Draw(fireNoise, drawPosition, null, new Color(mainColor), 0f, fireNoise.Size() * 0.5f, scale * 0.15f, 0, 0f);

        Main.spriteBatch.ResetToDefaultUI();
        return false;
    }

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        Main.spriteBatch.PrepareForShaders();

        Vector3 mainColor = RocheLimitBlackHole.TemperatureGradient.SampleColor(0.37f).ToVector3();
        Vector3 coronaColor = Vector3.One;
        Vector2 drawPosition = Item.Center - Main.screenPosition;

        // Supply information to the sun shader.
        ManagedShader sunShader = ShaderManager.GetShader("HeavenlyArsenal.RocheLimitSunShader");
        sunShader.TrySetParameter("coronaIntensityFactor", 0.23f);
        sunShader.TrySetParameter("mainColor", mainColor);
        sunShader.TrySetParameter("darkerColor", mainColor);
        sunShader.TrySetParameter("coronaColor", coronaColor);
        sunShader.TrySetParameter("subtractiveAccentFactor", Vector3.Zero);
        sunShader.TrySetParameter("sphereSpinTime", Main.GlobalTimeWrappedHourly * 0.2f);
        sunShader.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 1, SamplerState.LinearWrap);
        sunShader.SetTexture(GennedAssets.Textures.Extra.PsychedelicWingTextureOffsetMap, 2, SamplerState.LinearWrap);
        sunShader.Apply();

        // Draw the sun.
        Texture2D fireNoise = GennedAssets.Textures.Noise.FireNoiseA;
        Main.spriteBatch.Draw(fireNoise, drawPosition, null, new Color(mainColor), rotation, fireNoise.Size() * 0.5f, 0.3f, 0, 0f);

        Main.spriteBatch.ResetToDefault();
        return false;
    }

    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;
}
