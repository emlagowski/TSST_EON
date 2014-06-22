using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DijkstraAlgorithm
{
    class Dijkstra
    {

        int[,] L ={
                {-1,  5, -1, -1, -1,  3, -1, -1}, 
                { 5, -1,  2, -1, -1, -1,  3, -1}, 
                {-1,  2, -1,  6, -1, -1, -1, 10}, 
                {-1, -1,  6, -1,  3, -1, -1, -1},
                {-1, -1, -1,  3, -1,  8, -1,  5}, 
                { 3, -1, -1, -1,  8, -1,  7, -1}, 
                {-1,  3, -1, -1, -1,  7, -1,  2}, 
                {-1, -1, 10, -1,  5, -1,  2, -1} 
            };
        
        private int rank = 0;
        private int[,] weights;
        private int[] nodes;
        public int[] costs;
        private int trank = 0;
        public Dijkstra(int[,] paramArray)
        {
            rank = paramArray.GetLength(0);
            weights = new int[rank, rank ];
            nodes = new int[rank];
            costs = new int[rank];

            weights = paramArray;

            for (int i = 0; i < rank; i++)
            {
                nodes[i] = i;
            }
            nodes[0] = -1;
            for (int i = 1; i < rank; i++)
                costs[i] = weights[0, i];
        }
        public void DijkstraSolving()
        {
            int minValue = Int32.MaxValue;
            int minNode = 0;
            for (int i = 0; i < rank; i++)
            {
                if (nodes[i] == -1)
                    continue;
                if (costs[i] > 0 && costs[i] < minValue)
                {
                    minValue = costs[i];
                    minNode = i;
                }
            }
            nodes[minNode] = -1;
            for (int i = 0; i < rank; i++)
            {
                if (weights[minNode, i] < 0)
                    continue;
                if (costs[i] < 0)
                {
                    costs[i] = minValue + weights[minNode, i];
                    continue;
                }
                if ((costs[minNode] + weights[minNode, i]) < costs[i])
                    costs[i] = minValue + weights[minNode, i];
            }
        }
        public void Run()
        {
            for (trank = 0; trank <= rank; trank++)
            {
                DijkstraSolving();
                Console.WriteLine("iteration" + trank);
                for (int i = 0; i < rank; i++)
                    Console.Write(costs[i] + " ");
                Console.WriteLine("");
                for (int i = 0; i < rank; i++)
                    Console.Write(nodes[i] + " ");
                Console.WriteLine("");
            }
        }
    }
}
