using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParseExcelToSqlite.Models
{
    public class Local
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        //public string CodLocal { get; set; }="";
        public int CodNivel { get; set; } = 0;
        public string Codigo { get; set; } = "";
        public string Nome { get; set; } = "";
        public int DependeDeId { get; set; } = 0;



        // [ForeignKey("DependeDeId")]
        // public Local LocalPai { get; set; } = new(); // Propriedade de navegação opcional

    }
}
