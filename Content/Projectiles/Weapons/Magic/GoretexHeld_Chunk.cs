using Luminance.Assets;
using Luminance.Common.DataStructures;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Luminance.Common.Utilities.Utilities;
using static NoxusBoss.Assets.GennedAssets;
//using Luminance.Assets.GennedAssets.Textures.Noise


namespace HeavenlyArsenal.Content.Projectiles.Weapons.Magic;

public class GoretexHeld_Chunk : ModProjectile
{
    

   
    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public ref float Time => ref Projectile.ai[0];

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
    }

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 40;
        
        Projectile.hostile = false;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.hide = true;
        Projectile.timeLeft = 240;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;

        CooldownSlot = ImmunityCooldownID.General;
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        //writer.Write(Time);
        //writer.Write(GravityUnaffected);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
       // Time = reader.ReadInt32();
       // GravityUnaffected = reader.ReadBoolean();
    }

    public override void OnSpawn(IEntitySource source)
    {
        base.OnSpawn(source);
    }


    public override void AI()
    {
        //todo: Spit out a chunk of gore that travels for a bit before becoming affected by gravity and falling

    }
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D BloomCircleSmall = ModContent.Request<Texture2D>("NoxusBoss/Assets/Textures/Extra/GreyscaleTextures/BloomCircleSmall").Value;


        float scaleFactor = Projectile.width / 50f;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition + Projectile.velocity;
        Main.spriteBatch.Draw(BloomCircleSmall, drawPosition, null, Projectile.GetAlpha(Color.DarkRed) with { A = 0 } * 0.2f, 0f, BloomCircleSmall.Size() * 0.5f, scaleFactor * 1.2f, 0, 0f);
        Main.spriteBatch.Draw(BloomCircleSmall, drawPosition, null, Projectile.GetAlpha(Color.Red) with { A = 0 } * 0.4f, 0f, BloomCircleSmall.Size() * 0.5f, scaleFactor * 0.64f, 0, 0f);
        Main.spriteBatch.Draw(BloomCircleSmall, drawPosition, null, Projectile.GetAlpha(Color.Orange) with { A = 0 } * 0.4f, 0f, BloomCircleSmall.Size() * 0.5f, scaleFactor * 0.3f, 0, 0f);
        Texture2D texture = GennedAssets.Textures.Gores.AvatarRubble_0_0_1;

        Main.spriteBatch.Draw(texture, drawPosition, null, Projectile.GetAlpha(Color.White) * 0.5f, Projectile.rotation, texture.Size() * 0.5f, scaleFactor * 1.2f, 0, 0f);

        return false;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        overPlayers.Add(index);
    }
}