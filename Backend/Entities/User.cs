using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using BackendAPI.Entities.Enums;
using Microsoft.AspNetCore.Identity;

namespace BackendAPI.Entities
{
    public class User : IdentityUser
    {
        /*
         * As imagems e multimédia dos posts irão usar uma classe Attachment. Como não dá para passar imagens por JSON, irá haver um endpoint que espera um IFormFile, armazena a imagem na DB e retorna um objeto Attachment
         * Ao atualizar, remover ou adicionar, o ficheiro no sistema de ficheiros deverá sofrer alterações. 
         */
        //fazer com que o mesmo seja extendido da classe identityuser
        //no controller endpoints para atualizar pwd ou atualizar informacao em geral (passando um usermodelself)
        public String Name { get; set; }
        public virtual Attachment Photo { get; set; }
        public String Country { get; set; } //Código de 2 letras ISO, para disponibilizar conteúdo recomendado e providenciar estatísticas, assim como a bandeira do utilizador
        public String Locale { get; set; }
        public String City { get; set; }
        public String Description { get; set; }
        public double TravelledKilometers { get; set; }
        public String Facebook { get; set; }
        public String Instagram { get; set; }
        public String Twitter { get; set; }
        public virtual Ranking Ranking { get; set; }
        public virtual List<UserGroup> Groups { get; set; }
        public virtual List<GroupInvite> GroupInvites { get; set; }
        public virtual List<TripInvite> TripInvites { get; set; }
        public virtual List<Post> Posts { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreationDate { get; set; }
        public virtual List<UserTrip> Trips { get; set; }
        public virtual List<User> Followers { get; set; }
        public virtual List<User> Following { get; set; }
    }
}
