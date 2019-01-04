using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using LiteDB;

namespace testProgrammers.CRUD.Core
{
    public class Registro
    {
        [Key, BsonId]
        public int Id { get; set; }

        public string Nome { get; set; }

        [Required]
        public MailAddress Email { get; set; }

        public string EndEmail
        {
            get
            {
                return Email?.Address;
            }
            set
            {
                if (value != null)
                    Email = new MailAddress(value);
                else
                    Email = null;
            }
        }

        [Required]
        public string Telefone { get; set; }
    }
}