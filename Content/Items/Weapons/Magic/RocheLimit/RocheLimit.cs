using HeavenlyArsenal.Core.Items;
using Luminance.Assets;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity;
using NoxusBoss.Content.Rarities;
using NoxusBoss.Core.GlobalInstances;
using Terraria.GameContent.ItemDropRules;

namespace HeavenlyArsenal.Content.Items.Weapons.Magic.RocheLimit;

// TODO -- Investigate bugs pertaining to rendering the black hole.
public class RocheLimit : ModItem
{
    public override string LocalizationCategory => "Items.Weapons.Magic";

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    /// <summary>
    ///     The rate at which this weapon consumes mana.
    /// </summary>
    internal static int ManaConsumptionRate => LumUtils.SecondsToFrames(0.08f);

    public override void SetStaticDefaults()
    {
        GlobalNPCEventHandlers.ModifyNPCLootEvent += (npc, npcLoot) =>
        {
            if (npc.type == ModContent.NPCType<NamelessDeityBoss>())
            {
                var normalOnly = new LeadingConditionRule(new Conditions.NotExpert());

                {
                    normalOnly.OnSuccess(ItemDropRule.Common(Type));
                }

                npcLoot.Add(normalOnly);
            }
        };

        ItemLootGlobalItem.RegisterLoot(NamelessDeityBoss.TreasureBagID, ItemDropRule.Common(Type));
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
        if (Main.netMode == NetmodeID.Server)
        {
            return false;
        }

        Main.spriteBatch.PrepareForShaders(null, true);

        var mainColor = RocheLimitBlackHole.TemperatureGradient.SampleColor(0.37f).ToVector3();
        var coronaColor = Vector3.One;
        var drawPosition = position;

        // Supply information to the sun shader.
        var sunShader = ShaderManager.GetShader("HeavenlyArsenal.RocheLimitSunShader");
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
        if (Main.netMode == NetmodeID.Server)
        {
            return false;
        }

        Main.spriteBatch.PrepareForShaders();

        var mainColor = RocheLimitBlackHole.TemperatureGradient.SampleColor(0.37f).ToVector3();
        var coronaColor = Vector3.One;
        var drawPosition = Item.Center - Main.screenPosition;

        // Supply information to the sun shader.
        var sunShader = ShaderManager.GetShader("HeavenlyArsenal.RocheLimitSunShader");
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

    public override bool CanUseItem(Player player)
    {
        return player.ownedProjectileCounts[Item.shoot] <= 0;
    }
}