using System.Collections.Generic;
using System.Linq;

namespace Utils
{
    public struct Vertex
    {
        public List<Vertex> GetNeighbors()
        {
            return new List<Vertex>();
        }
    }
    
    public static class GraphAlgo
    {
        /**
         * Returns null if no 
         */
        public static bool TryCreateBubble(HashSet<Vertex> newlyPlaced, out HashSet<Vertex> newBubble)
        {
            HashSet<Vertex> allNeighbors = new HashSet<Vertex>(newlyPlaced.SelectMany(p => p.GetNeighbors()).Where(p => !newlyPlaced.Contains(p))) ;
            var enclosures = allNeighbors.Select(Bfs);

            // merge equivalent enclosures (completely overlapping)
            {
                var enclosures_ = new List<HashSet<Vertex>>();
                foreach (var enc in enclosures)
                {
                    // stepping into the middle of a bubble
                    if (enc.Count <= 1)
                    {
                        
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

        private static HashSet<Vertex> Bfs(Vertex start)
        {
            var reached = new HashSet<Vertex>(new Vertex[] { start });
            var todo = new Queue<Vertex>(start.GetNeighbors());
            while (todo.TryDequeue(out var p))
            {
                if (!reached.Add(p))
                {
                    continue;
                }

                foreach (var q in p.GetNeighbors())
                {
                    todo.Enqueue(q);   
                }
            }
            
            return reached;
        }
    }
}