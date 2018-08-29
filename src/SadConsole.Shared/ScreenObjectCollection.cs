﻿using Microsoft.Xna.Framework;

using System;
using System.Collections.Generic;

namespace SadConsole
{
    /// <summary>
    /// Manages the parent and children relationship for <see cref="ScreenObject"/>.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("IScreen Collection")]
    public class ScreenObjectCollection : IEnumerable<ScreenObject>, System.Collections.IEnumerable
    {
        protected List<ScreenObject> screens;
        protected WeakReference<ScreenObject> owningScreen;

        public int Count => screens.Count;

        /// <summary>
        /// When true, the collection cannot be modified.
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// Gets or sets a child object for this collection.
        /// </summary>
        /// <param name="index">The index of the child object.</param>
        /// <returns></returns>
        public ScreenObject this[int index]
        {
            get => screens[index];
            set
            {
                if (IsLocked) throw new Exception("The collection is locked and cannot be modified.");
                if (screens[index] == value) return;

                var oldConsole = screens[index];
                screens[index] = value;
                RemoveScreenParent(oldConsole);
                SetScreenParent(value);
            }
        }

        /// <summary>
        /// Creates a new screen object collection and parents it to the <paramref name="owner"/> object.
        /// </summary>
        /// <param name="owner">The owning object of this collection.</param>
        public ScreenObjectCollection(ScreenObject owner)
        {
            screens = new List<ScreenObject>();
            owningScreen = new WeakReference<ScreenObject>(owner);
        }

        /// <summary>
        /// Removes all consoles.
        /// </summary>
        public void Clear()
        {
            if (IsLocked) throw new Exception("The collection is locked and cannot be modified.");
            screens.Clear();
        }

        /// <summary>
        /// Returns true if this console list contains the specified <paramref name="screen"/>.
        /// </summary>
        /// <param name="screen">The console to search for.</param>
        /// <returns></returns>
        public bool Contains(ScreenObject screen)
        {
            return screens.Contains(screen);
        }

        /// <summary>
        /// When true, indicates that the <paramref name="screen"/> is at the top of the collection stack.
        /// </summary>
        /// <param name="screen">The screen object to check.</param>
        /// <returns>True when the object is on top.</returns>
        public bool IsTop(ScreenObject screen)
        {
            if (screens.Contains(screen))
                return screens.IndexOf(screen) == screens.Count - 1;

            return false;
        }

        /// <summary>
        /// Adds a new child object to this collection.
        /// </summary>
        /// <exception cref="Exception">Thrown when the <see cref="IsLocked"/> property is set to true.</exception>
        /// <param name="screen">The child object.</param>
        public void Add(ScreenObject screen)
        {
            if (IsLocked) throw new Exception("The collection is locked and cannot be modified.");

            if (!screens.Contains(screen))
                screens.Add(screen);

            SetScreenParent(screen);
        }

        /// <summary>
        /// Inserts a child object at the specified <paramref name="index"/>.
        /// </summary>
        /// <exception cref="Exception">Thrown when the <see cref="IsLocked"/> property is set to true.</exception>
        /// <param name="index">The 0-based index to insert the object at.</param>
        /// <param name="screen">The child object.</param>
        public void Insert(int index, ScreenObject screen)
        {
            if (IsLocked) throw new Exception("The collection is locked and cannot be modified.");

            if (!screens.Contains(screen))
                screens.Insert(index, screen);

            SetScreenParent(screen);
        }

        /// <summary>
        /// Removes a new child object from this collection.
        /// </summary>
        /// <exception cref="Exception">Thrown when the <see cref="IsLocked"/> property is set to true.</exception>
        /// <param name="screen">The child object.</param>
        public void Remove(ScreenObject screen)
        {
            if (IsLocked) throw new Exception("The collection is locked and cannot be modified.");

            if (screens.Contains(screen))
                screens.Remove(screen);

            RemoveScreenParent(screen);
        }

        /// <summary>
        /// Moves the specified <paramref name="screen"/>  to the top of the collection.
        /// </summary>
        /// <param name="screen">The child object.</param>
        public void MoveToTop(ScreenObject screen)
        {
            if (screens.Contains(screen))
            {
                screens.Remove(screen);
                screens.Add(screen);
            }
        }

        /// <summary>
        /// Moves the specified <paramref name="screen"/>  to the bottom of the collection.
        /// </summary>
        /// <param name="screen">The child object.</param>
        public void MoveToBottom(ScreenObject screen)
        {
            if (screens.Contains(screen))
            {
                screens.Remove(screen);
                screens.Insert(0, screen);
            }
        }

        /// <summary>
        /// Gets the 0-based index of the <paramref name="screen"/>.
        /// </summary>
        /// <param name="screen">The child object.</param>
        public int IndexOf(ScreenObject screen)
        {
            return screens.IndexOf(screen);
        }

        //public IConsole NextValidConsole(IConsole currentConsole)
        //{
        //    if (screens.Contains(currentConsole))
        //    {
        //        var index = screens.IndexOf(currentConsole);
        //        var counter = 0;
        //        do
        //        {
        //            index++;
        //            counter++;

        //            if (index == screens.Count)
        //                index = 0;

        //            if (screens[index].IsVisible)
        //            {
        //                return screens[index];
        //            }
        //        } while (counter < screens.Count);

        //    }

        //    return null;
        //}

        //public IConsole PreviousValidConsole(IConsole currentConsole)
        //{
        //    if (screens.Contains(currentConsole))
        //    {
        //        var index = screens.IndexOf(currentConsole);
        //        var counter = 0;
        //        do
        //        {
        //            index--;
        //            counter++;

        //            if (index == -1)
        //                index = screens.Count - 1;

        //            if (screens[index].IsVisible)
        //            {
        //                return screens[index];
        //            }
        //        } while (counter < screens.Count);

        //    }

        //    return null;
        //}

        private void SetScreenParent(ScreenObject screen)
        {
            if (owningScreen.TryGetTarget(out ScreenObject owner) && screen != owner)
                screen.Parent = owner;
        }

        private void RemoveScreenParent(ScreenObject screen)
        {
            if (owningScreen.TryGetTarget(out ScreenObject owner) && screen == owner)
                screen.Parent = null;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return screens.GetEnumerator();
        }

        /// <inheritdoc />
        public IEnumerator<ScreenObject> GetEnumerator()
        {
            return screens.GetEnumerator();
        }

    }
}