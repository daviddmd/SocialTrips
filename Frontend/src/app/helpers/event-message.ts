import {EventType} from "../shared/enums/EventType";
import {ActivityType} from "../shared/enums/ActivityType";
import {TransportType} from "../shared/enums/TransportType";
import {UserGroupRole} from "../shared/enums/UserGroupRole";

export function getEventMessage(eventType: EventType): string {
  switch (eventType) {
    case EventType.GROUP_CREATE:
      return "Criação do Grupo";
    case EventType.GROUP_USER_ENTER:
      return "Entrada de Utilizador";
    case EventType.GROUP_USER_ENTER_INVITE:
      return "Entrada do Utilizador por Convite";
    case EventType.GROUP_USER_LEAVE:
      return "Saída do Utilizador";
    case EventType.GROUP_INVITE_CREATE:
      return "Criação de Convite";
    case EventType.GROUP_INVITE_REJECT:
      return "Rejeição de Convite";
    case EventType.GROUP_DETAILS_UPDATE:
      return "Actualização dos Detalhes do Grupo";
    case EventType.GROUP_USER_ROLE_CHANGE:
      return "Mudança das Permissões do Utilizador";
    case EventType.GROUP_DELETE:
      return "Remoção do Grupo";
    case EventType.GROUP_USER_BAN:
      return "Proibição do Utilizador";
    case EventType.GROUP_USER_UNBAN:
      return "Remoção da Proibição do Utilizador";
    case EventType.TRIP_CREATE:
      return "Criação da Viagem";
    case EventType.TRIP_DETAILS_UPDATE:
      return "Actualização dos detalhes da viagem";
    case EventType.TRIP_USER_ENTER:
      return "Entrada de Utilizador";
    case EventType.TRIP_USER_ENTER_INVITE:
      return "Entrada do Utilizador por Convite";
    case EventType.TRIP_USER_LEAVE:
      return "Saída do Utilizador";
    case EventType.TRIP_INVITE_CREATE:
      return "Criação de Convite";
    case EventType.TRIP_INVITE_REJECT:
      return "Rejeição de Convite";
    case EventType.TRIP_DELETE:
      return "Remoção da Viagem";
    default:
      return "Outro";

  }

}

export function getActivityName(activityType: ActivityType): string {
  switch (activityType) {
    case ActivityType.LODGING:
      return "Alojamento";
    case ActivityType.TRANSPORT:
      return "Transporte";
    case ActivityType.VISIT:
      return "Visita";
    case ActivityType.FOOD:
      return "Comida";
    case ActivityType.SHOPPING:
      return "Compras";
    case ActivityType.EXCURSION:
      return "Excursão";
    default:
      return "Outra";
  }
}

export function getTransportTypeName(transportType: TransportType): string {
  switch (transportType) {
    case TransportType.Driving:
      return "Carro";
    case TransportType.Walking:
      return "A pé";
    case TransportType.Bicycling:
      return "Bicicleta";
    case TransportType.Transit:
      return "Transporte Público";
    case TransportType.Other:
      return "Outro tipo";
    case TransportType.None:
      return "Nenhum";
    default:
      return "Outro";
  }
}

export function getUserGroupRoleName(userGroupRole: UserGroupRole): string {
  switch (userGroupRole) {
    case UserGroupRole.REGULAR:
      return "Utilizador";
    case UserGroupRole.MANAGER:
      return "Gestor";
    case UserGroupRole.MODERATOR:
      return "Moderador";
    case UserGroupRole.NONE:
      return "Nenhum";
    default:
      return "Outro";

  }
}
