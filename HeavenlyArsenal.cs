global using LumUtils = Luminance.Common.Utilities.Utilities;
global using WotGUtils = NoxusBoss.Core.Utilities.Utilities;
using System.Runtime.CompilerServices;
using CalamityMod.Projectiles.Enemy;
using CalamityMod.UI.CalamitasEnchants;
using HeavenlyArsenal.Common;
using HeavenlyArsenal.Content.Items.Accessories.VoidCrestOath;
using NoxusBoss.Content.Items;

//using HeavenlyArsenal.Content.Items.Accessories.VoidCrestOath;

//using HeavenlyArsenal.Content.Items.Misc;

namespace HeavenlyArsenal;

// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
public class HeavenlyArsenal : Mod
{
    public static class MultiSegmentNPCIterator
    {
        /// <summary>
        ///     Iterates ONLY NPCs whose ModNPC implements IMultiSegmentNPC.
        /// </summary>
        public static IMultiSegIterator<NPC> All
            => new(Main.npc.AsSpan(0, Main.maxNPCs));
    }

    public readonly ref struct IMultiSegIterator<T> where T : NPC
    {
        private readonly ReadOnlySpan<T> span;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IMultiSegIterator(ReadOnlySpan<T> span)
        {
            this.span = span;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(span.GetEnumerator());
        }

        public ref struct Enumerator
        {
            private ReadOnlySpan<T>.Enumerator enumerator;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Enumerator(ReadOnlySpan<T>.Enumerator enumerator)
            {
                this.enumerator = enumerator;
            }

            public T Current
                => enumerator.Current;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                while (enumerator.MoveNext())
                {
                    var npc = enumerator.Current;

                    if (npc.active && npc.ModNPC is IMultiSegmentNPC)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }

    public static bool forceOpenRift = false;

    public static IMultiSegmentNPC CurrentMultiSegmnetNPC;

    public static bool MultiSegmentEnabler;

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

        //On_NPC.AI 
        On_Projectile.Damage += MultisegmentCollideEnabler;
        On_Projectile.CanHitWithMeleeWeapon += MultisegmentCheckSetter;
        On_Projectile.Colliding += ExtraHitboxCollide;
    }

    public static void MultisegmentCollideEnabler(On_Projectile.orig_Damage orig, Projectile self)
    {
        if (self.owner == Main.myPlayer && self.type != ModContent.ProjectileType<SulphuricAcidBubble>())
        {
            foreach (var npc in MultiSegmentNPCIterator.All)
            {
                if (npc.ModNPC is IMultiSegmentNPC multisegmentguy)
                {
                    if (self.friendly && CombinedHooks.CanHitNPCWithProj(self, npc) is not false)
                    {
                        ref var extrahitboxes = ref multisegmentguy.ExtraHitBoxes();

                        for (var i = 0; i < extrahitboxes.Count; i++)
                        {
                            //if (self.Distance(extrafhitboxes[i].Hitbox.Center()) < 100)
                            //    Main.NewText(self.ToString());
                            if (extrahitboxes[i].Active)
                            {
                                if (extrahitboxes[i].UniqueIframes && extrahitboxes[i].ProjectileCollide && extrahitboxes[i].ImmuneTime <= 0)
                                {
                                    if (extrahitboxes[i].Hitbox.IntersectsConeFastInaccurate(self.Center, 100, 0, 360))
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
        }

        MultiSegmentEnabler = true;
        orig(self);
        MultiSegmentEnabler = false;
    }

    public static bool MultisegmentCheckSetter(On_Projectile.orig_CanHitWithMeleeWeapon orig, Projectile Self, Entity entity)
    {
        if (MultiSegmentEnabler)
        {
            if (entity is NPC npc && npc.ModNPC is IMultiSegmentNPC multi)
            {
                CurrentMultiSegmnetNPC = multi;
            }
        }

        return orig(Self, entity);
    }

    public static bool ExtraHitboxCollide(On_Projectile.orig_Colliding orig, Projectile self, Rectangle myRect, Rectangle targetRect)
    {
        var result = orig(self, myRect, targetRect);

        // Find which NPC this targetRect belongs to.
        NPC targetNPC = null;

        foreach (var npc in MultiSegmentNPCIterator.All)
        {
            if (npc.Hitbox == targetRect)
            {
                targetNPC = npc;

                break;
            }
        }

        if (targetNPC?.ModNPC is IMultiSegmentNPC multi)
        {
            ref var extraHitboxes = ref multi.ExtraHitBoxes();

            for (var i = 0; i < extraHitboxes.Count; i++)
            {
                var box = extraHitboxes[i];

                if (!box.Active || !box.ProjectileCollide)
                {
                    continue;
                }

                var canDamage = true;

                if (self.ModProjectile is { } modProj2)
                {
                    var modCanDamage = modProj2.Colliding(myRect, box.Hitbox);

                    if (modCanDamage.HasValue && !modCanDamage.Value)
                    {
                        canDamage = false;
                    }
                }

                if (self.WhipPointsForCollision.Count > 0)
                {
                    for (var x = 0; x < self.WhipPointsForCollision.Count; x++)
                    {
                        if (self.WhipPointsForCollision[x].Distance(box.Hitbox.Center()) > 20)
                        {
                            continue;
                        }

                        //Rectangle whip = new Rectangle((int)self.WhipPointsForCollision[x].X, (int)self.WhipPointsForCollision[x].Y, 30, 30);
                        //Main.NewText($"{x}, whip: {whip.Center()}, target: {targetRect.Center}");
                        //Dust.NewDustPerfect(self.WhipPointsForCollision[x], DustID.Cloud, Vector2.Zero);
                        if (box.Hitbox.IntersectsConeFastInaccurate(self.WhipPointsForCollision[x], 20, 0, MathHelper.TwoPi))
                        {
                            //for(int y = 0; y < 40;y++)
                            // {
                            //     Vector2 pos = Vector2.Lerp(self.WhipPointsForCollision[x], box.Hitbox.Center(), y/40f);
                            //     Dust a = Dust.NewDustPerfect(pos, DustID.Blood, Vector2.Zero, 0, Color.Red);
                            //     a.noGravity = true;
                            //     a.scale = 3;
                            // }
                            //Main.NewText(self.ToString());
                            result = true;
                            multi.OnHitBoxCollide(i, self);
                        }
                    }
                }

                if (myRect.Intersects(box.Hitbox) && canDamage)
                {
                    result = true;
                    multi.OnHitBoxCollide(i, self);
                }
            }
        }

        return result;
    }

    public static void TryAddModIngredient(Recipe recipe, string modName, string itemName)
    {
        if (ModLoader.TryGetMod(modName, out var mod))
        {
            var item = mod.Find<ModItem>(itemName);

            if (item != null)
            {
                recipe.AddIngredient(item.Type);
            }
        }
    }

    private static bool CanEnterRift_Hook(Func<bool> orig)
    {
        if (forceOpenRift)
        {
            return true;
        }

        return true; //orig();  
    }
}