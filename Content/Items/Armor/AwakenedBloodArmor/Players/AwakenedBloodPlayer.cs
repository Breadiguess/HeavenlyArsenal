using HeavenlyArsenal.Common.Ui;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using Terraria.Audio;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor.Players;

public class AwakenedBloodPlayer : ModPlayer
{
    public override void PostUpdate()
    {
        if (!AwakenedBloodSetActive)
        {
            return;
        }

        HandleForm();

        ManageBloodBoost();

        ConvertClot();
    }

    public override void PreUpdate()
    {
        if (GainTimer > 0)
        {
            GainTimer--;
        }

        if (!AwakenedBloodSetActive)
        {
            return;
        }

        var bloodPercent = blood / (float)MaxResource;
        var clotPercent = clot / (float)MaxResource;
        var bloodclot = (blood + clot) / (float)MaxResource;

        WeaponBar.DisplayBar(Color.AntiqueWhite, Color.Crimson, bloodPercent, 150, 0, new Vector2(0, -20));
        WeaponBar.DisplayBar(Color.Crimson, Color.AntiqueWhite, clotPercent, 150, 0, new Vector2(0, -30));
        WeaponBar.DisplayBar(Color.HotPink, Color.Silver, bloodclot, 150, 1, new Vector2(0, -40));
        //this shit sucks

        //Main.NewText($"{bloodPercent}, clot: {clotPercent}, decay timer: {clotDecayTimer}");

        ControlResource();
    }

    public override void ResetEffects()
    {
        AwakenedBloodSetActive = false;
    }

    public override void ArmorSetBonusActivated()
    {
        if (!AwakenedBloodSetActive)
        {
            return;
        }

        if (CurrentForm == Form.Offense)
        {
            var value = 76;

            if (value > 75 && !BloodBoostActive)
            {
                BloodBoostDrainTimer = 0;
                BloodBoostActive = true;
                SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.BloodCry);
            }
        }
        else if (CurrentForm == Form.Defense)
        {
            Player.HealEffect(clot);
            Player.statLife += clot;
            clot = 0;
        }
    }

    #region values

    public enum Form
    {
        Offense,

        Defense
    }

    public Form CurrentForm = Form.Offense;

    public bool AwakenedBloodSetActive;

    public bool BloodBoostActive;

    public int BloodBoostSink = 6 * 60;

    public int BloodBoostTotalTime;

    public int BloodBoostDrainTimer;

    public int blood;

    public int clot;

    public int clotDecayTimer;

    /// <summary>
    ///     Time to control the rate at which the player gains blood
    /// </summary>
    public int GainTimer;

    public int CombinedResource => blood + clot;

    public int MaxResource = 100;

    #endregion

    #region Hit NPC

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (AwakenedBloodSetActive && GainTimer <= 0) //&& !BlackListProjectileNPCs.BlackListedNPCs.Contains(target.type))
        {
            GainBlood();
            ControlResource();
        }

        base.OnHitNPC(target, hit, damageDone);
    }

    public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (AwakenedBloodSetActive && GainTimer <= 0) //&& !BlackListProjectileNPCs.BlackListedNPCs.Contains(target.type))
        {
            GainTimer = 20;
            GainBlood();
            ControlResource();
        }
    }

    public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (AwakenedBloodSetActive && GainTimer <= 0) // && !BlackListProjectileNPCs.BlackListedNPCs.Contains(target.type))
        {
            GainBlood();
            ControlResource();
        }
    }

    #endregion

    #region hitByNPC or Projectile

    public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
    {
        if (AwakenedBloodSetActive)
        {
            BloodLoss(hurtInfo);
        }
    }

    public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
    {
        if (AwakenedBloodSetActive)
        {
            BloodLoss(hurtInfo);
        }
    }

    #endregion

    #region helpers

    private void HandleForm()
    {
        switch (CurrentForm)
        {
            case Form.Offense:
                ManageOffense();

                break;

            case Form.Defense:
                ManageDefense();

                break;
        }
    }

    private void ManageOffense()
    {
        var Type = ModContent.ProjectileType<BloodNeedle>();
        var TendrilCount = 2;
        var TendrilBaseDamage = 600;

        if (Player.ownedProjectileCounts[Type] < TendrilCount)
        {
            for (var i = 0; i < TendrilCount; i++)
            {
                var a = Projectile.NewProjectileDirect(Player.GetSource_FromThis(), Player.Center, Vector2.Zero, Type, TendrilBaseDamage, 1);
                a.localAI[0] = i + 1;
            }
        }

        Player.statDefense -= 75;
      
    }

    private void ManageDefense()
    {
        Player.statDefense += 25;
        PurgeClot();
    }

    public void ControlResource()
    {
        blood = Utils.Clamp(blood, 0, 100);
        clot = Utils.Clamp(clot, 0, 100);

        if (CombinedResource > MaxResource)
        {
            var difference = MaxResource - blood;
        }
    }

    public void GainBlood()
    {
        if (GainTimer <= 0 && CombinedResource < MaxResource)
        {
            blood += 5;
            clotDecayTimer++;
        }

        GainTimer = 20;
    }

    public void ConvertClot()
    {
        clotDecayTimer++;

        var ClotMax = 0;
        ClotMax = CurrentForm == Form.Defense ? 60 : 180;

        if (clotDecayTimer ! < ClotMax)
        {
            return;
        }

        if (CombinedResource >= MaxResource)
        {
            return;
        }

        var value = (int)Math.Round(MaxResource * (blood / (float)MaxResource));

        value /= 4;
        blood -= value;

        clot += value;
        clotDecayTimer = 0;
    }

    public void BloodLoss(Player.HurtInfo hurtInfo)
    {
        var bloodLoss = (int)(hurtInfo.Damage * 0.1f);
        blood -= bloodLoss;

        if (blood < 0)
        {
            blood = 0;
        }
        //Main.NewText($"Lost {bloodLoss} blood. Total Blood: {blood}");
    }

    public void PurgeClot()
    {
        if (clot <= 0 || Player.statLife >= Player.statLifeMax2)
        {
            return;
        }

        Player.HealEffect(clot * 4);
        Player.statLife += clot * 4;
        clot = 0;
    }

    public void ManageBloodBoost()
    {
        if (!BloodBoostActive)
        {
            return;
        }

        var DrainGate = BloodBoostTotalTime < BloodBoostSink ? 4 : 2;

        Player.GetDamage(DamageClass.Generic) += 0.55f;
        Player.GetArmorPenetration(DamageClass.Generic) += 15;
        Player.GetCritChance(DamageClass.Generic) += 10;

        if (blood > 0 && BloodBoostDrainTimer > DrainGate)
        {
            blood--;
            BloodBoostDrainTimer = 0;
        }

        if (blood <= 0)
        {
            BloodBoostActive = false;
        }

        BloodBoostDrainTimer++;

        BloodBoostTotalTime++;
    }

    #endregion
}

