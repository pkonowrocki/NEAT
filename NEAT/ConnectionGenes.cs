using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEAT
{
    public class ConnectionGenes
    {
        public int inNode { get; }
        public int outNode { get; }
        public double weight { get; set; }
        public Boolean expressed { get; set; }
        public int innovation { get; }

        public ConnectionGenes(int _inNode, int _outNode, double _weight, Boolean _expressed, int _innovation)
        {
            inNode = _inNode;
            outNode = _outNode;
            weight = _weight;
            expressed = _expressed;
            innovation = _innovation;
        }

        public void Disable()
        {
            expressed = false;
        }

        public ConnectionGenes Copy()
        {
            return new ConnectionGenes(inNode, outNode, weight, expressed, innovation);
        }
    }
}
