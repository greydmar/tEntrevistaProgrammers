using System;
using System.Configuration;
using System.Runtime.InteropServices;
using LiteDB;

namespace testProgrammers.CRUD.Core
{
    /// <summary>
    /// Organizar e controlar acesso ao engine do litedb
    /// </summary>
    public class LiteDbService: DisposableObject
    {
        private const string ColecaoSequences = "Sequences";

        private LiteDatabase _liteDb;
        private BsonMapper _mapper;
        private ConnectionString _connString;
        private bool _disposed;

        private static Lazy<LiteDbService> _instance = new Lazy<LiteDbService>();

        private LiteDbService()
        {
            _connString = new ConnectionString(ConfigurationManager.ConnectionStrings["DefaultDb"].ConnectionString);
            _mapper = BsonMapper.Global;
        }

        public static LiteDbService Default
        {
            get { return _instance.Value; }
        }

        public static void Inicializar()
        {
            if (_instance.IsValueCreated)
                return;

            Default.InicializarInterno();
        }

        private void InicializarInterno()
        {
            // Inicializações adicionais e verificações
        }

        protected override void Dispose(bool disposing)
        {
            if (_liteDb != null)
                _liteDb.Dispose();

            _liteDb = null;
            _disposed = true;
        }

        protected override void EnsureNotDisposed(string message = null)
        {
            base.EnsureNotDisposed("Provedor de acesso ao LiteDb já foi encerrado. Operação indisponível");
        }

        public void DefinirMapeamento()
        {

        }

        public LiteRepository GetLiteDbAccess()
        {
            EnsureNotDisposed();

            var db = EnsureDatabase();

            return new LiteRepository(db, false);
        }

        private LiteDatabase EnsureDatabase()
        {
            _liteDb = new LiteDatabase(_connString, _mapper);
            return _liteDb;
        }

        public Tuple<string, long> ObterSequence(string collectionName)
        {
            lock (this)
                using (var repositorio = GetLiteDbAccess())
                {
                    var tSeqRegistros =
                        repositorio.SingleOrDefault<Tuple<string, long>>(t => t.Item1 == collectionName,
                            ColecaoSequences);

                    if (tSeqRegistros == null)
                    {
                        tSeqRegistros = new Tuple<string, long>(collectionName, 0);
                        repositorio.Insert(tSeqRegistros);
                    }

                    return tSeqRegistros;

                }
        }

        public Tuple<string, long> SincronizarSequence(long id, string collectionName)
        {
            lock (this)
                using (var repositorio = GetLiteDbAccess())
                {
                    var tSeqRegistros = repositorio.Single<Tuple<string, long>>(item=> item.Item1 == collectionName);

                    repositorio.Update(tSeqRegistros, ColecaoSequences);

                    return tSeqRegistros;
                }
        }
    }
}