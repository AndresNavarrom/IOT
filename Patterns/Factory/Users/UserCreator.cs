using UMLIoT.Core.Users;

namespace UMLIoT.Patterns.Factory.Users;

public class UserCreator : Abstract.UserCreator
{
    private static int currentId = 1;
    private readonly string name;
    private readonly string email;
    private readonly string password;

    public UserCreator(string name, string email, string password)
    {
        this.name = name;
        this.email = email;
        this.password = password;
    }

    public override User UserCreatorMethod()
    {
        return new User(currentId++, name, email, password);
    }
}
