namespace insoles.Messages
{
    public class GraphRange
    {
        public int first { get; set; }
        public int last { get; set; }
        public GraphRange(int first, int last) 
        { 
            this.first = first;
            this.last = last;
        }
    }
}
