namespace BackendAPI.Entities.Enums
{
    public enum TransportType
    {
        //exceptuando o other, faz mapeamento 1:1 com a biblioteca
        Driving,
        Walking,
        Bicycling,
        //transporte público mais rápido
        Transit,
        Other,
        None
    }
}
