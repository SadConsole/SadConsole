﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using SadRogue.Primitives;
using SadConsole.Effects;
using SadConsole.StringParser;
using Newtonsoft.Json;

namespace SadConsole
{
    /// <summary>
    /// Represents a string that has foreground and background colors for each character in the string.
    /// </summary>
    [DataContract]
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public partial class ColoredString : IEnumerable<ColoredString.ColoredGlyphEffect>
    {
        [DataMember(Name = "Glyphs")]
        private ColoredGlyphEffect[] _characters;

        /// <summary>
        /// Gets a <see cref="ColoredGlyphEffect"/> from the string.
        /// </summary>
        /// <param name="index">The index in the string of the <see cref="ColoredGlyphEffect"/>.</param>
        /// <returns>The colored glyph representing the character in the string.</returns>
        public ColoredGlyphEffect this[int index]
        {
            get => _characters[index];
            set => _characters[index] = value;
        }

        /// <summary>
        /// Gets or sets the characters represneting this string. When set, first processes the string through <see cref="Parse(string, int, ICellSurface, ParseCommandStacks)"/>.
        /// </summary>
        public string String
        {
            get
            {
                if (_characters.Length == 0)
                {
                    return "";
                }

                System.Text.StringBuilder sb = new System.Text.StringBuilder(_characters.Length);

                for (int i = 0; i < _characters.Length; i++)
                {
                    sb.Append(_characters[i].GlyphCharacter);
                }

                return sb.ToString();
            }

            set => _characters = ColoredString.Parse(value)._characters;
        }

        /// <summary>
        /// The total number of <see cref="ColoredGlyphEffect"/> characters in the string.
        /// </summary>
        public int Length => _characters.Length;

        /// <summary>
        /// When true, instructs a caller to not render the glyphs of the string.
        /// </summary>
        [DataMember]
        public bool IgnoreGlyph;

        /// <summary>
        /// When true, instructs a caller to not render the foreground color.
        /// </summary>
        [DataMember]
        public bool IgnoreForeground;

        /// <summary>
        /// When true, instructs a caller to not render the background color.
        /// </summary>
        [DataMember]
        public bool IgnoreBackground;

        /// <summary>
        /// When true, instructs a caller to not render the effect.
        /// </summary>
        [DataMember]
        public bool IgnoreEffect = true;

        /// <summary>
        /// When true, instructs a caller to not render the mirror state.
        /// </summary>
        [DataMember]
        public bool IgnoreMirror;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ColoredString() { }

        /// <summary>
        /// Creates a new instance of the ColoredString class with the specified blank characters.
        /// </summary>
        /// <param name="capacity">The number of blank characters.</param>
        public ColoredString(int capacity)
        {
            _characters = new ColoredGlyphEffect[capacity];
            for (int i = 0; i < capacity; i++)
            {
                _characters[i] = new ColoredGlyphEffect() { GlyphCharacter = ' ' };
            }
        }

        /// <summary>
        /// Creates a new instance of the ColoredString class with the specified string value. Calls <see cref="Parse(string, int, ICellSurface, ParseCommandStacks)"/> first to process the string.
        /// </summary>
        /// <param name="value">The backing string.</param>
        public ColoredString(string value) => String = value;

        /// <summary>
        /// Creates a new instance of the ColoredString class with the specified string value, foreground and background colors, and a cell effect.
        /// </summary>
        /// <param name="value">The backing string.</param>
        /// <param name="foreground">The foreground color for each character.</param>
        /// <param name="background">The background color for each character.</param>
        /// <param name="mirror">The mirror for each character.</param>
        public ColoredString(string value, Color foreground, Color background, Mirror mirror = Mirror.None)
        {
            var stacks = new ParseCommandStacks();
            stacks.AddSafe(new ParseCommandRecolor() { R = foreground.R, G = foreground.G, B = foreground.B, A = foreground.A, CommandType = CommandTypes.Foreground });
            stacks.AddSafe(new ParseCommandRecolor() { R = background.R, G = background.G, B = background.B, A = background.A, CommandType = CommandTypes.Background });
            stacks.AddSafe(new ParseCommandMirror() { Mirror = mirror, CommandType = CommandTypes.Mirror });
            _characters = Parse(value, initialBehaviors: stacks)._characters;
        }

        /// <summary>
        /// Creates a new instance of the ColoredString class with the specified string value, foreground and background colors, and a cell effect.
        /// </summary>
        /// <param name="value">The backing string.</param>
        /// <param name="appearance">The appearance to use for each character.</param>
        public ColoredString(string value, ColoredGlyphEffect appearance) : this(value, appearance.Foreground, appearance.Background, appearance.Mirror) { }

        /// <summary>
        /// Combines a <see cref="ColoredGlyphEffect"/> array into a <see cref="ColoredString"/>.
        /// </summary>
        /// <param name="glyphs">The glyphs to combine.</param>
        public ColoredString(params ColoredGlyphEffect[] glyphs) => _characters = glyphs.ToArray();

        /// <summary>
        /// Combines a <see cref="ColoredGlyph"/> array into a <see cref="ColoredString"/>.
        /// </summary>
        /// <param name="glyphs">The glyphs to combine.</param>
        public ColoredString(params ColoredGlyph[] glyphs)
        {
            _characters = new ColoredGlyphEffect[glyphs.Length];
            for (int i = 0; i < glyphs.Length; i++)
                _characters[i] = ColoredGlyphEffect.FromColoredGlyph(glyphs[i]);
        }

        /// <summary>
        /// Returns a new <see cref="ColoredString"/> object by cloning this instance.
        /// </summary>
        /// <returns>A new <see cref="ColoredString"/> object.</returns>
        public ColoredString Clone()
        {
            ColoredString returnObject = new ColoredString(_characters.Length)
            {
                IgnoreBackground = IgnoreBackground,
                IgnoreForeground = IgnoreForeground,
                IgnoreGlyph = IgnoreGlyph,
                IgnoreEffect = IgnoreEffect
            };

            for (int i = 0; i < _characters.Length; i++)
            {
                returnObject._characters[i] = _characters[i].Clone();
            }
            return returnObject;
        }

        /// <summary>
        /// Returns a new <see cref="ColoredString"/> object using a substring of this instance from the index to the end.
        /// </summary>
        /// <param name="index">The index to copy the contents from.</param>
        /// <returns>A new <see cref="ColoredString"/> object.</returns>
        public ColoredString SubString(int index) => SubString(index, _characters.Length - index);

        /// <summary>
        /// Returns a new <see cref="ColoredString"/> object using a substring of this instance.
        /// </summary>
        /// <param name="index">The index to copy the contents from.</param>
        /// <param name="count">The count of <see cref="ColoredGlyphEffect"/> objects to copy.</param>
        /// <returns>A new <see cref="ColoredString"/> object.</returns>
        public ColoredString SubString(int index, int count)
        {
            if (index + count > _characters.Length)
            {
                throw new System.IndexOutOfRangeException();
            }

            ColoredString returnObject = new ColoredString(count)
            {
                IgnoreBackground = IgnoreBackground,
                IgnoreForeground = IgnoreForeground,
                IgnoreGlyph = IgnoreGlyph,
                IgnoreEffect = IgnoreEffect
            };

            for (int i = 0; i < count; i++)
            {
                returnObject._characters[i] = _characters[i + index].Clone();
            }

            return returnObject;
        }

        /// <summary>
        /// Applies the referenced cell effect to every character in the colored string.
        /// </summary>
        /// <param name="effect">The effect to apply.</param>
        public void SetEffect(ICellEffect effect)
        {
            for (int i = 0; i < _characters.Length; i++)
            {
                _characters[i].Effect = effect;
            }
        }

        /// <summary>
        /// Applies the referenced color to every character foreground in the colored string.
        /// </summary>
        /// <param name="color">The color to apply.</param>
        public void SetForeground(Color color)
        {
            for (int i = 0; i < _characters.Length; i++)
            {
                _characters[i].Foreground = color;
            }
        }

        /// <summary>
        /// Applies the referenced color to every character background in the colored string.
        /// </summary>
        /// <param name="color">The color to apply.</param>
        public void SetBackground(Color color)
        {
            for (int i = 0; i < _characters.Length; i++)
            {
                _characters[i].Background = color;
            }
        }

        /// <summary>
        /// Applies the referenced glyph to every character in the colored string.
        /// </summary>
        /// <param name="glyph">The glyph to apply.</param>
        public void SetGlyph(int glyph)
        {
            for (int i = 0; i < _characters.Length; i++)
                _characters[i].Glyph = glyph;
        }

        /// <summary>
        /// Returns a string representing the glyphs in this object.
        /// </summary>
        /// <returns>A string composed of each glyph in this object.</returns>
        public override string ToString() => String;

        /// <summary>
        /// Gets an enumerator for the <see cref="ColoredGlyphEffect"/> objects in this string.
        /// </summary>
        /// <returns>The enumerator in the string.</returns>
        public IEnumerator<ColoredGlyphEffect> GetEnumerator() => ((IEnumerable<ColoredGlyphEffect>)_characters).GetEnumerator();

        /// <summary>
        /// Gets an enumerator for the <see cref="ColoredGlyphEffect"/> objects in this string.
        /// </summary>
        /// <returns>The enumerator in the string.</returns>
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<ColoredGlyphEffect>)_characters).GetEnumerator();

        /// <summary>
        /// Creates a new colored string from the specified gradient and text.
        /// </summary>
        /// <param name="colors">The gradient of colors to apply to the text.</param>
        /// <param name="text">The text the colored string contains.</param>
        /// <returns>A colored string.</returns>
        public static ColoredString FromGradient(Gradient colors, string text)
        {
            var colorArray = colors.ToColorArray(text.Length);
            var output = new ColoredString(text.Length);
            for (int i = 0; i < text.Length; i++)
            {
                output[i].Foreground = colorArray[i];
                output[i].Glyph = text[i];
            }

            return output;
        }

        /// <summary>
        /// Combines two ColoredString objects into a single ColoredString object. Ignore* values are only copied when both strings Ignore* values match.
        /// </summary>
        /// <param name="string1">The left-side of the string.</param>
        /// <param name="string2">The right-side of the string.</param>
        /// <returns></returns>
        public static ColoredString operator +(ColoredString string1, ColoredString string2)
        {
            ColoredString returnString = new ColoredString(string1.Length + string2.Length);

            for (int i = 0; i < string1.Length; i++)
                returnString._characters[i] = string1._characters[i].Clone();

            for (int i = 0; i < string2.Length; i++)
                returnString._characters[i + string1.Length] = string2._characters[i].Clone();

            returnString.IgnoreGlyph = string1.IgnoreGlyph && string2.IgnoreGlyph;
            returnString.IgnoreForeground = string1.IgnoreForeground && string2.IgnoreForeground;
            returnString.IgnoreBackground = string1.IgnoreBackground && string2.IgnoreBackground;
            returnString.IgnoreEffect = string1.IgnoreEffect && string2.IgnoreEffect;

            return returnString;
        }

        /// <summary>
        /// Combines a colored string and string. The last colored glyph in the colored string is used for all of the characters in the added string.
        /// </summary>
        /// <param name="string1">The colored string.</param>
        /// <param name="string2">The string.</param>
        /// <returns>A new colored string instance.</returns>
        public static ColoredString operator +(ColoredString string1, string string2)
        {

            var returnString = new ColoredString(string1.Length + string2.Length);

            for (int i = 0; i < string1.Length; i++)
                returnString._characters[i] = string1._characters[i].Clone();

            ColoredGlyphEffect templateCharacter;

            if (string1.Length != 0)
                templateCharacter = string1[string1.Length - 1];
            else
                templateCharacter = new ColoredGlyphEffect();

            for (int i = 0; i < string2.Length; i++)
            {
                ColoredGlyphEffect newChar = templateCharacter.Clone();
                newChar.GlyphCharacter = string2[i];
                returnString._characters[i + string1.Length] = newChar;
            }

            returnString.IgnoreGlyph = string1.IgnoreGlyph;
            returnString.IgnoreForeground = string1.IgnoreForeground;
            returnString.IgnoreBackground = string1.IgnoreBackground;
            returnString.IgnoreEffect = string1.IgnoreEffect;

            return returnString;
        }

        /// <summary>
        /// Combines a string and a colored string. The first colored glyph in the colored string is used for all of the characters in the added string.
        /// </summary>
        /// <param name="string1">The string.</param>
        /// <param name="string2">The colored string.</param>
        /// <returns>A new colored string instance.</returns>
        public static ColoredString operator +(string string1, ColoredString string2)
        {
            var returnString = new ColoredString(string1)
            {
                IgnoreGlyph = string2.IgnoreGlyph,
                IgnoreForeground = string2.IgnoreForeground,
                IgnoreBackground = string2.IgnoreBackground,
                IgnoreEffect = string2.IgnoreEffect
            };

            return returnString + string2;
        }
    }
}