public class AwakenedBloodDraw : PlayerDrawLayer
{
    public override bool IsHeadLayer => false;

    public override Position GetDefaultPosition()
    {
        return new AfterParent(PlayerDrawLayers.Head);
    }

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        return true;
        // drawInfo.drawPlayer.head == EquipLoader.GetEquipSlot(Mod, nameof(ShintoArmorHelmetAll), EquipType.Head);
    }

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        // DrawDebug(ref drawInfo);

        ManageBloodBoostVFX(drawInfo.drawPlayer);
    }

    public void ManageBloodBoostVFX(Player player)
    {
        var bloodplayer = player.GetModPlayer<AwakenedBloodPlayer>();

        if (!bloodplayer.AwakenedBloodSetActive && bloodplayer.blood <= 0)
        {
            return;
        }

        if (!bloodplayer.BloodBoostActive)
        {
            return;
        }

        var suctionShader = ShaderManager.GetFilter("HeavenlyArsenal.BloodFrenzy");

        suctionShader.TrySetParameter("globalTime", Main.GlobalTimeWrappedHourly);
        suctionShader.TrySetParameter("intensityFactor", 1);
        suctionShader.TrySetParameter("opacity", 00);
        suctionShader.TrySetParameter("psychadelicExponent", 0);
        suctionShader.TrySetParameter("psychedelicColorTint", Color.Crimson.ToVector4());
        suctionShader.TrySetParameter("colorAccentuationFactor", 0f);

        suctionShader.SetTexture(GennedAssets.Textures.Extra.BloodWater, 2);

        suctionShader.Activate();

        //sampler baseTexture : register(s0);
        //sampler psychedelicTexture : register(s1);
        //sampler noiseTexture : register(s2);

        //float globalTime;
        //float opacity;
        // float intensityFactor;
        // float psychedelicExponent;
        // float colorAccentuationFactor;
        // float3 colorToAccentuate;
        // float4 goldColor;
        //  float4 psychedelicColorTint;
    }

    protected void DrawDebug(ref PlayerDrawSet drawInfo)
    {
        var bloodplayer = drawInfo.drawPlayer.GetModPlayer<AwakenedBloodPlayer>();

        var blood = $"Blood: {bloodplayer.blood}";
        var gaincooldown = $"Blood gain timer: {bloodplayer.GainTimer}";
        var clot = $"clot: {bloodplayer.clot}";
        var Decaytimer = $"blood decay Time: {bloodplayer.clotDecayTimer}";

        var Bloodboostactive = $"Bloodboost Active?: {bloodplayer.BloodBoostActive}";
        var combinedString = blood + ", " + gaincooldown + ", " + clot + ", " + Decaytimer + ", " + Bloodboostactive;
        var DrawPos = drawInfo.drawPlayer.Center - Main.screenPosition;
        var Offset = Vector2.UnitY * -120 + Vector2.UnitX * -120;
        Utils.DrawBorderString(Main.spriteBatch, combinedString, DrawPos + Offset, Color.AntiqueWhite);
    }
}