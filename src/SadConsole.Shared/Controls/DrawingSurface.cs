﻿using SadConsole.Surfaces;

namespace SadConsole.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// A simple surface for drawing text that can be moved and sized like a control.
    /// </summary>
    [DataContract]
    public class DrawingSurface: ControlBase
    {
        /// <summary>
        /// Creates a new drawing surface control with the specified width and height.
        /// </summary>
        /// <param name="width">Width of the control.</param>
        /// <param name="height">Height of the control.</param>
        public DrawingSurface(int width, int height) : base(width, height)
        {
            Surface = new BasicNoDraw(width, height);
            base.TabStop = false;
        }

        public override void Update(TimeSpan time)
        {
            Surface.Update(time);
        }

        [OnDeserializedAttribute]
        private void AfterDeserialized(StreamingContext context)
        {
            base.TabStop = false;
        }
    }
}
