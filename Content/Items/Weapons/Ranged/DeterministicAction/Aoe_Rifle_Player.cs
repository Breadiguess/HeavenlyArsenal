namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.DeterministicAction
{
    internal class Aoe_Rifle_Player : ModPlayer
    {

       

        public int BulletCount 
        {
            get => Player.HeldItem.GetGlobalItem<Aoe_Rifle_ClipItem>().TotalBullets;
            set;
        }

        //hitting shots with rifle builds authority
        // spend authority to ???

        // WOUND THE WORLD, TEAR DOWN THEIR PRECIOUS STARS AND SNUFF THE LIGHT FROM THEIR EYES
        public const int MAX_AUTHORITY_TIMER = 60 * 7;
        public bool Active
        {
            get => Player.HeldItem.type == ModContent.ItemType<Aoe_Rifle_Item>();
        }
        /// <summary>
        /// Resource
        /// </summary>
        public int Authority { get; set; }

        public const int MAX_AUTHORITY = 8;
        /// <summary>
        /// timer for when authority starts to diminish
        /// </summary>
        public int AuthorityTimer { get; set; }
        public int AuthorityGainCooldown = 0;
        public override void PostUpdateMiscEffects()
        {
            if (!Active)
                return;

            if (AuthorityTimer > 0)
                AuthorityTimer--;
            if (AuthorityTimer <= 0 && Authority > 0)
            {
                Authority--;
                AuthorityTimer = MAX_AUTHORITY_TIMER;
            }


            


        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.type == ModContent.ProjectileType<Aoe_Rifle_Laser>() && Active && AuthorityGainCooldown <=0)
            {
                Authority = Math.Min(Authority + 1, MAX_AUTHORITY);
                AuthorityTimer = MAX_AUTHORITY_TIMER;
                AuthorityGainCooldown = 60;

            }

        }

        public override void ResetEffects()
        {
            if (!Active) { 
                Authority = 0;
                AuthorityTimer = MAX_AUTHORITY_TIMER;
            }
            if(AuthorityGainCooldown>0)
            AuthorityGainCooldown--;
        }




        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            base.SyncPlayer(toWho, fromWho, newPlayer);
        }
        public override void SendClientChanges(ModPlayer clientPlayer)
        {
            base.SendClientChanges(clientPlayer);
        }
        public override void CopyClientState(ModPlayer targetCopy)
        {
            base.CopyClientState(targetCopy);
        }



    }
}
