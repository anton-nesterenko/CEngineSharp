﻿using CEngineSharp_Client.Graphics;
using CEngineSharp_Client.Net;
using CEngineSharp_Client.Net.Packets;
using CEngineSharp_Client.Net.Packets.PlayerUpdatePackets;
using CEngineSharp_Client.World.Content_Managers;
using SFML.Graphics;
using SFML.Window;
using System.Collections.Generic;
using TGUI;

namespace CEngineSharp_Client.World.Entity
{
    public class Player : IEntity
    {

        public Sprite Sprite { get; set; }

        public string Name { get; set; }

        public Vector2i Position { get; set; }

        public bool CanMove { get; set; }

        public bool IsMoving { get; set; }

        public float PlayerSpeed { get; set; }

        public Camera Camera { get; set; }

        public int Hp { get; set; }

        private readonly List<Item> _inventory;

        private byte _previousStep;

        public byte Step
        {
            get
            {
                return (byte)(this.Sprite.TextureRect.Left / 32);
            }
            set
            {
                this.Sprite.TextureRect = new IntRect(value * 32, (int)this.Direction * 32, 32, 32);
            }
        }

        public Directions Direction
        {
            get
            {
                return (Directions)(this.Sprite.TextureRect.Top / 32);
            }
            set
            {
                this.Sprite.TextureRect = new IntRect(this.Step * 32, (int)value * 32, 32, 32);
            }
        }

        public Player(Texture texture, Vector2i position)
        {
            this.Position = position;
            this.Sprite = new Sprite(texture);
            _inventory = new List<Item>();

            this.Direction = Directions.Down;
            this.Step = 0;
            this.CanMove = true;
            this.PlayerSpeed = .2f;
            this.Camera = new Camera(this);
        }

        public int MouseCordToSlotNum(int mouseX, int mouseY)
        {
            var gameRenderer = (RenderManager.Instance.CurrentRenderer as GameRenderer);

            var invenPosX = (int)gameRenderer.Gui.Get<Picture>("picInventory").Position.X;
            var invenPosY = (int)gameRenderer.Gui.Get<Picture>("picInventory").Position.Y;

            var x = (mouseX - invenPosX) / 32;
            var y = (mouseY - invenPosY) / 32;

            return (x + (y * 5));
        }

        public Item GetInventoryItem(int slotNum)
        {
            if (slotNum >= _inventory.Count || slotNum < 0)
                return null;

            return _inventory[slotNum];
        }

        public Item GetInventoryItem(int invenX, int invenY)
        {
            return this.GetInventoryItem(this.MouseCordToSlotNum(invenX, invenY));
        }

        public void ClearInventory()
        {
            _inventory.Clear();
        }

        public void AddInventoryItem(Item item)
        {
            _inventory.Add(item);

            int slotNum = _inventory.Count - 1;

            var gameRenderer = (RenderManager.Instance.CurrentRenderer as GameRenderer);

            if (gameRenderer != null)
            {
                var invenPosX = gameRenderer.Gui.Get<Picture>("picInventory").Position.X;
                var invenPosY = gameRenderer.Gui.Get<Picture>("picInventory").Position.Y;

                var itemPosY = (int)invenPosY + (32 * (slotNum / 5));

                var itemPosX = (int)invenPosX + (32 * (slotNum - (((itemPosY - (int)invenPosY) / 32) * 5)));

                item.Sprite.Position = new Vector2f(itemPosX, itemPosY);
            }
        }

        public void TryDropInventoryItem(int mouseX, int mouseY)
        {
            var slotNum = this.MouseCordToSlotNum(mouseX, mouseY);

            if (this.GetInventoryItem(slotNum) == null) return;

            var dropItemPacket = new DropItemPacket();
            dropItemPacket.WriteData(slotNum);
            NetManager.Instance.SendPacket(dropItemPacket);
        }

        public void Warp(int newX, int newY, Directions direction)
        {
            this.Direction = direction;

            MapManager.Map.GetTile(this.Position.X, this.Position.Y).IsOccupied = false;

            this.Position = new Vector2i(newX, newY);

            MapManager.Map.GetTile(newX, newY).IsOccupied = true;

            this.Sprite.Position = new Vector2f(newX * 32, newY * 32);
        }

