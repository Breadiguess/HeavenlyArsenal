using HeavenlyArsenal.Common.Utilities;
using HeavenlyArsenal.Content.Items.Armor.ShintoArmor;
using Microsoft.Build.Tasks;
using NoxusBoss.Core.Graphics.RenderTargets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Armor.Vanity.ScavSona
{
    internal class ScavSona_Arm_Renderer : PlayerDrawLayer
    {
        public static InstancedRequestableTarget ArmTex;
        public override void Load()
        {
            On_PlayerDrawLayers.DrawPlayer_10_BackAcc += what;

            ArmTex = new InstancedRequestableTarget();
            Main.ContentThatNeedsRenderTargets.Add(ArmTex);
        }

        private void what(On_PlayerDrawLayers.orig_DrawPlayer_10_BackAcc orig, ref PlayerDrawSet drawinfo)
        {
            orig(ref drawinfo);
        }

        public override Position GetDefaultPosition()
        {
            return new BeforeParent(PlayerDrawLayers.BackAcc);
        }

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            return drawInfo.drawPlayer.body == EquipLoader.GetEquipSlot(Mod, nameof(ScavSona_Dress), EquipType.Body);
        }


        

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            if (drawInfo.drawPlayer.GetModPlayer<ScavSona_ArmManager>().Active == false)
                return;
            if (ScavSona_IKArm.ScavSona_IKArm_Target == null)
                return;
            for(int i = 0; i< 6; i++)
            {
                DrawData b= new DrawData(ScavSona_IKArm.ScavSona_IKArm_Target, drawInfo.BodyPosition() + new Vector2(1f,0).RotatedBy(i/6f * MathHelper.TwoPi), null, Color.White, 0, ScavSona_IKArm.ScavSona_IKArm_Target.Size() / 2, 2, 0);

                b.color = Color.White.MultiplyRGB(drawInfo.colorArmorHead);
                b.shader = drawInfo.cBody;
                  drawInfo.DrawDataCache.Add(b);
            }
            DrawData a = new DrawData(ScavSona_IKArm.ScavSona_IKArm_Target, drawInfo.BodyPosition(), null, Color.Black, 0, ScavSona_IKArm.ScavSona_IKArm_Target.Size() / 2, 2, 0);

            a.shader = drawInfo.cBody;
            drawInfo.DrawDataCache.Add(a);

            return;

            if(drawInfo.shadow != 0f || drawInfo.drawPlayer.dead)
        {
                return;
            }

           // ArmTex.Request(208, 100, drawInfo.drawPlayer.whoAmI, RenderIntoTarget);

            if (!ArmTex.TryGetTarget(drawInfo.drawPlayer.whoAmI, out var portalTexture) || portalTexture is null)
            {
                return;
            }

            var val = (float)Math.Sin(Main.GlobalTimeWrappedHourly / 2) * 2;
            var Rot = drawInfo.drawPlayer.fullRotation + MathHelper.ToRadians(drawInfo.drawPlayer.direction * -45);
            var position = drawInfo.HeadPosition() + new Vector2(0, -20f + val).RotatedBy(Rot);

            var rift = new DrawData(portalTexture, position, null, Color.White, Rot + MathHelper.Pi, portalTexture.Size() * 0.5f, 1f, 0);
            //drawInfo.DrawDataCache.Add(rift);
            //drawRift(ref drawInfo);

        }
    }
}
