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

        public string Address()
        {
            var address = $"{ContactName} " +
                          $"{ContactMailAddress} " +
                          $"{ContactMailCity}, {ContactMailState} {ZipCode5}";

            if (!string.IsNullOrEmpty(ZipCode4))
            {
                address += $"-{ZipCode4}";
            }

            return address;
        }
  }
}
