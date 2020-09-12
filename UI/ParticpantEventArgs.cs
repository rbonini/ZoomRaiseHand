using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaiseHandApp.UI
{
    public class ParticpantEventArgs:EventArgs
    {
        public ParticpantEventArgs(string name)
        {
            Name = name;
        }
        public string Name { get; private set; }
    }
}
