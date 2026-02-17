using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.Localization;
using Terraria.ObjectData;

namespace HeavenlyArsenal.Content.Tiles
{
    internal class GloriousBlubberPlaced : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileLighted[Type] = true;
            Main.tileLavaDeath[Type] = true;
            TileID.Sets.FramesOnKillWall[Type] = true; // Necessary since Style3x3Wall uses AnchorWall
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3Wall);
            TileID.Sets.DisableSmartCursor[Type] = true;
            TileObjectData.newTile.Width = 6;
            TileObjectData.newTile.Height = 5;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16, 16 }; //
            TileObjectData.addTile(Type);
            LocalizedText name = CreateMapEntryName();
            // name.SetDefault("Gyate Gyate");
            AddMapEntry(new Color(139, 0, 0), name);
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            
                 if (Main.rand.NextBool(222222))
                {
                    SoundEngine.PlaySound(SoundID.BloodZombie);
                   
                }
            }
        }
    
}

