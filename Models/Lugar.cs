using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParseExcelToSqlite.Models
{
    public class Lugar
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string NomeDistrito { get; set; } = "";
        public string NomeConcelho { get; set; } = "";
        public string NomeFreguesia { get; set; } = "";
        public string NomeLugar { get; set; } = "";
    }
}