        public void Move(int newX, int newY, Directions direction)
        {
            this.Direction = direction;

            MapManager.Map.GetTile(this.Position.X, this.Position.Y).IsOccupied = false;

            this.Position = new Vector2i(newX, newY);

            MapManager.Map.GetTile(newX, newY).IsOccupied = true;

            switch (this.Step)
            {
                case 0:
                    this._previousStep = 0;
                    this.Step++;
                    break;
                case 2:
                    this._previousStep = 2;
                    this.Step--;
                    break;
                case 1:
                    if (this._previousStep == 2)
                        this.Step--;
                    else
                        this.Step++;
                    break;
            }
        }

        public void TryMove()
        {
            if (this.IsMoving && this.CanMove)
            {
                var movementPacket = new PlayerMovementPacket();

                int x = this.Position.X;
                int y = this.Position.Y;

                switch (this.Direction)
                {
                    case Directions.Down:

                        // Client side check to see if the tile is blocked.
                        if (y < (MapManager.Map.Height - 1) && !MapManager.Map.GetTile(x, y + 1).Blocked && !MapManager.Map.GetTile(x, y + 1).IsOccupied)
                        {
                            MapManager.Map.GetTile(x, y).IsOccupied = false;
                            y += 1;
                            movementPacket.WriteData(x, y, this.Direction);
                            NetManager.Instance.SendPacket(movementPacket);
                            this.CanMove = false;
                        }

                        break;

                    case Directions.Up:
                        // Client side check to see if the tile is blocked.
                        if (y > 0 && !MapManager.Map.GetTile(x, y - 1).Blocked && !MapManager.Map.GetTile(x, y - 1).IsOccupied)
                        {
                            MapManager.Map.GetTile(x, y).IsOccupied = false;
                            y -= 1;
                            movementPacket.WriteData(x, y, this.Direction);
                            NetManager.Instance.SendPacket(movementPacket);
                            this.CanMove = false;
                        }

                        break;

                    case Directions.Right:
                        // Client side check to see if the tile is blocked.
                        if (x < (MapManager.Map.Width - 1) && !MapManager.Map.GetTile(x + 1, y).Blocked && !MapManager.Map.GetTile(x + 1, y).IsOccupied)
                        {
                            MapManager.Map.GetTile(x, y).IsOccupied = false;
                            x += 1;
                            movementPacket.WriteData(x, y, this.Direction);
                            NetManager.Instance.SendPacket(movementPacket);
                            this.CanMove = false;
                        }

                        break;

                    case Directions.Left:
                        // Client side check to see if the tile is blocked.
                        if (x > 0 && !MapManager.Map.GetTile(x - 1, y).Blocked && !MapManager.Map.GetTile(x - 1, y).IsOccupied)
                        {
                            MapManager.Map.GetTile(x, y).IsOccupied = false;
                            x -= 1;
                            movementPacket.WriteData(x, y, this.Direction);
                            NetManager.Instance.SendPacket(movementPacket);
                            this.CanMove = false;
                        }

                        break;
                }

                this.Position = new Vector2i(x, y);
            }
        }


        public void Update(GameTime gameTime)
        {
            var x = this.Position.X * 32;
            var y = this.Position.Y * 32;

            if (x == this.Sprite.Position.X && y == this.Sprite.Position.Y)
            {
                this.CanMove = true;
                this.Camera.Update(this.PlayerSpeed, gameTime);
                return;
            }

            var position = this.Sprite.Position;

            if (x < this.Sprite.Position.X)
            {
                position.X = this.Sprite.Position.X - (this.PlayerSpeed * gameTime.UpdateTime);

                if (x >= position.X)
                {
                    this.CanMove = true;
                    position.X = x;
                }
            }
            else if (x > this.Sprite.Position.X)
            {
                position.X = this.Sprite.Position.X + (this.PlayerSpeed * gameTime.UpdateTime);

                if (x <= position.X)
                {
                    this.CanMove = true;
                    position.X = x;
                }
            }

            if (y < this.Sprite.Position.Y)
            {
                position.Y = this.Sprite.Position.Y - (this.PlayerSpeed * gameTime.UpdateTime);

                if (y >= position.Y)
                {
                    this.CanMove = true;
                    position.Y = y;
                }
            }
            else if (y > this.Sprite.Position.Y)
            {
                position.Y = this.Sprite.Position.Y + (this.PlayerSpeed * gameTime.UpdateTime);

                if (y <= position.Y)
                {
                    this.CanMove = true;
                    position.Y = y;
                }
            }

            this.Sprite.Position = position;

            this.Camera.Update(this.PlayerSpeed, gameTime);
        }

        public void Draw(RenderTarget target)
        {
            target.Draw(this.Sprite);
        }

        public void DrawInventory(RenderWindow window)
        {
            foreach (var item in _inventory)
                window.Draw(item.Sprite);
        }
    }
}