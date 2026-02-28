using CalRemix.UI.ElementalSystem;
using Luminance.Assets;
using NoxusBoss.Assets;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Leech;

public class UmbralLarva : BaseBloodMoonNPC
{
    public override int MaxBlood => 60;

    public override BloodMoonBalanceStrength Strength => new(1,1,1);

    public override void OnSpawn(IEntitySource source)
    {
        base.OnSpawn(source);
        for(int i = 0; i< spine.Length; i++)
        {
            spine[i] = NPC.Center;
        }
    }
    protected override void SetDefaults2()
    {
        NPC.width = 30;
        NPC.height = 30;
        NPC.lifeMax = 30_000;
        NPC.damage = 80;
        NPC.defense = 50;
        NPC.npcSlots = 0.1f;
        NPC.knockBackResist = 0f;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.aiStyle = -1;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.Size = new Vector2(20, 20);
       
    }

    public override void AI()
    {
        UpdateSpine(NPC.Center);

        Target = Main.player[NPC.FindClosestPlayer()];
        if(Target != null)
        {
            NPC.velocity = NPC.DirectionTo(Target.Center).RotatedBy(MathHelper.ToRadians(30)*MathF.Cos(Main.GameUpdateCount/10f + NPC.whoAmI*10)) * (10 * LumUtils.InverseLerp(0, 100, NPC.Distance(Target.Center)));
        }
        //NPC.velocity = NPC.DirectionTo(Main.MouseWorld) * (30*LumUtils.InverseLerp(0, 100, NPC.Distance(Main.MouseWorld)));
    }

    #region render code
    struct WormVert : IVertexType
    {
        public Vector3 Position;   // IMPORTANT: Vector3 for BasicEffect
        public Vector2 UV;
        public Color Color;

