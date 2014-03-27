using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sharush.DataTypes.Flags;
using Sharush.DataTypes.Metadata;

namespace Sharush.DataTypes
{
    public class HashInfo
    {
        public HashInfo(string hash)
        {
            this.Hash = hash;
        }

        public string Hash { get; private set; }

        public enum BlobType { Audio, Image, Video }

        public BlobType Type { get; set; }

        public double CompressionRatio { get; set; }

        public FileEntry[] Files { get; set; }

        public FileEntry[] ExtraFiles { get; set; }


        public string OriginalMimeType { get; set; }

        /// <summary>
        /// The original file that was uploaded, as-is.
        /// </summary>
        public string Original { get; set; }

        public IMetadata[] Metadata { get; set; }

        public IFlags Flags { get; set; }
    }
}
