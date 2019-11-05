﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using SadConsole.Components;
using SadRogue.Primitives;

namespace SadConsole
{
    /// <summary>
    /// A generic object processed by SadConsole. Provides parent/child, components, and position.
    /// </summary>
    public class ScreenObject
    {
        private ScreenObject _parentObject;
        private Point _position;
        private bool _isVisible = true;
        private bool _isEnabled = true;

        /// <summary>
        /// Raised when the <see cref="Parent"/> property changes.
        /// </summary>
        public event EventHandler<NewOldValueEventArgs<ScreenObject>> ParentChanged;

        /// <summary>
        /// Raised when the <see cref="Position"/> property changes.
        /// </summary>
        public event EventHandler<NewOldValueEventArgs<Point>> PositionChanged;

        /// <summary>
        /// Raised when the <see cref="IsVisible"/> property changes.
        /// </summary>
        public event EventHandler VisibleChanged;

        /// <summary>
        /// Raised when the <see cref="IsEnabled"/> property changes.
        /// </summary>
        public event EventHandler EnabledChanged;

        /// <summary>
        /// A filtered list from <see cref="Components"/> where <see cref="IComponent.IsUpdate"/> is <see langword="true"/>.
        /// </summary>
        protected List<IComponent> ComponentsUpdate;

        /// <summary>
        /// A filtered list from <see cref="Components"/> where <see cref="IComponent.IsDraw"/> is <see langword="true"/>.
        /// </summary>
        protected List<IComponent> ComponentsDraw;

        /// <summary>
        /// A filtered list from <see cref="Components"/> where <see cref="IComponent.IsMouse"/> is <see langword="true"/>.
        /// </summary>
        protected List<IComponent> ComponentsMouse;

        /// <summary>
        /// A filtered list from <see cref="Components"/> where <see cref="IComponent.IsKeyboard"/> is <see langword="true"/>.
        /// </summary>
        protected List<IComponent> ComponentsKeyboard;

        /// <summary>
        /// A filtered list from <see cref="Components"/> that is not set for update, draw, mouse, or keyboard.
        /// </summary>
        protected List<IComponent> ComponentsEmpty;

        /// <summary>
        /// A collection of components processed by this console.
        /// </summary>
        public ObservableCollection<IComponent> Components { get; private set; }

        /// <summary>
        /// The child objects of this instance.
        /// </summary>
        public ScreenObjectCollection Children { get; }

        /// <summary>
        /// The parent object that this instance is a child of.
        /// </summary>
        public ScreenObject Parent
        {
            get => _parentObject;
            set
            {
                if (value == this) throw new Exception("Cannot set parent to itself.");
                if (_parentObject == value) return;

                if (_parentObject == null)
                {
                    _parentObject = value;
                    _parentObject.Children.Add(this);
                    OnParentChanged(null, _parentObject);
                }
                else
                {
                    ScreenObject oldParent = _parentObject;
                    _parentObject = null;
                    oldParent.Children.Remove(this);
                    _parentObject = value;

                    _parentObject?.Children.Add(this);
                    OnParentChanged(oldParent, _parentObject);
                }
            }
        }

        /// <summary>
        /// The position of the object on the screen.
        /// </summary>
        public Point Position
        {
            get => _position;
            set
            {
                if (_position == value) return;

                Point oldPoint = _position;
                _position = value;
                OnPositionChanged(oldPoint, _position);
            }
        }

        /// <summary>
        /// A position that is based on the current <see cref="Position"/> and <see cref="Parent"/> position, in pixels.
        /// </summary>
        public Point AbsolutePosition { get; protected set; }

        /// <summary>
        /// Gets or sets the visibility of this object.
        /// </summary>
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible == value) return;

