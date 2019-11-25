using System;
using BayesServer;
using BayesServer.Inference.RelevanceTree;

namespace Bayes
{
    class Program
    {
        public static void Main()
        {
            string Hopc, Eopc;

            Console.WriteLine("\t\t BAYES DIABETES NETWORK\n");
            Console.WriteLine("This program will calculate the probability of you having diabetes using the following conditions: ");
            Console.WriteLine("Having diabetes is conditioned by the genre (Male / Female) and your age (The interval used is from 18 - 50).");
            Console.WriteLine("Also, the probability of having diabetes conditions the probability of being overweight.\n");

            Console.WriteLine("To calculate your probabilities, answer the following cuestions with a 'y' or 'n' for Yes and No:");
            Console.WriteLine("Are you a Male? (H) y/n:");
            Hopc = Console.ReadLine();
            Console.WriteLine("Do you have between 18 to 50 years? (E) y/n:");
            Eopc = Console.ReadLine();

            // Create Network
            var network = new Network("Diabetes");

            // Add Nodes
            var hTrue = new State("True");
            var hFalse = new State("False");
            var h = new Node("H", hTrue, hFalse);

            var eTrue = new State("True");
            var eFalse = new State("False");
            var e = new Node("B", eTrue, eFalse);

            var dTrue = new State("True");
            var dFalse = new State("False");
            var d = new Node("D", dTrue, dFalse);

            var oTrue = new State("True");
            var oFalse = new State("False");
            var o = new Node("O", oTrue, oFalse);

            network.Nodes.Add(h);
            network.Nodes.Add(e);
            network.Nodes.Add(d);
            network.Nodes.Add(o);

            // Add Links
            network.Links.Add(new Link(h, d));
            network.Links.Add(new Link(e, d));
            network.Links.Add(new Link(d, o));

            // P(H)
            var tableH = h.NewDistribution().Table;
            tableH[hTrue] = 0.4979;
            tableH[hFalse] = 0.5021;
            h.Distribution = tableH;

            // P(E)
            var tableE = e.NewDistribution().Table;
            tableE[eTrue] = 0.523;
            tableE[eFalse] = 0.477;
            e.Distribution = tableE;

            // P(D|H,E)
            var tableD = d.NewDistribution().Table;
            tableD[dTrue, hTrue, eTrue] = 0.11016995;
            tableD[dTrue, hTrue, eFalse] = 0.11016995;
            tableD[dTrue, hFalse, eTrue] = 0.109;
            tableD[dTrue, hFalse, eFalse] = 0.11016995;

            tableD[dFalse, hTrue, eTrue] = 0.88983005;
            tableD[dFalse, hFalse, eFalse] = 0.88983005;
            tableD[dFalse, hFalse, eTrue] = 0.891;
            tableD[dFalse, hTrue, eFalse] = 0.88983005;
            d.Distribution = tableD;

            // P(O|D)
            var tableO = o.NewDistribution().Table;
            tableO[oTrue, dTrue] = 0.725;
            tableO[oTrue, dFalse] = 0.725;
            tableO[oFalse, dTrue] = 0.275;
            tableO[oFalse, dFalse] = 0.275;
            o.Distribution = tableO;

            // use the factory design pattern to create the necessary inference related objects
            var factory = new RelevanceTreeInferenceFactory();
            var inference = factory.CreateInferenceEngine(network);
            var queryOptions = factory.CreateQueryOptions();
            var queryOutput = factory.CreateQueryOutput();

            // Check Male(H)
            if (Hopc == "y") inference.Evidence.SetState(hTrue);
            else inference.Evidence.SetState(hFalse);

            // Check Age(E)
            if (Eopc == "y") inference.Evidence.SetState(eTrue);
            else inference.Evidence.SetState(eFalse);

            var queryD = new Table(d);
            inference.QueryDistributions.Add(queryD);
            inference.Query(queryOptions, queryOutput);

            // P(D|H, E)                   
            Console.WriteLine("Diabetes Probability: P(D|H,E) = " + queryD[dTrue] + " %\n");

            // P(O|H,E)
            inference.Evidence.Clear();

            var queryO = new Table(o);
            inference.QueryDistributions.Add(queryO);
            var queryE = new Table(e);
            inference.QueryDistributions.Add(queryE);
            inference.Query(queryOptions, queryOutput);
            Console.WriteLine("Your probability to have overweight in the future given your answers:" +
                "P(O|H,E) = " + ((queryD[dTrue] * queryO[oTrue] * queryE[eTrue]) + (queryD[dFalse] * queryO[oTrue] * queryE[eFalse])));

            // P(H|O)
            inference.Evidence.Clear();
            Console.WriteLine("\nNow, let's do it the other way. Let's calculate the probability that is a man who has diabetes and overweight");
            var queryH = new Table(h);
            inference.QueryDistributions.Add(queryH);
            inference.Query(queryOptions, queryOutput);
            Console.WriteLine("P(H|O) = " + ((queryO[oTrue] * queryH[hTrue]) / ((queryO[oTrue] * queryH[hTrue]) + (queryO[oFalse] * queryH[hFalse]))) + "\n");

            //// P(E|O)
            //inference.Evidence.Clear();
            //Console.WriteLine("The probability of being 18 - 50 years old given that is someone who has diabetes and overweight");
            //inference.QueryDistributions.Add(queryH);
            //inference.Query(queryOptions, queryOutput);
            //Console.WriteLine("P(E|O) = " + ((queryO[oTrue] * queryE[eTrue]) / ((queryO[oTrue] * queryE[eTrue]) + (queryO[oFalse] * queryE[eFalse]))) + "\n");

            Console.ReadLine();

        }        
    }
}
