using CalamityMod;
using HeavenlyArsenal.Common;
using HeavenlyArsenal.Core.Systems;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Artillery_Crab
{
    public partial class BloodCrab
    {

       

        public override void ModifyHitByItem(Player player, Item item, ref NPC.HitModifiers modifiers)
        {


            Vector2 shellNormal = -Vector2.UnitY;
            Vector2 impactDir = (player.Center - NPC.Center).SafeNormalize(Vector2.Zero);
            float dot = Vector2.Dot(impactDir, shellNormal);
            const float shellThresholdStart = 0.01f;

            if (dot > shellThresholdStart)
            {
                modifiers.FinalDamage *= 0f;
                modifiers.HideCombatText();
                modifiers.DisableCrit();
                if(item.type != ItemID.SlapHand)
                modifiers.DisableKnockback();
                NPC.life++;
                NPC.ForceNetUpdate();
            }
        }
        public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            Player attacker = Main.player[projectile.owner];
            if (attacker == null || !attacker.active)
                return;

            //fuck you virid vangaurd oyu piece of shit why is it that your contact is the exact same place as THE FUCKING NPC CENTER SO IT TURNS INTO ZERO AND MAKES MY LIFE DIFFICULT

            Vector2 contact = projectile.Hitbox.ClosestPointInRect(NPC.Center);

            Vector2 playerImpact =
                (NPC.Center - attacker.Center).SafeNormalize(Vector2.Zero);
            Vector2 impactDir =
                (contact - NPC.Center).SafeNormalize(Vector2.Zero);

            float dot = Vector2.Dot(impactDir, -Vector2.UnitY);
            //TODO: a few things to take into consideration.
            // 1. if the projectile is a whip, alsocheck the location of the player and allow it to hit so long as the player is withthin the threshold.
            //2. same for held projectiles.
            //3. if the weapon is a minion, take reduced damage if it strikes the underside.
            bool isWhip = ProjectileID.Sets.IsAWhip[projectile.type] || projectile.DamageType == DamageClass.MeleeNoSpeed || attacker.heldProj == projectile.whoAmI;


            if (projectile.IsMinionOrSentryRelated)
            {

                modifiers.FinalDamage *= 0.72f;
                modifiers.Knockback *= 0.3f;

            }
            //GO FUCK YOURSELF LASERSSS
            if( isWhip || attacker.Distance(projectile.Center )< 120)
            {
                //here we'reassuming that this is evil and being spawned inside, so we're just gonna ask for the dot of the player and replace eit with this one.

                if (attacker.active)
                {

                    float playerDot =
                        Vector2.Dot(playerImpact, -Vector2.UnitY);

                    dot = -playerDot;

                }
            }



            //Main.NewText(dot);
            //RayCastVisualizer.Raycasts.Add(new(NPC.Center, NPC.Center + impactDir * 1200, Color.White, 30));

            const float shellThresholdStart = -0.5f;
            if (dot > shellThresholdStart)
            { 
                modifiers.FinalDamage *= 0f;
                modifiers.HideCombatText();
                modifiers.DisableCrit();
                modifiers.DisableKnockback();
                NPC.life++;
                NPC.ForceNetUpdate();
            }
            modifiers.ScalingArmorPenetration = AddableFloat.Zero+ 0.4f;
            modifiers.DefenseEffectiveness = MultipliableFloat.One;

            //Main.NewText(modifiers.DefenseEffectiveness.Value);

        }
        public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
        {


        }
    }
}
