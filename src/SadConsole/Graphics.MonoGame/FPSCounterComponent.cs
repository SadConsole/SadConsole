﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Text;

namespace SadConsole
{
    public partial class Game
    {
        /// <summary>
        /// A component to draw how many frames per second the engine is performing at.
        /// </summary>
        public class FPSCounterComponent : DrawableGameComponent
        {
            Console surface;

            int frameRate = 0;
            int frameCounter = 0;
            TimeSpan elapsedTime = TimeSpan.Zero;


            public FPSCounterComponent(Microsoft.Xna.Framework.Game game)
                : base(game)
            {
                surface = new Console(30, 1) {DefaultBackground = Color.Black};
                surface.Clear();
                DrawOrder = 8;
                Global.GraphicsDevice.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
            }


            public override void Update(GameTime gameTime)
            {
                elapsedTime += gameTime.ElapsedGameTime;

                if (elapsedTime > TimeSpan.FromSeconds(1))
                {
                    elapsedTime -= TimeSpan.FromSeconds(1);
                    frameRate = frameCounter;
                    frameCounter = 0;
                }
            }


            public override void Draw(GameTime gameTime)
            {
                frameCounter++;
                surface.Clear();
                surface.Print(0, 0, $"fps: {this.frameRate}", Color.White, Color.Black);
                surface.Draw(gameTime.ElapsedGameTime);

                Global.GraphicsDevice.SetRenderTarget(null);
                Global.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
                Global.SpriteBatch.Draw(surface.LastRenderResult, Vector2.Zero, Color.White);
                Global.SpriteBatch.End();
            }
        }
    }
}
