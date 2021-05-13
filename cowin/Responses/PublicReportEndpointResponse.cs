using cowin.Models;

namespace cowin.Responses
{
    public class PublicReportEndpointResponse
    {
        public TopBlock topBlock { get; set; }
    }
    public class TopBlock
    {
        public TopBlock()
        {
            registration = new();
            vaccination = new();
        }
        public Registration registration { get; set; }
        public Vaccination vaccination { get; set; }
    }
}
