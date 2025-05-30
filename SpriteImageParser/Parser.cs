namespace SpriteImageParser.Core
{
    /// <summary>
    /// A tooling class for detecting sprite regions in an image represented as a 2D array of pixels.
    /// </summary>
    public class Parser
    {
        /// <summary>
        /// Detects sprite regions in a given image represented as a 2D array of pixels.
        /// </summary>
        /// <param name="image">The 2D array of pixels to detect sprites in.</param>
        /// <param name="yTolerance">The variance in the Y axis to account for when grouping
        /// sprites together in a single row. (Plus/Minus N amount of pixels.)</param>
        /// <param name="transparencyMask">Optional; if provided, this pixel value will be taken
        /// as the transparency color for non-transparent images.</param>
        /// <returns>A list of <see cref="SpriteRegion"/>s with the locations and dimensions
        /// of all sprites found in the image.</returns>
        public static List<SpriteRegion> DetectSpritesInImage(
            Pixel[,] image,
            int yTolerance = 3,
            Pixel? transparencyMask = null)
        {
            var detectedSprites = new List<SpriteRegion>(); // return value
            int width = image.GetLength(0);
            int height = image.GetLength(1);
            bool[,] visited = new bool[width, height];      // initializing visited array
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (visited[x, y]) { continue; }
                    Pixel pixel = image[x, y];
                    bool isTransparent = pixel.IsTransparent;
                    if (transparencyMask.HasValue)
                    {
                        var m = transparencyMask.Value;
                        isTransparent =
                            (pixel.R == m.R && pixel.G == m.G && pixel.B == m.B && pixel.A == m.A);
                    }
                    if (!isTransparent)
                    {
                        SpriteRegion rect = FloodFill(image, x, y, visited, transparencyMask);
                        detectedSprites.Add(rect);
                    }
                    else { visited[x, y] = true; }
                }
            }
            detectedSprites = [.. detectedSprites.OrderBy(r => r.Y).ThenBy(r => r.X)];

            var groupedRows = GroupByRow(detectedSprites, yTolerance);

            NormalizeSpriteRows(groupedRows);

            return [.. groupedRows.SelectMany(row => row).OrderBy(r => r.Y).ThenBy(r => r.X)];
        }


        /// <summary>
        /// Groups sprite regions by their Y coordinate, allowing for a specified tolerance.
        /// </summary>
        /// <param name="regions">The full list of detected sprite regions in an image.</param>
        /// <param name="yTolerance">The Y-axis potential variance to account for when grouping
        /// sprite groups.</param>
        /// <returns>A matrix of lists of <see cref="SpriteRegion"/> representing each row of
        /// sprites.</returns>
        private static List<List<SpriteRegion>> GroupByRow(List<SpriteRegion> regions, int yTolerance)
        {
            var rows = new List<List<SpriteRegion>>();
            foreach (var region in regions)
            {
                bool added = false;
                foreach (var row in rows)
                {
                    // Instead of comparing to just row[0], compare to all regions in the row
                    if (row.Any(r => Math.Abs(r.Y - region.Y) <= yTolerance))
                    {
                        row.Add(region);
                        added = true;
                        break;
                    }
                }
                if (!added) { rows.Add([region]); }
            }
            return rows;
        }


        /// <summary>
        /// Normalizes the sprite regions in each row to have the same width and height, based
        /// on the dimensions of the largest sprite in the row.
        /// </summary>
        /// <param name="groupedRows">The list of sprite rows (represented as lists of
        /// <see cref="SpriteRegion"/>) that forms the spritesheet.</param>
        private static void NormalizeSpriteRows(List<List<SpriteRegion>> groupedRows)
        {
            foreach (var row in groupedRows)
            {
                int maxWidth = row.Max(r => r.Width);
                int maxHeight = row.Max(r => r.Height);
                int maxBottom = row.Max(r => r.Y + r.Height);
                for (int i = 0; i < row.Count; i++)
                {
                    var sprite = row[i];
                    int newY = maxBottom - maxHeight;
                    int yOffset = sprite.Y - newY;
                    int xOffset = (maxWidth - sprite.Width) / 2;
                    sprite.Y -= yOffset; // move up or down to align bottom
                    sprite.X -= xOffset; // move left or right to center
                    sprite.Width = maxWidth;
                    sprite.Height = maxHeight;
                    row[i] = sprite; // reassign the modified struct
                }
            }
        }


        /// <summary>
        /// Performs a flood fill algorithm to find the bounding box of a sprite region.
        /// </summary>
        /// <param name="image">The 2D array of pixels to analyze via flood fill.</param>
        /// <param name="startX">The X coordinate of the array to start applying the flood fill from.</param>
        /// <param name="startY">The Y coordinate of the array to start applying the flood fill from.</param>
        /// <param name="visited">Whether particular pixels in the collection have been traversed or not.</param>
        /// <param name="mask">Optional; if the image is not transparent, a pixel color can be provided
        /// to use as a transparency mask color.</param>
        /// <returns>A <see cref="SpriteRegion"/> containing the full boundaries of the current sprite.</returns>
        private static SpriteRegion FloodFill(Pixel[,] image, int startX, int startY, bool[,] visited, Pixel? mask)
        {
            int width = image.GetLength(0);
            int height = image.GetLength(1);
            Queue<(int x, int y)> queue = new();
            queue.Enqueue((startX, startY));
            visited[startX, startY] = true;
            int minX = startX, maxX = startX, minY = startY, maxY = startY;
            while (queue.Count > 0)
            {
                var (x, y) = queue.Dequeue();
                // Check neighbors
                foreach (var (nx, ny) in GetNeighbors(x, y, width, height))
                {
                    if (visited[nx, ny]) continue;
                    Pixel neighborPixel = image[nx, ny];
                    bool isTransparent = neighborPixel.IsTransparent;
                    if (mask.HasValue)
                    {
                        var m = mask.Value;
                        isTransparent =
                            (neighborPixel.R == m.R && neighborPixel.G == m.G && neighborPixel.B == m.B);
                    }
                    if (!isTransparent)
                    {
                        visited[nx, ny] = true;
                        queue.Enqueue((nx, ny));
                        // Update bounding box
                        if (nx < minX) minX = nx;
                        if (nx > maxX) maxX = nx;
                        if (ny < minY) minY = ny;
                        if (ny > maxY) maxY = ny;
                    }
                    else
                    { visited[nx, ny] = true; } // Mark as visited even if transparent to avoid reprocessing
                }
            }
            return new SpriteRegion
            {
                X = minX,   Y = minY,
                Width = maxX - minX + 1,
                Height = maxY - minY + 1
            };
        }


        /// <summary>
        /// Gets the neighboring pixels of a given pixel in an image.
        /// </summary>
        /// <param name="x">The X coordinate of the pixel.</param>
        /// <param name="y">The Y coordinate of the pixel.</param>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <returns>A list of tuples representing the coordinates of the neighboring pixels.</returns>
        private static List<(int, int)> GetNeighbors(int x, int y, int width, int height)
        {
            var neighbors = new List<(int, int)>();
            // Add all 8 possible neighbors
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue; // Skip the current pixel
                    int nx = x + dx;
                    int ny = y + dy;
                    if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                    { neighbors.Add((nx, ny)); }
                }
            }
            return neighbors;
        }
    }
}