namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Artillery_Crab
{
    public partial class BloodCrab
    {

        public ButterflyAttachPoint[] ButterflyAttachPoints = new ButterflyAttachPoint[12];

        public void UpdateButterflyAttachPoints()
        {
            for (int i = 0; i < ButterflyAttachPoints.Length; i++)
            {
                var Atp = ButterflyAttachPoints[i];

                int x = i % 2 == 0 ? 3 : 0;
                int sign = i % 2 == 0 ? 1 : -1;
                Vector2 FirstPos = _bloodCrabLegs[x].Skeleton.Position(2);
                Vector2 SecondPos = _bloodCrabLegs[x].Skeleton.Position(3);

                if (Atp != null)
                {
                    Atp.Position = Vector2.Lerp(FirstPos, SecondPos, i/(float)ButterflyAttachPoints.Length * 0.7f);
                    Atp.Position += new Vector2(-10 * MathF.Sin(i) * sign, sign == -1? 8: 10).RotatedBy(FirstPos.AngleTo(SecondPos) + MathHelper.PiOver2);
                }
            }
        }

        private void InitializeAttachPoints()
        {
            for (int i = 0; i < ButterflyAttachPoints.Length; i++)
            {
                ButterflyAttachPoints[i] = new(NPC.Center);
            }
        }

        public sealed class ButterflyAttachPoint
        {
            public bool Filled = false;
            public Vector2 Position;
            public int AttacheeIndex = -1;
            public ButterflyAttachPoint(Vector2 position)
            {
                Position = position;
            }

        }
    }
}
