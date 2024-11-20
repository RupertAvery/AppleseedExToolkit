namespace AFSTools
{

    public class ApexProject
    {
        public string ArchivePath { get; set; }
        public string Path { get; set; }
        public Dictionary<string, ApexFile> Files { get; set; }
        public string SourceISO { get; set; }
        public string TargetISO { get; set; }

        public ApexProject()
        {
            Files = new Dictionary<string, ApexFile>();
        }
    }
}
