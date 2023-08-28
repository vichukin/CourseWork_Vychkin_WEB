using CourseWork_Vychkin_WEB.Models;
using Newtonsoft.Json;

namespace CourseWork_Vychkin_WEB.Data
{
    public class Address
    {
        public int Id { get; set; }

        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string FormattedAddress { get; set; }
        public string AddressLabel { get; set; }
        public House? House { get; set; }
    }
}
