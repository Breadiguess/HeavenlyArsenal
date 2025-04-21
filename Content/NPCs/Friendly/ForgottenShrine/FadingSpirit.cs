using HeavenlyArsenal.Content.Subworlds;
using HeavenlyArsenal.Content.Subworlds.Generation;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.Graphics.SpecificEffectManagers;
using NoxusBoss.Core.Utilities;
using ReLogic.Content;
using ReLogic.Graphics;
using SubworldLibrary;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace HeavenlyArsenal.Content.NPCs.Friendly.ForgottenShrine;

public partial class FadingSpirit : ModNPC
{
    public enum SpiritAIState
    {
        WanderAbout,
        Linger,
        Vanish
    }

    /// <summary>
    /// The amount of standard names available for this spirit to chose.
    /// </summary>
    public static int TotalStandardNames => 33;

    /// <summary>
    /// The amount of rare names available for this spirit to chose.
    /// </summary>
    public static int TotalRareNames => 3;

    /// <summary>
    /// The chance for a spirit to spawn in with a rare name.
    /// </summary>
    public static int RareNameChooseChance => 400;

    /// <summary>
    /// The current state of this spirit.
    /// </summary>
    public SpiritAIState State
    {
        get => (SpiritAIState)NPC.ai[0];
        set => NPC.ai[0] = (int)value;
    }

    /// <summary>
    /// A general-purpose AI timer for this spirit. Gets reset whenever the state is naturally changed.
    /// </summary>
    public int AITImer
    {
        get => (int)NPC.ai[1];
        set => NPC.ai[1] = value;
    }

    public override void SetStaticDefaults() => NPCID.Sets.CountsAsCritter[Type] = true;

    public override void SetDefaults()
    {
        NPC.npcSlots = 1f;
        NPC.width = 30;
        NPC.height = 44;
        NPC.damage = 0;
        NPC.defense = 0;
        NPC.lifeMax = 1;
        NPC.aiStyle = -1;
        NPC.knockBackResist = 0f;
        NPC.noGravity = true;
        NPC.noTileCollide = false;
        NPC.dontTakeDamage = true;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.friendly = true;
        NPC.hide = true;
        NPC.Opacity = 0f;
        AIType = -1;

        SpawnModBiomes =
        [
            ModContent.GetInstance<ForgottenShrineBiome>().Type
        ];

        if (Main.netMode != NetmodeID.Server)
            NPCNameFontSystem.RegisterFontForNPCID(Type, DisplayName.Value, Mod.Assets.Request<DynamicSpriteFont>("Assets/Fonts/FadingSpiritNameText", AssetRequestMode.ImmediateLoad).Value);

        Main.rand ??= new UnifiedRandom();
        NPC.GivenName = this.GetLocalizedValue($"Name{Main.rand.Next(TotalStandardNames) + 1}");
        if (Main.rand.NextBool(RareNameChooseChance))
            NPC.GivenName = this.GetLocalizedValue($"RareName{Main.rand.Next(TotalRareNames) + 1}");
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.AddRange(
        [
            new MoonLordPortraitBackgroundProviderBestiaryInfoElement(), // TODO -- Make custom.
            new FlavorTextBestiaryInfoElement("Mods.HeavenlyArsenal.NPCs.FadingSpirit.BestiaryEntry")
        ]);
    }

    public override void AI()
    {
        switch (State)
        {
            case SpiritAIState.WanderAbout:
                DoBehavior_WanderAbout();
                break;
            case SpiritAIState.Linger:
                DoBehavior_Linger();
                break;
            case SpiritAIState.Vanish:
                DoBehavior_Vanish();
                break;
        }

        NPC.rotation = Math.Clamp(NPC.velocity.X * 0.075f, -0.2f, 0.2f);
        AITImer++;
    }

    private void HoverAboveGround(float desiredHoverDistance)
    {
        Vector2 groundPosition = LumUtils.FindGroundVertical(NPC.Center.ToTileCoordinates()).ToWorldCoordinates();
        Vector2 waterPosition = NPC.Center;
        while (waterPosition.Y < Main.maxTilesY * 16 - 16f && !Collision.WetCollision(waterPosition, 1, 1))
            waterPosition.Y += 16f;
        waterPosition.Y -= 24f;

        float distanceToGround = MathF.Min(NPC.Distance(groundPosition), NPC.Distance(waterPosition));
        bool shouldAscend = distanceToGround < desiredHoverDistance;

        // Hover in place vertically if sufficiently close to being within the desired hover distance zone.
        if (MathHelper.Distance(distanceToGround, desiredHoverDistance) <= 10f)
            NPC.velocity.Y *= 0.9f;

        // Descend if above the hover desired distance.
        else if (shouldAscend)
            NPC.velocity.Y -= 0.08f;
        else if (!shouldAscend)
            NPC.velocity.Y += 0.02f;

        // Clamp vertical speed values so that acceleration doesn't get out of control.
        NPC.velocity.Y = Math.Clamp(NPC.velocity.Y, -2f, 2f);
    }

    /// <summary>
    /// Makes this spirit's opacity fade to a given value over time.
    /// </summary>
    private void FadeTowards(float idealOpacity) => NPC.Opacity = NPC.Opacity.StepTowards(idealOpacity, 0.007f);

    /// <summary>
    /// Switches this spirit's state, resetting things in the proces.s
    /// </summary>
    private void SwitchState(SpiritAIState newState)
    {
        State = newState;
        AITImer = 0;
        NPC.netUpdate = true;
    }

