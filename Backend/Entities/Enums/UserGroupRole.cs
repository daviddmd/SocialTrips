namespace BackendAPI.Entities.Enums
{
    public enum UserGroupRole
    {
        //moderador apenas pode remover posts, funciona como um utilizador normal para além disso
        REGULAR,
        MANAGER,
        MODERATOR,
        NONE
    }
}
