namespace uic_forms.models
{
    public class Contact
    {
        public string ContactName { get; set; }
        public string ContactMailCity { get; set; }
        public string ContactMailState { get; set; }
        public string ContactMailAddress { get; set; }
        public string ZipCode5 { get; set; }
        public string ZipCode4 { get; set; }
        public int ContactType { get; set; }

        public string Zip()
        {
            var zip = $"{ZipCode5}";

            if (!string.IsNullOrEmpty(ZipCode4))
            {
                zip += $"-{ZipCode4}";
            }

            return zip;
        }
  }
}
