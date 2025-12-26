namespace ComprehensiveToolPlatform.Service
{
    public class CategoryVo
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public int Sort { get; set; }

        public bool IsActive { get; set; }

        public List<ApplicationVo> Applications { get; set; }
    }
}
