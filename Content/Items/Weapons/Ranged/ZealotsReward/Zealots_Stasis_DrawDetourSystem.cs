using InfernumMode.Core.OverridingSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.ZealotsReward
{
    internal sealed class Zealots_Stasis_DrawDetourSystem : ModSystem
    {
        private static MethodInfo drawNPCDirectInnerMethod;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            drawNPCDirectInnerMethod = typeof(Main).GetMethod(
                "DrawNPCDirect_Inner",
                BindingFlags.Instance | BindingFlags.NonPublic);

            if (drawNPCDirectInnerMethod is null)
                throw new Exception("Could not find Main.DrawNPCDirect_Inner");

            On_Main.DrawNPCDirect += DrawNPCDirectHook;


            On_NPC.FindFrame += FindFrameHook;
        }

        private void FindFrameHook(On_NPC.orig_FindFrame orig, NPC self)
        {
           if(self.TryGetGlobalNPC<Zealots_Stasis_NPC>(out var stasis))
            {
                if(stasis.IsFrozen)
                return;
            }

            orig(self);
        }
        public override void Unload()
        {
            if (Main.dedServ)
                return;

            On_Main.DrawNPCDirect -= DrawNPCDirectHook;
            drawNPCDirectInnerMethod = null;
        }

        private void DrawNPCDirectHook(On_Main.orig_DrawNPCDirect orig, Main self, SpriteBatch spriteBatch, NPC npc, bool behindTiles, Vector2 screenPos)
        {
            if (!npc.active || !npc.TryGetGlobalNPC(out Zealots_Stasis_NPC stasis))
            {
                orig(self, spriteBatch, npc, behindTiles, screenPos);
                return;
            }

            bool shouldUseFrozenPass = stasis.DrawToFrozenRT;

            // Normal world pass: suppress the regular draw for frozen NPCs.
            if (shouldUseFrozenPass && !Zealots_Stasis_System.DrawingFrozenTarget)
                return;

            // Frozen RT pass: draw them with the normal NPC pipeline.
            if (shouldUseFrozenPass && Zealots_Stasis_System.DrawingFrozenTarget)
            {
               // orig(self, spriteBatch, npc, behindTiles, screenPos);
                DrawNPCDirectWithoutPostDraw(self, spriteBatch, npc, behindTiles, screenPos);
                return;
            }

            orig(self, spriteBatch, npc, behindTiles, screenPos);
        }

        private static void DrawNPCDirectWithoutPostDraw(Main self, SpriteBatch spriteBatch, NPC npc, bool behindTiles, Vector2 screenPos)
        {
            try
            {
                Color npcColor = npc.GetAlpha(Lighting.GetColor((int)npc.Center.X / 16, (int)npc.Center.Y / 16));

                NPCLoader.DrawEffects(npc, ref npcColor);

                if (NPCLoader.PreDraw(npc, spriteBatch, screenPos, npcColor))
                {
                    object[] args =
                    {
                        spriteBatch,
                        npc,
                        behindTiles,
                        screenPos,
                        npcColor
                    };

                    drawNPCDirectInnerMethod.Invoke(self, args);
                    npcColor = (Color)args[4];
                }
            }
            catch
            {
            }
        }
    }
}