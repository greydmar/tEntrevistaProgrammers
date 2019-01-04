using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using testProgrammers.CRUD.Core;

namespace testeProgrammers.CRUD.IU
{
    public class Controlador: DisposableObject
    {
        private Fichario _fichario;

        public Controlador(Fichario fichario)
        {
            _fichario = fichario;
        }

        protected override void Dispose(bool disposing)
        {
            if (_fichario!=null)
                _fichario.Dispose();
            _fichario = null;

            base.Dispose(disposing);
        }

        public Registro CriarRegistro()
        {
            EnsureNotDisposed();
            return _fichario.CriarEmMemoria();
        }

        public bool RemoverPorId(int idRegistro)
        {
            EnsureNotDisposed();
            return _fichario.Excluir(idRegistro);
        }

        public Registro LocalizarPorId(int idRegistro)
        {
            EnsureNotDisposed();
            return _fichario
                .Listar(registro=> registro.Id == idRegistro)
                .FirstOrDefault();
        }

        public void GravarRegistro(Registro transiente)
        {
            EnsureNotDisposed();
            _fichario.Gravar(transiente);
        }

        public IList<Registro> ListarTodos()
        {
            EnsureNotDisposed();
            return _fichario
                .Listar(null)
                .ToList();
        }
    }
}
