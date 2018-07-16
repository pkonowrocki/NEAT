using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEAT
{
    public class NodeGenes
    {
        public enum TYPE { INPUT, HIDDEN,OUTPUT};
        public TYPE type { get; }
        public int id { get; }
        public double? value=null;


        public List<ConnectionGenes> inputs = new List<ConnectionGenes>();

        public NodeGenes(TYPE _type, int _id)
        {
            type = _type;
            id = _id;
        }
        public NodeGenes(TYPE _type, int _id, List<ConnectionGenes> _inputs)
        {
            type = _type;
            id = _id;
            inputs = _inputs;
        }

        public NodeGenes Copy()
        {
            return new NodeGenes(type, id);
        }
       

    }
}
