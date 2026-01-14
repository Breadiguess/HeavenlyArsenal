using HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Summoner.Thralls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Summoner
{
    public class SummonerBloodController : IBloodConstructController
    {
        private readonly BloodBlightParasite_Player symbiote;
        private readonly Player player;
        public SummonerBloodController(BloodBlightParasite_Player symbiote)
        {
            this.symbiote = symbiote;
            this.player = symbiote.Player;
            foreach (ThrallType type in Enum.GetValues(typeof(ThrallType)))
                thrallsByType[type] = new HashSet<int>();
        }
        
        public readonly Dictionary<ThrallType, HashSet<int>> thrallsByType = new();
        public readonly Dictionary<ThrallType, int> maxPerType = new();

        // Limits
        private int maxThralls = 0;

        // State
        private BloodBand currentBand;
        private bool ascended;

        public bool overmindActive;
        int overmindID;

        #region helpers
        private bool CanSpawn(ThrallType type)
        {
            return thrallsByType[type].Count < maxPerType.GetValueOrDefault(type, 0);
        }


        private void SpawnThrall(ThrallType type)
        {
            if (!CanSpawn(type))
                return;

            if (player.whoAmI != Main.myPlayer)
                return;

            Vector2 spawnPos = player.Center + Main.rand.NextVector2Circular(40f, 40f);

            int proj = Projectile.NewProjectile(
                player.GetSource_FromThis(),
                spawnPos,
                Vector2.Zero,
                ThrallTypeToProjectile(type),
                symbiote.GetThrallDamage(),
                0f,
                player.whoAmI
            );

            thrallsByType[type].Add(proj);
        }


        private void CleanupDeadThralls()
        {
            foreach (var kv in thrallsByType)
            {
                kv.Value.RemoveWhere(id =>
                    !Main.projectile.IndexInRange(id) ||
                    !Main.projectile[id].active);
            }
        }


        private void FormOvermind()
        {
            if (overmindActive)
                return;

            overmindActive = true;

            if (player.whoAmI == Main.myPlayer)
            {
                overmindID = Projectile.NewProjectile(
                    player.GetSource_FromThis(),
                    player.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<BloodOvermind>(),
                    0,
                    0f,
                    player.whoAmI
                );
            }
        }


        private static int ThrallTypeToProjectile(ThrallType type)
        {
            return type switch
            {
                ThrallType.BasicThrall0 => ModContent.ProjectileType<BloodThrallProjectile>(),   // example
                //ThrallType.BasicThrall1 => ModContent.ProjectileType<BloodThrallProjectile1>(),  // example
                //ThrallType.BasicThrall2 => ModContent.ProjectileType<BloodThrallProjectile2>(),  // example
                //ThrallType.WingedThrall => ModContent.ProjectileType<BloodWingedThrallProjectile>(),
                //ThrallType.FlowerThrall => ModContent.ProjectileType<BloodFlowerThrallProjectile>(),
                //ThrallType.HydraThrall => ModContent.ProjectileType<BloodHydraThrallProjectile>(),
                ThrallType.NerveWormThrall => ModContent.ProjectileType<NeuronWormThrall>(),

                _ => ModContent.ProjectileType<BloodThrallProjectile>()
            };
        }

        private void ForEachThrall(Action<Projectile, ThrallType> action)
        {
            foreach (var (type, ids) in thrallsByType)
            {
                foreach (int id in ids)
                {
                    if (!Main.projectile.IndexInRange(id))
                        continue;

                    Projectile p = Main.projectile[id];
                    if (!p.active)
                        continue;

                    action(p, type);
                }
            }
        }
        private void UpdateThrallDamage()
        {
            ForEachThrall((proj, type) =>
            {
                int baseDamage = symbiote.GetThrallDamage();

                proj.damage = type switch
                {
                    ThrallType.BasicThrall0 => baseDamage,
                    ThrallType.BasicThrall1 => (int)(baseDamage * 1.1f),
                    ThrallType.BasicThrall2 => (int)(baseDamage * 1.25f),

                    ThrallType.WingedThrall => (int)(baseDamage * 0.8f),
                    ThrallType.FlowerThrall => (int)(baseDamage * 0.6f),

                    ThrallType.HydraThrall => (int)(baseDamage * 1.6f),
                    ThrallType.NerveWormThrall => (int)(baseDamage * 0.55f),

                    _ => baseDamage
                };
            });
        }


        #endregion

        void IBloodConstructController.OnAscensionStart()
        {
            FormOvermind();
        }

        void IBloodConstructController.OnBandChanged(BloodBand newBand)
        {
            currentBand = newBand;

            maxPerType.Clear();

            switch (newBand)
            {
                case BloodBand.Low:
                    KillAllThralls();
                    return;

                case BloodBand.MidLow:
                    maxPerType[ThrallType.BasicThrall0] = 2;
                    maxPerType[ThrallType.NerveWormThrall] = 6;
                    break;

                case BloodBand.MidHigh:
                    maxPerType[ThrallType.BasicThrall0] = 3;
                    maxPerType[ThrallType.WingedThrall] = 1;
                    maxPerType[ThrallType.NerveWormThrall] = 10;
                    break;

                case BloodBand.High:
                    maxPerType[ThrallType.BasicThrall0] = 4;
                    maxPerType[ThrallType.WingedThrall] = 2;
                    maxPerType[ThrallType.HydraThrall] = 1;
                    maxPerType[ThrallType.NerveWormThrall] = 16;
                    break;
            }
        }

        private void KillAllThralls()
        {

        }

        void IBloodConstructController.OnCrash()
        {
            KillAllThralls();
            Main.projectile[overmindID].active = false;
        }
        void IBloodConstructController.OnPurge()
        {

        }
        void IBloodConstructController.Update(Player player)
        {
            CheckOvermind();
            CleanupDeadThralls();
            SpawnThrall(ThrallType.NerveWormThrall);
            UpdateThrallDamage();
        }

        private void CheckOvermind()
        {
            if (overmindActive)
            {
                overmindActive = Main.projectile[overmindID].active;
            }
        }
    }

}