        public static readonly VertexDeclaration Declaration = new(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(20, VertexElementFormat.Color, VertexElementUsage.Color, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration => Declaration;

        public WormVert(Vector2 position, Vector2 uv, Color color)
        {
            Position = new Vector3(position, 0f);
            UV = uv;
            Color = color;
        }
    }


    const int SpineCount = 7;
    Vector2[] spine = new Vector2[SpineCount];
    Vector2[] velocity = new Vector2[SpineCount];
    private Vector2 lastTangent;

    //todo: this doesn't feel very worm- like, its too simple and the npcs can move in ways that make it feel a lot less natural.
    // we want them to basically follow the previous without getting too close or too far from the segment.
    void UpdateSpine(Vector2 head)
    {
        spine[0] = head;

        const float rest = 14f;
        const float stiffness = 0.9f; // spring strength
        const float velocityDamping = 0.22f; // per-frame damping on velocities
        const float maxSpeed = 30f; // guard against extreme velocities
        const int constraintIters = 3; // positional correction iterations
        const float perpendicularBlend = 0.25f; // reduce lateral jitter

        // Integrate forces -> velocities -> positions
        for (int i = 1; i < SpineCount; i++)
        {
            var toPrev = spine[i - 1] - spine[i];
            var dist = toPrev.Length();

            if (dist > 0.0001f)
            {
                var dir = toPrev / dist;

                // spring force proportional to distance error
                var error = dist - rest;
                var springForce = dir * (error * stiffness);

                // small perpendicular smoothing force to reduce sharp corners:
                // compute a tangent and reduce velocity component that is perpendicular to the direct follow direction
                var currentVel = velocity[i];
                var perp = new Vector2(-dir.Y, dir.X);
                var lateral = Vector2.Dot(currentVel, perp) * perp;
                var lateralCorrection = -lateral * perpendicularBlend;

                velocity[i] += springForce + lateralCorrection;
            }

            // damping and clamp speed
            velocity[i] *= velocityDamping;
            var spd = velocity[i].Length();
            if (spd > maxSpeed)
            {
                velocity[i] = velocity[i] / spd * maxSpeed;
            }

            spine[i] += velocity[i];
        }

        // Positional constraint relaxation to keep segments at approximately 'rest' distance
        for (int iter = 0; iter < constraintIters; iter++)
        {
            for (int i = 1; i < SpineCount; i++)
            {
                var delta = spine[i] - spine[i - 1];
                var dist = delta.Length();
                if (dist < 0.0001f)
                    continue;

                // Move segments to satisfy rest distance.
                // Use half-correction so the chain remains stable, but keep head pinned.
                var diff = (dist - rest) / dist;
                var correction = delta * 0.5f * diff;

                // Apply correction
                spine[i] -= correction;
                // Only move the previous segment if it's not the head (head is pinned to 'head' parameter)
                if (i - 1 != 0)
                {
                    spine[i - 1] += correction;
                }
                else
                {
                    // If previous is the head, pin it exactly to the head position
                    spine[0] = head;
                }
            }

            // Ensure head remains exactly pinned after each iteration
            spine[0] = head;
        }

        // After constraints, update velocities to reflect positional changes so future integration is consistent.
        for (int i = 1; i < SpineCount; i++)
        {
            // Simple velocity correction: new_velocity = (new_pos - old_pos) (approximate)
            // We approximate old position by subtracting velocity we just integrated, so compute a smoothed velocity.
            // This reduces popping when constraints move segments.
            var recentVel = spine[i] - (spine[i] - velocity[i]);
            velocity[i] = Vector2.Lerp(velocity[i], recentVel, 0.2f);
        }
    }

    List<WormVert> buildSpine()
    {
        List<WormVert> verts = new();

        float width = 10f;

        for (int i = 0; i < SpineCount; i++)
        {
            Vector2 tangent;
            if (i == 0)
                tangent = spine[1] - spine[0];
            else if (i == SpineCount - 1)
                tangent = spine[i] - spine[i - 1];
            else
                tangent = (spine[i + 1] - spine[i - 1]) * 0.5f;

            tangent = tangent.SafeNormalize(Vector2.UnitX);


            Vector2 normal = tangent.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2);


            // project previous normal onto new tangent plane
            Vector2 newTangent = tangent;
            Vector2 prevTangent = lastTangent;

            Vector2 axis = newTangent - prevTangent;
            if (axis.LengthSquared() > 0.0001f)
            {
                float angle = (float)Math.Atan2(
                    prevTangent.X * newTangent.Y - prevTangent.Y * newTangent.X,
                    Vector2.Dot(prevTangent, newTangent)
                );
                normal = normal.RotatedBy(angle);
            }

            lastTangent = newTangent;


            float taper = MathHelper.Lerp(1f, 0.2f, i / (float)SpineCount);
            float w = width * taper;

            Vector2 left = spine[i] - normal * w - Main.screenPosition;
            Vector2 right = spine[i] + normal * w - Main.screenPosition;


            float v = i / (float)(SpineCount - 1);
            Color a = Lighting.GetColor((left+Main.screenPosition).ToTileCoordinates());
            Color b = Lighting.GetColor((right+Main.screenPosition).ToTileCoordinates());
            verts.Add(new WormVert(left, new Vector2(v, 0), a));
            verts.Add(new WormVert(right, new Vector2(v, 1), b));
        }
        return verts;

    }
    public BasicEffect WormEffect;
    void renderWorm(Vector2 pos)
    {
        WormVert[] verts = buildSpine().ToArray();
        if (verts.Length < 4)
            return;

        GraphicsDevice device = Main.instance.GraphicsDevice;
        Texture2D tex = ModContent.Request<Texture2D>(
            "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Leech/UmbralLarva"
        ).Value;

    
        // Hard reset the states you care about.
        device.RasterizerState = RasterizerState.CullNone;
        device.BlendState = BlendState.AlphaBlend;              
        device.DepthStencilState = DepthStencilState.None;
        device.SamplerStates[0] = SamplerState.PointClamp;
        device.Textures[0] = tex;
        if(WormEffect == null)
        WormEffect = new(device)
        {
            TextureEnabled = true,
            Texture = tex,
            VertexColorEnabled = true,
           
            View = Main.GameViewMatrix.ZoomMatrix,
            Projection = Matrix.CreateOrthographicOffCenter
                (
                    0,
                    Main.screenWidth,
                    Main.screenHeight,
                    0,
                    -1000f,
                    1000f
                )
        };
        WormEffect.View = Main.GameViewMatrix.ZoomMatrix;
        WormEffect.Projection = Matrix.CreateOrthographicOffCenter
                (
                    0,
                    Main.screenWidth,
                    Main.screenHeight,
                    0,
                    -1000f,
                    1000f
                );

        foreach (var pass in WormEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
            device.DrawUserPrimitives(
                PrimitiveType.TriangleStrip,
                verts,
                0,
                verts.Length - 2
            );
        }

   


    }
    #endregion

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if(!NPC.IsABestiaryIconDummy)
        renderWorm(NPC.Center - screenPos);



     
        return false;
    }
    public override bool CheckActive()
    {
        return false;
    }

}

public class BloodSpat : ModProjectile
{
    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetDefaults()
    {
        Projectile.hostile = true;
        Projectile.friendly = false;

        Projectile.width = Projectile.height = 14;
        Projectile.timeLeft = 300;

        Projectile.penetrate = 1;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.aiStyle = -1;
    }

    public override void AI()
    {
        var b = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Rain_BloodMoon, 0, 0, 0, Color.Crimson);

        var a = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.Blood, 0, 0, 0, Color.Purple);

        a.velocity = Projectile.velocity;
        b.velocity = Projectile.velocity;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        return false;
    }
}