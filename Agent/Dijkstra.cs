using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent
{
    public class Dijkstra
    {

        private static int MAXNODES = 5;
        private static int INFINITY = 9999;//Int32.MaxValue;
        int n = 5;
        int[,] weight = new int[MAXNODES, MAXNODES];
        int[] distance = new int[MAXNODES];
        int[] precede = new int[MAXNODES];

        public int[] evaluate(int[,] weights,int source, int destination)
        {
            //int[,] w ={         {99,1,2,99,3},
            //                    {1,99,99,99,7},
            //                    {2,99,99,5,4},
            //                    {99,99,5,99,6},
            //                    {3,7,4,6,99}};

            weight = buildAdjacencyMatrix(weights);

            //int s = 1;
            //int d = 4;

            int s = source - 1;
            int d = destination - 1;

            buildSpanningTree(s, d);
            //displayResult(getShortestPath(s, d));
            int[] res = getShortestPath(s, d);
            for (int i = 0; i < res.Length; i++)
            {
                res[i] += 1;
            }
            int[] result = new int[res.Length];
            for (int i = res.Length-1, j=0; i >=0; i--, j++)
            {
                result[j] = res[i];
            }
            return result;
        }

        void buildSpanningTree(int source, int destination)
        {
            Boolean[] visit = new Boolean[MAXNODES];

            for (int i = 0; i < n; i++)
            {
                distance[i] = INFINITY;
                precede[i] = INFINITY;
            }
            distance[source] = 0;

            int current = source;
            while (current != destination)
            {
                int distcurr = distance[current];
                int smalldist = INFINITY;
                int k = -1;
                visit[current] = true;

                for (int i = 0; i < n; i++)
                {
                    if (visit[i])
                        continue;

                    int newdist = distcurr + weight[current, i];
                    if (newdist < distance[i])
                    {
                        distance[i] = newdist;
                        precede[i] = current;
                    }
                    if (distance[i] < smalldist)
                    {
                        smalldist = distance[i];
                        k = i;
                    }
                }
                current = k;
            }
        }

        /**
         * Get the shortest path across a tree that has had its path weights
         * calculated.
         */
        int[] getShortestPath(int source, int destination)
        {
            int i = destination;
            int finall = 0;
            int[] path = new int[MAXNODES];

            path[finall] = destination;
            finall++;
            while (precede[i] != source)
            {
                i = precede[i];
                path[finall] = i;
                finall++;
            }
            path[finall] = source;

            int[] result = new int[finall + 1];
            Array.Copy(path, 0, result, 0, finall + 1);
            return result;
        }

        /**
         * Print the result.
         */
        void displayResult(int[] path)
        {
            int displayResult = 0;
            Console.WriteLine("\nThe shortest path followed is : \n");
            for (int i = path.Length - 1; i > 0; i--)
            {
                Console.WriteLine("\t\t( " + path[i] + " ->" + path[i - 1] +
                        " ) with cost = " + weight[path[i], path[i - 1]]);
                displayResult += weight[path[i], path[i - 1]];
            }
            Console.WriteLine("For the Total Cost = " +
                //        distance[path[path.Length-1]]);
                    displayResult);
        }
    //----------------------------------
    public int[,] buildAdjacencyMatrix(int[,] arr)
    {
        int maxval = 0;
        for (int g = 0; g < arr.GetLength(0); g++)
        {
            
            maxval = Math.Max(maxval, arr[g, 0]);
            maxval = Math.Max(maxval, arr[g, 1]);
        }
        

        int[,] adjMtrx = new int[maxval, maxval];
                  
        for (int j = 0; j < adjMtrx.GetLength(0); j++)
            {
                for (int k = 0; k < adjMtrx.GetLength(1); k++)
                {
                    adjMtrx[j, k] = INFINITY;
                }
            
            }

            for (int i = 0; i < arr.GetLength(0); i++)
            {
                adjMtrx[arr[i, 0]-1, arr[i, 1]-1] = arr[i, 2];
                adjMtrx[arr[i, 1]-1, arr[i, 0]-1] = arr[i, 2];
            }

        //------PRINT
            for (int j = 0; j < adjMtrx.GetLength(0); j++)
            {
                for (int k = 0; k < adjMtrx.GetLength(1); k++)
                {
                    Console.Write(adjMtrx[j, k] + " ");
                }
                Console.WriteLine("");
            }

            return adjMtrx;
    }


    //----------------------------------
    }

}
