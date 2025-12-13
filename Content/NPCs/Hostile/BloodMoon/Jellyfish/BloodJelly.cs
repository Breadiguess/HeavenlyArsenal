using CalamityMod;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using NoxusBoss.Core.Graphics.RenderTargets;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Jellyfish
{
    partial class BloodJelly : BloodMoonBaseNPC
    {
        #region Setup
        public override bool canBeSacrificed => false;
        public override bool canBebuffed => false;
        public override void SetStaticDefaults()
        {
            NPCID.Sets.DoesntDespawnToInactivityAndCountsNPCSlots[Type] = true;
            NPCID.Sets.MustAlwaysDraw[Type] = true;

            Main.ContentThatNeedsRenderTargets.Add(BestiaryTarget);
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            //for all the completionists out there
            int avatarID = ModContent.NPCType<AvatarOfEmptiness>();
            bestiaryEntry.UIInfoProvider = new HighestOfMultipleUICollectionInfoProvider(
                new CommonEnemyUICollectionInfoProvider(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[Type], true),
                new CommonEnemyUICollectionInfoProvider(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[avatarID], true));
            bestiaryEntry.Info.AddRange([

                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.BloodMoon,
				new FlavorTextBestiaryInfoElement("Mods.HeavenlyArsenal.Bestiary.BloodJelly1"),
                ]);
        }

        public static InstancedRequestableTarget BestiaryTarget
        {
            get;
            set;
        } = new InstancedRequestableTarget();


        public override string Texture => "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Jellyfish/BloodJelly";

        private Dictionary<int, (Vector2[], Vector2[])> Tendrils = new Dictionary<int, (Vector2[], Vector2[])>(2);
        private readonly static Vector2[] tendrilOffsets = new Vector2[]
        {
                new Vector2(-30, 1),
                new Vector2(10,0),

                new Vector2(30, 1),
                new Vector2(-10, 0),
                new Vector2(0, -30)
        };

        public int ThreatCount
        {
            get => ThreatIndicies.Count;
        }
        public List<int> ThreatIndicies;
        public int CosmeticTime
        {
            get => (int)NPC.localAI[0];
            set => NPC.localAI[0] = value;
        }
        private readonly int tendrilCount = 5;
        private int MaxCapacity;
        public override void SetDefaults()
        {
            NPC.lifeMax = 41934;
            NPC.damage = 300;
            NPC.defense = 300;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            NPC.Size = new Vector2(40, 40);
            NPC.aiStyle = -1;
            NPC.knockBackResist = 0.2f;
            NPC.Calamity().VulnerableToWater = true;
            SpawnModBiomes =
            [
               ModContent.GetInstance<RiftEclipseBloodMoon>().Type
            ];
        }
        public override void OnSpawn(IEntitySource source)
        {

            for (int i = 0; i < tendrilCount; i++)
            {
                if (i < tendrilCount - 1)
                {
                    if (i % 2 != 0) Tendrils.Add(i, (new Vector2[27], new Vector2[27]));

                    else Tendrils.Add(i, (new Vector2[17], new Vector2[17]));


                }
                else
                    Tendrils.Add(i, (new Vector2[30], new Vector2[30]));
            }
            CosmeticTime += NPC.whoAmI * 10;
            int thing = Main.rand.Next(10, 30);

            MaxCapacity = Main.rand.Next(thing, thing + 20);
            ThreatIndicies = new List<int>(MaxCapacity);
            for (int i = 0; i < thing; i++)
            {
                Projectile d = Projectile.NewProjectileDirect(source, NPC.Center, Vector2.Zero, ModContent.ProjectileType<TheThreat>(), 150, 10);
                ThreatIndicies.Add(d.whoAmI);
                //Main.NewText(d.whoAmI + ", " + i);
                TheThreat a = d.ModProjectile as TheThreat;
                a.ownerIndex = NPC.whoAmI;
            }
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            base.SendExtraAI(writer);
            //writer.Write(MaxCapacity);

            //for(int i = 0; i< ThreatIndicies.Capacity;i++)
             //   writer.Write(ThreatIndicies[i]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            base.ReceiveExtraAI(reader);
                //MaxCapacity = reader.ReadInt32();

            //ThreatIndicies.Clear();

            
           // for (int i = 0; i < ThreatIndicies.Capacity; i++)
            //{
            //    Main.NewText(reader.ReadInt32());
           //     ThreatIndicies.Add(reader.ReadInt32());
           // }
        }


        #endregion
        public override void AI()
        {
          
            if (NPC.ai[2] != 0)
            {

                NPC.Center = Vector2.Lerp(NPC.Center, Main.MouseWorld, 0.2f);
                NPC.ai[2] = 0;
                //NPC.GetGlobalNPC<FastUpdateGlobal>().speed
                return;
            }

            //            return;
            StateMachine();
            Time++;
        }

        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            Player thing;
            projectile.TryGetOwner(out thing);
            currentTarget = thing;
        }

        public override void PostAI()
        {
            if (OpenInterpolant > 0 && CurrentState != Behavior.Railgun)
                OpenInterpolant = float.Lerp(OpenInterpolant, 0, 0.1f);

            recoilInterp = float.Lerp(recoilInterp, 0, 0.2f);
             //NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;
            manageTendrils();
            CosmeticTime++;

            if (CurrentState != Behavior.DiveBomb && CurrentState != Behavior.StickAndExplode && Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (Time % 20 == 0 && Main.rand.NextBool(2) && ThreatIndicies.Count < ThreatIndicies.Capacity)
                {
                    SoundEngine.PlaySound(GennedAssets.Sounds.Environment.DivineStairwayStep with { Pitch = 0.1f, MaxInstances = 0, PitchVariance = 1 }, NPC.Center);
                    Projectile d = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<TheThreat>(), 160, 10);
                    ThreatIndicies.Add(d.whoAmI);
                    TheThreat a = d.ModProjectile as TheThreat;
                    a.ownerIndex = NPC.whoAmI;
                }
            }
        }

        void manageTendrils()
        {
            Vector2 BodyRot = (NPC.rotation + MathHelper.PiOver2).ToRotationVector2();
            float dampening = 0.5f;
            float waveSpeed = 01f;     // Controls how fast the tendril oscillates
            float waveStrength = 270f;   // Controls how wide the tendril swings
            float segmentLength = 3f;

            for (int j = 0; j < Tendrils.Count; j++)
            {
                var _tendrilPos = Tendrils[j].Item1;
                var _tendrilVel = Tendrils[j].Item2;

                _tendrilPos[0] = NPC.Center;
                for (int i = 1; i < _tendrilPos.Length; i++)
                {
                    if (_tendrilPos[i] == Vector2.Zero)
                        _tendrilPos[i] = NPC.Center;
                    //scillating direction using sine wave along tendril
                    float offset = MathHelper.ToRadians(-45.74f);

                    //if (j % 2 == 0)
                    //offset = 24.75f;
                    float wave = MathHelper.ToRadians((float)Math.Sin(CosmeticTime / 10.1f * waveSpeed) * (1 + waveStrength));
                    offset = float.Lerp(offset, wave, 0.2f);
                    float RotationOffset = 0;
                    RotationOffset = j % 3 == 0 ? offset : -offset;
                    //RotationOffset = j % 2 == 0 ? offset:offset ;

                    if (j == tendrilCount - 1)
                    {
                        if (CurrentState != Behavior.StickAndExplode)
                            RotationOffset = 0;
                        else
                        {
                            float thing = MathHelper.ToRadians(MathF.Sin((CosmeticTime + 20) / 10.1f) * 46.75f);
                            RotationOffset = float.Lerp(RotationOffset, thing, 1f);
                        }
                        //Main.NewText(RotationOffset);
                    }

                    //Vector2 perp = BodyRot.RotatedBy(MathHelper.PiOver2 * wave * waveStrength / 20f);

                    Vector2 targetPos = _tendrilPos[i - 1] + (BodyRot.RotatedBy(RotationOffset)) * segmentLength;

                    Vector2 alignVel = (targetPos - _tendrilPos[i]) * 0.5f;


                    if (j != tendrilCount - 1)
                        _tendrilVel[i] = Vector2.Lerp(_tendrilVel[i], alignVel, 1f);
                    else
                        _tendrilVel[i] = Vector2.Lerp(_tendrilVel[i], alignVel, 1f);
                    _tendrilPos[i] += _tendrilVel[i] ;


                    if (_tendrilPos[i] == Vector2.Zero)
                        _tendrilPos[i] = NPC.Center;
                }
            }
        }
    }
}
