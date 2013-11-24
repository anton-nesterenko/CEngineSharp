﻿using CEngineSharp_Client.Graphics;
using SFML.Graphics;
using System;

namespace CEngineSharp_Client.World
{
    public class Item
    {
        public string Name { get; private set; }

        public Sprite Sprite { get; private set; }

        public Item(string name, Texture itemTexture)
        {
            this.Name = name;
            this.Sprite = new Sprite(itemTexture);
        }
    }
}