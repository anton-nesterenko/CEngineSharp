﻿using CEngineSharp_Client.Graphics.TextureManager;
using CEngineSharp_Client.Net;
using CEngineSharp_Client.Net.Packets;
using CEngineSharp_Client.World;
using CEngineSharp_Client.World.Content_Managers;
using CEngineSharp_Client.World.Entity;
using SFML.Graphics;
using SFML.Window;
using TGUI;

namespace CEngineSharp_Client.Graphics
{
    public class GameRenderer : Renderer
    {
        private int fpsCounter;
        private int fpsTimer;
        private bool inventoryVisible;

        public GameTextureManager TextureManager { get; private set; }


        public GameRenderer(RenderWindow window)
            : base(window)
        {
            _window.KeyPressed += _window_KeyPressed;
            _window.KeyReleased += _window_KeyReleased;
            _window.Resized += _window_Resized;
            _window.MouseButtonPressed += _window_MouseButtonPressed;

            this.TextureManager = new GameTextureManager();
            this.TextureManager.LoadTextures();

            Globals.CurrentResolutionWidth = (int)_window.GetView().Size.X;
            Globals.CurrentResolutionHeight = (int)_window.GetView().Size.Y;

            this.CanRender = true;
        }

        public override void Render(GameTime gameTime)
        {
            _window.DispatchEvents();

            _window.Clear();

            if (Globals.InGame)
            {
                if (MapManager.Map != null)
                    MapManager.Map.Draw(_window);

                this.Gui.Draw();

                if (this.inventoryVisible)
                    GameWorld.GetPlayer(Globals.MyIndex).DrawInventory(_window);

                var healthBar = this.Gui.Get<LoadingBar>("healthBar");
                if (GameWorld.GetPlayer(Globals.MyIndex).HP != healthBar.Value)
                {
                    healthBar.Value = GameWorld.GetPlayer(Globals.MyIndex).HP;
                    healthBar.Text = "HP: " + healthBar.Value + "/" + healthBar.Maximum;
                }
            }

            _window.Display();

            #region Fps Logic

            if (this.fpsTimer < gameTime.TotalElapsedTime)
            {
                this.Gui.Get<Label>("labelFps").Text = "Fps: " + this.fpsCounter;
                this.fpsCounter = 0;
                this.fpsTimer = (int)gameTime.TotalElapsedTime + 1000;
            }

            this.fpsCounter++;

            #endregion Fps Logic
        }

        public void AddChatMessage(string message, Color color)
        {
            try
            {
                ChatBox chatBox = this.Gui.Get<ChatBox>("textChat");

                // if (chatBox.GetLineAmount() > Constants.MAX_CHAT_LINES)
                //  chatBox.RemoveLine(0);

                chatBox.AddLine(message, color);
            }
            catch (Exception)
            {
            }
        }

        public void DisplayAlert(string title, string alertMessage, int x, int y, Color color)
        {
            var messageBoxAlert = this.Gui.Get<MessageBox>("messageBoxAlert");

            messageBoxAlert.Title = title;
            messageBoxAlert.Position = new Vector2f(x, y);
            messageBoxAlert.TextColor = color;
            messageBoxAlert.Visible = true;
            messageBoxAlert.Get<Label>("labelAlert").Text = alertMessage;
            messageBoxAlert.Get<Label>("labelAlert").TextSize = 20;
            messageBoxAlert.Get<Label>("labelAlert").Position = new Vector2f(40, 50);
        }

