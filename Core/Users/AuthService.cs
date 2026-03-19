namespace UMLIoT.Core.Users;

public class AuthService
{
    private readonly UserRepository userRepository;

    public AuthService(UserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public bool login(string email, string password, string? ignored = null)
    {
        var user = userRepository.findByEmail(email);
        return user is not null && user.GetPassword() == password;
    }

    public void logout()
    {
        Console.WriteLine("Sesión cerrada");
    }

    public User? getCurrentUser() => currentUser;
}
