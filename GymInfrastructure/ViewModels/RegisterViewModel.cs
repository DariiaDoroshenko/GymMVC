namespace GymInfrastructure.ViewModels
{
    public class RegisterViewModel
    {
        public string Name { get; set; }
        public int? Year { get; set; } // <-- nullable


        public string Email { get; set; }   
        public string Password { get; set; }    
        public string PhoneNumber { get; set; }
    }
}
