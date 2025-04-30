using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace HeavenlyArsenal.Content.Items.Armor.Haemsong
{
	internal class BloodMeterSystem : ModSystem
	{
		internal UserInterface MyInterface;
		internal BloodState bloodState;
        private GameTime _lastUpdateUiGameTime;
        public override void Load()
        {
            if (!Main.dedServ)
            {
                MyInterface = new UserInterface();
                bloodState = new BloodState();
                bloodState.Activate();
            }
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1)
            {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "VectorMod: Blood Meter",
                    delegate
                    {
                        if (_lastUpdateUiGameTime != null && MyInterface?.CurrentState != null)
                        {
                            MyInterface.Draw(Main.spriteBatch, _lastUpdateUiGameTime);
                        }
                        return true;
                    },
                    InterfaceScaleType.UI));
            }
        }
        public override void Unload()
        {
            bloodState?.Deactivate();
            bloodState = null;
        }
        public override void UpdateUI(GameTime gameTime)
        {
            _lastUpdateUiGameTime = gameTime;
            if (MyInterface?.CurrentState != null)
            {
                MyInterface.Update(gameTime);
            }
            BloodMeterSystem UISystemInstance = ModContent.GetInstance<BloodMeterSystem>();
            UISystemInstance.MyInterface.SetState(UISystemInstance.bloodState);
            BloodPlayer player = Main.LocalPlayer.GetModPlayer<BloodPlayer>();
            if (player.fullBloodArmor)
            {
                ShowMyUI();
            }
            else
            {
                HideMyUI();
            }
        }
        internal void ShowMyUI()
        {
            MyInterface?.SetState(bloodState);
        }
        internal void HideMyUI()
        {
            MyInterface?.SetState(null);
        }

    }
    internal class BloodState : UIState
    {
        int bloodFrame;
        int bloodFrameTimer;
        BloodMeter bloodMeter;
        UIImage bloodImage;
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            BloodPlayer player = Main.LocalPlayer.GetModPlayer<BloodPlayer>();
            Texture2D texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Armor/Haemsong/Blood").Value;
            Texture2D texture2 = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Armor/Haemsong/BloodMeter").Value;
            Texture2D texture3 = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Armor/Haemsong/Clot").Value;
            Rectangle rect1 = bloodMeter.GetInnerDimensions().ToRectangle();
            Rectangle rect2 = new(rect1.X, rect1.Y, texture.Width, texture.Height / 7);
            Rectangle rect3 = texture2.Frame(2, 1, player.offenseMode ? 1 : 0, 0);
            Rectangle rect4 = new(rect1.X, rect1.Y, texture3.Width, texture3.Height / 2);
            int clot = 0;
            while (player.blood.Count > clot && player.blood[clot] > player.bloodExpiration)
            {
                clot++;
            }
            if (bloodMeter.IsMouseHovering)
            {
                Main.hoverItemName = player.blood.Count - clot + "% Blood" + "\n" + clot + "% Clot";
            }
            if (player.blood.Count - clot > 0)
            {
                for (int i = 0; i < player.blood.Count / 16 + 1; i++)
                {
                    spriteBatch.Draw(texture, new Rectangle(rect2.X + 18, rect2.Y - player.blood.Count / 2 + i * 8 + 80, rect2.Width, rect2.Height), texture.Frame(1, 7, 0, i == 0 ? bloodFrame : 6), Color.White);
                }
            }
            for (int i = 0; i < clot / 20 + 1; i++)
            {
                spriteBatch.Draw(texture3, new Rectangle(rect4.X + 18, rect4.Y + 80 - clot / 2 + i * 10, rect4.Width, rect4.Height), texture3.Frame(1, 2, 0, i == 0 ? 0 : 1), Color.White);
            }
            spriteBatch.Draw(texture2, rect1, rect3, Color.White);
        }
        public override void OnInitialize()
        {
            bloodMeter = new();
            bloodMeter.Height.Set(90, 0);
            bloodMeter.Width.Set(50, 0);
            bloodMeter.HAlign = 0.44f;
            bloodMeter.VAlign = 0.03f;
            bloodImage = new(ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Armor/Haemsong/BloodMeterBG"));
            bloodImage.Height.Set(90, 0);
            bloodImage.Width.Set(50, 0);
            bloodImage.HAlign = bloodImage.VAlign = 0.5f;
            bloodMeter.Append(bloodImage);
            Append(bloodMeter);
            bloodFrame = 0;
            bloodFrameTimer = 0;
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (++bloodFrameTimer > 11)
            {
                bloodFrameTimer = 0;
                if (++bloodFrame > 5)
                {
                    bloodFrame = 0;
                }
            }
        }
    }
    public class BloodMeter : UIElement
    {
        private bool dragging;
        private Vector2 offset;
        private void DragStart(UIMouseEvent evt)
        {
            offset = new Vector2(evt.MousePosition.X - Left.Pixels, evt.MousePosition.Y - Top.Pixels);
            dragging = true;
        }
        private void DragStop(UIMouseEvent evt)
        {
            Vector2 endMousePosition = evt.MousePosition;
            dragging = false;
            Left.Set(endMousePosition.X - offset.X, 0f);
            Top.Set(endMousePosition.Y - offset.Y, 0f);
            Recalculate();
        }
        public override void LeftMouseDown(UIMouseEvent evt)
        {
            base.LeftMouseDown(evt);
            if (evt.Target == this)
            {
                DragStart(evt);
            }
        }
        public override void LeftMouseUp(UIMouseEvent evt)
        {
            base.LeftMouseUp(evt);
            if (evt.Target == this)
            {
                DragStop(evt);
            }
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (ContainsPoint(Main.MouseScreen))
            {
                Main.LocalPlayer.mouseInterface = true;
            }
            if (dragging)
            {
                Left.Set(Main.mouseX - offset.X, 0f);
                Top.Set(Main.mouseY - offset.Y, 0f);
                Recalculate();
            }
            var parentSpace = Parent.GetDimensions().ToRectangle();
            if (!GetDimensions().ToRectangle().Intersects(parentSpace))
            {
                Left.Pixels = Utils.Clamp(Left.Pixels, 0, parentSpace.Right - Width.Pixels);
                Top.Pixels = Utils.Clamp(Top.Pixels, 0, parentSpace.Bottom - Height.Pixels);
                Recalculate();
            }
        }
    }
}
