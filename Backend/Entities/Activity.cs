using System;
using BackendAPI.Entities.Enums;
namespace BackendAPI.Entities
{
    public class Activity
    {
        //eventualmente adicionar um campo chamado Full Address que vai buscar uma localização completa ao Google Maps API, com código postal, cidade, vila, sítio, país, para efeitos de filtro
        public int Id { get; set; }
        public DateTime BeginningDate { get; set; }
        //no caso de ser viagem, o API determina automaticamente a duração esperada
        public DateTime EndingDate { get; set; }
        public String Address { get; set; } //A morada pública, que o utilizador define ou é automaticamente definida (ou sugerida)
        public String Description { get; set; }
        //O Google Place ID usado para calcular distâncias e localizar no mapa
        //https://developers.google.com/maps/documentation/places/web-service/place-id
        public String GooglePlaceId { get; set; }
        public String RealAddress { get; set; } //A morada que corresponde ao GooglePlaceId, para efeitos de pesquisa
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double ExpectedBudget { get; set; }
        public virtual Trip Trip { get; set; }
        /*
         * transport é o tipo de actividade intermédio entre 2 actividades não do tipo transport.
         * Exemplo: Lodging (Localização) -> Transport (Pé) -> Visit (Localização) -> Transport (Carro) -> Food (Localização) -> Transport (Bicicleta) -> Lodging (Localização)
         * Transport como localização será o ponto de partida da viagem
         * A distância de transport não conta, apenas pares entre Transport contam.
         * O transporte é obrigatório. Se não determinado, é assumido que o transporte é automóvel. A actividade de transporte será criada automaticamente e inserida no meio da nova e anterior.
         * Exemplo: Anterior: FEUP. Nova: ISEP. É enviado um pedido para o sistema que responde com todos os métodos de transporte disponíveis e o utilizador escolhe um baseado no método de transporte e hora de chegada
         * A actividade de transporte é criada baseada na data de partida, se transporte público, ou imediatamente se a pé ou de carro, e calculada a hora de chegada automaticamente. Pode-se passar um bool
         * O modelo de actividade para um transporte+nova actividade será:
         *  Beginning date, recebido a partir da escolha do utilizador, desde que a data de início seja superior à data de fim da imediatamente anterior
         *  Ending Date, recebido a partir da escolha do utilizador, desde que a data de fim seja superior à data de início da actividade de transporte
         *  Address será a morada de partida especificada se a pé ou automóvel (ponto de encontro) ou estação/paragem se público
         *  Descrição será a descrição localizada (automaticamente gerado, mas personalizável) desta actividade, preferencialmente a duração estimada com a descrição do tipo de transporte (Viagem de Autocarro de X a Y 15 minutos estimados)
         *  Não terá um GooglePlaceId, será ignorado, antes tendo um:
         *  TransportType, que será usado com a actividade anterior para calcular a distância total (automático na adição de uma nova actividade na viagem em questão)
         *  Terá um ExpectedBudget, definido pela pessoa que o adiciona. Pode ser 0.
         *  Já uma actividade normal, seja a mesma uma visita, almoço, ou alojamento, terá um GooglePlaceId, que representa uma localização no Google Maps.
         *  Isto é útil, visto que podemos fazer pares de actividades com GooglePlaceId, e baseado no tipo de transporte da actividade de transporte no meio deles, calculamos a distância com o API.
         *  O modelo de adicionar actividade irá ter sempre uma actividade nela, que é a actividade de transporte. Ao adicionar no sistema, insere primeiro a de transporte, e depois a em questão.
         *  O controlador terá um endpoint que na lista de actividades da viagem atual, passando o GooglePlaceId da nova actividade, retorna as opções de transporte. O utilizador pode escolher uma, ou até ignorar a sugestão
         *  E definir manualmente o tipo de transporte e a data de início e data de fim/duração. Após isso, passa a nova actividade a criar juntamente com esta actividade de transporte.
         *  Uma actividade de transporte não pode ter uma localização inerente (GooglePlaceId), porque pode ser sujeito a bastantes transportes (linhas de metro/comboio).
         *  Ao controlador receber este modelo com duas actividades, uma de actividade em si, e outra de transporte, adiciona primeiro o transporte, e depois a actividade, calcula a distância entre a anterior e a próxima e soma essa distância à viagem
         *  Se for a primeira actividade do dia, pode ser null o transporte, caso contrário irá dar erro.
         *  Ao remover uma actividade, remove o transporte imediatamente antes. Não é possível remover actividades intermédias, tendo de refazer o itinerário (remover do fim para o início). Só é possível adicionar a partir do fim.
         *  As actividades são agrupadas numa viagem através do seu dia.
         */
        public ActivityType ActivityType { get; set; }
        public TransportType TransportType { get; set; }

    }
}
