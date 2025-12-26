namespace ComprehensiveToolPlatform.Service
{
    public class ApplicationCreateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Sort { get; set; }
        public bool IsActive { get; set; }
        public string CategoryId { get; set; }
        public string FileType { get; set; }
        public int FileSize { get; set; }
    }
}
