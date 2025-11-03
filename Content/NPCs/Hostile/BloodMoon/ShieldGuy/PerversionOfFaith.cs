using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.ShieldGuy
{
    partial class PerversionOfFaith : BloodmoonBaseNPC
    {
        public override string Texture => "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/ShieldGuy/PerversionOfFaith";
        public int Time
        {
            get => (int)NPC.ai[0];
            set => NPC.ai[0] = value;
        }
        public override int bloodBankMax
        {
            get => base.bloodBankMax;
            set => base.bloodBankMax = value;
        }
        public override void SetDefaults()
        {
            
            NPC.aiStyle = -1;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            NPC.Size = new Vector2(30, 30);
            NPC.defense = 400;
            NPC.damage = 300;
            NPC.lifeMax = 400;


        }
        public override void OnSpawn(IEntitySource source)
        {

        }
        public override void AI()
        {
            playerTarget = Main.player[NPC.FindClosestPlayer()];
            NPC.velocity = NPC.Center.AngleTo(playerTarget.Center).ToRotationVector2();
            NPC.velocity += new Vector2(0, MathF.Sin(Time));

            Time++;
        }
        
    }
}
