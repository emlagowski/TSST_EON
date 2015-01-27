using System;

namespace SubnetworkController
{
    public class Dijkstra
    {
        static int _maxnodes = 5;
        const int Infinity = 9999; //Int32.MaxValue;

        int[,] _weight = new int[_maxnodes, _maxnodes];
        int[] _distance = new int[_maxnodes];
        int[] _precede = new int[_maxnodes];

        public int[] Evaluate(int[,] weights, int source, int destination)
        {
            // Testing data
            //int[,] w ={         {99,1,2,99,3},
            //                    {1,99,99,99,7},
            //                    {2,99,99,5,4},
            //                    {99,99,5,99,6},
            //                    {3,7,4,6,99}};
            //int s = 1;
            //int d = 4;

            _weight = buildAdjacencyMatrix(weights);

            _maxnodes = _weight.GetLength(0);
            _distance = new int[_maxnodes];
            _precede = new int[_maxnodes];

            var s = source - 1;
            var d = destination - 1;

            BuildSpanningTree(s, d);

            var res = GetShortestPath(s, d);

            if (PathCost(res) > Infinity)
                return null;


            for (var i = 0; i < res.Length; i++)
            {
                res[i] += 1;
            }
            var result = new int[res.Length];
            for (int i = res.Length - 1, j = 0; i >= 0; i--, j++)
            {
                result[j] = res[i];
            }
            return result;
        }

        void BuildSpanningTree(int source, int destination)
        {
            var visit = new Boolean[_maxnodes];

            for (var i = 0; i < _maxnodes; i++)
            {
                _distance[i] = Infinity;
                _precede[i] = Infinity;
            }
            _distance[source] = 0;

            var current = source;
            while (current != destination)
            {

                var distcurr = _distance[current];
                var smalldist = Infinity;
                var k = -1;
                visit[current] = true;

                for (var i = 0; i < _maxnodes; i++)
                {
                    if (visit[i])
                        continue;

                    var newdist = distcurr + _weight[current, i];
                    if (newdist < _distance[i])
                    {
                        _distance[i] = newdist;
                        _precede[i] = current;
                    }
                    if (_distance[i] < smalldist)
                    {
                        smalldist = _distance[i];
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
        int[] GetShortestPath(int source, int destination)
        {
            var i = destination;
            var finall = 0;
            var path = new int[_maxnodes];

            path[finall] = destination;
            finall++;
            while (_precede[i] != source)
            {
                i = _precede[i];
                path[finall] = i;
                finall++;
            }
            path[finall] = source;

            var result = new int[finall + 1];
            Array.Copy(path, 0, result, 0, finall + 1);
            return result;
        }

        private int[,] buildAdjacencyMatrix(int[,] arr)
        {
            var maxval = 0;
            for (var g = 0; g < arr.GetLength(0); g++)
            {

                maxval = Math.Max(maxval, arr[g, 0]);
                maxval = Math.Max(maxval, arr[g, 1]);
            }


            var adjMtrx = new int[maxval, maxval];

            for (var j = 0; j < adjMtrx.GetLength(0); j++)
            {
                for (var k = 0; k < adjMtrx.GetLength(1); k++)
                {
                    adjMtrx[j, k] = Infinity;
                }

            }

            for (var i = 0; i < arr.GetLength(0); i++)
            {
                adjMtrx[arr[i, 0] - 1, arr[i, 1] - 1] = arr[i, 2];
                adjMtrx[arr[i, 1] - 1, arr[i, 0] - 1] = arr[i, 2];
            }

            //------PRINT
            for (var j = 0; j < adjMtrx.GetLength(0); j++)
            {
                for (var k = 0; k < adjMtrx.GetLength(1); k++)
                {
                    //Console.Write(adjMtrx[j, k] + " ");
                }
                //Console.WriteLine("");
            }

            return adjMtrx;
        }

        private int PathCost(int[] path)
        {
            var displayResult = 0;
            for (var i = path.Length - 1; i > 0; i--)
            {
                displayResult += _weight[path[i], path[i - 1]];
            }
            return displayResult;
        }
    }
}