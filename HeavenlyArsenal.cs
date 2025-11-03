global using LumUtils = Luminance.Common.Utilities.Utilities;
global using WotGUtils = NoxusBoss.Core.Utilities.Utilities;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Enemy;
using CalamityMod.UI.CalamitasEnchants;
//using HeavenlyArsenal.Content.Items.Accessories.VoidCrestOath;

using HeavenlyArsenal.Content.Items.Accessories.VoidCrestOath;
using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Leech;
using Microsoft.Xna.Framework;

//using HeavenlyArsenal.Content.Items.Misc;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.RuntimeDetour;
using MonoMod.RuntimeDetour.HookGen;
using NoxusBoss.Content.Items;
using NoxusBoss.Content.NPCs.Bosses.CeaselessVoid;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal
{
    // Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
    public class HeavenlyArsenal : Mod
    {
        
        public static bool forceOpenRift = false;
        public override void Load()
        {
            EnchantmentManager.ItemUpgradeRelationship[ModContent.ItemType<MetallicChunk>()] = ModContent.ItemType<VoidCrestOath>();
            /*
            if (ModLoader.GetMod("NoxusBoss") != null)
            {
                // Replace the following line:  
                // Type riftType = ModContent.GetModNPC<CeaselessVoidRift>().Type;  

                // With this corrected line:  
                Type riftType = ModContent.NPCType<CeaselessVoidRift>();
                if (riftType != null)
                {
                    PropertyInfo prop = riftType.GetProperty("CanEnterRift", BindingFlags.Public | BindingFlags.Static);
                    MethodInfo getter = prop?.GetGetMethod();
                    // Type forceRiftOpen = prop.PropertyType;
                    //MethodInfo detourMethod =  forceOpenRift.get
                    if (getter != null)
                    {

                        Hook hook = new Hook(getter, new Func<Func<bool>, bool>(CanEnterRift_Hook));
                        //HookEndpointManager.Add(getter, hook);
                    }
                }
            
            */


            On_Projectile.Damage += MultisegmentCollideEnabler;
            On_Projectile.CanHitWithMeleeWeapon += MultisegmentCheckSetter;
            On_Projectile.Colliding += ExtraHitboxCollide;
        }
        public static IMultiSegmentNPC CurrentMultiSegmnetNPC = null;
        public static bool MultiSegmentEnabler = false;
        public static void MultisegmentCollideEnabler(On_Projectile.orig_Damage orig, Projectile self)
        {
            if (self.owner == Main.myPlayer && self.type != ModContent.ProjectileType<SulphuricAcidBubble>())
            {
                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (npc.ModNPC is IMultiSegmentNPC multisegmentguy)
                    {
                        if (self.friendly && CombinedHooks.CanHitNPCWithProj(self, npc) is not false)
                        {
                            ref List<ExtraNPCSegment> extrahitboxes = ref multisegmentguy.ExtraHitBoxes();
                            for (int i = 0; i < extrahitboxes.Count; i++)
                            {
                                //if (self.Distance(extrahitboxes[i].Hitbox.Center()) < 100)
                                //    Main.NewText(self.ToString());
                                if (extrahitboxes[i].Active)
                                    if (extrahitboxes[i].UniqueIframes && extrahitboxes[i].ProjectileCollide && extrahitboxes[i].ImmuneTime <= 0)
                                    {
                                       
                                        if (extrahitboxes[i].Hitbox.IntersectsConeFastInaccurate(self.Center, 100, 0, 360) )
                                        {
                                           // Main.NewText("slap that bitchass mf");

                                            multisegmentguy.OnHitBoxCollide(i, self);
                                            extrahitboxes[i].ImmuneTime = extrahitboxes[i].Immunity;
                                        }
                                    }
                            }
                        }
                    }
                }
            }
            MultiSegmentEnabler = true;
            orig(self);
            MultiSegmentEnabler = false;
        }
        public static bool MultisegmentCheckSetter(On_Projectile.orig_CanHitWithMeleeWeapon orig, Projectile Self, Entity entity)
        {
            if (MultiSegmentEnabler)
                if (entity is NPC npc && npc.ModNPC is IMultiSegmentNPC multi)
                    CurrentMultiSegmnetNPC = multi;
            return orig(Self, entity);
        }
        public static bool ExtraHitboxCollide(On_Projectile.orig_Colliding orig, Projectile self, Rectangle myRect, Rectangle targetRect)
        {
            bool result = orig(self, myRect, targetRect);

            // Find which NPC this targetRect belongs to.
            NPC targetNPC = null;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.active && npc.Hitbox == targetRect)
                {
                    targetNPC = npc;
                    break;
                }
            }

            // Only run if it’s one of *your* NPCs.
            if (targetNPC?.ModNPC is IMultiSegmentNPC multi)
            {
                ref var extraHitboxes = ref multi.ExtraHitBoxes();

                for (int i = 0; i < extraHitboxes.Count; i++)
                {
                    var box = extraHitboxes[i];
                    if (!box.Active || !box.ProjectileCollide)
                        continue;

                    if (myRect.Intersects(box.Hitbox))
                    {
                        result = true;
                        multi.OnHitBoxCollide(i, self);
                    }
                }
            }

            return result;
        }

        public static void ClearAllBuffs(NPC npc)
        {
            for (int i = 0; i < NPC.maxBuffs; i++)
            {
                
                if (npc.buffType[i] > 0)
                {
                    Main.NewText(npc.ToString() + $", {npc.FindBuffIndex(i).ToString()} ");
                    npc.DelBuff(i);
                    i--;
                }
            }
        }


        public override void PostSetupContent()
        {
           
        }


        private static bool CanEnterRift_Hook(Func<bool> orig)
        {
            if (forceOpenRift)
            {
                return true; 
            }
            return true;//orig();  
        }
       
    }
}
