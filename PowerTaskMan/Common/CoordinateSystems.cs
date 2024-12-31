using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerTaskMan.Common
{

    public interface ICoordinatePair
    {
        public float X { get; set; }
        public float Y { get; set; }
    }

    class CoordinatePair : ICoordinatePair
    {
        public float X { get; set; }
        public float Y { get; set; }
    }
}
