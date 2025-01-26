using System;
using System.Collections.Generic;
using System.Linq;

namespace Utils
{
    public static class GraphAlgo
    {
        private static Func<GridPoint, bool> _notBubble = GridGen.Not(GridGen.IsBubble);
        
        /**
         * Returns null if no 
         */
        public static bool TryCreateBubble(HashSet<GridPoint> newlyPlaced, out HashSet<GridPoint> newBubble)
        {
            HashSet<GridPoint> allNeighbors = new HashSet<GridPoint>(
                newlyPlaced.SelectMany(p => GridGen.GetNeighbors(p, _notBubble)).Where(p => !newlyPlaced.Contains(p))) ;
            var enclosures = allNeighbors.Select(Bfs);

            // merge equivalent enclosures (completely overlapping)
            {
                var enclosures_ = new List<HashSet<GridPoint>>();
                foreach (var enc in enclosures)
                {
                    // stepping into the middle of a bubble
                    if (enc.Count <= 1)
                    {
                        continue;
                    }
                    
                    foreach (var enc_ in enclosures_)
                    {
                        foreach (var q in enc_)
                        {
                            if (enc.Contains(q)) // only care about complete overlap, meaning one overlap check is enough
                            {
                                goto has_same;
                            }
                        }
                    }
                    
                    enclosures_.Add(enc);
                    
                    has_same:
                    continue;
                }

                enclosures = enclosures_;
            }

            if (enclosures.Count() == 1)
            {
                newBubble = null;
                return false;
            }

            var smallestEnclosure = enclosures.Min(enc => enc.Count);
            enclosures = enclosures.Where(enc => enc.Count == smallestEnclosure);
            

            newBubble = enclosures.First(); // fuck it let RNG do the tie breaking
            return true;
        }

        private static HashSet<GridPoint> Bfs(GridPoint start)
        {
            var reached = new HashSet<GridPoint>(new[] { start });
            var todo = new Queue<GridPoint>(GridGen.GetNeighbors(start, _notBubble));
            while (todo.TryDequeue(out var p))
            {
                if (!reached.Add(p))
                {
                    continue;
                }

                foreach (var q in GridGen.GetNeighbors(p, _notBubble))
                {
                    todo.Enqueue(q);   
                }
            }
            
            return reached;
        }
    }
}