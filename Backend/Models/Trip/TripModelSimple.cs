using BackendAPI.Models.Attachment;
using System;

namespace BackendAPI.Models.Trip
{
    public class TripModelSimple
    {
        //usado para referenciar o(s) objecto(s) de trip no grupo. não debitar demasiada informação a utilizadores quando vão buscar o objeto grupo
        public int Id { get; set; }
        public String Name { get; set; }
        public String Image { get; set; }
        public String Description { get; set; }
        public DateTime BeginningDate { get; set; }
        public DateTime EndingDate { get; set; }
        public double ExpectedBudget { get; set; }
        public double TotalDistance { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsPrivate { get; set; }
    }
}
