using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Content.Particles;
using HeavenlyArsenal.Core;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Luminance.Common.Utilities.Utilities;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Jellyfish
{
    partial class BloodJelly : BloodmoonBaseNPC
    {
        #region Setup
        public override string Texture => "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Jellyfish/Jellyfish";
        public enum JellyState
        {
            Idle,
            TrackTarget
        }
        public int Time
        {
            get => (int)NPC.ai[0];
            set => NPC.ai[0] = value;
        }
        public JellyState CurrentState
        {
            get => (JellyState)NPC.ai[1];
            set => NPC.ai[1] = (int)value;
        }
        public ref float SquishInterp => ref NPC.localAI[0];

        #endregion
        public override void SetDefaults()
        {
            NPC.lifeMax = 40000;
            NPC.damage = 300;
            NPC.defense = 300;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            NPC.Size = new Vector2(40, 80);
            NPC.aiStyle = -1;
        }

        public override void AI()
        {
            
        }
    }
}
