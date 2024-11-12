namespace AFSTools
{

    public class ApexProject
    {
        public string Path { get; set; }
        public Dictionary<string, ApexFile> Files { get; set; }

        public ApexProject()
        {
            Files = new Dictionary<string, ApexFile>();
        }
    }
}
