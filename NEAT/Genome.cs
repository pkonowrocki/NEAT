using System;
using System.Collections.Generic;
using System.Drawing;
using System.Media;


namespace NEAT
{
    public class Genome : IComparable<Genome>
    {
        const double PROBABILITY_PERTURBING = 0.9;
        public double score;
        public Dictionary<int, ConnectionGenes> connections { get; set; }
        public Dictionary<int, NodeGenes> nodes { get; set; }
        
        int inputsNum = 0;
        int outputsNum = 0;
        public Specie GenSpecie;
        private double compute_at(NodeGenes a)
        { 
            if (a.value != null) return (double)a.value;
            a.value = 0;
            foreach (ConnectionGenes inp in a.inputs)
            {
                if(inp.expressed)
                    a.value = a.value + inp.weight * compute_at(nodes[inp.inNode]);
            }
            return Math.Tanh((double)a.value);

        }
        public double[] Compute(double[] _inputs)
        {
            for (int i = 0; i < inputsNum; i++)
                nodes[i].value = _inputs[i];
            double[] _outputs = new double[outputsNum];

            for (int i = 0; i < outputsNum; i++)
                _outputs[i] = compute_at(nodes[inputsNum + i]);
            foreach (NodeGenes a in nodes.Values)
                a.value = null;
            return _outputs;
        }
        public Genome()
        {
            nodes = new Dictionary<int, NodeGenes>();
            connections = new Dictionary<int, ConnectionGenes>();
        }
        public void AddNodeGene(NodeGenes node)
        {
            nodes.Add(node.id, node);
            if (node.type == NodeGenes.TYPE.INPUT) inputsNum++;
            if (node.type == NodeGenes.TYPE.OUTPUT) outputsNum++;
           // System.Diagnostics.Debug.WriteLine("\noutp: " + outputsNum + " inp: " + inputsNum);
        }
        public void AddConnectionGene(ConnectionGenes connection)
        {
            connections.Add(connection.innovation, connection);
            nodes[connection.outNode].inputs.Add(connection);

        }
        public void ConnectionMutation(Random r, InnovationGen ConnectionInnov)
        {
            int first_node = r.Next(nodes.Count);
            int second_node = r.Next(nodes.Count);
            int[] nodesKeys = new int[nodes.Count];
            nodes.Keys.CopyTo(nodesKeys, 0);
            NodeGenes n1 = nodes[nodesKeys[first_node]];
            NodeGenes n2 = nodes[nodesKeys[second_node]];
            bool rev = false;
            if (n1.type == NodeGenes.TYPE.HIDDEN && n2.type == NodeGenes.TYPE.INPUT)
                rev = true;
            else if (n1.type == NodeGenes.TYPE.OUTPUT && (n2.type == NodeGenes.TYPE.HIDDEN || n2.type == NodeGenes.TYPE.INPUT))
                rev = true;

            if (n1.type == NodeGenes.TYPE.INPUT && n2.type == NodeGenes.TYPE.INPUT)
                return;

            if (n1.type == NodeGenes.TYPE.OUTPUT && n2.type == NodeGenes.TYPE.OUTPUT)
                return;


            bool connected = false;
            foreach (KeyValuePair<int, ConnectionGenes> aConnection in connections)
            {

                if (aConnection.Value.inNode == n1.id && aConnection.Value.outNode == n2.id)
                {
                    connected = true;
                    break;
                }
                else if (aConnection.Value.inNode == n2.id && aConnection.Value.outNode == n1.id)
                {
                    connected = true;
                    break;
                }
            }
            if (connected)
                return;
            try
            {
                ConnectionGenes aNewConnection = new ConnectionGenes(rev ? n2.id : n1.id, rev ? n1.id : n2.id, r.NextDouble() * 2.0 - 1.0, true, SzudzikFun(n2.id, n1.id)); //ConnectionInnov.GetInnovation());//
                connections.Add(aNewConnection.innovation, aNewConnection);
                nodes[aNewConnection.outNode].inputs.Add(aNewConnection);
            }
            catch(Exception e)
            {
                return;
            }
            
        }
        public void NodeMutation(Random r, InnovationGen NodeInnov, InnovationGen ConnectionInnov)
        {

            if (connections.Count == 0) return;
            ConnectionGenes[] list = new ConnectionGenes[connections.Count];
            connections.Values.CopyTo(list, 0);
            
            ConnectionGenes aConnection = list[r.Next(connections.Count)];

            NodeGenes inNode = nodes[aConnection.inNode];
            NodeGenes outNode = nodes[aConnection.outNode];
            aConnection.Disable();

            NodeGenes aNewNode = new NodeGenes(NodeGenes.TYPE.HIDDEN, SzudzikFun(inNode.id, outNode.id));//NodeInnov.GetInnovation());
            ConnectionGenes inNew = new ConnectionGenes(inNode.id, aNewNode.id, 1.0, true,SzudzikFun(inNode.id, aNewNode.id));// ConnectionInnov.GetInnovation());//
            ConnectionGenes outNew = new ConnectionGenes(aNewNode.id, outNode.id, aConnection.weight, true, SzudzikFun(aNewNode.id, outNode.id)); //ConnectionInnov.GetInnovation());//
            try
            {
                nodes.Add(aNewNode.id, aNewNode);
                connections.Add(inNew.innovation, inNew);
                connections.Add(outNew.innovation, outNew);
            }
            catch(Exception e)
            {
                return;
            }
            

            nodes[inNew.outNode].inputs.Add(inNew);
            nodes[outNew.outNode].inputs.Add(outNew);
        }
        public void Mutation(Random r)
        {
            foreach(ConnectionGenes aConnection in connections.Values)
            {
                if (r.NextDouble() < PROBABILITY_PERTURBING)
                {
                    aConnection.weight = aConnection.weight * (r.NextDouble() * 4.0 - 2.0);
                }
                else
                {
                    aConnection.weight = r.NextDouble() * 4.0 - 2.0;
                }

            }
        }
        public static Genome Cross(Genome a, Genome b, Random r)
        {
            Genome child = new Genome();
            foreach (KeyValuePair<int, NodeGenes> aNode in a.nodes)
                child.AddNodeGene(aNode.Value.Copy());


            foreach (KeyValuePair<int, ConnectionGenes> aConnection in a.connections)
                if (b.connections.ContainsKey(aConnection.Key))
                {
                    ConnectionGenes childConnectionGene = r.NextDouble() >= 0.5 ? aConnection.Value.Copy() : b.connections[aConnection.Value.innovation].Copy();
                    child.AddConnectionGene(childConnectionGene);
                }
                else
                {
                    ConnectionGenes childConnectionGene = aConnection.Value.Copy();
                    child.AddConnectionGene(childConnectionGene);
                }
            return child;
        }
        public static double CompatibilityDistance(Genome genome1, Genome genome2, double c1, double c2, double c3)
        {
            int excessGenes = CountExcessGenes(genome1, genome2);
            int disjointGenes = CountDisjointGenes(genome1, genome2);
            double avgWeightDiff = AverageWeightDiff(genome1, genome2);

            return excessGenes * c1 + disjointGenes * c2 + avgWeightDiff * c3;
        }
        public static int CountMatchingGenes(Genome genome1, Genome genome2)
        {
            int MatchingGenes = 0;
            //List<int> nodeKeys1 = new List<int>(genome1.nodes.Keys);
            //List<int> nodeKeys2 = new List<int>(genome2.nodes.Keys);
            List<int> connKeys1 = new List<int>(genome1.connections.Keys);
            List<int> connKeys2 = new List<int>(genome2.connections.Keys);

            foreach (int i in connKeys1)
            {
                if (connKeys2.Contains(i))
                    MatchingGenes++;
            }
            return MatchingGenes;

        }
        public static int CountExcessGenes(Genome genome1, Genome genome2)
        {
            int ExcessGenes = 0;
            //List<int> nodeKeys1 = new List<int>(genome1.nodes.Keys);
            //List<int> nodeKeys2 = new List<int>(genome2.nodes.Keys);
            List<int> connKeys1 = new List<int>(genome1.connections.Keys);
            List<int> connKeys2 = new List<int>(genome2.connections.Keys);

            //nodeKeys1.Sort();
            //nodeKeys2.Sort();
            connKeys1.Sort();
            connKeys2.Sort();
            {
            //    if(nodeKeys1.Count!=0 && nodeKeys2.Count!=0)
            //    if (nodeKeys1[nodeKeys1.Count - 1] > nodeKeys2[nodeKeys2.Count - 1])
            //    {
            //        foreach (int i in nodeKeys1)
            //            if (i > nodeKeys2[nodeKeys2.Count - 1]) ExcessGenes++;
            //    }
            //    else
            //    {
            //    foreach (int i in nodeKeys2)
            //        if (i > nodeKeys1[nodeKeys1.Count - 1]) ExcessGenes++;
            //    }
                if (connKeys1.Count != 0 && connKeys2.Count != 0)
                if (connKeys1[connKeys1.Count - 1] > connKeys2[connKeys2.Count - 1])
                {
                    foreach (int i in connKeys1)
                        if (i > connKeys2[connKeys2.Count - 1]) ExcessGenes++;
                }
                else
                {
                    foreach (int i in connKeys2)
                        if (i > connKeys1[connKeys1.Count - 1]) ExcessGenes++;
                }
            }
            return ExcessGenes;

        }
        public static int CountDisjointGenes(Genome genome1, Genome genome2)
        {
            int DisjointGenes = 0;
            //List<int> nodeKeys1 = new List<int>(genome1.nodes.Keys);
            //List<int> nodeKeys2 = new List<int>(genome2.nodes.Keys);
            List<int> connKeys1 = new List<int>(genome1.connections.Keys);
            List<int> connKeys2 = new List<int>(genome2.connections.Keys);

            //nodeKeys1.Sort();
            //nodeKeys2.Sort();
            connKeys1.Sort();
            connKeys2.Sort();
            int iterto;
          
           
                //if (nodeKeys1.Count != 0 && nodeKeys2.Count != 0)
                //{
                //    iterto = nodeKeys1[nodeKeys1.Count - 1] < nodeKeys2[nodeKeys2.Count - 1] ? nodeKeys1[nodeKeys1.Count - 1] : nodeKeys2[nodeKeys2.Count - 1];

                //    for (int i = 0; i <= iterto; i++)
                //    {
                //        if (nodeKeys1.Contains(i) ^ nodeKeys2.Contains(i)) DisjointGenes++;
                //    }
                //}
                if (connKeys1.Count != 0 && connKeys2.Count != 0)
                {
                    iterto = connKeys1[connKeys1.Count - 1] < connKeys2[connKeys2.Count - 1] ? connKeys1[connKeys1.Count - 1] : connKeys2[connKeys2.Count - 1];

                foreach (int i in connKeys1)
                    if (!connKeys2.Contains(i)) DisjointGenes++;
                foreach (int i in connKeys2)
                    if (!connKeys1.Contains(i)) DisjointGenes++;
                
                }
            

            return DisjointGenes;
        }
        public static double AverageWeightDiff(Genome genome1, Genome genome2)
        {
            List<int> connKeys1 = new List<int>(genome1.connections.Keys);
            List<int> connKeys2 = new List<int>(genome2.connections.Keys);
            double WeightDiff=0;

                for (int i = 0; i < connKeys1.Count; i++)
                    if (connKeys2.Contains(connKeys1[i]))
                        WeightDiff = WeightDiff + Math.Abs(genome1.connections[connKeys1[i]].weight - genome2.connections[connKeys1[i]].weight);


            return WeightDiff/CountMatchingGenes(genome1,genome2);
        }
        public static Bitmap Draw(Genome gen, string name="test")
        {
            Bitmap bitmap = new Bitmap(500, 500);
            Graphics g = Graphics.FromImage(bitmap);
            Pen pen = new Pen(Brushes.Blue);
            int node_size = 10;
            int inputs = 0;
            int hidden = 0;
            int outputs = 0;
            Dictionary<NodeGenes, Point> Gnodes = new Dictionary<NodeGenes, Point>();

            foreach (NodeGenes aNode in gen.nodes.Values)
            {
                if (aNode.type == NodeGenes.TYPE.INPUT) inputs++;
                else if (aNode.type == NodeGenes.TYPE.HIDDEN) hidden++;
                else if (aNode.type == NodeGenes.TYPE.OUTPUT) outputs++;
            }
            int _inputs = 0;
            int _outputs = 0;
            int _hidden = 0;
            foreach (NodeGenes aNode in gen.nodes.Values)
            {
                if (aNode.type == NodeGenes.TYPE.INPUT)
                {
                    Gnodes.Add(aNode, new Point((int)((_inputs + 0.5) * bitmap.Width) / inputs, bitmap.Height - node_size));
                    _inputs++;
                }
                else if (aNode.type == NodeGenes.TYPE.HIDDEN)
                {
                    Gnodes.Add(aNode, new Point((int)((_hidden + 0.5) * bitmap.Width) / hidden, (int)((_hidden + 0.5) * bitmap.Height) / hidden));
                    _hidden++;
                }
                else if (aNode.type == NodeGenes.TYPE.OUTPUT)
                {
                    Gnodes.Add(aNode, new Point((int)((_outputs + 0.5) * bitmap.Width) / outputs, node_size));
                    _inputs++;
                }
                g.DrawEllipse(pen, Gnodes[aNode].X, Gnodes[aNode].Y, node_size, node_size);
                
            }

            foreach (KeyValuePair<int,ConnectionGenes> aConn in gen.connections)
            {
                Point x1 = Gnodes[gen.nodes[aConn.Value.inNode]];
                Point x2 = Gnodes[gen.nodes[aConn.Value.outNode]];
                g.DrawLine(pen, x1, x2);
            }

            bitmap.Save(name+".bmp");
                
            return bitmap;

        }
        public static void Write(Genome a, string name ="test")
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(name + ".txt")) 
            {
                
                file.WriteLine("ID\t\tTYPE");
                foreach (KeyValuePair<int, NodeGenes> aNode in a.nodes)
                    file.WriteLine(aNode.Value.id + "\t\t" + aNode.Value.type);

                file.WriteLine("\nIN\t\tOUT\t\tINNOVATION\t\tWEIGHT");
                foreach (KeyValuePair<int, ConnectionGenes> aConn in a.connections)
                    if (aConn.Value.expressed)
                        file.WriteLine(aConn.Value.inNode + "\t\t" + aConn.Value.outNode + "\t\t" + aConn.Value.innovation + "\t\t\t" + aConn.Value.weight);
            }
            
        }

        public int CompareTo( Genome r)
        {
            return -score.CompareTo(r.score);
        }
        int SzudzikFun(int x, int y)
        {
            return (x >= y) ? (x * x + x + y) : (y * y + x);
        }
    }

    


}
