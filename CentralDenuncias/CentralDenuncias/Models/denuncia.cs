﻿namespace CentralDenuncias.Models
{
    public class denuncia
    {
        public int id { get; set; }
        public string link_membro { get; set; }
        public string descricao { get; set; }
        public string provas { get; set; }
        public string staffer { get; set; }
        public DateTime data_criacao { get; set; }
        public DateTime data_alteracao { get; set; }
    }
}