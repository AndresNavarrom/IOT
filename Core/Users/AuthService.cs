namespace UMLIoT.Core.Users;

public class AuthService
{
    public bool login(string email, string password)
    {
        return !string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password);
    }

    public void logout()
    {
    }
}
