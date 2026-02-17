using HeavenlyArsenal.Common.Players;
using NoxusBoss.Core.DialogueSystem;
using NoxusBoss.Core.SolynEvents;

namespace HeavenlyArsenal.Core.Systems.SolynEvents;

internal class SolynAffirmsYou : SolynEvent
{
    public override int TotalStages => 1;

    public static bool CanStart => Main.LocalPlayer.GetModPlayer<TransgenderPlayer>().Enabled;

    private static string Prefix => "SolynGenderChangePotionUsed";

    public override void OnModLoad()
    {
        var conv5 = DialoguePatchFactory.BuildAndRegisterFromMod("SolynGenderChangePotionUsed", "Start");

        conv5.WithAppearanceCondition
        (
            c =>
            {
                var player = Main.LocalPlayer;

                if (!ModContent.GetInstance<SolynIntroductionEvent>().Finished && ModContent.GetInstance<StargazingEvent>().Finished)
                {
                    return true;
                }

                return false;
            }
        );

        conv5.MakeSpokenByPlayer("Player1");
        conv5.LinkChain("Start", "Solyn1", "Solyn2", "Player1");

        conv5
            .WithRerollCondition(conversation => !conversation.AppearanceCondition());

        ConversationSelector.PriorityConversationSelectionEvent += SelectHoverDialogue;
    }

    private Conversation SelectHoverDialogue()
    {
        if (!Finished && CanStart)
        {
            return DialogueManager.FindByRelativePrefix(Prefix);
        }

        return null;
    }

    public override void PostUpdateNPCs()
    {
        if (Solyn is null)
        {
            return;
        }

        //sloppy, but i couldn't figure out how to make it start.
        if (DialogueManager.FindByRelativePrefix(Prefix).SeenBefore("Solyn2"))
        {
            SafeSetStage(1);
        }
    }
}