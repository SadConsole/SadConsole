﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SadConsole.Surfaces;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SadConsole.Controls;

namespace SadConsole.Renderers
{
    /// <summary>
    /// Draws a text surface to the screen.
    /// </summary>
    [DataContract]
    public class ControlsConsole : Basic
    {
        /// <summary>
        /// Controls to render.
        /// </summary>
        public List<Controls.ControlBase> Controls { get; set; } = new List<SadConsole.Controls.ControlBase>();

        /// <summary>
        /// Renders a surface to the screen.
        /// </summary>
        /// <param name="surface">The surface to render.</param>
        public override void Render(Surfaces.SurfaceBase surface, bool force = false)
        {
            RenderBegin(surface, force);
            RenderCells(surface, force);

            foreach (var control in Controls)
                RenderControlCells(control, surface.IsDirty || force);

            //RenderControls(surface, force);
            RenderTint(surface, force);
            RenderEnd(surface, force);
        }

        public void RenderControlCells(ControlBase control, bool draw = false)
        {
            if (!draw) return;
            if (control.Surface.Tint.A == 255) return;

            if (control.Surface.DefaultBackground.A != 0)
                Global.SpriteBatch.Draw(control.Parent.Font.FontImage, control.Surface.AbsoluteArea, control.Parent.Font.GlyphRects[control.Parent.Font.SolidGlyphIndex], control.Surface.DefaultBackground, 0f, Vector2.Zero, SpriteEffects.None, 0.2f);

            for (var i = 0; i < control.Surface.RenderCells.Length; i++)
            {
                ref var cell = ref control.Surface.RenderCells[i];

                if (!cell.IsVisible) continue;

                Rectangle renderRect = control.Surface.RenderRects[i];
                renderRect.Location += control.Position.ConsoleLocationToPixel(control.Parent.Font);

                if (cell.Background != Color.Transparent && cell.Background != control.Surface.DefaultBackground)
                    Global.SpriteBatch.Draw(control.Parent.Font.FontImage, renderRect, control.Parent.Font.GlyphRects[control.Parent.Font.SolidGlyphIndex], cell.Background, 0f, Vector2.Zero, SpriteEffects.None, 0.3f);

                if (cell.Foreground != Color.Transparent)
                    Global.SpriteBatch.Draw(control.Parent.Font.FontImage, renderRect, control.Parent.Font.GlyphRects[cell.Glyph], cell.Foreground, 0f, Vector2.Zero, cell.Mirror, 0.4f);

                foreach (var decorator in cell.Decorators)
                {
                    if (decorator.Color != Color.Transparent)
                        Global.SpriteBatch.Draw(control.Parent.Font.FontImage, renderRect, control.Parent.Font.GlyphRects[decorator.Glyph], decorator.Color, 0f, Vector2.Zero, decorator.Mirror, 0.5f);
                }
            }
        }

        //public virtual void RenderControls(Surfaces.SurfaceBase surface, bool force = false)
        //{
        //    if ((surface.IsDirty || force) && surface.Tint.A != 255)
        //    {
        //        int cellCount;
        //        Rectangle rect;
        //        Point point;
        //        Controls.ControlBase control;
        //        Cell cell;
        //        Font font;
        //        CellDecorator decorator;

        //        // For each control
        //        for (int i = 0; i < Controls.Count; i++)
        //        {
        //            if (Controls[i].IsVisible)
        //            {
        //                control = Controls[i];
        //                cellCount = control.Cells.Length;

        //                font = control.AlternateFont ?? surface.Font;

        //                // Draw background of each cell for the control
        //                for (int cellIndex = 0; cellIndex < cellCount; cellIndex++)
        //                {
        //                    cell = control[cellIndex];

        //                    if (cell.IsVisible)
        //                    {
        //                        point = control.GetPointFromIndex(cellIndex);
        //                        point = new Point(point.X + control.Position.X, point.Y + control.Position.Y);

        //                        if (surface.ViewPort.Contains(point.X, point.Y))
        //                        {
        //                            point = new Point(point.X - surface.ViewPort.Left, point.Y - surface.ViewPort.Top);
        //                            rect = surface.RenderRects[surface.GetIndexFromPoint(point)];

        //                            if (cell.Background != Color.Transparent)
        //                                Global.SpriteBatch.Draw(font.FontImage, rect, font.GlyphRects[font.SolidGlyphIndex], cell.Background, 0f, Vector2.Zero, SpriteEffects.None, 0.23f);

        //                            if (cell.Foreground != Color.Transparent)
        //                                Global.SpriteBatch.Draw(font.FontImage, rect, font.GlyphRects[cell.Glyph], cell.Foreground, 0f, Vector2.Zero, cell.Mirror, 0.26f);

        //                            for (int d = 0; d < cell.Decorators.Length; d++)
        //                            {
        //                                decorator = cell.Decorators[d];

        //                                if (decorator.Color != Color.Transparent)
        //                                    Global.SpriteBatch.Draw(surface.Font.FontImage, surface.RenderRects[i], surface.Font.GlyphRects[decorator.Glyph], decorator.Color, 0f, Vector2.Zero, decorator.Mirror, 0.5f);
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        [OnDeserialized]
        private void AfterDeserialized(StreamingContext context)
        {
        }
    }
}
