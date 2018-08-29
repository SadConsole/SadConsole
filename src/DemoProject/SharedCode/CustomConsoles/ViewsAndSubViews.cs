﻿using Microsoft.Xna.Framework;
using ColorHelper = Microsoft.Xna.Framework.Color;

using SadConsole.Surfaces;
using System;
using Console = SadConsole.Console;
using SadConsole;
using SadConsole.Input;

namespace StarterProject.CustomConsoles
{

    // Using a ConsoleList which lets us group multiple consoles 
    // into a single processing entity
    class ViewsAndSubViews: Console
    {
        Console mainView;
        Console subView;
        
        public ViewsAndSubViews(): base(1,1)
        {
            mainView = new Console(60, 23);
            subView = Console.FromSurface(mainView.GetViewSurface(new Rectangle(0, 0, 20, 23)));

            IsVisible = false;
            UseMouse = true;

            mainView.DrawLine(new Point(59, 0), new Point(59, 22), Color.White, glyph: ConnectedLineThin[(int)ConnectedLineIndex.Left]);

            // Setup main view
            mainView.Position = new Point(0, 2);
            mainView.MouseMove += (s, e) => { if (e.MouseState.Mouse.LeftButtonDown) { e.MouseState.Cell.Background = Color.Blue; mainView.IsDirty = true; } };
            mainView.DirtyChanged += (s, e) => subView.IsDirty = true;

            // Setup sub view
            subView.Position = new Point(60, 2);
            //subView.SetViewFromSurface(new Rectangle(0, 0, 20, 23), mainView);
            subView.MouseMove += (s, e) => { if (e.MouseState.Mouse.LeftButtonDown) { e.MouseState.Cell.Background = Color.Red; subView.IsDirty = true; } };
            subView.DirtyChanged += (s, e) => mainView.IsDirty = true;

            // Ad the consoles to the list.
            Children.Add(mainView);
            Children.Add(subView);
        }

        public override bool ProcessMouse(MouseConsoleState state)
        {
            // Process mouse for each console
            var childState = new MouseConsoleState(mainView, state.Mouse);

            if (childState.IsOnConsole)
                return mainView.ProcessMouse(childState);

            childState = new MouseConsoleState(subView, state.Mouse);

            if (childState.IsOnConsole)
                return subView.ProcessMouse(childState);

            return false;
        }
    }
}
