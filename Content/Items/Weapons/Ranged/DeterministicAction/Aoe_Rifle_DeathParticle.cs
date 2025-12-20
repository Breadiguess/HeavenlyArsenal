using HeavenlyArsenal.Content.Particles;
using HeavenlyArsenal.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.Renderers;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.DeterministicAction
{
    public class Aoe_Rifle_DeathParticle : BaseParticle
    {
        public static ParticlePool<Aoe_Rifle_DeathParticle> pool = new(500, GetNewParticle<Aoe_Rifle_DeathParticle>);

        public static Texture2D[] SymbolList = new Texture2D[]
        {
            AssetDirectory.Textures.Items.Weapons.Ranger.Finality_Chinese.Value,
            AssetDirectory.Textures.Items.Weapons.Ranger.Finality_Norse.Value,
            AssetDirectory.Textures.Items.Weapons.Ranger.Finality_Omega.Value,
            AssetDirectory.Textures.Items.Weapons.Ranger.Finality_German.Value,
            AssetDirectory.Textures.Items.Weapons.Ranger.End_Turkish.Value
        };

        public Texture2D symbol;
        public Vector2 offset;
        public Vector2 position;
        public Vector2 Velocity;
        public float Rotation;
        public int TimeLeft;
        public int MaxTime;
        public Player player;
        public int flyDirection;
        public void Prepare(Vector2 Position,float Rotation,int MaxTime, Player player ,int TextureIndex = 0)
        {

            position = Position;
            this.Rotation = Rotation;
            this.TimeLeft = MaxTime;
            this.MaxTime = TimeLeft;
            this.player = player;
            
            if (TextureIndex < SymbolList.Length)
                symbol = SymbolList[TextureIndex];
            else
                symbol = SymbolList[0];

            flyDirection = Math.Sign(Main.rand.NextFloatDirection());
        }

        public override void FetchFromPool()
        {
            base.FetchFromPool();
            Velocity = Vector2.Zero;

            offset = position - player.Center;
        }

        public override void Update(ref ParticleRendererSettings settings)
        {

            position = offset + player.Center;
            position += Velocity;

            Velocity += new Vector2(offset.X, -1) * (1 - LumUtils.InverseLerp(MaxTime, 0, TimeLeft));
            //todo: float upwards from the target
            TimeLeft--;
            if (TimeLeft <= 0)
            {
                ShouldBeRemovedFromRenderer = true;
            }
        }

        public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch)
        {
            //todo: normalize all sizes so that they appear at roughly the same size
            //the symbols 'burn' away at the end of their lives, like crumbling into dust
            float Scale = symbol.Height/100f  * 0.2f;
            float OpacityInterp = LumUtils.InverseLerpBump(MaxTime, MaxTime - 7, MaxTime - 60, 0, TimeLeft);
            Color thing = Color.Lerp(Color.Transparent, Color.Crimson, LumUtils.InverseLerp(0, MaxTime, TimeLeft)) * OpacityInterp;
            Vector2 DrawPos = position - Main.screenPosition;
           
            for(int i = 0; i< 10; i++)
            {
                Main.EntitySpriteDraw(symbol, DrawPos + new Vector2(3, 0).RotatedBy(i / 10f * MathHelper.TwoPi), null, Color.Black * OpacityInterp, 0, symbol.Size() / 2, Scale, 0);
            }
            Main.EntitySpriteDraw(symbol, DrawPos, null, thing, 0, symbol.Size() / 2, Scale, 0);
        }
    }
}