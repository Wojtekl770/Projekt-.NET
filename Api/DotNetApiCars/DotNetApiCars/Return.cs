namespace DotNetApiCars
{
    //tzreba poprawic
    public class ReturnRequest
    {
        public int Rent_Id { get; set; }
        public string Client_Id { get; set; } = "";
        public string Platform { get; set; } = "";
        public string Email { get; set; } = "";

    }
}