        protected override void LoadInterface()
        {
            this.Gui.RemoveAllWidgets();

            Color color = Color.Black;
            color.A = 100;
            ChatBox textChat = this.Gui.Add(new ChatBox(themeConfigurationPath), "textChat");
            textChat.Position = new Vector2f(5, 350);
            textChat.Size = new Vector2f(400, 200);
            textChat.BackgroundColor = color;

            EditBox textMyChat = this.Gui.Add(new EditBox(themeConfigurationPath), "textMyChat");
            textMyChat.Position = new Vector2f(5, textChat.Position.Y + textChat.Size.Y + 5);
            textMyChat.Size = new Vector2f(textChat.Size.X, 40);
            textMyChat.ReturnKeyPressedCallback += textMyChat_ReturnKeyPressedCallback;
            textMyChat.Transparency = 150;

            MessageBox messageBoxAlert = this.Gui.Add(new MessageBox(themeConfigurationPath), "messageBoxAlert");
            messageBoxAlert.Visible = false;
            messageBoxAlert.ClosedCallback += messageBoxAlert_ClosedCallback;
            messageBoxAlert.Add(new Label(), "labelAlert");
            messageBoxAlert.Size = new Vector2f(400, 100);

            Label labelFps = this.Gui.Add(new Label(themeConfigurationPath), "labelFps");
            labelFps.TextSize = 30;
            labelFps.TextColor = Color.Black;
            labelFps.Position = new Vector2f(700, 10);

            Button buttonInventory = this.Gui.Add(new Button(themeConfigurationPath), "buttonInventory");
            buttonInventory.Text = "Inventory";
            buttonInventory.Position = new Vector2f(550, 600);
            buttonInventory.Size = new Vector2f(100, 25);
            buttonInventory.Visible = true;
            buttonInventory.LeftMouseClickedCallback += buttonInventory_LeftMouseClickedCallback;


            Button buttonLogout = this.Gui.Add(new Button(themeConfigurationPath), "buttonLogout");
            buttonLogout.Text = "Logout";
            buttonLogout.Position = new Vector2f(700, 600);
            buttonLogout.Size = new Vector2f(100, 25);
            buttonLogout.Visible = true;
            buttonLogout.LeftMouseClickedCallback += buttonLogout_LeftMouseClickedCallback;

            Picture picInventory = this.Gui.Add(new Picture(Constants.FILEPATH_GRAPHICS + "/Gui/Inventory.png"), "picInventory");
            picInventory.Position = new Vector2f(500, 400);
            picInventory.LeftMouseClickedCallback += picInventory_LeftMouseClickedCallback;
            picInventory.Visible = false;

            LoadingBar healthBar = this.Gui.Add(new LoadingBar(themeConfigurationPath), "healthBar");
            healthBar.Text = "HP";
            healthBar.Size = new Vector2f(300, 20);
            healthBar.Position = new Vector2f(0, 10);
            healthBar.Maximum = 100;
        }

        private void buttonInventory_LeftMouseClickedCallback(object sender, CallbackArgs e)
        {
            this.inventoryVisible = !this.inventoryVisible;

            this.Gui.Get<Picture>("picInventory").Visible = this.inventoryVisible;

        }

        private void picInventory_LeftMouseClickedCallback(object sender, CallbackArgs e)
        {
            GameWorld.GetPlayer(Globals.MyIndex).TryDropInventoryItem((int)e.Mouse.X, (int)e.Mouse.Y);
        }

        private void _window_KeyReleased(object sender, KeyEventArgs e)
        {
            switch (e.Code)
            {
                case Keyboard.Key.Up:
                    if (Globals.KeyDirection == Directions.Up)
                    {
                        Globals.KeyDirection = Directions.None;
                    }
                    break;

                case Keyboard.Key.Down:
                    if (Globals.KeyDirection == Directions.Down)
                    {
                        Globals.KeyDirection = Directions.None;
                    }
                    break;

                case Keyboard.Key.Right:
                    if (Globals.KeyDirection == Directions.Right)
                    {
                        Globals.KeyDirection = Directions.None;
                    }
                    break;

                case Keyboard.Key.Left:
                    if (Globals.KeyDirection == Directions.Left)
                    {
                        Globals.KeyDirection = Directions.None;
                    }
                    break;
            }
        }

        private void _window_KeyPressed(object sender, KeyEventArgs e)
        {
            switch (e.Code)
            {
                case Keyboard.Key.Up:
                    Globals.KeyDirection = Directions.Up;
                    break;

                case Keyboard.Key.Down:
                    Globals.KeyDirection = Directions.Down;

                    break;

                case Keyboard.Key.Right:
                    Globals.KeyDirection = Directions.Right;

                    break;

                case Keyboard.Key.Left:
                    Globals.KeyDirection = Directions.Left;
                    break;

                case Keyboard.Key.Space:
                    if (Globals.InGame)
                        MapManager.Map.TryPickupItem();
                    break;
            }
        }

        private void messageBoxAlert_ClosedCallback(object sender, CallbackArgs e)
        {
            RenderManager.SetRenderState(RenderStates.Render_Menu);
            Networking.Disconnect();
        }

        private void buttonLogout_LeftMouseClickedCallback(object sender, CallbackArgs e)
        {

            Networking.Disconnect();
        }

        private void textMyChat_ReturnKeyPressedCallback(object sender, CallbackArgs e)
        {
            var textMyChat = this.Gui.Get<EditBox>("textMyChat");
            var chatMessagePacket = new ChatMessagePacket();

            chatMessagePacket.WriteData(textMyChat.Text);
            Networking.SendPacket(chatMessagePacket);
            textMyChat.Text = "";
        }

        private void _window_MouseButtonPressed(object sender, MouseButtonEventArgs e)
        {
        }

        private void _window_Resized(object sender, SizeEventArgs e)
        {
            //Globals.CurrentResolutionHeight = (int)e.Height;
            //Globals.CurrentResolutionWidth = (int)e.Width;

            //int offsetX = (int)(e.Width - GameWorld.GetPlayer(Globals.MyIndex).Camera.ViewWidth);
            //int offsetY = (int)(e.Height - GameWorld.GetPlayer(Globals.MyIndex).Camera.ViewHeight);
        }
    }
}