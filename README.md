# SpriteImageParser (SIP)

**SpriteImageParser** is a lightweight C# library for detecting sprite regions in raster images (e.g., PNGs, GIFs). It takes a 2D array of pixels, analyzes connected non-transparent regions, and returns bounding boxes for each detected sprite. Excellent examples of the type of spritesheets expected by this tool can be found on [Sprites INC.](https://sprites-inc.co.uk/), a repository of sprite data focused on the MegaMan series of games. (Disclaimer: TohnoCoding has no relation to Sprites INC. other than acknowledging their work is awesome.)

This tool is designed to be part of an asset pipeline or editor — it does not read or write image files directly. It follows a simple "one job, one tool" philosophy.

---
## 🧩 Features
- Detects sprite regions based on alpha/transparency
- Handles empty spacing between frames
- Sorts results top-to-bottom, left-to-right
- JSON and XML serialization of output
- No runtime dependencies beyond .NET
---
## 🧪 Quick Example
```csharp
// Load a Bitmap and convert to Pixel[,] array
Bitmap bm = new Bitmap("path_to_your_image.png");
Pixel[,] pixels = new Pixel[bm.Width, bm.Height];
for (int x = 0, x < bm.Width; x++)
{
    for (int y = 0; y < bm.Height; y++)
    {
        Color c = bm.GetPixel(x, y);
        pixels[x, y] = new Pixel(c.R, c.G, c.B, c.A);
    }
}

//Detect sprites in currently loaded Pixel array
List<SpriteRegion> rectangles = Parser.DetectSpritesInImage(pixels);

// Export to JSON with name prefix "Frame" and duration of 1 (time scale is user-determined)
string json = SpriteRegionExporter.serializeToJson(rectangles, "Frame", 1f);
```
---
##  🧱 Output Format
Detected regions are serialized into this format:

**JSON:**
```json
[
    {
      "Name": "Sprite000001",
      "Duration": 1000,
      "Frame": {
        "X": 194,
        "Y": 4,
        "Width": 34,
        "Height": 41
      }
    },
    {
      "Name": "Sprite000002",
      "Duration": 1000,
      "Frame": {
        "X": 244,
        "Y": 4,
        "Width": 34,
        "Height": 41
      }
    }
]
```

**XML:**
```xml
<Spritesheet>
	<SpriteRegion>
		<Name>Sprite000001</Name>
		<Frame>
			<X>194</X>
			<Y>4</Y>
			<Width>34</Width>
			<Height>41</Height>
		</Frame>
	</SpriteRegion>
	<SpriteRegion>
		<Name>Sprite000001</Name>
		<Frame>
			<X>244</X>
			<Y>4</Y>
			<Width>34</Width>
			<Height>41</Height>
		</Frame>
	</SpriteRegion>
</Spritesheet>
```
---
## ⚙️ API Overview

#### `Parser.DetectSpritesInImage(Pixel[,] pixels, byte tolerance = 0)`
-   **Input**: A 2D array of `Pixel` structs (RGBA).
-   **Output**: A `List<SpriteRegion>` with detected bounding rectangles.
    

#### `SpriteRegionExporter.SerializeToJson(...)`
-   Serializes a list of regions to JSON.    
-   Supports custom frame field name and default duration.
    

#### `SpriteRegionExporter.SerializeToXml(...)`
-   Outputs equivalent structure in XML.
---
## 📦 Data Types

```csharp
public struct Pixel {
    public byte R, G, B, A;
}

public class SpriteRegion {
    public int X, Y, Width, Height;
    public string Name;
    public float Duration;
}
```
---
## 🚫 Not Included
-   This library does not load image files — pass in a `Pixel[,]` from your own pipeline.
-   This library does not parse or deserialize formats — it only exports.
