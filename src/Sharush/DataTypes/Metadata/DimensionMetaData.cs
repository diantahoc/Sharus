using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sharush.DataTypes.Metadata
{
    public class DimensionMetaData : IMetadata
    {
        public int Height { get; set; }
        public int Width { get; set; }
    }
}
