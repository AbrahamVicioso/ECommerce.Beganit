namespace ECommerce.Beganit.AdminPanel.Models.ViewModels
{
    public class CustomerAddressViewModel
    {
        public int Id { get; set; }

        public int? CustomerId { get; set; }

        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string PostalCode { get; set; }

        public string Country { get; set; }

        public string PhoneNumber { get; set; }

        public bool? IsDefault { get; set; }

        public string AddressType { get; set; }
    }
}
