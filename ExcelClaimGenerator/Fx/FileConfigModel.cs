namespace ExcelClaimGenerator
{
    public class FileConfigModel
    {
        
        public string SenderIDQualifier { get; set; }
        public string SenderID { get; internal set; }
        public int? BillerID { get; internal set; }
        public int? SubmitterID { get; set; }
    }
}