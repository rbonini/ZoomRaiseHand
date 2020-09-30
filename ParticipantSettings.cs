using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaiseHandApp
{
    public class ParticipantSettings
    {
        public bool ReplaceAnd { get; set; }

        public List<char> SplitNamesOn { get; set; }

        public List<string>SkipCountNames { get; set; }
        public List<RaiseHandParticipant>Participants { get; set; }
    }
}
