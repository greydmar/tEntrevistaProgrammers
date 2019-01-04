using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using testeProgrammers.CRUD.IU;
using testeProgrammers.CRUD.IU.Views;
using testProgrammers.CRUD.Core;
using Terminal.Gui;

namespace testeProgrammers
{
    using System;

    public class Program
    {
        public static void Main()
        {
            Application.Init();

            LiteDbService.Inicializar();
            var fichario = new Fichario();
            var controleDados = new Controlador(fichario);

            JanelaPrincipal.Criar(controleDados, Application.Top);
            Application.Run();
        }
    }
}
