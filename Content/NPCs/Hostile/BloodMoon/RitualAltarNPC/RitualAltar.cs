using CalamityMod;
using CalamityMod.Items.Materials;
using HeavenlyArsenal.Content.Items.Materials.BloodMoon;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC
{
    partial class RitualAltar : BloodmoonBaseNPC
    {
        public override string Texture => "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/RitualAltarNPC/RitualAltarConcept";

        public ref float Time => ref NPC.ai[0];
        float SpeedMulti = 1;
        public override float buffPrio => 0;
        public override bool canBeSacrificed => false;


        public int SacrificeCooldown;
        public override int bloodBankMax => 100;

        public bool isSacrificing = false;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 1;
            NPCID.Sets.CantTakeLunchMoney[NPC.type] = true;
            NPCID.Sets.DoesntDespawnToInactivityAndCountsNPCSlots[Type] = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.rotation = -MathHelper.PiOver2;
            CreateLimbs();
            CultistCoordinator.CreateNewCult(NPC, Main.rand.Next(5, 8));
            for(int i = 0; i< Main.rand.Next(5); i++)
            {
                float thing = 1;
                if (i % 2 == 0)
                    thing = -1;
                Vector2 offset = new Vector2(10*thing*i, 0);
                NPC.NewNPCDirect(NPC.GetSource_FromThis(), NPC.Center, ModContent.NPCType<FleshlingCultist.FleshlingCultist>());
            }
        }
        public enum AltarAI
        {
            LookingForSacrifice,
            Sacrificing,

            lookForBuffTargets,
            Buffing,

            WalkTowardsPlayer
        }
        public AltarAI currentAIState = AltarAI.LookingForSacrifice;



        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.NormalvsExpert(ModContent.ItemType<PenumbralMembrane>(), 3, 10));
            npcLoot.Add(ItemDropRule.CoinsBasedOnNPCValue(ModContent.NPCType<RitualAltar>()));

            npcLoot.Add(ModContent.ItemType<BloodOrb>(), 1, 10, 18);
        }


        public override void SetDefaults()
        {
            NPC.width = 80;
            NPC.height = 80;
            NPC.lifeMax = 350_000;
            NPC.damage = 300;
            NPC.defense = 120;
            NPC.knockBackResist = 0f;
            NPC.noGravity = false;
            NPC.noTileCollide = false;

        }

        public override void AI()
        {

            NPC.velocity.X = 0;
            locateSacrifices();

            StateMachine();

            SpeedMulti = Math.Abs(MathF.Sin(Time + NPC.whoAmI) * 2);
            NPC.direction = Math.Sign(NPC.velocity.X.NonZeroSign()) != 0 ? Math.Sign(NPC.velocity.X) : 1;
            //NPC.velocity.X = NPC.AngleTo(Main.LocalPlayer.Center).ToRotationVector2().X * 10 * MathF.Tanh(Vector2.Distance(NPC.Center, Main.MouseWorld));
            //Main.NewText(SacrificeCooldown);
            if (isSacrificing && (NPCTarget == null || !NPCTarget.active))
                isSacrificing = false;
            if (NPCTarget == null || !NPCTarget.active)
                NPCTarget = default;


            //NPC.Center = Main.LocalPlayer.Calamity().mouseWorld;
            Time++;
        }


        void balanceHead(float interp = 0.2f)
        {
            Vector2 d = Vector2.Zero;
            for (int i = 0; i < _limbs.Count(); i++)
            {

                d += _limbs[i].EndPosition;
                if (_limbs[i] == default)
                {
                    CreateLimbs();
                }
            }
            d /= 4;

            NPC.rotation = NPC.rotation.AngleLerp(d.AngleTo(NPC.Center), interp);
        }
        public override void PostAI()
        {
            balanceHead(0.01f);
            UpdateList();
            UpdateLimbMotion();

            updateGravity();


            if (NPCTarget != null)
            {
                currentTarget = NPCTarget;
            }
            else
            if (playerTarget != null)
                currentTarget = playerTarget;

        }
    }



}
