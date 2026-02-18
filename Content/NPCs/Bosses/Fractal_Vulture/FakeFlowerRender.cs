using System.Collections.Generic;
using System.Linq;
using Luminance.Common.Easings;
using Luminance.Common.Utilities;
using NoxusBoss.Core.Graphics.LightingMask;
using NoxusBoss.Core.Graphics.RenderTargets;
using NoxusBoss.Core.Netcode;
using NoxusBoss.Core.Netcode.Packets;
using NoxusBoss.Core.World.TileDisabling;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture;

public abstract class FakeFlowerRender : ModSystem
{
    public static int Count =>
    renderSystems.Values.Sum(system => system.tilePoints.Count);

    public class PlantTileData
    {
        public Point Position;

        public float GrowthInterpolant;

        public PlantTileData(Point p)
        {
            Position = p;
        }
    }
    protected static Vector2 GetAnchorWorldPosition(Point tilePos)
    {
        return new Vector2(
            (tilePos.X + 0.5f) * 16f,
            (tilePos.Y + 1f) * 16f
        );
    }
    public virtual bool DropAfterAnimation => true;

    public virtual bool AffectedByLight => true;

    public abstract int ItemID { get; }

    public abstract int TileID { get; }

    public InstancedRequestableTarget OverallTarget { get; } = new();

    protected readonly List<PlantTileData> tilePoints = new();

    internal static Dictionary<string, FakeFlowerRender> renderSystems = new();

    internal static Dictionary<FakeFlowerRender, List<PlantTileData>> allTilePoints
    {
        get
        {
            var dictionary = new Dictionary<FakeFlowerRender, List<PlantTileData>>();

            foreach (var value in renderSystems.Values)
            {
                dictionary[value] = value.tilePoints;
            }

            return dictionary;
        }
    }

    public abstract void InstaceRenderFunction(bool disappearing, float growthInterpolant, float growthInterpolantModified, int i, int j, SpriteBatch spriteBatch);

    public override void OnModLoad()
    {
        renderSystems[Name] = this;
        On_Main.DoDraw_Tiles_Solid += RenderPlantInstancesWrapper;
        Main.ContentThatNeedsRenderTargets.Add(OverallTarget);
    }

    public override void OnWorldLoad()
    {
        tilePoints.Clear();
    }

    public override void OnWorldUnload()
    {
        tilePoints.Clear();
    }

    public override void PostUpdateEverything()
    {
        if (TileDisablingSystem.TilesAreUninteractable)
        {
            return;
        }

        var tileID = TileID;

        for (var i = 0; i < tilePoints.Count; i++)
        {
            var tileSafely = Framing.GetTileSafely(tilePoints[i].Position);
            var value = !tileSafely.HasTile || tileSafely.TileType != tileID;
            tilePoints[i].GrowthInterpolant = LumUtils.Saturate(tilePoints[i].GrowthInterpolant - value.ToDirectionInt() * 0.01f);
            UpdatePoint(tilePoints[i].Position);
        }

        for (var j = 0; j < tilePoints.Count; j++)
        {
            if (tilePoints[j].GrowthInterpolant <= 0f)
            {
                var position = tilePoints[j].Position;

                if (Main.netMode != 1 && DropAfterAnimation)
                {
                    var num = Item.NewItem(new EntitySource_TileBreak(position.X, position.Y), position.ToWorldCoordinates(0f, 12f), ItemID);
                    Main.item[num].velocity = Vector2.UnitY * -0.4f;
                }

                tilePoints.RemoveAt(j);
                j--;
            }
        }
    }

    private void RenderPlantInstancesWrapper(On_Main.orig_DoDraw_Tiles_Solid orig, Main self)
    {
        if (tilePoints.Count >= 1 && !TileDisablingSystem.TilesAreUninteractable)
        {
            OverallTarget.Request(WotGUtils.ViewportArea.Width, WotGUtils.ViewportArea.Height, 0, RenderInstances);

            if (OverallTarget.TryGetTarget(0, out var target) && target != null)
            {
                Main.spriteBatch.Begin
                    (SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                if (AffectedByLight)
                {
                    LightingMaskTargetManager.PrepareShader();
                }

                Main.spriteBatch.Draw(target, Main.screenLastPosition - Main.screenPosition, Color.White);
                Main.spriteBatch.End();
            }
        }

        orig(self);
    }

    private void RenderInstances()
    {
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);

        try
        {
            var tileID = TileID;

            foreach (var tilePoint in tilePoints)
            {
                var growthInterpolant = tilePoint.GrowthInterpolant;
                var value = EasingCurves.Elastic.Evaluate(EasingType.Out, growthInterpolant.Squared());
                value = MathHelper.Lerp(value, 1f, LumUtils.InverseLerp(0.25f, 0.5f, growthInterpolant));
                var tileSafely = Framing.GetTileSafely(tilePoint.Position);
                var disappearing = !tileSafely.HasTile || tileSafely.TileType != tileID;
                InstaceRenderFunction(disappearing, growthInterpolant, value, tilePoint.Position.X, tilePoint.Position.Y, Main.spriteBatch);
            }
        }
        finally
        {
            Main.spriteBatch.End();
        }
    }

    internal void AddPointInternal(Point point)
    {
        if (!tilePoints.Any(p => p.Position == point))
        {
            tilePoints.Add(new PlantTileData(point));
        }
    }

    public void AddPoint(Point point)
    {
        if (Main.netMode == 1 && !tilePoints.Any(p => p.Position == point))
        {
            PacketManager.SendPacket<AddGenesisPlantPointPacket>(Name, point.X, point.Y);
        }

        AddPointInternal(point);
    }

    public virtual void UpdatePoint(Point p) { }

    public override void SaveWorldData(TagCompound tag)
    {
        tag["PlantPoints"] = tilePoints.Select(d => d.Position).ToList();
       
    }

    public override void LoadWorldData(TagCompound tag)
    {
        var list = tag.GetList<Point>("PlantPoints").ToList();

        for (var i = 0; i < list.Count; i++)
        {
            tilePoints.Add
            (
                new PlantTileData(list[i])
                {
                    GrowthInterpolant = 1f
                }
            );
        }
       
    }
}