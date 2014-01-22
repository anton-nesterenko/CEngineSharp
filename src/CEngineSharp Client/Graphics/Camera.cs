﻿﻿using CEngineSharp_Client.World.Content_Managers;
using CEngineSharp_Client.World.Entity;
using SFML.Graphics;
using SFML.Window;
using System;

namespace CEngineSharp_Client.Graphics
{
    public class Camera
    {
        private View _view;

        public Player Target { get; set; }

        public float ViewWidth { get { return _view.Size.X; } }

        public float ViewHeight { get { return _view.Size.Y; } }

        public float ViewLeft { get; set; }

        public float ViewTop { get; set; }

        public Camera(Player target)
        {
            _view = new View((RenderManager.Instance.CurrentRenderer).GetWindow().DefaultView);
            this.Target = target;
        }

        public void SetCenter(Vector2f center)
        {
            this.ViewLeft += center.X - this._view.Center.X;
            this.ViewTop += center.Y - this._view.Center.Y;
            this._view.Center = center;
        }

        public Vector2f GetCenter()
        {
            return this._view.Center;
        }

        public View GetView()
        {
            return _view;
        }

        public void SetView(View view)
        {
            _view = view;
        }

        public void Zoom(float factor)
        {
            _view.Zoom(factor);
        }

        public void SnapToTarget()
        {
            var x = this.Target.Position.X * 32;
            var y = this.Target.Position.Y * 32;

            var center = this.GetCenter();

            if (x >= (RenderManager.Instance.CurrentResolutionWidth / 2) && x <= (MapManager.Map.Width * 32) - (RenderManager.Instance.CurrentResolutionWidth / 2))
            {
                center.X = x;
            }
            else if (x > ((MapManager.Map.Width * 32) - (RenderManager.Instance.CurrentResolutionWidth / 2)))
            {
                center.X = (MapManager.Map.Width * 32) - (RenderManager.Instance.CurrentResolutionWidth / 2);
            }

            if (y >= (RenderManager.Instance.CurrentResolutionHeight / 2) && y <= (MapManager.Map.Height * 32) - (RenderManager.Instance.CurrentResolutionHeight / 2))
            {
                center.Y = y;
            }
            else if (y > ((MapManager.Map.Height * 32) - (RenderManager.Instance.CurrentResolutionHeight / 2)))
            {
                center.Y = (MapManager.Map.Height * 32) - (RenderManager.Instance.CurrentResolutionHeight / 2);
            }

            this.SetCenter(center);
        }

        public void Update(float playerSpeed)
        {
            this.SetCenter(this.Target.Sprite.Position);

            var x = this.Target.Sprite.Position.X;
            var y = this.Target.Sprite.Position.Y;

            var center = this.GetCenter();

            if (x >= (RenderManager.Instance.CurrentResolutionWidth / 2) && x <= (MapManager.Map.Width * 32) - (RenderManager.Instance.CurrentResolutionWidth / 2))
            {
                if (x < _view.Center.X)
                {
                    center.X = _view.Center.X - playerSpeed;
                }
                else if (x > _view.Center.X)
                {
                    center.X = _view.Center.X + playerSpeed;
                }
            }

            if (y >= (RenderManager.Instance.CurrentResolutionHeight / 2) && y <= (MapManager.Map.Height * 32) - (RenderManager.Instance.CurrentResolutionHeight / 2))
            {
                if (y < _view.Center.Y)
                {
                    center.Y = _view.Center.Y - playerSpeed;
                }
                else if (y > _view.Center.Y)
                {
                    center.Y = _view.Center.Y + playerSpeed;
                }
            }

            if (center.X < (RenderManager.Instance.CurrentResolutionWidth / 2))
                center.X = RenderManager.Instance.CurrentResolutionWidth / 2;

            if (x >= (MapManager.Map.Width * 32) - (RenderManager.Instance.CurrentResolutionWidth / 2))
                center.X = (MapManager.Map.Width * 32) - (RenderManager.Instance.CurrentResolutionWidth / 2);

            if (center.Y < (RenderManager.Instance.CurrentResolutionHeight / 2))
                center.Y = RenderManager.Instance.CurrentResolutionHeight / 2;

            if (y >= (MapManager.Map.Height * 32) - (RenderManager.Instance.CurrentResolutionHeight / 2))
                center.Y = (MapManager.Map.Height * 32) - (RenderManager.Instance.CurrentResolutionHeight / 2);

            this.SetCenter(center);
        }
    }
}