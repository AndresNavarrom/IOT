using UMLIoT.Patterns.Factory.Users;

namespace UMLIoT.Core.Users;

public class UserRepository
{
    private readonly List<User> users = new();

    public User registerUser(UserCreatorClass userCreator)
    {
        var user = userCreator.UserCreatorMethod();
        users.Add(user);
        return user;
    }

    public User? findById(int id)
    {
        return users.FirstOrDefault(x => x.GetId() == id);
    }

    public User? findByEmail(string email)
    {
        return users.FirstOrDefault(x => x.GetEmail().Equals(email, StringComparison.OrdinalIgnoreCase));
    }
}
