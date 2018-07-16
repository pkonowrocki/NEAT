using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEAT
{
    public class Population
    {
        InnovationGen nodeInnov = new InnovationGen();
        InnovationGen connInnov = new InnovationGen();
        Random random = new Random();
        public List<Specie> species = new List<Specie>();
        //public List<Genome> agents = new List<Genome>();
        public List<Genome> agents = new List<Genome>();
        public List<Genome> deadagents = new List<Genome>();
        
        const double C1 = 1.0;
        const double C2 = 1.0;
        const double C3 = 0.4;
        double DT = 0.3;
        double MUTATION_RATE = 0.7;
        double ADD_CONN_RATE = 0.4;
        double ADD_NODE_RATE = 0.3;
        
        public int population;
        public double bestscore = 9999;
        public int iter = 1;


        public Population(int _population, int _inputs, int _outputs)
        {
            agents.Add(new Genome());
            population = _population;
            

            for (int j = 0; j < _inputs; j++)
                agents[0].AddNodeGene(new NodeGenes(NodeGenes.TYPE.INPUT, nodeInnov.GetInnovation()));
            for (int j = 0; j < _outputs; j++)
                agents[0].AddNodeGene(new NodeGenes(NodeGenes.TYPE.OUTPUT, nodeInnov.GetInnovation()));

            for (int i = 1; i < _population; i++)
            {
                agents.Add(new Genome());
                agents[i]=Genome.Cross(agents[0], agents[0], random);
            }
            species.Add(new Specie(agents[0]));
            foreach (Genome aGenome in agents)
            {
                aGenome.ConnectionMutation(random, connInnov);
                aGenome.GenSpecie = species[0];
            }
            SeclectSpecies();
        }
        public void KillAgent(Genome agent, double EvaluationFunction)
        {
            agent.score = EvaluationFunction;///agent.GenSpecie.size;
            deadagents.Add(agent);
            deadagents.Sort();
            agents.Remove(agent);
        }

        public delegate double eval(Object obj=null);
        public void NextGeneration()
        {
            agents.Clear();
            int j = 0;
            for (int i = 0; i<1; i++)
            {
                agents.Add(deadagents[i]);
                j++;
            }

            bestscore = deadagents[0].score;
            
            for(; j<population; j++)
            {
                agents.Add(Genome.Cross(agents[0], deadagents[j-1], random));
                if(random.NextDouble()<MUTATION_RATE) agents[j-1].Mutation(random);
                if (random.NextDouble() < ADD_CONN_RATE) agents[j-1].ConnectionMutation(random,connInnov);
                if (random.NextDouble() < ADD_NODE_RATE) agents[j-1].NodeMutation(random,nodeInnov,connInnov);
            }
            deadagents.Clear();
            SeclectSpecies();
            iter++;
        }
        void SeclectSpecies()
        {
            species.Clear();
            species.Add(new Specie(agents[0]));
            agents[0].GenSpecie = species[0];

            for (int i = 1; i < population; i++)
            {
                bool CreateNew = true;
                for (int s = 0; s < species.Count; s++)
                {
                    double dist = Genome.CompatibilityDistance(species[s].mascot, agents[s], C1, C2, C3);
                    if (dist < DT)
                    {
                        agents[i].GenSpecie = species[s];
                        species[s].size++;
                        CreateNew = false;
                    }
                    species[s].number = (byte)s;
                }
                if (CreateNew)
                {
                    species.Add(new Specie(agents[i]));
                    //agents[i].GenSpecie = species[species.Count - 1];
                }
            }
        }
    }

    public class Specie
    {
        public Genome mascot;
        public int size = 0;
        public byte number = 0;
        public Specie(Genome _mascot)
        {
            mascot = _mascot;
            size++;
            _mascot.GenSpecie = this;
        }

    }
}
