namespace CentralDenuncias.Models
{
    public class ip
    {
        public int id { get; set; }
        public string nome_staffer { get; set; }
        public string ip_staffer { get; set; }
        public DateTime data_acesso { get; set; } // A hora é sempre salva com 3 horas a mais no fuso de Brasília
    }
}
