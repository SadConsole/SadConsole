﻿using Microsoft.Xna.Framework;

using System;
using System.Collections.Generic;
using System.Text;
using SadConsole.Surfaces;
using SadConsole.Input;

namespace StarterProject.CustomConsoles
{
    class ScrollableConsole : SadConsole.Console
    {
        SadConsole.ControlsConsole controlsContainer;
        SadConsole.Controls.ScrollBar scrollBar;

        int scrollingCounter;

        public ScrollableConsole(int width, int height, int bufferHeight) : base(width - 1, bufferHeight)
        {
            controlsContainer = new SadConsole.ControlsConsole(1, height);

            textSurface.RenderArea = new Rectangle(0, 0, width, height);

            scrollBar = SadConsole.Controls.ScrollBar.Create(System.Windows.Controls.Orientation.Vertical, height);
            scrollBar.IsEnabled = false;
            scrollBar.ValueChanged += ScrollBar_ValueChanged;

            controlsContainer.Add(scrollBar);
            controlsContainer.Position = new Point(Position.X + width - 1, Position.Y);
            controlsContainer.IsVisible = true;
            controlsContainer.MouseCanFocus = false;
            controlsContainer.ProcessMouseWithoutFocus = true;

            virtualCursor.IsVisible = true;
            IsVisible = false;

            scrollingCounter = 0;
        }

        private void ScrollBar_ValueChanged(object sender, EventArgs e)
        {
            // Do our scroll according to where the scroll bar value is
            textSurface.RenderArea = new Rectangle(0, scrollBar.Value, textSurface.Width, textSurface.RenderArea.Height);
        }

        protected override void OnPositionChanged(Point oldLocation)
        {
            // Keep the controls console (which is our scroll bar) in sync with where this console is.
            controlsContainer.Position = new Point(Position.X + Width, Position.Y);
        }

        protected override void OnVisibleChanged()
        {
            // Show and hide the scroll bar.
            controlsContainer.IsVisible = this.IsVisible;
        }

        public override void Draw(TimeSpan delta)
        {
            // Draw our console and then draw the scroll bar.
            base.Draw(delta);
            controlsContainer.Draw(delta);
        }

        public override void Update(TimeSpan delta)
        {
            // Update our console and then update the scroll bar
            base.Update(delta);
            controlsContainer.Update(delta);

            // If we detect that this console has shifted the data up for any reason (like the virtual cursor reached the
            // bottom of the entire text surface, OR we reached the bottom of the render area, we need to adjust the 
            // scroll bar and follow the cursor
            if (TimesShiftedUp != 0 | virtualCursor.Position.Y == textSurface.RenderArea.Height + scrollingCounter)
            {
                // Once the buffer has finally been filled enough to need scrolling, turn on the scroll bar
                scrollBar.IsEnabled = true;

                // Make sure we've never scrolled the entire size of the buffer
                if (scrollingCounter < textSurface.Height - textSurface.RenderArea.Height)
                    // Record how much we've scrolled to enable how far back the bar can see
                    scrollingCounter += TimesShiftedUp != 0 ? TimesShiftedUp : 1;

                scrollBar.Maximum = (textSurface.Height + scrollingCounter) - textSurface.Height;

                // This will follow the cursor since we move the render area in the event.
                scrollBar.Value = scrollingCounter;

                // Reset the shift amount.
                TimesShiftedUp = 0;
            }
        }

        public override bool ProcessMouse(MouseInfo info)
        {
            // Check the scroll bar for mouse info first. If mouse not handled by scroll bar, then..
            if (!controlsContainer.ProcessMouse(info))
            {
                // Process this console normally.
                return base.ProcessMouse(info);
            }

            // If we get here, then the mouse was over the scroll bar.
            return true;
        }
    }

}
