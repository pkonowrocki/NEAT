using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEAT
{
    class Evaluator
    {
        InnovationGen nodeInnov;
        InnovationGen connInnov;
        Random random = new Random();

        const double C1 = 1.0;
        const double C2 = 1.0;
        const double C3 = 0.4;
        double DT = 10;
        double MUTATION_RATE = 0.5;
        double ADD_CONN_RATE = 0.1;
        double ADD_NODE_RATE = 0.1;

        int population_size;

        private List<Genome> genomes;
        private List<Genome> nextGenGenomes;

        private List<Species> species;

       




    }

    class Species
    {
        public Genome mascot;

    }
}
