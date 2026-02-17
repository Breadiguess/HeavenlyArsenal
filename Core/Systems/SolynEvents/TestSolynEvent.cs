using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using NoxusBoss.Content.NPCs.Friendly;
using NoxusBoss.Core.DialogueSystem;
using NoxusBoss.Core.SolynEvents;
using Terraria;

namespace HeavenlyArsenal.Core.Systems.SolynEvents
{

    public class SolynHoverEvent : SolynEvent
    {
        public override int TotalStages => 3;

        private int hoverTimer;


        public static bool CanStart => Main.dayTime && !Main.raining;

        public override void OnModLoad()
        {
            var conv = DialoguePatchFactory.BuildAndRegisterFromMod(
                relativePrefix: "SolynHoverEventDialogue", rootNodeKey: "Start");
            conv.WithAppearanceCondition(instance => CanStart).LinkFromStartToFinish();
            conv.WithRerollCondition(instance => !instance.AppearanceCondition());
            conv.LinkChain("Start", "Solyn1", "Solyn2", "Solyn3");

            ConversationSelector.PriorityConversationSelectionEvent += SelectHoverDialogue;
        }

        private Conversation SelectHoverDialogue()
        {
            if (!Finished && CanStart)
                return DialogueManager.FindByRelativePrefix("SolynHoverEventDialogue");
            return null;
        }

        public override void PostUpdateNPCs()
        {
            if (Solyn is null)
                return;

            NPC npc = Solyn.NPC;
            Player player = Main.player[Player.FindClosest(npc.Center, 1, 1)];

            // Stage 0: Waiting for conversation
            if (Started && DialogueManager.FindByRelativePrefix("SolynHoverEventDialogue").SeenBefore("Solyn2"))
            {
                SafeSetStage(1);
                hoverTimer = 0;
            }

            // Stage 1: Hovering above player
            if (Stage == 1 && !Finished)
            {
                Solyn.SwitchState(SolynAIType.PuppeteeredByQuest);
                Solyn.CanBeSpokenTo = false;
                npc.noGravity = true;

                // Target hover position above player’s head
                Vector2 targetPos = player.Center + new Vector2(0f, -90f);
                npc.Center = Vector2.Lerp(npc.Center, targetPos, 0.05f);
                npc.velocity *= 0.9f;
                npc.spriteDirection = (player.Center.X - npc.Center.X).NonZeroSign();

                hoverTimer++;
                if (hoverTimer % 60 == 0)
                    Solyn.UseStarFlyEffects();

                // After 10 seconds (600 ticks), land
                if (hoverTimer >= 600)
                {
                    SafeSetStage(2);
                }
            }

            // Stage 2: Landing / ending event
            if (Stage == 2 && !Finished)
            {
                Solyn.SwitchState(SolynAIType.WaitToTeleportHome);
                npc.noGravity = false;
                Solyn.CanBeSpokenTo = true;
                SafeSetStage(TotalStages);
            }
        }


    }


}
