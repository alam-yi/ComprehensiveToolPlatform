using ComprehensiveToolPlatform.Repository.Models;

namespace ComprehensiveToolPlatform.Service
{
    public class HomeViewVo
    {
        public List<Category> Categories { get; set; }
        public Dictionary<string, List<Application>> ApplicationsByCategory { get; set; }
    }
}
