using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace HeavenlyArsenal.Content.Subworlds.Generation.Bridges;

public class BridgeGenerationSettings
{
    /// <summary>
    /// The width of beams for the bridge before the shrine.
    /// </summary>
    public int BridgeBeamWidth
    {
        get
        {
            /*
                round(height * abs(sin(pi * x / width))) < 1
                height * abs(sin(pi * x / width)) < 1
                abs(sin(pi * x / width)) < 1 / height
                sin(pi * x / width) < 1 / height
                sin(pi * x / width) = 1 / height
                pi * x / width = arcsin(1 / height)
                x = arcsin(1 / height) * width / pi
             */

            // For a bit of artistic preference, 0.5 will be used instead of 1 like in the original equation, making the beams a bit thinner.
            float intermediateArcsine = MathF.Asin(0.5f / BridgeArchHeight);
            int beamWidth = (int)MathF.Round(intermediateArcsine * BridgeArchWidth / MathHelper.Pi);
            if (BridgeArchHeight == 0)
                beamWidth = BridgeArchWidth / 33;

            return beamWidth;
        }
    }

    /// <summary>
    /// The height of beams for the bridge before the shrine.
    /// </summary>
    public int BridgeBeamHeight;

    /// <summary>
    /// The width of arches on the bridge.
    /// </summary>
    public int BridgeArchWidth;

    /// <summary>
    /// The amount of horizontal coverage that ropes underneath the bridge have, in tile coordinates.
    /// </summary>
    public int BridgeUndersideRopeWidth;

    /// <summary>
    /// The amount of vertical coverage that ropes beneath the bridge should have due to sag, in tile coordinates.
    /// </summary>
    public int BridgeUndersideRopeSag;

    /// <summary>
    /// The maximum height of arches on the bridge.
    /// </summary>
    public int BridgeArchHeight;

    /// <summary>
    /// The factor by which arches are exaggerated for big bridges with a rooftop.
    /// </summary>
    public float BridgeArchHeightBigBridgeFactor;

    /// <summary>
    /// The vertical thickness of the bridge.
    /// </summary>
    public int BridgeThickness;

    /// <summary>
    /// The amount of bridges it takes on a cyclical basis in order for a bridge to be created with a roof.
    /// </summary>
    public int BridgeRooftopsPerBridge;

    /// <summary>
    /// The amount of vertical space taken up by bottom of rooftops.
    /// </summary>
    public int BridgeRooftopDynastyWoodLayerHeight;

    /// <summary>
    /// The amount of vertical space dedicated to walls above pillars but below rooftops.
    /// </summary>
    public int BridgeRoofWallUndersideHeight;

    /// <summary>
    /// The baseline height of walls behind the bridge. Determines things such as how high rooftops are as a baseline.
    /// </summary>
    public int BridgeBackWallHeight;

    /// <summary>
    /// The set of possible rooftops that can be selected for a given roof on the bridge.
    /// </summary>
    public List<ShrineRooftopSet> BridgeRooftopConfigurations = new List<ShrineRooftopSet>();
}
