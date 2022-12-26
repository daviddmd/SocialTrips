using System;
using System.Collections.Generic;

namespace BackendAPI.Entities
{
    public class Ranking
    {
        public int Id { get; set; }
        public String Description { get; set; }
        public String Name { get; set; }
        public String Color { get; set; }
        /*
         * O número mínimo de quilómetros necessários para atingir este ranking
         * Podem-se criar novos rankings com números variáveis de quilómetros mínimos, mas nunca possível criar ou alterar um existente para existirem 2 com o mesmo número
         * Quando se altera, cria ou remove um ranking, são alterados os rankings de todos os utilizadores, se assim se revelar necessário
         * Por exemplo: Novato - 0 km Principiante - 5 km Intermediário - 10 km SuperViajante - 50 km
         * No final de cada viagem o número de quilómetros percorridos de cada utilizadpr é actualizado,
         * e com tal, se este número for maior que o número mínimo de quilómetros do(s) ranking superior(es) mais próximo(s), o mesmo é actualizado (excepto se for já o máximo)
         */
        public double MinimumKilometers { get; set; }
        public virtual List<User> UserList { get; set; }

    }
}
