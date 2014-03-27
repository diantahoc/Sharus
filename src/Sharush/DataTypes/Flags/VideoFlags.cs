using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sharush.DataTypes.Flags
{
    public class VideoFlags : IFlags
    {
        public bool AutoPlay { get; set; }

        public bool Loop { get; set; }

        public bool Mute { get; set; }
    }
}
