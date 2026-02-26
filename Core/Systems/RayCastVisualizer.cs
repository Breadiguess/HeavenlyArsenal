namespace HeavenlyArsenal.Core.Systems
{
    internal class RayCastVisualizer : ModSystem
    {
        public static List<Raycast> Raycasts = new();

        public static List<RaycastText> Texts = new();
        public override void PostUpdateEverything()
        {
            for (int i = Raycasts.Count - 1; i >= 0; i--)
            {
                Raycasts[i].Update();

                if (Raycasts[i].IsDead)
                    Raycasts.RemoveAt(i);
            }

            for (int i = Texts.Count - 1; i >= 0; i--)
            {
                Texts[i].Update();

                if (Texts[i].IsDead)
                    Texts.RemoveAt(i);
            }
        }

        public override void PostDrawTiles()
        {
            foreach (var rays in Raycasts)
                rays.DrawDust();

            foreach (var rays in Texts)
                rays.DrawDust();
        }
    }

    public class Raycast
    {
        public Vector2 Start;

        public Vector2 End;

        public int TimeLeft = 4;
        private Color color;

        public bool IsDead => TimeLeft <= 0;
        public Raycast(Vector2 start, Vector2 end, Color color, int TimeLeft = 4)
        {
            this.Start = start;
            this.End = end;
            this.color = color;
            this.TimeLeft = TimeLeft;
        }

       

        public void Update()
        {
            TimeLeft--;
        }

        public void DrawDust()
        {
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, default, default, null, Main.GameViewMatrix.TransformationMatrix);

            Utils.DrawLine(Main.spriteBatch, Start, End, color, color, 2);

            Main.spriteBatch.End();
        }
    }
    public class RaycastText
    {
        public Vector2 WorldAnchor;

        public Color color;
        public string Value;
        public int TimeLeft = 2;
        public bool IsDead => TimeLeft <= 0;
        public RaycastText(string value, Vector2 WorldAnchor, Color color)
        {
            this.Value = value;
            this.WorldAnchor = WorldAnchor;
            this.color = color;
        }
        public void Update()
        {
            TimeLeft--;
        }
        public void DrawDust()
        {
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, default, default, null, Main.GameViewMatrix.TransformationMatrix);

            
            Utils.DrawBorderString(Main.spriteBatch, Value, WorldAnchor - Main.screenPosition, color, 0.4f);

            Main.spriteBatch.End();
        }
    }
}
