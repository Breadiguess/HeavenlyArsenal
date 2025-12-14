using System.Collections.Generic;
using System.Linq;
using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Leech;
using Terraria.Audio;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC;

internal partial class RitualAltar
{
    public List<NPC> Sacrifices = new(Main.npc.Length);

    private int buffCost;

    private readonly List<NPC> nearbyNpcs = new();

    #region Sacrifice System

    private bool CheckCandidates(NPC npc)
    {
        if (BlackListProjectileNPCs.BlackListedNPCs.Contains(npc.type))
        {
            return false;
        }

        // Main.NewText("PassedBlacklist");
        if (RitualSystem.IsNPCBuffed(npc))
        {
            return false;
        }

        //Main.NewText("PassedRitualSystem");
        if (npc.type == NPC.type)
        {
            return false;
        }

        if (npc.immortal || npc.dontTakeDamage)
        {
            return false;
        }

        //Main.NewText("Passed self check");
        if (!npc.active)
        {
            return false;
        }

        if (npc.Equals(NPC) || npc.type == ModContent.NPCType<RitualAltar>())
        {
            return false;
        }

        if (npc.ModNPC == null)
        {
            //Main.NewText(npc.FullName);
            return false;
        }

        //Main.NewText("Passed Modnpc Check");
        if (npc.ModNPC is not BloodMoonBaseNPC)
        {
            return false;
        }

        if (npc.Distance(NPC.Center) > 700)
        {
            return false;
        }

        if (npc.type == ModContent.NPCType<Umbralarva>())
        {
            return false;
        }

        if (npc.type == ModContent.NPCType<FleshlingCultist.FleshlingCultist>())
        {
            var cult = CultistCoordinator.GetCultOfNPC(npc);

            if (cult != CultistCoordinator.GetCultOfNPC(NPC))
            {
                return false;
            }
        }

        return true;
    }

    private void locateSacrifices()
    {
        if (SacrificeCooldown-- <= 0)
        {
            foreach (var npc in Main.npc)
            {
                if (!CheckCandidates(npc))
                {
                    continue;
                }

                if (!Sacrifices.Contains(npc))
                {
                    Sacrifices.Add(npc);
                }
            }
        }
    }

    private void UpdateList()
    {
        //first: trim the list. if there's an inactive npc in the list, remove it from the list.
        //TODO: sort by BloodMoonBaseNPC BuffPrio (so that lower buff priority targets get selected to be sacrifices)
        // and by distance (so that the first index is always the closest npc)
        Sacrifices.RemoveAll(id => id == null || !id.active || RitualSystem.IsNPCBuffed(id) || id.type == ModContent.NPCType<RitualAltar>());
        Sacrifices.RemoveAll(id => { return id.ModNPC is BloodMoonBaseNPC b ? b.canBeSacrificed : true; });

        //Main.NewText($"BuffedNPCs count: {RitualSystem.BuffedNPCs.Count}");

        //todo: Sort by SacrificePrio; 1 = top priority, 0 = pretty much ignored.
        Sacrifices.Sort((a, b) => Vector2.Distance(a.Center, NPC.Center).CompareTo(Vector2.Distance(b.Center, NPC.Center)));

        Sacrifices.Sort
        (
            (a, b) =>
            {
                var aPrio = a.ModNPC is BloodMoonBaseNPC aBloodmoon ? aBloodmoon.SacrificePrio : 0f;
                var bPrio = b.ModNPC is BloodMoonBaseNPC bBloodmoon ? bBloodmoon.SacrificePrio : 0f;

                return bPrio.CompareTo(aPrio);
            }
        );
        /*
        string a = "";
        int i = 0;
        foreach (NPC npc in Sacrifices)
        {

            BloodMoonBaseNPC d = npc.ModNPC as BloodMoonBaseNPC;
            a += $"{npc.FullName}, whoami? {npc.whoAmI}, Sacrifice Prio: {d.SacrificePrio}, {i}\n";
            i++;
        }
        if(Sacrifices.Count>0 || a.Length>0)
            Main.NewText(a);
        */
    }

    // Tunables
    private const float FleshlingLeniency = 300f; // only buff FleshlingCultist if within this distance

    private float BuffCost()
    {
        return bloodBankMax / 5f;
    }

    private float SacrificeThreshold()
    {
        return bloodBankMax / 2.25f;
    }

