using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.NPCs.Yharon;
using CalamityMod.Projectiles.Rogue;
using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.BigCrab;
using HeavenlyArsenal.Content.Projectiles.Weapons.Ranged;
using Luminance.Assets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.siphonophore
{
    public enum CryoAI
    {
        Null, //testing only. does not lead into any other states.
        Idle, 
        SeekTarget, //this will differ depending on specialization and the different amounts of zooids. for example, a fast cryonophore will be more evasive while approaching a target,
                    //while a tankier cryonophore will be more aggressive and walk up to you. a ranged cryonophore will try to keep its distance and shoot at you.
                   
        Attack, 

        Scatter, 
              //upon dying, or being severely damaged, the core will fracture and scatter as many of its zooids around as it can.
              //the core will be able to reattach to its zooids if they are close enough, and will be able to use them as a sort of shield. 

        Death // the core shatters. any zooids close enough are also shattered.
              // however, if they're far enough away, they can escape. upon doing so, they will lose their connection to their old core.
              // they can be attached to a new cryonophore core if one is present.
    }

    public enum ZooidType
    {
        Core, 
        Structure, //the basic zooid. the more of these the Cryonophore has, the more health it has. this should be configurable.
        Defensive, // the more of these the zooid has, the more defense it gets. this should be configurable.
        Special, // the more of these it has, the faster it can use special abilities andwhatnot. this should be configurable.
        Blastozooid, //rocket launcha (but its ice).  this should be configurable.
        Claw, //close range melee zooid. increases damage and allows for melee attacks. the amount of damage it does should be configurable.
        Tentacle, //Mobility zooid, the more of these the cryonophore has, the faster it can move.
        

        //SPECIALIZED
        //upon spawning in, the cryonophore will randomly be assigned a specialization.
        // this section is for those specialized zooids. they can only be chosen if the core's specialization is correct.

        Wing, // the more of these the cryonophore has, the faster it can move. additionally, this will allow the cryonophore to fly. however, it comes with a health penalty.
        Arm, // the more of these the cryonophore has, the more damage it does. however, it comes with a health penalty.
        burrower, // this allows the cryonophore to burrow through the ground like a worm. additionally, it grants a defense bonus.
        ArcaneConduit, // this zooid allows the cryonophore to use ice magic. there is no downside to this zooid, but it is very rare. 

        Empty //This is solely used for the grid. if there is ever a siphonophore with the empty zooid type, it should kill itself, NOW
    }

    public enum EvoNiche
    {
        None, // No specialization. basic cryonophore. stupid idiot.
        Offensive, // the cryonophore is specialized for offense. higher chance of spawning with a blastozooid or claw Zooid.
        Tank, // the cryonophore is specialized for defense.not a lot of movement speed, but it will last a lot longer.
        Skyborn, // the cryonophore is specialized for aerial combat. it will be able to fly and move quickly, but it will have less health.
        Magician, // the cryonophore is specialized for magic. it will be able to use ice magic, such as conjuring a blizzard or shooting icicles.
                  // this is intended to be a glass cannon, but its not impossible for it to also be tanky.


    }
    public class SiphonophoreGrid
    {
        public readonly int Size; // must be odd so there is a true center
        private ZooidType[,] _cells;
        public int Center => Size / 2;
        public SiphonophoreGrid(int size = 11)
        {
            if (size % 2 == 0)
            {
                size -= 1;
            }
            //throw new ArgumentException("Grid size must be odd.");
            Size = size;
            _cells = new ZooidType[size, size];

            // initialize all to Empty
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    _cells[x, y] = ZooidType.Empty;
            // place the Core in the exact middle
            _cells[Center, Center] = ZooidType.Core;
        }

        public ZooidType this[int x, int y]
        {
            get => _cells[x, y];
            set => _cells[x, y] = value;
        }

        public bool InBounds(int x, int y)
            => x >= 0 && x < Size && y >= 0 && y < Size;
    }

    // A placement‐rule takes: the grid, the zooid‐type being placed,
    // and the target coordinates—and returns true if that placement is allowed.
    public delegate bool PlacementRule(SiphonophoreGrid grid, int x, int y);

    public static class PlacementRulesRegistry
    {
        public static readonly Dictionary<ZooidType, PlacementRule> Rules
            = new Dictionary<ZooidType, PlacementRule>()
            {
                // Structure zooids must be adjacent (4‑way) to the Core or another Structure
                [ZooidType.Structure] = (grid, x, y) =>
                IsEmptyAndAdjacentTo(grid, x, y, ZooidType.Core, ZooidType.Structure),

                // Defensive zooids may only attach to Structure zooids
                [ZooidType.Defensive] = (grid, x, y) =>
                IsEmptyAndAdjacentTo(grid, x, y, ZooidType.Structure),

                // Tentacles must be at least 2 cells away from the Core
                [ZooidType.Tentacle] = (grid, x, y) =>
                grid.InBounds(x, y)
                && grid[x, y] == ZooidType.Empty
                && (Math.Abs(x - grid.Center) + Math.Abs(y - grid.Center) >= 2),

                // Blastozooid (rockets) can only be placed on the “rim” (distance = max)
                [ZooidType.Blastozooid] = (grid, x, y) =>
                grid.InBounds(x, y)
                && grid[x, y] == ZooidType.Empty
                && (Math.Abs(x - grid.Center) == grid.Center
                    || Math.Abs(y - grid.Center) == grid.Center),

                // Arcane conduits must be in a straight line from the Core
                [ZooidType.ArcaneConduit] = (grid, x, y) =>
                grid.InBounds(x, y)
                && grid[x, y] == ZooidType.Empty
                && (x == grid.Center || y == grid.Center),

                  // Wing zooids must attach adjacent horizontally to another Wing or Structure, and have empty space above or below for flight
            [ZooidType.Wing] = (grid, x, y) =>
                grid.InBounds(x, y)
                && grid[x, y] == ZooidType.Empty
                && ((x > 0 && (grid[x-1, y] == ZooidType.Wing || grid[x-1, y] == ZooidType.Structure))
                 || (x < grid.Size-1 && (grid[x+1, y] == ZooidType.Wing || grid[x+1, y] == ZooidType.Structure)))
                && ((y > 0 && grid[x, y-1] == ZooidType.Empty) || (y < grid.Size-1 && grid[x, y+1] == ZooidType.Empty)),

            // Claw zooids must attach adjacent to Structure or Core and be within 2 cells of the Core
            [ZooidType.Claw] = (grid, x, y) =>
                grid.InBounds(x, y)
                && grid[x, y] == ZooidType.Empty
                && IsEmptyAndAdjacentTo(grid, x, y, ZooidType.Core, ZooidType.Structure)
                && (Math.Abs(x - grid.Center) + Math.Abs(y - grid.Center) <= 2)};

        // Helper: is the target empty and adjacent (N/E/S/W) to any of the given types?
        //god kill me already
        private static bool IsEmptyAndAdjacentTo(
            SiphonophoreGrid grid, int x, int y, params ZooidType[] neighbors)
        {
            if (!grid.InBounds(x, y) || grid[x, y] != ZooidType.Empty)
                return false;

            var offsets = new (int dx, int dy)[] { (1, 0), (-1, 0), (0, 1), (0, -1) };
            foreach (var (dx, dy) in offsets)
            {
                int nx = x + dx, ny = y + dy;
                if (grid.InBounds(nx, ny)
                    && neighbors.Contains(grid[nx, ny]))
                    return true;
            }
            return false;
        }
    }


    class Cryonophore_Core : ModNPC
    {
        #region Setup
        private const int CellSize = 32;
        public ref float Time => ref NPC.ai[0];
        public ref float ZooidNumber => ref NPC.ai[1]; // the number of zooids the core has attached to it.
        public ref float CoreState => ref NPC.ai[3];
        public CryoAI CurrentState;
        public ZooidType ZType;
        public EvoNiche Specialization;
        public static readonly SoundStyle HitSound = new("CalamityMod/Sounds/NPCHit/CryogenHit", 3);
        public override void SetDefaults()
        {
            NPC.width = 32;
            NPC.height = 32;
            NPC.damage = 0;
            NPC.defense = 0;
            NPC.lifeMax = 800;
            NPC.value = 1000f;
            NPC.npcSlots = 0f; // to be determined. will be set using the amount of zooids the core has attached to it.
            NPC.knockBackResist = 0f;
            NPC.aiStyle = -1;
            NPC.noTileCollide = false; //change if trying to burrow
            NPC.noGravity = true; //change if can fly
            NPC.dontTakeDamage = false;
            NPC.HitSound = HitSound;
        }
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

        public bool hasArcane { get; private set; }

        public static int[] ValidTargets = { ModContent.NPCType<AnAffrontToGod>(), ModContent.NPCType<Yharon>(), ModContent.NPCType<SuperDummyNPC>(), ModContent.NPCType<ArtilleryCrab>() };
        public List<int> ZooidList = new List<int>(); // the list of zooids that are attached to this core.
        private float tentacleCount;
        public SiphonophoreGrid Grid { get; private set; }

        /// <summary>
        /// Counts how many attached zooids of the given type this core currently has.
        /// Uses the ZooidList which stores the NPC.whoAmI indices of each spawned zooid.
        /// </summary>
        /// <param name="type">The ZooidType to count.</param>
        /// <returns>Number of zooids of that type.</returns>
        private int CountZooidsOfType(ZooidType type)
        {
            int count = 0;
            // Iterate through each stored zooid NPC index
            foreach (int npcIndex in ZooidList.ToList())
            {
                if (npcIndex < 0 || npcIndex >= Main.maxNPCs) continue;
                NPC zooid = Main.npc[npcIndex];
                if (!zooid.active || !(zooid.ModNPC is Cryonophore_Zooid cz))
                {
                    // Remove any invalid or inactive entries
                    ZooidList.Remove(npcIndex);
                    continue;
                }
                if (cz.ZType == type)
                    count++;
            }
            return count;
        }
        public override void OnSpawn(IEntitySource source)
        {   // The core will spawn with a random specialization. this will determine the type of zooids it spawns with.
            //this will be used to determine the type of zooids it spawns with.
            int coin = Main.rand.Next(0, 2);
            if(coin == 0)
            {
               //no specialization. get fucked.
               Specialization = EvoNiche.None;
            }
            else
            {
                int rand = Main.rand.Next(0, 100);
                if (rand < 25)
                {
                    Specialization = EvoNiche.Offensive;
                }
                else if (rand < 50)
                {
                    Specialization = EvoNiche.Tank;
                }
                else if (rand < 75)
                {
                    Specialization = EvoNiche.Skyborn;
                }
                else
                {
                    Specialization = EvoNiche.Magician;
                }
            }
            CurrentState = CryoAI.Idle;
            ZType = ZooidType.Core;

           
            var zooidTypes = new List<ZooidType> {
             ZooidType.Structure, ZooidType.Defensive, ZooidType.Tentacle,
                ZooidType.Claw, ZooidType.Arm, ZooidType.Wing,
                 ZooidType.Blastozooid, ZooidType.ArcaneConduit
            };
            List<int> weights = new List<int>();
            foreach (var zt in zooidTypes)
            {
                int w = 1;
                // Favor matching niche
                if ((Specialization == EvoNiche.Offensive) && zt == ZooidType.Structure) w = 5;
                if ((Specialization == EvoNiche.Tank) && zt == ZooidType.Defensive) w = 5;
                if ((Specialization == EvoNiche.Offensive) && (zt == ZooidType.Claw || zt == ZooidType.Arm)) w = 5;
                if ((Specialization == EvoNiche.Skyborn) && (zt == ZooidType.Tentacle || zt == ZooidType.Wing)) w = 5;
                if ((Specialization == EvoNiche.Magician) && (zt == ZooidType.Blastozooid || zt == ZooidType.ArcaneConduit)) w = 5;
                weights.Add(w);
            }

            Grid = new SiphonophoreGrid(size: 11);


           
            // Then, the core will set its ZooidNum to the number of cryonophores it currently has attached to it.
            // On spawn, this will be equal to the amount spawned.
            GenerateZooidPlacement(spawnCount: 11);
           


            int structureCount = CountZooidsOfType(ZooidType.Structure);
            int defensiveCount = CountZooidsOfType(ZooidType.Defensive);
            int burrowerCount = CountZooidsOfType(ZooidType.burrower);

            int BlastoidCount = CountZooidsOfType(ZooidType.Blastozooid);
            int tentacleCount = CountZooidsOfType(ZooidType.Tentacle);
            int clawCount = CountZooidsOfType(ZooidType.Claw);
            int armCount = CountZooidsOfType(ZooidType.Arm);
            int wingCount = CountZooidsOfType(ZooidType.Wing);
            int arcaneCount = CountZooidsOfType(ZooidType.ArcaneConduit);


            //Main.NewText($"Spawned: Cryonophore Core, Specialization: {Specialization}", Color.Green);
           // Main.NewText($"Counts: structure: {structureCount}, Defensive: {defensiveCount}, tentacle:{tentacleCount}, Claw: {clawCount}, Arm: {armCount}, Wing: {wingCount}, Conduits: {arcaneCount}, Blasto: {BlastoidCount}, Burrower: {burrowerCount} ");
            // Structure: increase HP
            if (structureCount > 0)
            {
                NPC.lifeMax += structureCount * 50;
                NPC.life = NPC.lifeMax;
            }
            // Defensive: increase defense
            if (defensiveCount > 0)
            {
                NPC.defense += defensiveCount * 5 ;
            }
            // Wings: enable flight if any wing attached
            if (wingCount > 1)
            {
                NPC.noGravity = true;
                // Optionally adjust maximum fall speed or vertical movement
            }
            else
            {
                NPC.noGravity = false;
            }
                // Arcane: allow magic attacks if present
            hasArcane = (arcaneCount > 0);

            // Note: We also applied tentacle and claw/arm effects in AI movement and attack (see above).
            NPC.netUpdate = true;  // sync the stat changes


        }


        #endregion
        private void GenerateZooidPlacement(int spawnCount)
        {
            // Phase 1: Build structure skeleton and spawn structure zooids
            var structure = new HashSet<Point> { new Point(Grid.Center, Grid.Center) };
            var frontier = new List<Point>();
            foreach (var off in new[] { new Point(1, 0), new Point(-1, 0), new Point(0, 1), new Point(0, -1) })
                frontier.Add(new Point(Grid.Center + off.X, Grid.Center + off.Y));

            int targetStruct = spawnCount / 2;
            int placedStruct = 0;
            for (int i = 0; i < targetStruct; i++)
            {
                if (frontier.Count == 0) break;
                // pick and remove random frontier cell
                int idx = Main.rand.Next(frontier.Count);
                var cell = frontier[idx];
                frontier.RemoveAt(idx);

                if (!Grid.InBounds(cell.X, cell.Y) || Grid[cell.X, cell.Y] != ZooidType.Empty)
                    continue;

                // place structure in grid
                Grid[cell.X, cell.Y] = ZooidType.Structure;
                structure.Add(cell);
                placedStruct++;

                // spawn a structure zooid NPC
                int offX = cell.X - Grid.Center;
                int offY = cell.Y - Grid.Center;
                int who = NPC.NewNPC(
                    NPC.GetSource_FromAI(),
                    (int)NPC.Center.X,
                    (int)NPC.Center.Y,
                    ModContent.NPCType<Cryonophore_Zooid>(),
                    ai2: (int)ZooidType.Structure,
                    ai3: NPC.whoAmI
                );
                ZooidList.Add(who);
                var z = Main.npc[who];
                z.localAI[0] = offX;
                z.localAI[1] = offY;

                // add neighbors to frontier
                foreach (var off in new[] { new Point(1, 0), new Point(-1, 0), new Point(0, 1), new Point(0, -1) })
                {
                    var nb = new Point(cell.X + off.X, cell.Y + off.Y);
                    if (Grid.InBounds(nb.X, nb.Y) && Grid[nb.X, nb.Y] == ZooidType.Empty && !frontier.Contains(nb))
                        frontier.Add(nb);
                }
            }

            // Phase 2: Attach specialized parts (fallback to core neighbors if needed)
            var attachable = new List<Point>();
            if (structure.Count > 1)
            {
                attachable = structure.SelectMany(c => new[] { new Point(c.X + 1, c.Y), new Point(c.X - 1, c.Y), new Point(c.X, c.Y + 1), new Point(c.X, c.Y - 1) })
                                       .Where(p => Grid.InBounds(p.X, p.Y) && Grid[p.X, p.Y] == ZooidType.Empty)
                                       .Distinct().ToList();
            }
            if (attachable.Count == 0)
            {
                var core = new Point(Grid.Center, Grid.Center);
                attachable = new[] { new Point(core.X + 1, core.Y), new Point(core.X - 1, core.Y), new Point(core.X, core.Y + 1), new Point(core.X, core.Y - 1) }
                             .Where(p => Grid.InBounds(p.X, p.Y) && Grid[p.X, p.Y] == ZooidType.Empty)
                             .ToList();
            }

            int remaining = spawnCount - placedStruct;
            var zooidTypes = new List<ZooidType> { ZooidType.Defensive, ZooidType.Tentacle, ZooidType.Claw, ZooidType.Arm, ZooidType.Wing, ZooidType.Blastozooid, ZooidType.ArcaneConduit };
            for (int i = 0; i < remaining && attachable.Count > 0; i++)
            {
                var type = ChooseWeightedType(zooidTypes);
                var valid = attachable.Where(p => PlacementRulesRegistry.Rules[type](Grid, p.X, p.Y)).ToList();
                if (valid.Count == 0) continue;

                var cell = valid[Main.rand.Next(valid.Count)];
                Grid[cell.X, cell.Y] = type;

                int offX = cell.X - Grid.Center;
                int offY = cell.Y - Grid.Center;
                int who = NPC.NewNPC(
                    NPC.GetSource_FromAI(),
                    (int)NPC.Center.X,
                    (int)NPC.Center.Y,
                    ModContent.NPCType<Cryonophore_Zooid>(),
                    ai2: (int)type,
                    ai3: NPC.whoAmI
                );
                ZooidList.Add(who);
                var z = Main.npc[who];
                z.localAI[0] = offX;
                z.localAI[1] = offY;
                attachable.Remove(cell);
            }

            NPC.netUpdate = true;
        }
    

        /// <summary>
        /// Choose a zooid type from the list, weighted by this core's specialization.
        /// </summary>
        private ZooidType ChooseWeightedType(List<ZooidType> types)
        {
            var weights = types.Select(zt => {
                int w = 1;
                if (Specialization == EvoNiche.Offensive && (zt == ZooidType.Claw || zt == ZooidType.Arm)) w = 5;
                if (Specialization == EvoNiche.Tank && zt == ZooidType.Defensive) w = 5;
                if (Specialization == EvoNiche.Skyborn && (zt == ZooidType.Tentacle || zt == ZooidType.Wing)) w = 5;
                if (Specialization == EvoNiche.Magician && (zt == ZooidType.Blastozooid || zt == ZooidType.ArcaneConduit)) w = 5;
                return w;
            }).ToList();
            int total = weights.Sum();
            int pick = Main.rand.Next(total);
            for (int i = 0; i < types.Count; i++)
            {
                pick -= weights[i]; if (pick < 0) return types[i];
            }
            return types[0];
        }
        public override void AI()
        {


            switch (CurrentState)
            {
                case CryoAI.Idle:
                    // Idle: gentle hovering/swirl behavior
                    NPC.velocity *= 0.98f;              // slow down motion
                                                        //NPC.rotation += 0.05f;             // slow rotation to look alive
                                                        // Check for player target
                    NPC.TargetClosest();
                    Player target = Main.player[NPC.target];
                    if (target.active && !target.dead)
                    {
                        float dist = Vector2.Distance(NPC.Center, target.Center);
                        if (dist < 600f)
                        {
                            // Transition to SeekTarget when player is near
                            CoreState = 1;
                            CurrentState = CryoAI.SeekTarget;
                            NPC.netUpdate = true;      // sync state change in multiplayer
                        }
                    }
                    break;

                case CryoAI.SeekTarget:
                    // Acquire target player
                    NPC.TargetClosest();
                    target = Main.player[NPC.target];
                    if (target == null || !target.active || target.dead)
                    {
                        // No valid target: go back to Idle
                        CoreState = (float)CryoAI.Idle;
                        NPC.netUpdate = true;
                        break;
                    }
                    // Compute direction toward target

                    Vector2 direction = target.Center - NPC.Center;
                    NPC.rotation = direction.ToRotation();
                    float distance = direction.Length();
                    if (distance > 0f) direction.Normalize();


                    int structCount = CountZooidsOfType(ZooidType.Structure);
                    float speedFactor = 1f / (1f + structCount * 0.1f);


                    float baseSpeed = 5f * speedFactor;              // base movement speed scaled
                                                                     // Increase speed per tentacle zooid:
                    float tentacleBonus = tentacleCount * 0.5f;


                    if (Specialization == EvoNiche.Offensive)
                    {
                        // Aggressive: move directly toward player


                        // NPC.velocity.X = direction * (baseSpeed + tentacleBonus);


                        if (NPC.noGravity == false)
                        {
                            NPC.velocity.X = direction.X * (baseSpeed + tentacleBonus);
                        }
                        else
                            NPC.velocity = direction * (baseSpeed + tentacleBonus);
                    }
                    else if (Specialization == EvoNiche.Offensive)
                    {
                        // Ranged: attempt to keep distance (back away if too close)
                        if (distance < 400f)
                        {
                            NPC.velocity = -direction * (baseSpeed - tentacleBonus);

                        }
                        else
                        {
                            NPC.velocity = direction * (baseSpeed - tentacleBonus);
                        }
                    }
                    else if (Specialization == EvoNiche.Tank)
                    {
                        // Defensive: circle or hover (simple example: slow move)
                        NPC.velocity = direction * (baseSpeed * 0.5f + tentacleBonus);
                    }
                    else
                    {
                        // Structure or other: move slowly to maintain formation
                        NPC.velocity = direction * (baseSpeed * 0.7f + tentacleBonus);
                    }

                    // Check if close enough to attack
                    if (distance < 300f)
                    {
                        CurrentState = CryoAI.Attack;
                        NPC.netUpdate = true;
                    }

                    break;

                case CryoAI.Attack:
                    // Attack logic: each zooid type performs its ability
                    // (We assume each zooid’s AI method will perform actions based on this state)
                    CoreState = (float)CryoAI.Attack;

                    break;

                // Scatter and Death can be added later
                default:
                    break;
            }
        }

        /*
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Grid == null)
                return base.Colliding(projHitbox, targetHitbox);

            // Check collision against each occupied cell
            for (int gx = 0; gx < Grid.Size; gx++)
            {
                for (int gy = 0; gy < Grid.Size; gy++)
                {
                    if (Grid[gx, gy] == ZooidType.Empty)
                        continue;

                    // Compute world position of cell
                    int dx = gx - Grid.Center;
                    int dy = gy - Grid.Center;
                    Vector2 cellOffset = new Vector2(dx, dy) * CellSize;
                    Vector2 rotated = cellOffset.RotatedBy(NPC.rotation);
                    Vector2 worldPos = NPC.Center + rotated - new Vector2(CellSize / 2);

                    Rectangle cellRect = new Rectangle(
                        (int)worldPos.X - (CellSize / 2),
                        (int)worldPos.Y - (CellSize / 2),
                        CellSize,
                        CellSize);

                    if (projHitbox.Intersects(cellRect))
                        return true;
                }
            }
            return false;
        }
        */
        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            //only the core gets to draw a healthbar
            return true;
        }

        public override bool ModifyCollisionData(
             Rectangle victimHitbox,
             ref int immunityCooldownSlot,
             ref MultipliableFloat damageMultiplier,
             ref Rectangle npcHitbox)
        {
            if (Grid != null)
            {
                bool first = true;
                int minX = 0, minY = 0, maxX = 0, maxY = 0;
                for (int gx = 0; gx < Grid.Size; gx++)
                {
                    for (int gy = 0; gy < Grid.Size; gy++)
                    {
                        if (Grid[gx, gy] == ZooidType.Empty)
                            continue;

                        int dx = gx - Grid.Center;
                        int dy = gy - Grid.Center;
                        Vector2 cellOffset = new Vector2(dx, dy) * CellSize;
                        Vector2 rotated = cellOffset.RotatedBy(NPC.rotation);
                        Vector2 world = NPC.Center + rotated;

                        int half = CellSize / 2;
                        int cx1 = (int)world.X - half;
                        int cy1 = (int)world.Y - half;
                        int cx2 = cx1 + CellSize;
                        int cy2 = cy1 + CellSize;

                        if (first)
                        {
                            minX = cx1; minY = cy1; maxX = cx2; maxY = cy2;
                            first = false;
                        }
                        else
                        {
                            minX = Math.Min(minX, cx1);
                            minY = Math.Min(minY, cy1);
                            maxX = Math.Max(maxX, cx2);
                            maxY = Math.Max(maxY, cy2);
                        }
                    }
                }
                npcHitbox = new Rectangle(minX, minY, maxX - minX, maxY - minY);
            }

            return false;
        }
    
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = GennedAssets.Textures.GreyscaleTextures.WhitePixel;

            Rectangle frame = new Rectangle(1, 1, texture.Width, texture.Height);

            Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
            

            Utils.DrawBorderString(spriteBatch, "| CurrentState: "+ CurrentState.ToString()+ " | Zooid Type: "+ ZType.ToString(), NPC.Center + Vector2.UnitX *300- Main.screenPosition, Color.White);
            Utils.DrawBorderString(spriteBatch, "| Specialization: " + Specialization.ToString(), new Vector2(NPC.Center.X + Vector2.UnitX.X *300, NPC.Center.Y + Vector2.UnitY.Y *20) - Main.screenPosition, Color.White);
            Utils.DrawBorderString(spriteBatch, "| WhoAmI: " + NPC.whoAmI.ToString(), new Vector2(NPC.Center.X + Vector2.UnitX.X * 20, NPC.Center.Y + Vector2.UnitY.Y * 40) - Main.screenPosition, Color.White);

            Main.EntitySpriteDraw(texture, NPC.Center - Main.screenPosition, frame, Color.Crimson, NPC.rotation, origin, 16f, SpriteEffects.None, 0f);

            
            if(Grid != null&& !NPC.IsABestiaryIconDummy)
            {
                for (int gx = 0; gx < Grid.Size; gx++)
                {
                    for (int gy = 0; gy < Grid.Size; gy++)
                    {
                        // compute cell offset
                        int dx = gx - Grid.Center;
                        int dy = gy - Grid.Center;
                        Vector2 cell = new Vector2(dx, dy) * CellSize;
                        Vector2 rotated = cell.RotatedBy(NPC.rotation);
                        Vector2 pos = NPC.Center + rotated - screenPos;
                        // draw symbol for cell content
                        char symbol = Grid[gx, gy] switch
                        {
                            ZooidType.Empty => '.',
                            ZooidType.Core => 'C',
                            _ => Grid[gx, gy].ToString()[0]
                        };
                        Utils.DrawBorderString(spriteBatch, symbol.ToString(), pos, Color.White);
                    }
                }
            }
           
            
            return false;
        }

    }
    class Cryonophore_Zooid : ModNPC
    {
        #region Setup
        private const int CellSize = 32;
        public ZooidType ZType;
        public EvoNiche Specialization;
        private Point gridOffset;
        public ref float Time => ref NPC.ai[0];
        public ref float Owner => ref NPC.ai[1];
        public ref float WhatAmi => ref NPC.ai[2];
        public ref float coreT => ref NPC.ai[3]; //core state

        public ref float offX => ref NPC.localAI[0];
        public ref float offY => ref NPC.localAI[1];

        public static readonly SoundStyle HitSound = new("CalamityMod/Sounds/NPCHit/CryogenHit", 3);

        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        public override void SetDefaults()
        {

            NPC.width = 20;
            NPC.height = 20;
            NPC.damage = 0;
            NPC.defense = 0;
            NPC.lifeMax = 800;
            NPC.value = 1000f;
            NPC.npcSlots = 0f;
            NPC.knockBackResist = 0f;
            NPC.aiStyle = -1;
            NPC.noTileCollide = false;
            NPC.noGravity = true;
            NPC.dontTakeDamage = false;
            NPC.HitSound = HitSound;
        }
        public override void SetStaticDefaults()
        {
            NPCID.Sets.CannotDropSouls[Type] = true;
        }


        public override void OnSpawn(IEntitySource source)
        {
            if (source is EntitySource_Parent parentSource && parentSource.Entity is NPC Core && Core.type == ModContent.NPCType<Cryonophore_Core>())
            {
                Owner = Core.whoAmI;

            }
            ZType = (ZooidType)WhatAmi;


            gridOffset = new Point((int)NPC.localAI[0], (int)NPC.localAI[1]);
            base.OnSpawn(source);
        }

        #endregion

        public override void AI()
        {
           

            if (Owner < Main.maxNPCs && Main.npc[(int)Owner].active)
            {
                NPC core = Main.npc[(int)Owner];
                CryoAI coreState = (CryoAI)core.ai[3];


                Vector2 cellWorldOffset = new Vector2(offX, offY) * CellSize;
                Vector2 rotatedOffset = cellWorldOffset.RotatedBy(core.rotation);
                NPC.Center = core.Center + rotatedOffset;


                switch ((ZooidType)Owner)
                {
                    case ZooidType.Blastozooid:
                        if (coreState == CryoAI.Attack && Main.rand.NextBool(60))
                        {
                            Player target = Main.player[core.target];
                            Vector2 dir = target.Center - NPC.Center;
                            dir.Normalize();
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, dir * 8f,
                                ModContent.ProjectileType<FrostShardFriendly>(), core.defDamage, 0f, Main.myPlayer);
                        }
                        break;

                    case ZooidType.Claw:
                        break;
                    case ZooidType.Arm:
                        // If core is attacking, rush player for melee hit
                        if (coreState == CryoAI.Attack)
                        {
                            Player target = Main.player[core.target];
                            Vector2 dir = target.Center - NPC.Center;
                            dir.Normalize();
                            NPC.velocity = dir * 6f;
                            // Simple hit detection (could use Collision check in real code)
                            if (NPC.Hitbox.Intersects(target.Hitbox))
                            {
                                target.Hurt(PlayerDeathReason.ByNPC(NPC.whoAmI), NPC.damage, 0);
                            }
                        }
                        break;

                    case ZooidType.ArcaneConduit:
                        if (coreState == CryoAI.Attack && Main.rand.NextBool(200))
                        {
                            Player target = Main.player[core.target];
                            int effect = Main.rand.Next(4);
                            if (effect == 0)
                            {
                                //TODO: make my own projectiles/
                                for (int i = 0; i < 5; i++)
                                {
                                    Vector2 pos = target.Center + new Vector2(Main.rand.NextFloat(-100, 100), Main.rand.NextFloat(-100, 100));
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), pos, new Vector2(0, 6f),
                                        ModContent.ProjectileType<FrostShardFriendly>(), core.defDamage, 0f, Main.myPlayer);
                                }
                            }
                            else if (effect == 1)
                            {
                                // Icicle: drop one from above
                                Vector2 spawn = new Vector2(target.Center.X, target.Center.Y - 600f);
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), spawn, new Vector2(0f, 8f),
                                    ModContent.ProjectileType<FrostShardFriendly>(), core.defDamage, 0f, Main.myPlayer);
                            }
                            else if (effect == 2)
                            {
                                // Ice Wall: a vertical wall of projectiles
                                for (int i = -2; i <= 2; i++)
                                {
                                    Vector2 spawn = target.Center + new Vector2(i * 20, 300f);
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), spawn, new Vector2(0f, -5f),
                                        ModContent.ProjectileType<FrostShardFriendly>(), core.defDamage, 0f, Main.myPlayer);
                                }
                            }
                            else
                            {
                                // Freeze: apply debuff to player
                                target.AddBuff(BuffID.Frozen, 60);
                            }
                        }
                        break;

                        // Other zooid types (Tentacle, Structure, etc.) primarily affect core stats and may have idle behavior.
                }
            }
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            //only the core gets to draw a healthbar
            return false;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {

            //eventually, there will be a texture for each zooid type. for now, each zooid type will look the same but have text that indicates what type it is.
            
            Texture2D texture = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
            Rectangle frame = new Rectangle(1, 1, texture.Width, texture.Height);
            Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
            //Utils.DrawBorderString(spriteBatch, "| Owner: " + Owner.ToString(), new Vector2(NPC.Center.X + Vector2.UnitX.X * 20, NPC.Center.Y + Vector2.UnitY.Y * 20) - Main.screenPosition, Color.White);
            //Utils.DrawBorderString(spriteBatch, " | Zooid Type: " + ZType.ToString(), NPC.Center + Vector2.UnitX * 20 - Main.screenPosition, Color.White);
            Main.EntitySpriteDraw(texture, NPC.Center - Main.screenPosition, frame, drawColor, NPC.rotation, origin, 16f, SpriteEffects.None, 0f);

            return false;
        }
    }
}
