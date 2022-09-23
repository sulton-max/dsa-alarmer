namespace GIBS.API.Client.Models
{
    public class GIBSImageFilter
    {
        public GIBSImageFilter
        (
            string layerIdentifier,
            DateTime time,
            string tileMatrixSet,
            uint tileMatrix,
            uint tileRow,
            uint tileCol,
            string formatSet
        )
        {
            LayerIdentifier = layerIdentifier;
            Time = time;
            TileMatrixSet = tileMatrixSet;
            TileMatrix = tileMatrix;
            TileRow = tileRow;
            TileCol = tileCol;
            FormatSet = formatSet;
        }

        public string LayerIdentifier { get; set; } = null;
        public DateTime Time { get; set; }
        public string TileMatrixSet { get; set; } = null;
        public uint TileMatrix { get; set; }
        public uint TileRow { get; set; }
        public uint TileCol { get; set; }
        public string FormatSet { get; set; } = null;
    }
}
