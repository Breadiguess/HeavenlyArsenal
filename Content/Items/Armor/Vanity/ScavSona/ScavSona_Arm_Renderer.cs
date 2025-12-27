using HeavenlyArsenal.Common.Utilities;
using HeavenlyArsenal.Content.Items.Armor.ShintoArmor;
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
                DrawData b= new DrawData(ScavSona_IKArm.ScavSona_IKArm_Target, drawInfo.BodyPosition() + new Vector2(0.5f,0).RotatedBy(i/6f * MathHelper.TwoPi), null, Color.Red, 0, ScavSona_IKArm.ScavSona_IKArm_Target.Size() / 2, 2, 0);

                drawInfo.DrawDataCache.Add(b);
            }
            DrawData a = new DrawData(ScavSona_IKArm.ScavSona_IKArm_Target, drawInfo.BodyPosition(), null, Color.Black, 0, ScavSona_IKArm.ScavSona_IKArm_Target.Size() / 2, 2, 0);

            drawInfo.DrawDataCache.Add(a);
        }
    }
}
