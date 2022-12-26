using System;
using System.Collections.Generic;

namespace BackendAPI.Entities
{
    public class Trip
    {
        public int Id { get; set; }
        public String Name { get; set; }
        public virtual Attachment Image { get; set; }
        public String Description { get; set; }
        public DateTime BeginningDate { get; set; }
        public DateTime EndingDate { get; set; }
        /*
         * dá uma viagem como terminada. a mesma continuará a ser pública. Impede novas atividades de serem adicionadas ou as datas de início/fim modificadas por todos os gestores/admins 
         * quando a mesma é definida como completa, a distância total percorrida nesta viagem é adicionada aos utilizadores que nela participaram. quando é definida como falsa (se o estado anterior foi verdadeiro)
         * esta mesma distância será subtraída (atualizada em todos os utilizadores); algoritmo = sum(totaldistance)(viagens completo=true) [utilizadores viagem atual]
         * Todos estes atributos juntamente hidden, completo, etc serão atualizados no controlador com método PUT, a distância total é automaticamente calculada, e os utilizadores terão a sua distância atualizada no mesmo
         * Cancelado não é um atributo, mas a remoção.
         * A distância total da viagem é dinamicamente atualizada à medida que atividades são adicionadas ou removidas da mesma no controlador da viagem. Calcula-se a distância total da viagem no momento da adição/remoção
         * Através da soma da distância entre paragens entre dias, o valor sendo atualizado quando uma atividade é adicionada/removida. Usa-se o API do google maps para este fim.
         */
        public bool IsCompleted { get; set; }
        //tornar uma viagem privada dentro de um grupo, é necessário convite para entrar nela (ou ser gestor)
        public bool IsPrivate { get; set; }
        // Para cancelar uma viagem, remove-se a mesma do sistema (totalmente) e subtrai-se a distância total (atualizam-se todos os membros desta viagem) dos membros da viagem em questão
        public double ExpectedBudget { get; set; } //À medida que mais actividades vão sendo adicionadas, o orçamento esperado vai aumentando
        //é esperado que a primeira e última actividades sejam do tipo lodging, mas não é obrigatório serem. são actividades como quaisquer outras.
        public virtual List<Activity> Activities { get; set; }
        public virtual List<Post> Posts { get; set; }
        public virtual Group Group { get; set; }
        /*
         * atualizado à medida que atividades vão sendo removidas ou adicionadas
         * quando uma viagem é dada como terminada (mais atividades não podem ser adicionadas), os utilizadores do grupo em questão têm as suas distâncias totais percorridas atualizadas
         */
        public double TotalDistance { get; set; }
        public virtual List<UserTrip> Users { get; set; }
        public virtual List<TripInvite> Invites { get; set; }
        public virtual List<TripEvent> Events { get; set; }
    }
}
