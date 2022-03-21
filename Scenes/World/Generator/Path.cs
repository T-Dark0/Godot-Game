using System.Collections.Generic;

class Path
{
    private List<Segment> segments;



    private struct Segment
    {
        private Vector2i from;
        private Vector2i to;
    }
}