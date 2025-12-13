using CalamityMod;
using CalamityMod.Items.Materials;
using HeavenlyArsenal.Common;
using HeavenlyArsenal.Content.Items.Materials.BloodMoon;
using HeavenlyArsenal.Content.Items.Weapons.Summon.BloodMoonWhip;
using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Leech
{
    partial class newLeech : BloodMoonBaseNPC, IMultiSegmentNPC
    {

        public override string Texture => "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Leech/UmbralLeech_Bestiary";
        private Dictionary<int, (Vector2[], Vector2[])> Tail = new Dictionary<int, (Vector2[], Vector2[])>(2);
        private static readonly Vector2[] WhiskerAnchors = new Vector2[]
        {
                new Vector2(16, 0),
                new Vector2(16, 14),
                new Vector2(5, 0),
                new Vector2(5, 14)
        };
        private static readonly Vector2[] tailOffsets = new Vector2[]
        {
            new Vector2(4,10),
            new Vector2(4, -10),

            new Vector2(-16,10),
            new Vector2(-16, -10)
        };
        private List<ExtraNPCSegment> _ExtraHitBoxes = new List<ExtraNPCSegment>();
        public ref List<ExtraNPCSegment> ExtraHitBoxes() => ref _ExtraHitBoxes;

        public int variant
        {
            get;
            set;
        }

        public Rectangle[] AdjHitboxes;
        public float accelerationInterp = 0;

        public int SegmentCount;

        public override int bloodBankMax => 50;

        //temporary debug ai slot
        public ref float Debug => ref NPC.ai[1];
        public Behavior CurrentState
        {
            get => (Behavior)NPC.ai[2];
            set => NPC.ai[2] = (float)value;
        }

        public ref float CosmeticTime => ref NPC.localAI[0];
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;
            NPCID.Sets.UsesMultiplayerProximitySyncing[Type] = true;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers()
            { // Influences how the NPC looks in the Bestiary
                CustomTexturePath = "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Leech/UmbralLeech_Bestiary", // If the NPC is multiple parts like a worm, a custom texture for the Bestiary is encouraged.
                Position = new Vector2(40f, 24f),
                PortraitPositionXOverride = 0f,
                PortraitPositionYOverride = 12f
            };
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            int avatarID = ModContent.NPCType<AvatarOfEmptiness>();
            bestiaryEntry.UIInfoProvider =
                new HighestOfMultipleUICollectionInfoProvider(new CommonEnemyUICollectionInfoProvider(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[Type], true),
                new CommonEnemyUICollectionInfoProvider(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[avatarID], true));


            bestiaryEntry.Info.AddRange([

                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.BloodMoon,

            new FlavorTextBestiaryInfoElement("Mods.HeavenlyArsenal.Bestiary.UmbralLeech")
            ]);
        }
        bool hasUsedEmergency;
        public override void SetDefaults()
        {
            NPC.lifeMax = 80_000;
            NPC.damage = 180;
            NPC.defense = 95;
            NPC.noGravity = true;
            NPC.aiStyle = -1;
            NPC.npcSlots = 7f;
            NPC.value = Terraria.Item.buyPrice(0, 1, 32, 6);
            NPC.noTileCollide = true;

            NPC.Size = new Vector2(30, 30);
            NPC.knockBackResist = 0.1f;
            

            NPC.DeathSound = AssetDirectory.Sounds.NPCs.Hostile.BloodMoon.UmbralLeech.DyingNoise;
            NPC.HitSound = SoundID.NPCHit1;

            SpawnModBiomes =
            [
                ModContent.GetInstance<RiftEclipseBloodMoon>().Type
            ];
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            base.SendExtraAI(writer);
            bool hasHitboxes = AdjHitboxes != null && AdjHitboxes.Length > 0;
            writer.Write(hasHitboxes);

            if (hasHitboxes)
            {
                writer.Write(AdjHitboxes.Length);
                for (int i = 0; i < AdjHitboxes.Length; i++)
                    writer.WriteVector2(AdjHitboxes[i].Location.ToVector2());

            }
            writer.Write((int)CurrentState);
            writer.Write(variant);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            base.ReceiveExtraAI(reader);
            bool hasHitboxes = reader.ReadBoolean();
            if (hasHitboxes)
            {
                int length = reader.ReadInt32();
                if (AdjHitboxes == null || AdjHitboxes.Length != length)
                    AdjHitboxes = new Rectangle[length];
                for (int i = 0; i < length; i++)
                    AdjHitboxes[i].Location = reader.ReadVector2().ToPoint();
                
            }
            else
            {
                AdjHitboxes = null;
            }

            CurrentState = (Behavior)reader.ReadInt32();
            variant = reader.ReadInt32();
        }

        public override void OnSpawn(IEntitySource source)
        {
            variant = Main.rand.Next(0, 3);
            UmbralLeechGores = new Asset<Texture2D>[7];
            for (int i = 1; i <= 7; i++)
                UmbralLeechGores[i - 1] = ModContent.Request<Texture2D>($"HeavenlyArsenal/Content/Gores/Enemy/BloodMoon/UmbralLeech/UmbralLeechGore{i}");


            SegmentCount = Main.rand.Next(6, 16);
            AdjHitboxes = new Rectangle[SegmentCount];
            for (int i = 0; i < AdjHitboxes.Length; i++)
            {
                AdjHitboxes[i].Width = 30;
                AdjHitboxes[i].Height = 30;
                AdjHitboxes[i].X = (int)NPC.Center.X;
                AdjHitboxes[i].Y = (int)NPC.Center.Y;
            }
            int tailSegmentNum = (int)Utils.Remap(SegmentCount, 6, 16, 8, 34);
            int reducedNum = (int)(tailSegmentNum / 1.4f);
            for (int i = 0; i < 4; i++)
            {
                if (i < 2)
                    Tail.Add(i, (new Vector2[tailSegmentNum], new Vector2[tailSegmentNum]));
                else
                    Tail.Add(i, (new Vector2[reducedNum], new Vector2[reducedNum]));
            }


            _ExtraHitBoxes = new List<ExtraNPCSegment>(SegmentCount);
            for (int i = 0; i < SegmentCount; i++)
                _ExtraHitBoxes.Add(new ExtraNPCSegment(AdjHitboxes[i], uniqueIframes: true));

            NPC.netUpdate = true;
        }
        public override void AI()
        {

           
            string debugPlayer = "tester2";

            if (Main.LocalPlayer.name.ToLower() == debugPlayer.ToLower() && NPC.ai[1] == 1)
            {

                NPC.Center = Main.MouseWorld;
                NPC.ai[1] = 0;
                return;
            }

            StateMachine();
            if (NPC.life < NPC.lifeMax / 3 && !hasUsedEmergency && !NPC.GetGlobalNPC<SacrificeNPC>().isSacrificed)
            {
                if (Main.rand.NextBool(3))
                {

                    CurrentState = Behavior.DisipateIntoBlood;
                }
                else
                    hasUsedEmergency = true;


            }

            Time++;

        }

        public override void UpdateLifeRegen(ref int damage)
        {
            if (CurrentState == Behavior.DisipateIntoBlood)
            {
                NPC.lifeRegenCount += 40000;
            }
        }
        public override void PostAI()
        {
            //do this in post ai so altars don't fuck it up

            ManageTail();
            UpdateHitboxes(0.2f);

            if (AdjHitboxes != null)
                NPC.rotation = AdjHitboxes[1].Center().AngleTo(NPC.Center);


            if (CurrentState != Behavior.Lunge)
                accelerationInterp = float.Lerp(accelerationInterp, 0, 0.2f);

            CosmeticTime++;
        }

        void UpdateHitboxes(float AlignmentStrength = 0.5f)
        {
            if (CurrentState == Behavior.DeathAnim && Time > 200)
                AlignmentStrength *= 0.2f;

            if (AdjHitboxes == null || AdjHitboxes.Length <= 0)
                return;

            float segmentLength = AdjHitboxes[0].Width - 6f;
            Vector2 half = new Vector2(AdjHitboxes[0].Width / 2f, AdjHitboxes[0].Height / 2f);

            AdjHitboxes[0].Location = (NPC.Center - half).ToPoint();

            Vector2 forward = NPC.rotation.ToRotationVector2();
            Vector2 lastCenter = NPC.Center;

            for (int i = 1; i < AdjHitboxes.Length; i++)
            {
                Vector2 prevCenter = AdjHitboxes[i - 1].Center();
                Vector2 currCenter = AdjHitboxes[i].Center();

                Vector2 toPrev = prevCenter - currCenter;
                float dist = toPrev.Length();
                if (dist < 0.001f)
                    continue;

                Vector2 dir = toPrev / dist;
                //create a offset to attempt to make the npc wiggle
                //the issue with this is that it makes the npc segments drift towards the top left hand corner instead of based on where the npc first bit.
                Vector2 WiggleOffset = new Vector2(MathF.Sin(CosmeticTime/10.1f ) * 3, 0).RotatedBy(toPrev.ToRotation() + MathHelper.PiOver2);
                //disable if not latched onto something, so you don't look stupid
                WiggleOffset *= CurrentState == Behavior.latch ? 2 : 0;
                Vector2 distanceTarget = prevCenter - dir * segmentLength;
                // Ideal position if fully aligned behind NPC’s rotation
                Vector2 alignTarget = lastCenter - forward * segmentLength;
                // Blend between distance-based following and axis alignment
                Vector2 blendedTarget = Vector2.Lerp(distanceTarget, alignTarget, AlignmentStrength);
                // stick together even if very fast
                float t = MathHelper.Clamp((dist - segmentLength) / segmentLength, 0f, 1f);
                float lerpFactor = MathHelper.Lerp(0.25f, 0.9f, t);
                Vector2 finalCenter = Vector2.Lerp(currCenter, blendedTarget + WiggleOffset, lerpFactor);
                AdjHitboxes[i].Location = (finalCenter - half + new Vector2(0.5f)).ToPoint();
                lastCenter = finalCenter;
            }

            for (int i = 0; i < AdjHitboxes.Length - 1; i++)
            {
                _ExtraHitBoxes[i].Hitbox = AdjHitboxes[i];
                if (_ExtraHitBoxes[i].ImmuneTime > 0)
                    _ExtraHitBoxes[i].ImmuneTime--;
            }
        }

        void ForceResetHitboxes()
        {
            UpdateHitboxes(1);
        }

        public override bool CheckDead()
        {
            if (NPC.life <= 0)
            {
                NPC.life = 1;
                NPC.dontTakeDamage = true;
                if (CurrentState != Behavior.DeathAnim)
                {
                    Time = 0;
                    CurrentState = Behavior.DeathAnim;
                }
                NPC.damage = -1;



            }




            return false;
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {

            return base.DrawHealthBar(hbPosition, ref scale, ref position);
            Rectangle Overall = new Rectangle();
            foreach (var i in AdjHitboxes)
            {
                Overall = Rectangle.Union(Overall, i);
            }
            position = Overall.Center();
        }
        public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
        {
            if (AdjHitboxes != null)
                foreach (var i in AdjHitboxes)
                {
                    boundingBox = Rectangle.Union(boundingBox, i);
                }
        }


        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.NormalvsExpert(ModContent.ItemType<ViscousWhip_Item>(), 36, 24));
            npcLoot.Add(ItemDropRule.NormalvsExpert(ModContent.ItemType<UmbralLeechDrop>(), 3, 2));
            npcLoot.Add(ItemDropRule.CoinsBasedOnNPCValue(ModContent.NPCType<newLeech>()));

            npcLoot.Add(ModContent.ItemType<BloodOrb>(), 1, 40, 48);
        }
        void ManageTail()
        {


            const float segmentLength = 4f;
            const float springiness = 0.01f;     // how much it flexes while moving
            const float damping = 0.9f;          // overall smoothness
            const float alignmentStrength = 0.6f; // how fast it aligns when still


            float moveFactor = 0;

            for (int i = 0; i < Tail.Count; i++)
            {
                var _tailPosition = Tail[i].Item1;
                var _tailVels = Tail[i].Item2;
                if (_tailPosition == null || _tailVels == null)
                    continue;

                Vector2 headDir = (AdjHitboxes[^1].Center() - AdjHitboxes[^2].Center()).SafeNormalize(Vector2.UnitX) * -1;
                float headRot = headDir.ToRotation() + MathHelper.Pi;


                Vector2 headPos = AdjHitboxes[^1].Center() + tailOffsets[i].RotatedBy(headRot);
                _tailPosition[0] = headPos;

                for (int j = 1; j < _tailPosition.Length; j++)
                {
                    if (_tailPosition[j] == Vector2.Zero)
                        _tailPosition[j] = headPos;
                    if (moveFactor > 0.05f)
                    {
                        _tailVels[j] *= damping;
                        _tailPosition[j] += _tailVels[j];

                        Vector2 toPrev = _tailPosition[j - 1] - _tailPosition[j];
                        float dist = toPrev.Length();
                        if (dist > 0.0001f)
                        {
                            Vector2 dir = toPrev / dist;
                            float diff = dist - segmentLength;
                            _tailPosition[j] += dir * diff * springiness * moveFactor;
                            _tailVels[j] += dir * diff * springiness * 0.5f * moveFactor;
                        }


                    }

                    float offset = 5.74f;

                    if (i > 1)
                        offset = 24.75f;

                    float RotationOffset = 0;
                    RotationOffset = i % 2 == 0 ? offset : -offset;


                    Vector2 targetPos = _tailPosition[j - 1] - headDir.RotatedBy(MathHelper.ToRadians(RotationOffset)) * segmentLength;

                    Vector2 alignVel = (targetPos - _tailPosition[j]) * alignmentStrength * (1f - moveFactor);
                    _tailVels[j] = Vector2.Lerp(_tailVels[j], alignVel, 1);

                    _tailPosition[j] += _tailVels[j];
                    int thing = i % 2 == 0 ? -1 : 1;
                    float wigglePhase = CosmeticTime * 0.12f + j * 0.5f;
                    float wiggleMag = MathHelper.Lerp(0.3f, 2.0f, (float)j / _tailPosition.Length) * thing;

                  
                    Vector2 wiggle = new Vector2(0, (float)Math.Sin(wigglePhase) * wiggleMag);
                    _tailPosition[j] += wiggle.RotatedBy(headRot) * 0.75f;
                }
            }
        }


    }
}
