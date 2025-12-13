using CalamityMod;
using CalamityMod.Items.Materials;
using CalamityMod.NPCs.TownNPCs;
using HeavenlyArsenal.Content.Items.Materials.BloodMoon;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using NoxusBoss.Content.Biomes;
using NoxusBoss.Core.Graphics.SpecificEffectManagers;
using ReLogic.Content;
using ReLogic.Graphics;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC
{
    partial class RitualAltar : BloodMoonBaseNPC
    {
        public override string Texture => "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/RitualAltarNPC/RitualAltarConcept";

        float SpeedMulti = 1;
        public override float buffPrio => 0;
        public override bool canBeSacrificed => false;


        public int SacrificeCooldown;
        public override int bloodBankMax => 100;

        public bool isSacrificing = false;
        private int Variant;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 1;
            NPCID.Sets.CantTakeLunchMoney[NPC.type] = true;
            NPCID.Sets.DoesntDespawnToInactivityAndCountsNPCSlots[Type] = true;

            ContentSamples.NpcBestiaryRarityStars[Type] = 0;
            NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                Scale = 0.15f,
                PortraitScale = 0.3f
            };
        }


        public override void SendExtraAI(BinaryWriter writer)
        {
            base.SendExtraAI(writer);
            foreach (var limb in _limbs)
            {
                writer.WriteVector2(limb.TargetPosition);
            }
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            base.ReceiveExtraAI(reader);
            for (int i = 0; i < _limbs.Length; i++)
            {
                var limb = _limbs[i];
                limb.TargetPosition = reader.ReadVector2();
                _limbs[i] = limb;
            }
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.rotation = -MathHelper.PiOver2;
            CreateLimbs();
            CultistCoordinator.CreateNewCult(NPC, Main.rand.Next(5, 8));
            for(int i = 0; i< Main.rand.Next(2,5); i++)
            {
                float thing = 1;
                if (i % 2 == 0)
                    thing = -1;
                Vector2 offset = new Vector2(10*thing*i, 0);
                //NPC.NewNPCDirect(NPC.GetSource_FromThis(), NPC.Center, ModContent.NPCType<FleshlingCultist.FleshlingCultist>());
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

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.RemoveAll(i => i is NPCPortraitInfoElement);

            // We can use AddRange instead of calling Add multiple times in order to add multiple items at once
            bestiaryEntry.Info.AddRange([
				// Sets the spawning conditions of this NPC that is listed in the bestiary.
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.BloodMoon,
                
				// Sets the description of this NPC that is listed in the bestiary.
				new FlavorTextBestiaryInfoElement($"Mods.{Mod.Name}.Bestiary.RitualAltar1"),
                
				// By default the last added IBestiaryBackgroundImagePathAndColorProvider will be used to show the background image.
				// The ExampleSurfaceBiome ModBiomeBestiaryInfoElement is automatically populated into bestiaryEntry.Info prior to this method being called
				// so we use this line to tell the game to prioritize a specific InfoElement for sourcing the background image.
				//new BestiaryPortraitBackgroundProviderPreferenceInfoElement(ModContent.GetInstance<ExampleSurfaceBiome>().ModBiomeBestiaryInfoElement),
                 new NPCPortraitInfoElement(0),
                new RiftBloodMoonBackground()
            ]);
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.NormalvsExpert(ModContent.ItemType<PenumbralMembrane>(), 3, 10));
            npcLoot.Add(ItemDropRule.CoinsBasedOnNPCValue(ModContent.NPCType<RitualAltar>()));

            npcLoot.Add(ModContent.ItemType<BloodOrb>(), 1, 10, 18);
        }


        public override void SetDefaults()
        {
            Variant = Main.rand.Next(1, 5);
            NPC.width = 80;
            NPC.height = 80;
            NPC.lifeMax = 350_000;
            NPC.damage = 300;
            NPC.defense = 120;
            NPC.npcSlots = 4f;
            NPC.knockBackResist = 0f;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            
            SpawnModBiomes =
            [
               ModContent.GetInstance<RiftEclipseBloodMoon>().Type, ModContent.GetInstance<DeadUniverseBiome>().Type
            ];
           
        }

        public override void AI()
        {

            // NPC.Center = Main.MouseWorld;
            //NPC.velocity.X = NPC.AngleTo(Main.LocalPlayer.Calamity().mouseWorld).ToRotationVector2().X * 10;//* NPC.Distance(Main.MouseWorld) ;

            NPC.velocity.X = 0;
            //return;
            locateSacrifices();
            UpdateList();
            StateMachine();

            SpeedMulti = Math.Abs(MathF.Sin(Time + NPC.whoAmI) * 2);
            NPC.direction = Math.Sign(NPC.velocity.X.NonZeroSign()) != 0 ? Math.Sign(NPC.velocity.X) : 1;
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
            float width = NPC.width * 0.3f;
            NPC.rotation = NPC.rotation.AngleLerp(d.AngleTo(NPC.Center), interp);
            _limbBaseOffsets[0] = new Vector2(-width, NPC.height / 2 - 20).RotatedBy(NPC.rotation + MathHelper.PiOver2);
            _limbBaseOffsets[1] = new Vector2(width, NPC.height / 2 - 20).RotatedBy(NPC.rotation + MathHelper.PiOver2);
            _limbBaseOffsets[2] = new Vector2(-width * 0.5f, NPC.height / 2 - 10).RotatedBy(NPC.rotation + MathHelper.PiOver2);
            _limbBaseOffsets[3] = new Vector2(width * 0.5f, NPC.height / 2 - 10).RotatedBy(NPC.rotation + MathHelper.PiOver2);
        }
        public override void PostAI()
        {
            bool isIdle = Math.Abs(NPC.velocity.X) < 0.5f;

            for (int i = 0; i < LimbCount; i++)
            {

                var limb = _limbs[i];

                //fallbacks
                if (limb.GrabPosition.HasValue)
                {
                    //don't grab above body
                    if (limb.GrabPosition.Value.Distance(NPC.Top) < 4 && MathF.Round(limb.GrabPosition.Value.Y) >= MathF.Round(NPC.Top.Y))
                    {
                        limb.StepCooldown = 0;
                        limb.StepProgress = 0;
                        limb.ShouldStep = true;
                    }
                    if (limb.GrabPosition.Value.Distance(NPC.Center + _limbBaseOffsets[i]) < 30)
                    {
                        //limb.ShouldStep = true;
                        //limb.StepCooldown = 0;
                        //limb.StepProgress = 0;
                    }

                }

                Vector2 basePos = NPC.Center + _limbBaseOffsets[i];

                /* if (isIdle && limb.StepProgress <= 0f)
                 {
                     Vector2 idlePos = GetIdleRestPosition(i, basePos);

                     // distance from current foothold to ideal idle spot
                     float idleError = Vector2.Distance(limb.GrabPosition ?? limb.EndPosition, idlePos);

                     // threshold: if leg is too far from where it *should* rest
                     if (idleError > 50f) // tune this
                     {
                         limb.PreviousGrabPosition = limb.GrabPosition ?? limb.EndPosition;
                         limb.GrabPosition = idlePos;
                         limb.StepProgress = 1f;
                         limb.StepCooldown = 10; // small so settling is quick

                         _limbs[i] = limb;
                         UpdateLimbState(ref _limbs[i], basePos, 0.4f, 15, i);
                         continue;
                     }
                 }*/

                float distToGround = Vector2.Distance(basePos, limb.GrabPosition.HasValue ? limb.GrabPosition.Value : limb.EndPosition);
                float max = limb.skeletonMaxLength;
                float stanceLength = max * 0.75f;

                if (limb.StepProgress <= 0f && ShouldRelease(i, limb, basePos))
                {
                    limb.PreviousGrabPosition = limb.GrabPosition ?? limb.EndPosition;
                    limb.GrabPosition = FindNewGrabPoint(basePos, i);
                    limb.StepProgress = 1f;
                }

                // 2. Animate step
                if (limb.StepProgress > 0f)
                {
                    //float t = 1f - limb.StepProgress;
                    //Vector2 start = limb.PreviousGrabPosition ?? limb.EndPosition;
                    //Vector2 end = limb.GrabPosition ?? start;

                    //float arc = (float)Math.Sin(t * MathHelper.Pi) * 402f;
                    //limb.TargetPosition = Vector2.Lerp(start, end, t) + new Vector2(0, -arc);

                    limb.StepProgress -= 0.1f;
                    if (limb.StepProgress <= 0f)
                    {
                        limb.StepProgress = 0f;
                        //limb.EndPosition = end;
                    }
                }
                else
                {
                    if (limb.GrabPosition.HasValue)
                        limb.TargetPosition = limb.GrabPosition.Value;
                }

                _limbs[i] = limb; // always write back
                UpdateLimbState(ref _limbs[i], basePos, 0.4f, 15, i);

            }
            balanceHead(0.025f);
            ApplyStanceHeightAdjustment();

            Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);               
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
