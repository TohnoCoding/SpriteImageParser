#pragma warning disable // CS8602 is being unnecessarily annoying in the XML builder
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Xml;

namespace SpriteImageParser.Core
{
    /// <summary>
    /// A class for exporting a collection of sprite regions to JSON or XML.
    /// </summary>
    public class SpriteRegionExporter
    {
        /// <summary>
        /// Serializes a list of sprite regions to a JSON string.
        /// </summary>
        /// <param name="regions">The list of sprite regions to serialize.</param>
        /// <param name="frameName">The name of the frame to use in the JSON output.</param>
        /// <param name="duration">Optional; the duration for each sprite frame, if applicable.</param>
        /// <returns>A JSON string representing the sprite regions provided.</returns>
        public static string SerializeToJson(List<SpriteRegion> regions,
            string frameName, 
            float? duration, 
            int yTolerance = 3)
        {
            var output = new List<Dictionary<string, object>>();
            var grouped = Parser.GroupByRow(regions, yTolerance);
            for (int rowIndex = 0; rowIndex < grouped.Count; rowIndex++)
            {
                var row = grouped[rowIndex];
                for (int spriteIndex = 0; spriteIndex < row.Count; spriteIndex++)
                {
                    var region = row[spriteIndex];
                    var regionData = new Dictionary<string, object>
                    {
                        { "Name", $"Row{rowIndex + 1:D6}Sprite{spriteIndex + 1:D6}" },
                        { frameName, new Dictionary<string, int>
                            {
                                { "X", region.X },
                                { "Y", region.Y },
                                { "Width", region.Width },
                                { "Height", region.Height }
                            }
                        },
                        { "Duration", duration.HasValue ? duration.Value : 1 }
                    };
                    output.Add(regionData);
                }
            }
            return JsonSerializer.Serialize(output, new JsonSerializerOptions { WriteIndented = true });
        }

        /// <summary>
        /// Serializes a list of sprite regions to a JSON string with a default frame name and duration.
        /// </summary>
        /// <param name="regions">The list of sprite regions to serialize.</param>
        /// <returns>A JSON string representing the sprite regions provided.</returns>
        public static string SerializeToJson(List<SpriteRegion> regions)
            => SerializeToJson(regions, "Frame", 1);

        /// <summary>
        /// Serializes a list of sprite regions to an XML string.
        /// </summary>
        /// <param name="regions">The list of sprite regions to serialize.</param>
        /// <param name="frameName">The name of the frame to use in the JSON output.</param>
        /// <param name="duration">Optional; the duration for each sprite frame, if applicable.</param>
        /// <returns>An XML string representing the sprite regions provided.</returns>
        public static string SerializeToXml(
            List<SpriteRegion> regions,
            string frameName,
            float? duration,
            int yTolerance = 3)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement("Spritesheet");
            doc.AppendChild(root);
            var grouped = Parser.GroupByRow(regions, yTolerance);
            for (int rowIndex = 0; rowIndex < grouped.Count; rowIndex++)
            {
                var row = grouped[rowIndex];
                for (int spriteIndex = 0; spriteIndex < row.Count; spriteIndex++)
                {
                    var region = row[spriteIndex];
                    XmlElement regionElement = doc.CreateElement("SpriteRegion");
                    regionElement.AppendChild(doc.CreateElement("Name")).InnerText =
                        $"Row{rowIndex + 1:D6}Sprite{spriteIndex + 1:D6}";
                    XmlElement frameElement = doc.CreateElement(frameName);
                    frameElement.AppendChild(doc.CreateElement("X"))
                        .InnerText = region.X.ToString();
                    frameElement.AppendChild(doc.CreateElement("Y"))
                        .InnerText = region.Y.ToString();
                    frameElement.AppendChild(doc.CreateElement("Width"))
                        .InnerText = region.Width.ToString();
                    frameElement.AppendChild(doc.CreateElement("Height"))
                        .InnerText = region.Height.ToString();
                    regionElement.AppendChild(frameElement);
                    regionElement.AppendChild(doc.CreateElement("Duration")).InnerText =
                        duration.HasValue ? duration.Value.ToString() : "1";
                    root.AppendChild(regionElement);
                }
            }
            return Beautify(doc);
        }

        /// <summary>
        /// Serializes a list of sprite regions to an XML string with a default frame name and duration.
        /// </summary>
        /// <param name="regions">The list of sprite regions to serialize.</param>
        /// <returns>An XML string representing the sprite regions provided.</returns>
        public static string SerializeToXml(List<SpriteRegion> regions)
            => SerializeToXml(regions, "Frame", 1);

        /// <summary>
        /// Beautifies an XML document by formatting it with indentation and new lines.
        /// </summary>
        /// <param name="doc">The XML document to beautify.</param>
        /// <returns>A string representation of the XML document with beautified indentation.</returns>
        static private string Beautify(XmlDocument doc)
        {
            StringBuilder sb = new();
            XmlWriterSettings settings = new()
            {
                Indent = true,
                NewLineHandling = NewLineHandling.Replace,
                NewLineChars = "\r\n",
                IndentChars = "    "
            };
            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            { doc.Save(writer); }
            return sb.ToString();
        }
    }
}
