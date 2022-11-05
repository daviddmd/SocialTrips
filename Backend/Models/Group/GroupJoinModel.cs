using System;
using System.ComponentModel.DataAnnotations;

namespace BackendAPI.Models.Group
{
    public class GroupJoinModel
    {

        [Required]
        public int GroupId { get; set; }
        //o utilizador juntou-se ao grupo com o convite x, convidado por y, na data z
        //claramente que vai ser authorized, e vai realizar uma verificação sobre o convite e o utilizador que entrou nesse caminho
        //group join e group leaves normais apenas precisam de autorizacao, sao uma questao de adicionar ou remover users da lista
        //opcional, caso o grupo seja privado
        public Guid? InviteId { get; set; }

    }
}