                _isVisible = value;
                OnVisibleChanged();
            }
        }

        /// <summary>
        /// Gets or sets the visibility of this object.
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled == value) return;

                _isEnabled = value;
                OnEnabledChanged();
            }
        }

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        public ScreenObject()
        {
            Components = new ObservableCollection<IComponent>();
            ComponentsUpdate = new List<IComponent>();
            ComponentsDraw = new List<IComponent>();
            ComponentsKeyboard = new List<IComponent>();
            ComponentsMouse = new List<IComponent>();
            Components.CollectionChanged += Components_CollectionChanged;
            Children = new ScreenObjectCollection(this);
        }

        /// <summary>
        /// Draws all <see cref="Components"/> and <see cref="Children"/>.
        /// </summary>
        /// <remarks>Only processes if <see cref="IsVisible"/> is <see langword="true"/>.</remarks>
        public virtual void Draw()
        {
            if (!IsVisible) return;

            foreach (IComponent component in ComponentsDraw.ToArray())
                component.Draw(this);

            foreach (ScreenObject child in new List<ScreenObject>(Children))
                child.Draw();
        }

        /// <summary>
        /// Updates all <see cref="Components"/> and <see cref="Children"/>.
        /// </summary>
        /// <remarks>Only processes if <see cref="IsPaused"/> is <see langword="false"/>.</remarks>
        public virtual void Update()
        {
            if (!IsEnabled) return;

            foreach (IComponent component in ComponentsUpdate.ToArray())
                component.Update(this);

            foreach (ScreenObject child in new List<ScreenObject>(Children))
                child.Update();
        }

        /// <summary>
        /// Gets components of the specified types.
        /// </summary>
        /// <typeparam name="TComponent">THe component to find</typeparam>
        /// <returns>The components found.</returns>
        public IEnumerable<IComponent> GetComponents<TComponent>()
            where TComponent : IComponent
        {
            foreach (IComponent component in Components)
            {
                if (component is TComponent)
                    yield return component;
            }
        }

        /// <summary>
        /// Gets the first component of the specified type.
        /// </summary>
        /// <typeparam name="TComponent">THe component to find</typeparam>
        /// <returns>The component if found, otherwise null.</returns>
        public IComponent GetComponent<TComponent>()
            where TComponent : IComponent
        {
            foreach (IComponent component in Components)
            {
                if (component is TComponent)
                    return component;
            }

            return null;
        }


        private void Components_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (object item in e.NewItems)
                    {
                        FilterAddItem((IComponent)item);
                        ((IComponent)item).OnAdded(this);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (object item in e.OldItems)
                    {
                        FilterRemoveItem((IComponent)item);
                        ((IComponent)item).OnRemoved(this);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (object item in e.NewItems)
                    {
                        FilterAddItem((IComponent)item);
                        ((IComponent)item).OnAdded(this);
                    }
                    foreach (object item in e.OldItems)
                    {
                        FilterRemoveItem((IComponent)item);
                        ((IComponent)item).OnRemoved(this);
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    throw new NotSupportedException("Calling Clear in this object is not supported. Use the RemoveAll extension method.");
                default:
                    throw new ArgumentOutOfRangeException();
            }

            void FilterAddItem(IComponent component)
            {
                if (component.IsDraw)
                {
                    if (!ComponentsDraw.Contains(component))
                        ComponentsDraw.Add(component);
                }

                if (component.IsUpdate)
                {
                    if (!ComponentsUpdate.Contains(component))
                        ComponentsUpdate.Add(component);
                }

                if (component.IsKeyboard)
                {
                    if (!ComponentsKeyboard.Contains(component))
                        ComponentsKeyboard.Add(component);
                }

                if (component.IsMouse)
                {
                    if (!ComponentsMouse.Contains(component))
                        ComponentsMouse.Add(component);
                }

                if (!component.IsDraw && !component.IsUpdate && !component.IsKeyboard && !component.IsMouse)
                {
                    if (!ComponentsEmpty.Contains(component))
                        ComponentsEmpty.Add(component);
                }

                ComponentsDraw.Sort(CompareComponent);
                ComponentsUpdate.Sort(CompareComponent);
                ComponentsKeyboard.Sort(CompareComponent);
                ComponentsMouse.Sort(CompareComponent);
            }

            void FilterRemoveItem(IComponent component)
            {
                if (component.IsDraw)
                {
                    if (ComponentsDraw.Contains(component))
                        ComponentsDraw.Remove(component);
                }

                if (component.IsUpdate)
                {
                    if (ComponentsUpdate.Contains(component))
                        ComponentsUpdate.Remove(component);
                }

                if (component.IsKeyboard)
                {
                    if (ComponentsKeyboard.Contains(component))
                        ComponentsKeyboard.Remove(component);
                }

                if (component.IsMouse)
                {
                    if (ComponentsMouse.Contains(component))
                        ComponentsMouse.Remove(component);
                }

                if (!component.IsDraw && !component.IsUpdate && !component.IsKeyboard && !component.IsMouse)
                {
                    if (!ComponentsEmpty.Contains(component))
                        ComponentsEmpty.Remove(component);
                }

                ComponentsDraw.Sort(CompareComponent);
                ComponentsUpdate.Sort(CompareComponent);
                ComponentsKeyboard.Sort(CompareComponent);
                ComponentsMouse.Sort(CompareComponent);
            }

            int CompareComponent(IComponent left, IComponent right)
            {
                if (left.SortOrder > right.SortOrder)
                    return 1;

                if (left.SortOrder < right.SortOrder)
                    return -1;

                return 0;
            }
        }

        /// <summary>
        /// Raises the <see cref="ParentChanged"/> event.
        /// </summary>
        /// <param name="oldParent">The previous parent.</param>
        /// <param name="newParent">The new parent.</param>
        protected virtual void OnParentChanged(ScreenObject oldParent, ScreenObject newParent)
        {
            SetAbsolutePosition();
            ParentChanged?.Invoke(this, new NewOldValueEventArgs<ScreenObject>(oldParent, newParent));
        }

        /// <summary>
        /// Raises the <see cref="PositionChanged"/> event.
        /// </summary>
        /// <param name="oldPosition">The previous position.</param>
        /// <param name="newPosition">The new position.</param>
        protected virtual void OnPositionChanged(Point oldPosition, Point newPosition)
        {
            SetAbsolutePosition();
            PositionChanged?.Invoke(this, new NewOldValueEventArgs<Point>(oldPosition, newPosition));
        }

        /// <summary>
        /// Called when the visibility of the object changes.
        /// </summary>
        protected virtual void OnVisibleChanged() =>
            VisibleChanged?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Called when the paused status of the object changes.
        /// </summary>
        protected virtual void OnEnabledChanged() =>
            EnabledChanged?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Sets a value for <see cref="AbsolutePosition"/> based on the <see cref="Position"/> of this instance and the <see cref="Parent"/> instance.
        /// </summary>
        protected virtual void SetAbsolutePosition()
        {
            AbsolutePosition = Position + (Parent?.AbsolutePosition ?? new Point(0, 0));

            foreach (Console child in Children)
                child.SetAbsolutePosition();
        }
    }
}
