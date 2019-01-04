using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using testProgrammers.CRUD.Core;

namespace testeProgrammers.CRUD.IU.Views
{
    public enum ModoEdicao
    {
        Leitura,
        Gravacao
    }

    public enum ModalResult
    {
        Confirmar,
        Descartar
    }

    public class ResultadoDialogo
    {
        public Registro Registro { get; set; }

        public bool Modificado { get; set; }

        public ModalResult Opcao { get; set; }
    }
}
