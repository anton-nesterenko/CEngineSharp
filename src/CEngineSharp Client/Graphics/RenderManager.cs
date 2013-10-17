﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEngineSharp_Client.Graphics
{
    public static class RenderManager
    {
        private static Renderer _renderer;

        public static Renderer CurrentRenderer { get { return _renderer; } private set { _renderer = value; } }

        private static RenderStates _renderstate;

        public static void Init()
        {
            _renderer = new MenuRenderer();
            _renderstate = RenderStates.Render_Menu;
        }

        public static void SetRenderState(RenderStates renderstate)
        {
            _renderstate = renderstate;
            _renderStateChanged = true;
        }

        private static bool _renderStateChanged = false;

        public static void Render()
        {
            if (_renderStateChanged)
            {
                switch (_renderstate)
                {
                    case RenderStates.Render_Game:
                        _renderer.Unload();
                        RenderManager.CurrentRenderer = new GameRenderer(_renderer.GetWindow());
                        _renderStateChanged = false;
                        break;

                    case RenderStates.Render_Menu:
                        _renderer.Unload();
                        RenderManager.CurrentRenderer = new MenuRenderer(_renderer.GetWindow());
                        _renderStateChanged = false;
                        break;
                }
            }

            _renderer.Render();
        }
    }
}