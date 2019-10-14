namespace ExcelClaimGenerator
{
    internal class ProviderInfo
    {
        public ProviderInfo()
        {
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int ProviderId { get; set; }
        public int ProviderTypeId { get; set; }
        public int? ProviderNPI { get; set; }
        public string AlternateId { get; set; }
        public string WorkAddress1 { get; set; }
        public string WorkAddress2 { get; set; }
        public string WorkCity { get; set; }
        public string WorkPhoneNumber { get; set; }
        public string SpecialtyCode { get; internal set; }
        public string FullName => $"{FirstName} {LastName}";


    }
}