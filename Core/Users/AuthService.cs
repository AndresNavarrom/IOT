namespace UMLIoT.Core.Users;

public class AuthService
{
    private User? currentUser;
    private readonly UserRepository userRepository;

    public AuthService(UserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public bool login(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return false;

        var user = userRepository.findByEmail(email);

        if (user != null && user.GetPassword() == password)
        {
            currentUser = user;
            return true;
        }

        return false;
    }

    public void logout()
    {
        currentUser = null;
    }

    public User? getCurrentUser() => currentUser;
}