    private void StateMachine()
    {
        switch (currentAIState)
        {
            // Look for targets to buff. If not enough blood to buff, flip to sacrifice mode.
            case AltarAI.lookForBuffTargets:
            case AltarAI.Buffing:
            {
                if (blood < BuffCost())
                {
                    //Main.NewText($"[AI] Blood low ({blood:F0} < {BuffCost():F0}) → LookingForSacrifice");
                    currentAIState = AltarAI.LookingForSacrifice;

                    break;
                }

                var startedBuffing = BuffOtherEnemies(); // now returns true if we engaged a target

                if (!startedBuffing)
                {
                    // Nothing to buff—idle behavior
                    WalkTowardsPlayer();
                    // remain in lookForBuffTargets and try again next tick
                }
                else
                {
                    currentAIState = AltarAI.Buffing;
                }

                break;
            }

            case AltarAI.LookingForSacrifice:
            {
                if (blood < SacrificeThreshold())
                {
                    //Main.NewText($"[AI] Blood {blood:F0} < {SacrificeThreshold():F0} → Sacrifice");
                    SacrificeNPC();
                }
                else
                {
                    //Main.NewText($"[AI] Blood OK ({blood:F0}) → lookForBuffTargets");
                    currentAIState = AltarAI.lookForBuffTargets;
                }

                break;
            }

            case AltarAI.Sacrificing:
            {
                SacrificeNPC();

                if (blood >= bloodBankMax)
                {
                    //Main.NewText($"[AI] Blood full ({blood:F0}) → lookForBuffTargets");
                    currentAIState = AltarAI.lookForBuffTargets;
                }

                break;
            }

            case AltarAI.WalkTowardsPlayer:
                WalkTowardsPlayer();

                break;
        }
    }

    private bool BuffOtherEnemies()
    {
        // Not enough blood to even try buffing.
        if (blood <= bloodBankMax / 4f)
        {
            return false;
        }

        nearbyNpcs.Clear();

        foreach (var npc in Main.npc)
        {
            if (npc.dontTakeDamage)
            {
                continue;
            }

            if (!npc.active)
            {
                continue;
            }

            if (npc.life <= 1)
            {
                continue;
            }

            if (npc.type == NPC.type)
            {
                continue;
            }

            if (BlackListProjectileNPCs.BlackListedNPCs.Contains(npc.type))
            {
                continue;
            }

            if (npc.Distance(NPC.Center) > 700f)
            {
                continue;
            }

            if (RitualSystem.BuffedNPCs.Contains(npc))
            {
                continue;
            }

            if (npc.ModNPC != null)
            {
                if (npc.ModNPC.Type == ModContent.NPCType<BloodMoonBaseNPC>())
                {
                    var d = npc.ModNPC as BloodMoonBaseNPC;

                    if (!d.canBebuffed)
                    {
                        continue;
                    }
                }
            }

            // Skip if recently resurrected & already in the ritual set
            RitualBuffNPC gn;
            npc.TryGetGlobalNPC(out gn);

            if (RitualSystem.BuffedNPCs.Contains(npc) && gn.WasRessurectedRecently)
            {
                continue;
            }

            var isFleshling = npc.type == ModContent.NPCType<FleshlingCultist.FleshlingCultist>();

            if (isFleshling && npc.Distance(NPC.Center) >= FleshlingLeniency)
            {
                continue;
            }

            nearbyNpcs.Add(npc);
        }

        if (nearbyNpcs.Count == 0)
        {
            return false;
        }

        //STOP IT
        nearbyNpcs.RemoveAll(npc => npc == null || !npc.active);

        // Sort by prio desc, then distance asc
        nearbyNpcs.Sort
        (
            (a, b) =>
            {
                var aPrio = a.ModNPC is BloodMoonBaseNPC ab ? ab.buffPrio : 0f;
                var bPrio = b.ModNPC is BloodMoonBaseNPC bb ? bb.buffPrio : 0f;

                var prioCompare = bPrio.CompareTo(aPrio);

                if (prioCompare != 0)
                {
                    return prioCompare;
                }

                var aDist = Vector2.Distance(a.Center, NPC.Center);
                var bDist = Vector2.Distance(b.Center, NPC.Center);

                return aDist.CompareTo(bDist);
            }
        );

        // Pick top candidate
        var target = nearbyNpcs[0];

        if (NPCTarget == null || !NPCTarget.active)
        {
            NPCTarget = target;
        }

        RitualBuffNPC tgtGN;
        NPCTarget.TryGetGlobalNPC(out tgtGN);
        var alreadyBuffed = tgtGN.hasRitualBuff || RitualSystem.BuffedNPCs.Contains(NPCTarget);

        if (!alreadyBuffed)
        {
            RitualSystem.AddNPC(NPCTarget); //.BuffedNPCs.Add(NPCTarget);
            tgtGN.isBeingBuffed = true;
            tgtGN.BuffGranter = NPC;

            //Main.NewText($"[Buff] Target: {NPCTarget.FullName} (prio {(NPCTarget.ModNPC as BloodMoonBaseNPC)?.buffPrio ?? 0f:F2}, " +
            //            $"dist {NPCTarget.Center.Distance(NPC.Center):F0})");
        }
        else
        {
            // Optionally try the next candidate if the first is already buffed
            var alt = nearbyNpcs.FirstOrDefault
            (
                n =>
                {
                    var gn2 = n.GetGlobalNPC<RitualBuffNPC>();

                    return !(gn2.hasRitualBuff || RitualSystem.BuffedNPCs.Contains(n));
                }
            );

            if (alt != null)
            {
                NPCTarget = alt;
                var gn3 = NPCTarget.GetGlobalNPC<RitualBuffNPC>();
                RitualSystem.AddNPC(NPCTarget);
                gn3.isBeingBuffed = true;
                gn3.BuffGranter = NPC;

                //Main.NewText($"[Buff] Switched target: {NPCTarget.FullName} (prio {(NPCTarget.ModNPC as BloodMoonBaseNPC)?.buffPrio ?? 0f:F2}, " +
                //              $"dist {NPCTarget.Center.Distance(NPC.Center):F0})");
            }
            else
            {
                //Main.NewText("[Buff] All candidates already buffed or invalid.");
                return false;
            }
        }

        var distToTarget = Vector2.Distance(NPC.Center, NPCTarget.Center);
        var slide = MathF.Tanh(distToTarget) * SpeedMulti * 3;
        NPC.velocity.X = NPC.AngleTo(NPCTarget.Center).ToRotationVector2().X * slide;

        return true;
    }

