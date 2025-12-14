using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Leech;
using Hjson;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Newtonsoft.Json.Linq;
using NoxusBoss.Core.DialogueSystem;
using NoxusBoss.Core.Graphics.UI.SolynDialogue;
using NoxusBoss.Core.SolynEvents;
using NoxusBoss.Core.World.GameScenes.RiftEclipse;
using Terraria.GameContent.Events;
//using HeavenlyArsenal.Content.Items.Accessories.Nightfall;

namespace HeavenlyArsenal.Core.Systems;

public class SolynDialogueAddon : ModSystem
{
    private ILHook _selectPlayerResponseHook;

    public override void Load()
    {
        var target = typeof(SolynDialogUIManager)
            .GetMethod("SelectPlayerResponse", BindingFlags.Instance | BindingFlags.NonPublic);

        if (target != null)
        {
            _selectPlayerResponseHook = new ILHook(target, Patch_SelectPlayerResponse);
        }
    }

    private void Patch_SelectPlayerResponse(ILContext il)
    {
        var c = new ILCursor(il);

        if (c.TryGotoNext
            (
                MoveType.Before,
                i => i.OpCode == OpCodes.Ret
            ))
        {
            // REMOVE the ret instruction
            c.Remove();
        }
    }

    public override void PostSetupContent()
    {
        CommentOnBloodMoon();
        TestDialogue();
        TestNPCDialogue();
        //ChiUpgradeDialogue();
        RiftEclipsePartyDialogue();
    }

    #region dialogue

    private static void CommentOnBloodMoon()
    {
        var conv = DialoguePatchFactory.BuildAndRegisterFromMod
        (
            "SolynCommentOnRiftBloodMoon",
            "Start"
        );

        conv.LinkChain("Start", "Solyn1", "Player1", "Solyn2", "Solyn3", "Solyn4");
        conv.WithAppearanceCondition(conv => Main.bloodMoon && RiftEclipseManagementSystem.RiftEclipseOngoing);
        conv.MakeFallback(3);
    }

    private static void TestDialogue()
    {
        var conv2 = DialoguePatchFactory.BuildAndRegisterFromMod("SolynBossActiveDialogue", "Start");

        conv2.WithAppearanceCondition(c => Main.npc.Any(n => n.active && n.boss))
            .LinkChain("Start", "Solyn1", "Player1", "Solyn2");

        conv2.MakeSpokenByPlayer("Player1");
        conv2.MakeFallback(5).WithRerollCondition(conversation => !conversation.AppearanceCondition());
    }

    private static void TestNPCDialogue()
    {
        var conv3 = DialoguePatchFactory.BuildAndRegisterFromMod("SolynUmbralLeechDialogue", "Start");

        conv3.WithAppearanceCondition
            (
                c =>
                {
                    var leechType = ModContent.NPCType<newLeech>();

                    return Main.npc.Any(n => n.active && n.type == leechType);
                }
            )
            .LinkChain("Start", "Player1", "Solyn1");

        conv3.MakeSpokenByPlayer("Player1").WithRerollCondition(conversation => !conversation.AppearanceCondition());
        conv3.MakeFallback(6);
    }

    /*
    static void ChiUpgradeDialogue()
    {


        var conv4 = DialoguePatchFactory.BuildAndRegisterFromMod("SolynChiUpgradeEquipped", "Start");
        conv4.WithAppearanceCondition(c =>
        {
            var player = Main.LocalPlayer;
            if (player.GetModPlayer<NightfallPlayer>().NightfallActive)
                return true;
            return false;
        })
        .LinkChain("Start", "Player1", "Solyn1", "Solyn2", "Solyn3", "Player2", "Solyn4");
        conv4.MakeSpokenByPlayer("Player1", "Player2");
        conv4.WithRerollCondition(conversation => !conversation.AppearanceCondition());
        conv4.LinkFromStartToFinish();

        conv4.MakeFallback(2);
    }*/
    public static void AffirmationDialogue()
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
        conv5.LinkChain("Start", "Solyn1", "Solyn2", "Player1", "Solyn3");
        conv5.LinkFromStartToFinishExcluding("Solyn3").WithRerollCondition(conversation => !conversation.AppearanceCondition());
    }

    private static void RiftEclipsePartyDialogue()
    {
        var conv6 = DialoguePatchFactory.BuildAndRegisterFromMod("SolynRiftFirstPartyDialogue", "Start");

        conv6.WithAppearanceCondition
            (
                c =>
                {
                    var player = Main.LocalPlayer;

                    if (RiftEclipseManagementSystem.RiftEclipseOngoing && BirthdayParty.PartyIsUp)
                    {
                        return true;
                    }

                    return false;
                }
            )
            .LinkChain("Start", "Solyn1", "Solyn2", "Player1", "Solyn3", "Solyn4");

        conv6.MakeSpokenByPlayer("Player1");
        conv6.WithRerollCondition(conversation => !conversation.AppearanceCondition());
        conv6.MakeFallback(30);
    }

    #endregion
}

