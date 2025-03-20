using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using NoxusBoss.Content.Dusts;

namespace HeavenlyArsenal.Content.Projectiles.Weapons.Melee.Nadir2
{
    public class nadir2Lantern : ModProjectile
    {
        // --- Simulation properties ---
        public List<VerletSimulatedSegment> Segments; // The segments forming the chain
        private int segmentCount = 5;               // Total number of segments
        private float segmentDistance = 13f;         // Distance between each segment
        private bool initialized = false;            // One-time initialization flag

        // --- Customizable properties ---
        /// <summary>
        /// Controls how much the lantern’s rotation is dampened.
        /// Typical values between 0.05 and 0.2 provide smooth rotation.
        /// </summary>
        public float rotationDampening = 0.1f;

        /// <summary>
        /// The texture path for drawing the chain (string).
        /// </summary>
        public string StringTexture = "HeavenlyArsenal/Content/Projectiles/Holdout/Nadir2/nadir2Lantern_String";

        /// <summary>
        /// The texture path for drawing the lantern.
        /// </summary>
        public string LanternTexture = "HeavenlyArsenal/Content/Projectiles/Holdout/Nadir2/nadir2Lantern";

    
        public Vector2 gravityVector = new Vector2(0f, 1.2f);





        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.aiStyle = 0;
            //Projectile.friendly = true;
            //Projectile.penetrate = -1;
            // Although timeLeft is set here, it is reset in AI so that the lantern persists.
            Projectile.timeLeft = 600;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            // Prevent despawning.
            Projectile.timeLeft = 2;

            if (!initialized)
            {
                InitializeSegments();
                initialized = true;
            }

            SimulateSegments();

            // Compute target rotation from the last two segments.
            if (Segments.Count >= 2)
            {
                Vector2 diff = Segments[segmentCount - 1].position - Segments[segmentCount - 2].position;
                float targetRotation = diff.ToRotation();
                Projectile.rotation = MathHelper.Lerp(Projectile.rotation, targetRotation, rotationDampening);
            }

            // Update the projectile’s center so it follows the simulation's endpoint.
            Projectile.Center = Segments[segmentCount - 1].position;

            // Light
            Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.8f, 0.6f));

           

            // Dust
            if (Main.rand.NextFloat() < 0.5f)
            {
                
                Vector2 dustPos = Projectile.Center + new Vector2(Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f));
                Dust dust = Dust.NewDustPerfect(dustPos, ModContent.DustType<TwinkleDust>(), new Vector2(0, 0.5f), 150, default, 1.2f);
                dust.noGravity = false;
            }
        }

        private void InitializeSegments()
        {
            Segments = new List<VerletSimulatedSegment>();
            // The top segment is anchored by the ai parameters.
            Vector2 anchorPoint = new Vector2(Projectile.ai[0], Projectile.ai[1]);
            for (int i = 0; i < segmentCount; i++)
            {
                VerletSimulatedSegment segment = new VerletSimulatedSegment(anchorPoint + Vector2.UnitY * segmentDistance * i);
                if (i == 0)
                    segment.locked = true; // Lock the top segment.
                Segments.Add(segment);
            }
        }

        private void SimulateSegments()
        {
            // The anchor is updated externally (in nadir2Holdout.cs) in ai[0] and ai[1].
            Vector2 anchorPoint = new Vector2(Projectile.ai[0], Projectile.ai[1]);
            Segments[0].oldPosition = Segments[0].position;
            Segments[0].position = anchorPoint;
           
            Segments = VerletSimulatedSegment.SimpleSimulation(Segments, segmentDistance, gravityVector);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D stringTex = ModContent.Request<Texture2D>(StringTexture).Value;
            Texture2D lanternTex = ModContent.Request<Texture2D>(LanternTexture).Value;
            SpriteBatch spriteBatch = Main.spriteBatch;

            // Draw Chain
            if(Segments is not null)
            {
                for (int i = 0; i < Segments.Count - 1; i++)
                {
                    Vector2 start = Segments[i].position;
                    Vector2 end = Segments[i + 1].position;
                    Vector2 diff = end - start;
                    float rotation = diff.ToRotation() + MathHelper.PiOver2;
                    float length = diff.Length();

                    spriteBatch.Draw(stringTex,
                        position: start - Main.screenPosition,
                        sourceRectangle: null,
                        color: lightColor,
                        rotation: rotation,
                        origin: new Vector2(stringTex.Width * 0.5f, stringTex.Height * 0.5f),
                        scale: new Vector2(length/ stringTex.Height, 0.8f),
                        effects: SpriteEffects.None,
                        layerDepth: 0f);
                }

                // Draw the lantern at the bottom segment.
                Vector2 lanternPos = Segments[segmentCount - 1].position;
                spriteBatch.Draw(lanternTex,
                    position: lanternPos - Main.screenPosition,
                    sourceRectangle: null,
                    color: lightColor,
                    rotation: Projectile.rotation - MathHelper.PiOver2,
                    origin: new Vector2(lanternTex.Width * 0.5f, 0),//lanternTex.Height),
                    scale: Projectile.scale,
                    effects: SpriteEffects.None,
                    layerDepth: 0f);

                return false;
            }
            return false;
        }
    }

    // Verletshitidkfksfioajsofjaosef


    public class VerletSimulatedSegment
    {
        public Vector2 position;    // Current position.
        public Vector2 oldPosition; // Previous position (for calculating velocity).
        public bool locked;         // If true, this segment is fixed (used for the anchor).

        public VerletSimulatedSegment(Vector2 startPosition, bool isLocked = false)
        {
            position = startPosition;
            oldPosition = startPosition;
            locked = isLocked;
        }

        /// <summary>
        /// Performs Verlet integration using the passed gravity vector, then enforces distance constraints.
        /// </summary>
        public static List<VerletSimulatedSegment> SimpleSimulation(List<VerletSimulatedSegment> segments, float targetDistance, Vector2 gravity)
        {
            // Verlet integration pass: update free segments (applying gravity).
            for (int i = 1; i < segments.Count; i++)
            {
                if (!segments[i].locked)
                {
                    Vector2 velocity = segments[i].position - segments[i].oldPosition;
                    Vector2 temp = segments[i].position;
                    segments[i].position += velocity + gravity;
                    segments[i].oldPosition = temp;
                }
            }
            // Constraint pass: enforce the target distance between adjacent segments.
            for (int i = 1; i < segments.Count; i++)
            {
                VerletSimulatedSegment current = segments[i];
                VerletSimulatedSegment previous = segments[i - 1];
                if (current.locked)
                    continue;
                Vector2 delta = current.position - previous.position;
                float deltaLength = delta.Length();
                if (deltaLength == 0f)
                    continue;
                float difference = (deltaLength - targetDistance) / deltaLength;
                Vector2 correction = delta * 0.5f * difference;
                current.position -= correction;
                if (!previous.locked)
                    previous.position += correction;
            }
            return segments;
        }
    }
}
