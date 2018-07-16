using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEAT
{
    public class InnovationGen
    {
        int currentInnovation = 0;
        public int GetInnovation()
        {
            return currentInnovation++;
        }
    }
}
