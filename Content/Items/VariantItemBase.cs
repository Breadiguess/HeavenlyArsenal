using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items
{
    /// <summary>
    ///this exists because im a lazy fuck and just want to be able to make varianted items quickly 
    /// </summary>
    public abstract class VariantItemBase : ModItem
    {

        public virtual int MaxVariants { get; set; }

        private int subID = 0; 


        public override void SetStaticDefaults()
        {

        }

        public override void SetDefaults()
        {
            subID = 0;
        }

        public override bool CanStackInWorld(Item item2)
        {
            var other = item2.ModItem as VariantItemBase;
            if (other != null && other.subID == 0)
                subID = 0;

            return base.CanStackInWorld(item2);
        }

        public override void OnSpawn(IEntitySource source)
        {
            if (source is EntitySource_Loot)
            {
                subID = Main.rand.Next(MaxVariants) + 1;
                switch (subID)
                {
                    case 1:
                        Item.height = 20;
                        break;
                    case 2:
                        Item.height = 22;
                        break;
                    default:
                        Item.height = 28;
                        break;
                }
            }
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            if (subID == 0)
                return true;

            Texture2D tex = ModContent.Request<Texture2D>(Texture + "World" + (subID).ToString()).Value;
            Main.EntitySpriteDraw(tex, Item.position - Main.screenPosition, null, lightColor, rotation, Vector2.Zero, scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}

