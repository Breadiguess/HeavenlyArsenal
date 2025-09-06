using CalamityMod.Buffs.Pets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Summon.BadSun
{
    public class DoomedSerenity : ModProjectile
    {
        #region Setup
        /// <summary>
        /// HoldEye
        /// </summary>
        public Vector2 EyePos
        {
            get;
            private set;
        }
        public bool HasTargets;
        //so what i'm imagining is that the projectile will be able to track multiple targets.
        //this is mostly made so that it can track the target with the most health using it's eye.
        public List<NPC> Targets = new List<NPC>();
        public override void SetDefaults()
        {
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.height = Projectile.width = 22;
            Projectile.sentry = true;
            Projectile.penetrate = -1;
            
        }
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }
        public ref float Time => ref Projectile.ai[0];
        public ref Player Owner => ref Main.player[Projectile.owner];

        public override void OnSpawn(IEntitySource source)
        {
            EyePos = Projectile.Center;

        }
        #endregion
        public override void AI()
        {
            Projectile.timeLeft = 2;
            HandleFloat();
            NPC Target = Projectile.FindTargetWithinRange(1000f, true);
            ManageEye(Target);
            Time++;
        }


        public void HandleReposition(Vector2 NewPos)
        {
            Projectile.Center = NewPos;
        }
        private void HandleFloat()
        {
            // Bob up and down in place using a sine wave
            float bobAmplitude = 0.04f;
            float bobSpeed = 0.004f; // radians per tick
            float offsetY = (float)Math.Sin(Time * bobSpeed) * bobAmplitude;
            Projectile.position.Y = Projectile.Center.Y - Projectile.height / 2 + offsetY;
        }
        private void ManageEye(NPC target)
        {
            if (target != null)
            {
                // Lerp EyePos towards the target, but clamp EyePos to stay within a 30 pixel radius from the projectile's center
                Vector2 desiredPos = target.Center;
                Vector2 center = Projectile.Center;
                Vector2 direction = desiredPos - center;
                float maxRadius = 10f;

                // Clamp the direction vector to maxRadius
                if (direction.Length() > maxRadius)
                {
                    direction = Vector2.Normalize(direction) * maxRadius;
                }

                Vector2 targetEyePos = center + direction;
                EyePos = Vector2.Lerp(EyePos, targetEyePos, 0.7f);
            }
            else
            {
                EyePos = Projectile.Center;
            }
        }


        private void DrawEye(SpriteBatch spriteBatch, float sizeMulti)
        {
            Texture2D Eye = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/Summon/BadSun/DoomedSerenityEye").Value;
            Vector2 DrawPos = EyePos - Main.screenPosition;
            Vector2 Origin = Eye.Size() * 0.5f;
            Main.EntitySpriteDraw(Eye, DrawPos, null, Color.White, 0, Origin, 1f * sizeMulti, SpriteEffects.None, 0);
        }
        private void DrawFlower(float sizeMulti)
        {
            Texture2D Flower = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/Summon/BadSun/BadSunComponent_Flower").Value;
            Vector2 DrawPos = Projectile.Center - Main.screenPosition;
            Vector2 Origin = Flower.Size() * 0.5f;
            Main.EntitySpriteDraw(Flower, DrawPos, null, Color.White, 0, Origin, 0.6f * sizeMulti, SpriteEffects.None, 0);
        }
        private void DrawDiadem(float sizeMulti)
        {
            Texture2D Diadem = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/Summon/BadSun/BadSunComponent_Diadem").Value;
            Vector2 DrawPos = Projectile.Center - Main.screenPosition;
            Vector2 Origin = Diadem.Size() * 0.5f;
            Main.EntitySpriteDraw(Diadem, DrawPos, null, Color.White, 0, Origin, 0.2f * sizeMulti, SpriteEffects.None, 0);
        }
        private void DrawMask(float sizeMulti)
        {
            Texture2D Mask = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/Summon/BadSun/BadSunComponent_Mask").Value;
            Vector2 DrawPos = EyePos - Main.screenPosition;
            Vector2 Origin = Mask.Size() * 0.5f;
            Main.EntitySpriteDraw(Mask, DrawPos, null, Color.White, 0, Origin, 0.2f * sizeMulti, SpriteEffects.None, 0);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            float sizeMulti = 0.25f;
            DrawFlower(sizeMulti);
            DrawDiadem(sizeMulti);
            DrawMask(sizeMulti);
            DrawEye(Main.spriteBatch, sizeMulti);
            return false;
        }
    }

    public class CensorData
    {
        public Vector2 Offset;   // fixed relative to NPC.Center
        public float SizeLerp;   // 0→1 interpolant

        public CensorData(Vector2 offset)
        {
            Offset = offset;
            SizeLerp = 0f;
        }
    }
    public class FuckYourImmunity : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        
        private readonly List<CensorData> _censors = new List<CensorData>();

        // Always clear immunity
        public override bool PreAI(NPC npc)
        {
            int debuffID = ModContent.BuffType<BlissFallen>();

            if (npc.buffImmune[debuffID])
                npc.buffImmune[debuffID] = false;
            return base.PreAI(npc);
        }

        
        public void AddCensor(NPC npc)
        {
            Vector2 randomOffset = new Vector2(
                Main.rand.NextFloat(-npc.width * .5f, npc.width * .5f),
                Main.rand.NextFloat(-npc.height * .5f, npc.height * .5f)
            );
            _censors.Add(new CensorData(randomOffset));
        }

        // Clean up if the buff fully expires
        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (!npc.HasBuff<BlissFallen>() && _censors.Count > 0)
            {
                _censors.Clear();
            }
        }

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (!npc.HasBuff<BlissFallen>() || _censors.Count == 0)
                return;

            Texture2D eyeTex = ModContent.Request<Texture2D>(
                "HeavenlyArsenal/Content/Items/Weapons/Summon/BadSun/DoomedSerenityEye"
            ).Value;

            for (int i = 0; i < _censors.Count; i++)
            {
                var censor = _censors[i];

                // Smoothly grow sizeLerp toward 1.0
                censor.SizeLerp = MathHelper.Lerp(censor.SizeLerp, 1f, 0.1f);

                Vector2 worldPos = npc.Center + censor.Offset;
                Vector2 drawPos = worldPos - Main.screenPosition;
                Vector2 origin = eyeTex.Size() * 0.5f;

                // scale goes from 0 → 0.2 (you can tweak the max)
                float scale = censor.SizeLerp * 0.2f;

                

                Main.EntitySpriteDraw(eyeTex, drawPos, null, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
            }
        }
    }

    public class BlissFallen : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }
        public override bool ReApply(NPC npc, int time, int buffIndex)
        {
            npc.GetGlobalNPC<FuckYourImmunity>().AddCensor(npc);

            return base.ReApply(npc, time, buffIndex);
        }
       
    }
}