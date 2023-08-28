namespace CourseWork_Vychkin_WEB.Data
{
    public class Rent
    {
        public int Id { get; set; }
        public string Username { get; set; } = default!;
        public int HouseId { get; set; }
        public int CountOfDay { get; set; }
        public double? Price { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public User? User { get; set; }
        public House? House { get; set; }

    }
}
