namespace CourseWork_Vychkin_WEB.Data
{
    public class House
    {
        public int Id { get; set; }
        //public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public double Price { get; set; }
        public int SquareMeter { get; set; }
        public int Rooms { get; set; }
        public int AddressId { get; set; }
        public int CategoryId { get; set; }
        public string Username { get; set; } = default!;
        public bool IsModerated { get; set; } = false;
        public User? User { get; set; }
        public Address? Address { get; set; } = default!;
        //public List<HouseAndCategory>? HouseAndCategories { get; set; }
        public Category? Category { get; set; }
        public List<Tag>? Tags { get; set; }
        public List<Image>? Images { get; set; }
        public List<Rent>? Rents { get; set; }

    }
}