public static class DialoguePatchFactory
{
    // Build a Conversation sourced from Mods.<modName>.Solyn.hjson
    // then basically trick NoxusBoss into accepting it.
    public static Conversation BuildAndRegisterFromMod
    (
        string relativePrefix,
        string rootNodeKey,
        string modName = "HeavenlyArsenal"
    )
    {
        // override with base 
        var filePath = $"Localization/en-US/Mods.{modName}.Solyn.hjson";
        var src = ModLoader.GetMod(modName) ?? throw new Exception($"Mod {modName} not found.");

        if (!src.FileExists(filePath))
        {
            throw new Exception($"Missing localization: {filePath}");
        }

        var raw = Encoding.UTF8.GetString(src.GetFileBytes(filePath));
        var json = HjsonValue.Parse(raw).ToString();
        var doc = JObject.Parse(json);

        // build the fully-qualified prefix used for parsing the RIGHT FILE NOT THE BUILT IN ONE, MY FILE HAHAHAHA file
        var LocalizationPath = $"Mods.{modName}.Solyn.{relativePrefix}.";
        var possible = new Dictionary<string, Dialogue>();

        foreach (var t in doc.SelectTokens("$..*"))
        {
            if (t.HasValues)
            {
                continue;
            }

            if (t is JObject o && o.Count == 0)
            {
                continue;
            }

            // reconstruct path (supports "x.y" keys)
            var path = "";
            var cur = t;

            for (JToken p = t.Parent!; p != null; p = p.Parent!)
            {
                path = p switch
                {
                    JProperty prop => prop.Name + (path == "" ? "" : "." + path),
                    JArray arr => arr.IndexOf(cur) + (path == "" ? "" : "." + path),
                    _ => path
                };

                cur = p;
            }

            path = path.Replace(".$parentVal", string.Empty);
            var fqPath = $"Mods.{modName}.Solyn.{path}.";

            if (!fqPath.StartsWith(LocalizationPath, StringComparison.Ordinal))
            {
                continue;
            }

            var textKey = fqPath.TrimEnd('.');
            // Convert to RELATIVE key by stripping full prefix -> leaves "Start", "Solyn1", etc.
            var relKey = textKey.Substring(LocalizationPath.Length).Trim('.');
            possible[relKey] = new Dialogue(textKey);
        }

        if (!possible.ContainsKey(rootNodeKey))
        {
            throw new Exception($"Root '{rootNodeKey}' not found under {LocalizationPath}");
        }

        //deprecated but works
        var tree = (DialogueTree)FormatterServices.GetUninitializedObject(typeof(DialogueTree));
        var conv = (Conversation)FormatterServices.GetUninitializedObject(typeof(Conversation));

        //Change readonly fields.... agony. lots of it.
        var fiTreePossible = typeof(DialogueTree).GetField("PossibleDialogue", BindingFlags.Public | BindingFlags.Instance);
        var fiTreeRoot = typeof(DialogueTree).GetField("Root", BindingFlags.Public | BindingFlags.Instance);
        var fiConvTree = typeof(Conversation).GetField("Tree", BindingFlags.Public | BindingFlags.Instance);
        //FieldInfo fiAppear = typeof(Conversation).GetProperty("AppearanceCondition")!.GetSetMethod(true);
        //FieldInfo fiReroll = typeof(Conversation).GetProperty("RerollCondition")!.GetSetMethod(true);
        //FieldInfo fiRootSel = typeof(Conversation).GetProperty("RootSelectionFunction")!.GetSetMethod(true);
        fiTreePossible.SetValue(tree, possible);
        fiTreeRoot.SetValue(tree, possible[rootNodeKey]);

        // assign Conversation internals
        fiConvTree.SetValue(conv, tree);
        typeof(Conversation).GetProperty("AppearanceCondition")!.SetValue(conv, () => true);
        typeof(Conversation).GetProperty("RerollCondition")!.SetValue(conv, () => false);
        typeof(Conversation).GetProperty("RootSelectionFunction")!.SetValue(conv, () => tree.Root);

        // register under the namespace wotg expects
        var managerKey = $"Mods.NoxusBoss.Solyn.{relativePrefix}.";
        // dictionary is mutable even if it is readonly, thankfully
        DialogueManager.Conversations[managerKey] = conv;

        return conv;
    }
}