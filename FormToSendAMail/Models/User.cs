namespace FormToSendAMail.Models
{
    public class User
    {
        public string FirstName { get; set; }  // Imię użytkownika
        public string Username { get; set; }   // Nazwa użytkownika
        public string Email { get; set; }      // Email użytkownika
        public bool HasAgreed { get; set; }    // Zgoda użytkownika
        public string CustomMessage { get; set; } // Personalizowana wiadomość
    }

}
