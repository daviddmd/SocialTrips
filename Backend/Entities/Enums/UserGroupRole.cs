using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
