using UMLIoT.Patterns.Factory.Users;

namespace UMLIoT.Core.Users;

public class UserRepository
{
    private readonly List<User> users;

    public UserRepository()
    {
        users = new List<User>();
    }

    public User registerUser(UserCreator userCreator)
    {
        User user = userCreator.UserCreatorMethod();
        users.Add(user);
        return user;
    }

    public User? findById(int id)
    {
        return users.FirstOrDefault(user => user.GetId() == id);
    }
}
