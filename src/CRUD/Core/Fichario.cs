using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Mail;
using System.Threading;
using LiteDB;

namespace testProgrammers.CRUD.Core
{
    public sealed class Fichario: DisposableObject
    {
        private const string ColecaoRegistros = "Registros";

        private static readonly Dictionary<long, Registro> _transientes = new Dictionary<long, Registro>();
        private static long _lid = -1;

        private LiteRepository _repositorio;

        public Fichario()
        {
            _repositorio = LiteDbService.Default.GetLiteDbAccess();

            InicializarSequenceSeNecessario();
        }

        protected override void Dispose(bool disposing)
        {
            if (_repositorio != null)
                _repositorio.Dispose();
            _repositorio = null;

            base.Dispose(disposing);
        }

        public IEnumerable<Registro> Listar(Expression<Func<Registro, bool >> predicate)
        {
            EnsureNotDisposed();

            if (predicate!=null)
                return _repositorio
                    .Query<Registro>(ColecaoRegistros)
                    .Where(predicate)
                    .ToList();

            return _repositorio
                .Query<Registro>(ColecaoRegistros)
                .ToList();
        }

        public void Gravar(Registro registro)
        {
            EnsureNotDisposed();

            var ehTransiente = EhTransiente(registro.Id);

            if (ehTransiente)
            {
                int id = registro.Id;
                BsonValue result;
                result = _repositorio.Insert(registro, ColecaoRegistros);
                registro.Id = result.AsInt32;

                ExcluirTransiente(id);
            }
            else
                _repositorio.Update(registro, ColecaoRegistros);
        }

        public bool Excluir(Registro registro)
        {
            EnsureNotDisposed();
            return Excluir(registro.Id);
        }

        public bool Excluir(int idRegistro)
        {
            EnsureNotDisposed();

            var ehTransiente = EhTransiente(idRegistro);

            if (!ehTransiente)
            {
                int excluidos = _repositorio.Delete<Registro>(item => item.Id == idRegistro, ColecaoRegistros);

                return excluidos > 0;
            }

            return ExcluirTransiente(idRegistro);
        }

        public Registro CriarEmMemoria()
        {
            EnsureNotDisposed();

            var id = Interlocked.Increment(ref _lid);
            var transiente = new Registro()
            {
                Id = (int)id,
                Email = new MailAddress("nenhum@email.definido.com"),
                Nome = string.Empty,
                Telefone = string.Empty
            };

            lock (_transientes)
            {
                LiteDbService.Default.SincronizarSequence(id, ColecaoRegistros);
                _transientes.Add(id, transiente);
            }
            return transiente;
        }

        #region transient simple support

        private bool EhTransiente(int idRegistro)
        {
            lock (_transientes)
            {
                return _transientes.ContainsKey(idRegistro);
            }
        }

        private bool ExcluirTransiente(int idRegistro)
        {
            lock (_transientes)
            {
                return _transientes.Remove(idRegistro);
            }
        }

        private Registro LerTransiente(int idRegistro)
        {
            lock (_transientes)
            {
                Registro result;
                if (!_transientes.TryGetValue(idRegistro, out result))
                    return null;
                return result;
            }
        }
        
        private static void InicializarSequenceSeNecessario()
        {
            if (Interlocked.Read(ref _lid) >= 0)
                return;

            lock (_transientes)
            {
                var tSeq = LiteDbService.Default
                    .ObterSequence(ColecaoRegistros);

                Interlocked.Exchange(ref _lid, tSeq.Item2);
            }
        }

        #endregion

    }
}
