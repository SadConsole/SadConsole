﻿namespace SadConsole.Entities
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using SadConsole.Consoles;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents a set of animations that can be positioned and rendered to the screen.
    /// </summary>
    [DataContract]
    public class Entity: Consoles.CellsRenderer, IConsole
    {
        #region Variables
        [DataMember(Name = "CurrentAnimation")]
        private string _currentAnimationName;

        /// <summary>
        /// Not yet used.
        /// </summary>
        [DataMember(Name="CenterOffset")]
        protected Point _centerOffset;

        /// <summary> 
        /// The animations.
        /// </summary>
        [DataMember(Name="Animations")]
        protected List<Animation> _animations = new List<Animation>();

        /// <summary>
        /// The currently animating animation.
        /// </summary>
        protected Animation _currentAnimation;

        /// <summary>
        /// A bounding box that represents the entire size of the current animation frame.
        /// </summary>
        protected Rectangle _animationBoundingBox = Rectangle.Empty;

        /// <summary>
        /// Backing field for CanUseKeyboard property;
        /// </summary>
        protected bool _canUseKeyboard;

        /// <summary>
        /// Backing field for CanUseMouse property;
        /// </summary>
        protected bool _canUseMouse;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the current animation's bounding box.
        /// </summary>
        public Rectangle AnimationBoundingBox { get { return _animationBoundingBox; } }

        /// <summary>
        /// The collision recatangle.
        /// </summary>
        [DataMember]
        public Rectangle CollisionBox { get; set; }

        /// <summary>
        /// Returns a read-only collection of all the animations this entity has.
        /// </summary>
        public System.Collections.ObjectModel.ReadOnlyCollection<Animation> Animations
        {
            get { return _animations.ToList().AsReadOnly(); }
        }

        /// <summary>
        /// Gets the current animation.
        /// </summary>
        public Animation CurrentAnimation { get { return _currentAnimation; } }

        /// <summary>
        /// Gets or sets a point that is used to offset the position of the entity.
        /// </summary>
        public Point PositionOffset { get; set; }

        /// <summary>
        /// Called when the current animation state changes. Parameters are: current entity, current animation, new state, old state.
        /// </summary>
        public System.Action<Entity, Animation, AnimationState, AnimationState> OnEntityAnimationStateChanged;

        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new entity with the Engine's default font.
        /// </summary>
        public Entity() : this(Engine.DefaultFont) { }

        /// <summary>
        /// Creates a new entity with the specified font.
        /// </summary>
        /// <param name="font">The font to use when rendering this entity.</param>
        public Entity(Font font): base(null, new SpriteBatch(Engine.Device))
        {
            Font = font;
            IsVisible = true;

            var defaultAnimation = new Animation("default", 1, 1);

            AddAnimation(defaultAnimation);
            SetCurrentAnimation(defaultAnimation);

            _currentAnimation.Font = font;
            _currentAnimation.CreateFrame();

            _animationBoundingBox = new Rectangle(0, 0, 1, 1);
        }
        #endregion

        #region Methods

        ///// <summary>
        ///// Renders the entity using the specified sprite batch. This method will not call Batch.Begin and Batch.End.
        ///// </summary>
        ///// <param name="batch">The sprite batch to render the entity.</param>
        //public void Render(SpriteBatch batch)
        //{
        //    var oldBatch = base.Batch;
        //    base.Batch = batch;
        //    base._skipBatchBeginEnd = true;
        //    base.Render();
        //    base.Batch = oldBatch;
        //    base._skipBatchBeginEnd = false;
        //}

        /// <summary>
        /// Renders the current entity's animation frame to a surface using the <see cref="Position"/> property for the location.
        /// </summary>
        /// <param name="surface">The surface to print the frame on.</param>
        public void RenderToSurface(CellSurface surface)
        {
            this.RenderToSurface(surface, this.Position);
        }

        /// <summary>
        /// Renders the current entity's animation frame to a surface at the specified location.
        /// </summary>
        /// <param name="surface">The surface to print the frame on.</param>
        /// <param name="location">Specifies the cell at which the frame will be printed at.</param>
        public void RenderToSurface(CellSurface surface, Point location)
        {
            // copy the current frame of the animation to the console at the specified location
            Frame frame = _currentAnimation.CurrentFrame;
            frame.Copy(0, 0, frame.Width, frame.Height, surface, location.X, location.Y);
        }

        ////TODO: This is not really working well now. Since positioning changed to be transform based, entity will always render 0,0 if you do not call begin and end.
        ///// <summary>
        ///// Renders the entity.
        ///// </summary>
        ///// <param name="batch">The sprite batched used for drawing the entity.</param>
        //public void Render(SpriteBatch batch, bool callBeginEnd = false)
        //{
        //    if (_currentAnimation != null && !_currentAnimation.IsEmpty)
        //    {
        //        if (IsVisible)
        //        {
        //            Point worldLocation;

        //            if (UseAbsolutePositioning)
        //                worldLocation = Location;
        //            else
        //                worldLocation = Location.ConsoleLocationToWorld(_font.CellWidth, _font.CellHeight);

        //            Matrix transform = Matrix.CreateTranslation(worldLocation.X, worldLocation.Y, 0f);

        //            transform = transform * Matrix.CreateTranslation(-_currentAnimation.Center.X * _font.CellWidth, -_currentAnimation.Center.Y * _font.CellHeight, 0f);

        //            if (callBeginEnd)
        //                batch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullNone, null, transform);

        //            Frame frame = _currentAnimation.CurrentFrame;

        //            for (int cellIndex = 0; cellIndex < frame.CellCount; cellIndex++)
        //            {
        //                var cell = frame[cellIndex];

        //                if (cell.IsVisible)
        //                {
        //                    if (cell.Effect != null && !cell.Effect.Finished)
        //                        cell.Effect.Process(Engine.GameTimeElapsedUpdate, cell);

        //                    // Draw the background of the cell as a solid color
        //                    if (cell.ActualBackground != Color.Transparent)
        //                        batch.Draw(Engine.BackgroundCell, frame.CellIndexRects[cellIndex], null, cell.ActualBackground, 0f, Vector2.Zero, SpriteEffects.None, 0.4f);

        //                    // "Print" the foreground of the cell on top of the background
        //                    if (cell.ActualForeground != Color.Transparent)
        //                        batch.Draw(Font.Image, frame.CellIndexRects[cellIndex], _font.CharacterIndexRects[cell.ActualCharacterIndex], cell.ActualForeground, 0f, Vector2.Zero, SpriteEffects.None, 0.1f);

        //                    // SHOULD HAVE THIS CELLINDEXRECTS I guess.. For position it shuold move all the rects around
        //                }
        //            }

        //            if (callBeginEnd)
        //                batch.End();
        //        }
        //    }
        //}

        /// <summary>
        /// Called by the owning console during it's update method. Calls update the current animation.
        /// </summary>
        public override void Update()
        {
            if (_currentAnimation != null)
            {
                _currentAnimation.Update();

                CellData = _currentAnimation.CurrentFrame;

                base.Update();
            }
        }


        /// <summary>
        /// Stops the current animation from playing.
        /// </summary>
        public void Stop()
        {
            _currentAnimation.Stop();
        }

        /// <summary>
        /// Starts the current animation.
        /// </summary>
        public void Start()
        {
            _currentAnimation.Start();
        }

        /// <summary>
        /// Sets the active animation.
        /// </summary>
        /// <param name="name">The name of the animation to activate.</param>
        public void SetActiveAnimation(string name)
        {
            var animation = _animations.Where(a => a.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            if (animation == null)
                return;

            SetCurrentAnimation(animation);
            
            if (_currentAnimation.Font != null)
                base.Font = _currentAnimation.Font;
            else
                base.Font = Engine.DefaultFont;

            UpdateAnimationBoundingBox();
            CellData = _currentAnimation.CurrentFrame;
            ResetViewArea();
        }

        /// <summary>
        /// Sets the active animation.
        /// </summary>
        /// <param name="name">The name of the animation to activate.</param>
        /// <remarks>This animation does not have to be part of the named animations added to the entity.</remarks>
        public void SetActiveAnimation(Animation animation)
        {
            SetCurrentAnimation(animation);

            if (_currentAnimation.Font != null)
                base.Font = _currentAnimation.Font;
            else
                base.Font = Engine.DefaultFont;

            UpdateAnimationBoundingBox();
            CellData = _currentAnimation.CurrentFrame;
            ResetViewArea();
        }

        public void RemoveAllAnimations()
        {
            _animations.Clear();
            SetCurrentAnimation(null);
        }

        /// <summary>
        /// Updates the AnimationBoundingBox property with the values from the current animation. If you change the Center of the animation, this should be called.
        /// <remarks>This is also called whenever the SetAnimation method is called.</remarks>
        /// </summary>
        public void UpdateAnimationBoundingBox()
        {
            _animationBoundingBox = new Rectangle(0 - _currentAnimation.Center.X, 0 - _currentAnimation.Center.Y, _currentAnimation.Width, _currentAnimation.Height);
        }

        /// <summary>
        /// Returns an animation by name.
        /// </summary>
        /// <param name="name">The name of the animation.</param>
        /// <returns>The animation if found, otherwise returns null.</returns>
        public Animation GetAnimation(string name)
        {
            return _animations.Where(a => a.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        }

        /// <summary>
        /// Adds an animation to the entity. Replacing any animation with the same name and setting the font of the animation to the entity's font.
        /// </summary>
        /// <param name="animation">The animation.</param>
        public void AddAnimation(Animation animation)
        {
            var oldAnimation = _animations.Where(a => a.Name.Equals(animation.Name, System.StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            if (oldAnimation != null)
                _animations.Remove(oldAnimation);
            
            animation.Font = _font;

            _animations.Add(animation);
        }

        /// <summary>
        /// Removes an animation from this entity.
        /// </summary>
        /// <param name="name">The name of an animation the entity has. If you try to remove the "default" animation, an exception will be thrown.</param>
        public void RemoveAnimation(string name)
        {
            if (name == "default")
                throw new System.Exception("Cannot remove default animation. Replace it by adding a new animation with the name 'default'");

            var oldAnimation = _animations.Where(a => a.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            if (oldAnimation != null)
            {
                _animations.Remove(oldAnimation);

                if (_currentAnimation == oldAnimation)
                    SetActiveAnimation("default");
            }
        }

        /// <summary>
        /// Removes an animation from this entity.
        /// </summary>
        /// <param name="animation">The animation to remove. If you try to remove the "default" animation, an exception will be thrown.</param>
        public void RemoveAnimation(Animation animation)
        {
            if (_animations.Contains(animation))
            {
                if (animation.Name == "default")
                    throw new System.Exception("Cannot remove default animation. Replace it by adding a new animation with the name 'default'");
                else
                {
                    _animations.Remove(animation);

                    if (_currentAnimation == animation)
                        SetActiveAnimation("default");
                }
            }
        }

        /// <summary>
        /// Updates all existing animations with the entity's font.
        /// </summary>
        protected void UpdateAnimationFont()
        {
            for (int i = 0; i < _animations.Count; i++)
            {
                _animations[i].Font = _font;
            }
        }

        protected void SetCurrentAnimation(Animation animation)
        {
            var oldAnimation = _currentAnimation;

            if (oldAnimation != null)
            {
                oldAnimation.State = AnimationState.Deactivated;
                oldAnimation.AnimationStateChanged -= CurrentAnimation_AnimationStateChanged;

                oldAnimation.Restart();
                oldAnimation.Stop();
            }

            _currentAnimation = animation;

            if (_currentAnimation != null)
            {
                _currentAnimation.AnimationStateChanged += CurrentAnimation_AnimationStateChanged;
                _currentAnimation.State = AnimationState.Activated;
            }
        }

        private void CurrentAnimation_AnimationStateChanged(object sender, AnimationStateChangedEventArgs e)
        {
            OnEntityAnimationStateChanged?.Invoke(this, _currentAnimation, e.NewState, e.PreviousState);
        }
        #endregion

        public override Matrix GetPositionTransform()
        {
            Point worldLocation;

            if (UseAbsolutePositioning)
                worldLocation = Position + PositionOffset;
            else
                worldLocation = Position.ConsoleLocationToWorld(CellSize.X, CellSize.Y) + PositionOffset.ConsoleLocationToWorld(CellSize.X, CellSize.Y);

            return Matrix.CreateTranslation(worldLocation.X, worldLocation.Y, 0f) * Matrix.CreateTranslation(-_currentAnimation.Center.X * _font.CellWidth, -_currentAnimation.Center.Y * _font.CellHeight, 0f);
        }

        #region IConsole
        private IParentConsole _parentConsole;

        public Console.Cursor VirtualCursor { get { return null; } set { } }

        public IParentConsole Parent
        {
            get { return _parentConsole; }
            set
            {
                if (_parentConsole != value)
                {
                    if (_parentConsole == null)
                    {
                        _parentConsole = value;
                        _parentConsole.Add(this);
                    }
                    else
                    {
                        var oldParent = _parentConsole;
                        _parentConsole = value;

                        oldParent.Remove(this);

                        if (_parentConsole != null)
                            _parentConsole.Add(this);

                    }
                }

            }
        }

        [DataMember]
        public bool CanUseKeyboard { get { return _canUseKeyboard; } set { _canUseKeyboard = value; } }

        [DataMember]
        public bool CanUseMouse { get { return _canUseMouse; } set { _canUseMouse = value; } }

        public bool CanFocus { get { return false; } set { } }

        public bool IsFocused { get { return false; } set { } }

        public bool ExclusiveFocus { get { return false; } set { } }

        public virtual bool ProcessMouse(Input.MouseInfo info)
        {
            return false;
        }

        public virtual bool ProcessKeyboard(Input.KeyboardInfo info)
        {
            return false;
        }
        #endregion

        [OnSerializing]
        private void BeforeSerializing(StreamingContext context)
        {
            if (_currentAnimation != null)
                _currentAnimationName = _currentAnimation.Name;
        }

        [OnDeserialized]
        private void AfterDeserialized(StreamingContext context)
        {
            if (_currentAnimationName != null)
                SetActiveAnimation(_currentAnimationName);
        }

        public void Save(string file)
        {
            //SadConsole.Serializer.Save<Entity>(this, file, new System.Type[] { typeof(List<Frame>) });
            Serialized data = new Serialized(this);
            data.Save(file);
        }

        public static Entity Load(string file)
        {
            //return SadConsole.Serializer.Load<Entity>(file, new System.Type[] { typeof(List<Frame>) });
            return Serialized.Load(file);
        }

        [DataContract]
        internal class Serialized
        {
            [DataMember]
            public Animation[] Animations;

            [DataMember]
            public string CurrentAnimation;

            [DataMember]
            public Point CenterOffset;

            [DataMember]
            public Rectangle CollisionBox { get; set; }

            public Serialized() { }

            public Serialized(Entity entity)
            {
                Animations = entity._animations.ToArray();
                CurrentAnimation = entity.CurrentAnimation != null ? entity.CurrentAnimation.Name : "default";
                CenterOffset = entity._centerOffset;
                CollisionBox = entity.CollisionBox;
            }

            public void Save(string file)
            {
                SadConsole.Serializer.Save(this, file, new System.Type[] { typeof(List<Frame>), typeof(Animation) });
            }

            public static Entity Load(string file)
            {
                var data = SadConsole.Serializer.Load<Serialized>(file, new System.Type[] { typeof(List<Frame>), typeof(Animation) });
                var entity = new Entity();

                entity._animations = new List<Animation>(data.Animations);
                entity._centerOffset = data.CenterOffset;
                entity.CollisionBox = data.CollisionBox;
                entity.SetActiveAnimation(data.CurrentAnimation);

                return entity;
            }
        }
    }

    
}
