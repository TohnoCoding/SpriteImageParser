namespace SpriteImageParser.Core
{
    /// <summary>
    /// A representation of a single pixel in an image.
    /// </summary>
    /// <param name="r">The red component of the pixel color.</param>
    /// <param name="g">The green component of the pixel color.</param>
    /// <param name="b">The blue component of the pixel color.</param>
    /// <param name="a">The alpha component of the pixel color, used to
    /// determine transparency.</param>
    public struct Pixel(byte r, byte g, byte b, byte a)
    {
        /// <summary>
        /// Represents the red component of the pixel color.
        /// </summary>
        public byte R = r;

        /// <summary>
        /// Represents the green component of the pixel color.
        /// </summary>
        public byte G = g;

        /// <summary> 
        /// Represents the blue component of the pixel color.
        /// </summary>
        public byte B = b;

        /// <summary>
        /// Represents the alpha component of the pixel color, which determines its transparency.
        /// </summary>
        public byte A = a;

        /// <summary>
        /// True if the pixel is transparent, i.e. has an alpha value of 0.
        /// </summary>
        public readonly bool IsTransparent => A == 0;
    }
}
