namespace SpriteImageParser.Core
{
    /// <summary>
    /// A representation of a rectangular region in a spritesheet.
    /// </summary>
    public struct SpriteRegion
    {
        /// <summary>
        /// The X coordinate of the top-left corner of the sprite region.
        /// </summary>
        public int X;

        /// <summary>
        /// The Y coordinate of the top-left corner of the sprite region.
        /// </summary>
        public int Y;

        /// <summary>
        /// The width of the sprite region in pixels.
        /// </summary>
        public int Width;

        /// <summary>
        /// The height of the sprite region in pixels.
        /// </summary>
        public int Height;
    }
}