    private void SacrificeNPC()
    {
        if (Sacrifices.Count > 0 && SacrificeCooldown <= 0)
        {
            if (isSacrificing && (NPCTarget == null || !NPCTarget.active))
            {
                isSacrificing = false;
            }

            if (NPCTarget == null)
            {
                if (!RitualSystem.BuffedNPCs.Contains(Sacrifices[0]) && Sacrifices[0].active)
                {
                    NPCTarget = Sacrifices[0];
                }
                else
                {
                    NPCTarget = null;
                }

                return;
            }

            if (!NPCTarget.active)
            {
                NPCTarget = null;

                return;
            }

            if (NPCTarget.type == ModContent.NPCType<FleshlingCultist.FleshlingCultist>())
            {
                var d = NPCTarget.ModNPC as FleshlingCultist.FleshlingCultist;
                d.CurrentState = FleshlingCultist.FleshlingCultist.Behaviors.WillingSacrifice;
            }

            if (RitualSystem.BuffedNPCs.Contains(NPCTarget))
            {
                return;
            }

            if (Vector2.Distance(NPCTarget.Center, NPC.Center) > 100f)
            {
                NPC.velocity.X = NPC.AngleTo(NPCTarget.Center).ToRotationVector2().X * SpeedMulti * MathF.Tanh(Vector2.Distance(NPC.Center, Main.MouseWorld));
            }

            if (Vector2.Distance(NPCTarget.Center, NPC.Center) < 100f && NPCTarget.active && !NPCTarget.boss)
            {
                var a = NPCTarget.GetGlobalNPC<SacrificeNPC>();

                if (!a.isSacrificed)
                {
                    a.isSacrificed = true;
                    a.Priest = this;
                    isSacrificing = true;
                    SoundEngine.PlaySound(SoundID.Item3, NPC.position);
                }
            }
        }

        else
        {
            if (playerTarget == null)
            {
                playerTarget = Main.player[NPC.FindClosestPlayer()];
            }

            if (!isSacrificing)
            {
                NPC.velocity.X = NPC.AngleTo(playerTarget.Center).ToRotationVector2().X * SpeedMulti * MathF.Tanh(Vector2.Distance(NPC.Center, Main.MouseWorld));
            }
        }
    }

    private void WalkTowardsPlayer()
    {
        if (playerTarget == null)
        {
            playerTarget = Main.player[NPC.FindClosestPlayer()];
        }

        NPC.velocity.X = NPC.AngleTo(playerTarget.Center).ToRotationVector2().X * SpeedMulti * MathF.Tanh(Vector2.Distance(NPC.Center, Main.MouseWorld));

        if (Sacrifices.Count > 0)
        {
            currentAIState = AltarAI.LookingForSacrifice;
        }
    }

    #endregion
}