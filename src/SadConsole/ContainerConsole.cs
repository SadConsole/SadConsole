﻿namespace SadConsole
{
    using SadConsole.Input;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A <see cref="Console"/> that only processes children and does not render anything.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Container")]
    public class ContainerConsole : Console
    {
        public ContainerConsole() 
            : base()
        {

        }

        public override void Draw(TimeSpan timeElapsed)
        {
            if (!IsVisible) return;
            
            var copyList = new List<Console>(Children);

            foreach (var child in copyList)
                child.Draw(timeElapsed);
        }

        public override void Update(TimeSpan timeElapsed)
        {
            if (IsPaused) return;

            var copyList = new List<Console>(Children);

            foreach (var child in copyList)
                child.Update(timeElapsed);
        }

        public override bool ProcessMouse(MouseConsoleState state)
        {
            return false;
        }

        public override bool ProcessKeyboard(SadConsole.Input.Keyboard info)
        {
            return false;
        }
    }
}
