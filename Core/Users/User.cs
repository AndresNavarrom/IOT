namespace UMLIoT.Core.Users;

public class User
{
    private int id;
    private string name;
    private string email;
    private string password;

    public User()
    {
        id = 0;
        name = string.Empty;
        email = string.Empty;
        password = string.Empty;
    }

    public User(int id, string name, string email, string password)
    {
        this.id = id;
        this.name = name;
        this.email = email;
        this.password = password;
    }

    public int GetId() => id;
    public string GetName() => name;
    public string GetEmail() => email;
    public string GetPassword() => password;
}
