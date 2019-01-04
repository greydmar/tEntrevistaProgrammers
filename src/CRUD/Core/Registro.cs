using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace testProgrammers.CRUD.Core
{
    public class Registro
    {
        [Key]
        public int Id { get; set; }

        public string Nome { get; set; }

        [Required]
        public MailAddress Email { get; set; }

        [Required]
        public string Telefone { get; set; }
    }
}