    /// <summary>
    /// Performs this spirit's wandering about state.
    /// </summary>
    private void DoBehavior_WanderAbout()
    {
        FadeTowards(1f);
        HoverAboveGround(40f);

        float horizontalAcceleration = LumUtils.InverseLerp(0f, 60f, AITImer) * 0.08f;
        NPC.velocity.X = Math.Clamp(NPC.velocity.X + NPC.spriteDirection * horizontalAcceleration, -2f, 2f);

        int bridgeWidth = ForgottenShrineGenerationHelpers.BridgeArchWidth;
        int xTileCoords = (int)(NPC.Center.X / 16f);
        int tiledBridgeX = xTileCoords % (bridgeWidth * ForgottenShrineGenerationHelpers.BridgeRooftopsPerBridge);
        bool nearCenterOfBridge = MathHelper.Distance(tiledBridgeX, bridgeWidth * 0.5f) <= 5 && SubworldSystem.IsActive<ForgottenShrineSubworld>();
        int lingerNearBridgeChance = 25;
        int randomLingerChance = 300;

        bool canSwitchState = AITImer >= LumUtils.SecondsToFrames(1f);

        if (canSwitchState)
        {
            // Have a high chance of lingering in place near bridges.
            // This effect doesn't happen if there's other spirits nearby, to prevent them all clumping together at bridges, though.
            if (nearCenterOfBridge)
            {
                bool otherSpiritNearby = false;
                foreach (NPC otherSpirit in Main.ActiveNPCs)
                {
                    if (otherSpirit.type == Type && otherSpirit.WithinRange(NPC.Center, 175f))
                    {
                        otherSpiritNearby = true;
                        break;
                    }
                }

                if (!otherSpiritNearby && Main.rand.NextBool(lingerNearBridgeChance))
                    SwitchState(SpiritAIState.Linger);
            }

            // Otherwise, just have a low chance to linger at sheer random.
            if (Main.rand.NextBool(randomLingerChance))
                SwitchState(SpiritAIState.Linger);
        }
    }

    /// <summary>
    /// Performs this spirit's lingering state.
    /// </summary>
    private void DoBehavior_Linger()
    {
        int lingerTime = LumUtils.SecondsToFrames(6.5f);
        int vanishChance = 4;

        FadeTowards(0.75f);

        NPC.velocity.X *= 0.97f;
        HoverAboveGround(12f);

        // Randomly shift direction.
        if (AITImer % 60 == 59 && Main.rand.NextBool(3))
            NPC.spriteDirection = Main.rand.NextFromList(-1, 1);

        if (AITImer >= lingerTime)
            SwitchState(Main.rand.NextBool(vanishChance) ? SpiritAIState.Vanish : SpiritAIState.WanderAbout);
    }

    /// <summary>
    /// Performs this spirit's vanishing state.
    /// </summary>
    private void DoBehavior_Vanish()
    {
        NPC.velocity *= 0.9f;

        FadeTowards(0f);
        if (NPC.Opacity <= 0f)
            NPC.active = false;
    }

    public override void DrawBehind(int index)
    {
        Main.instance.DrawCacheNPCsOverPlayers.Add(index);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Vector2 drawPosition = NPC.Center - screenPos;

        Main.spriteBatch.UseBlendState(BlendState.NonPremultiplied);
        Texture2D texture = TextureAssets.Npc[Type].Value;
        SpriteEffects direction = NPC.spriteDirection.ToSpriteDirection();
        Main.spriteBatch.Draw(texture, drawPosition, NPC.frame, NPC.GetAlpha(Color.White) * 0.6f, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, direction, 0f);
        Main.spriteBatch.ResetToDefault();

        float glowScale = NPC.scale * MathHelper.Lerp(0.95f, 1.05f, LumUtils.Cos01(NPC.whoAmI + NPC.Center.X * 0.01f + Main.GlobalTimeWrappedHourly * 33f));
        float hueShift = MathHelper.Lerp(-0.03f, 0.06f, NPC.whoAmI / 7 % 1f);
        Texture2D glow = GennedAssets.Textures.GreyscaleTextures.BloomCirclePinpoint;
        Vector2 glowDrawPosition = drawPosition - Vector2.UnitY.RotatedBy(NPC.rotation) * NPC.scale * 15f;
        Main.spriteBatch.Draw(glow, glowDrawPosition, null, NPC.GetAlpha(new Color(1f, 0.6f, 0.1f, 0f).HueShift(hueShift)) * 0.5f, 0f, glow.Size() * 0.5f, glowScale * 1.2f, 0, 0f);
        Main.spriteBatch.Draw(glow, glowDrawPosition, null, NPC.GetAlpha(new Color(1f, 0.7f, 0.2f, 0f).HueShift(hueShift)) * 0.7f, 0f, glow.Size() * 0.5f, glowScale * 0.6f, 0, 0f);
        Main.spriteBatch.Draw(glow, glowDrawPosition, null, NPC.GetAlpha(new Color(1f, 0.9f, 0.7f, 0f).HueShift(hueShift)) * 0.9f, 0f, glow.Size() * 0.5f, glowScale * 0.3f, 0, 0f);

        return false;
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        if (spawnInfo.Player.InModBiome<ForgottenShrineBiome>() && !LumUtils.AnyBosses())
            return 1f;

        return 0f;
    }

    public override int SpawnNPC(int tileX, int tileY)
    {
        Vector2 spawnPosition = new Vector2(tileX, tileY).ToWorldCoordinates();
        while (Collision.WetCollision(spawnPosition, 1, 1))
            spawnPosition.Y--;

        return NPC.NewNPC(new EntitySource_SpawnNPC(), (int)spawnPosition.X, (int)spawnPosition.Y, Type);
    }
}
