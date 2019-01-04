using System;
using System.Configuration;
using System.IO;
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

        private static Lazy<LiteDbService> _instance = new Lazy<LiteDbService>(() =>
        {
            return new LiteDbService();
        });

        private LiteDbService()
        {
            _connString = new ConnectionString(ConfigurationManager.ConnectionStrings["DefaultDb"].ConnectionString);
            var directory = Path.GetDirectoryName(_connString.Filename);

            if (!Directory.Exists(directory))
            {
                if (!Path.IsPathRooted(directory))
                {
                    var baseDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                    directory = Path.Combine(baseDirectory, directory);
                }
                Directory.CreateDirectory(directory);
            }

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

        private class SequenceValueWrapper
        {
            [BsonId]
            public int Id { get; set; }

            public string SequenceName { get; set; }

            [BsonField]
            public long Value { get; set; }

            public SequenceValueWrapper(){}

            public SequenceValueWrapper(string sequenceName, long value)
            {
                SequenceName = sequenceName;
                Value = value;
            }

            public static explicit  operator Tuple<string, long>(SequenceValueWrapper self)
            {
                return new Tuple<string, long>(self.SequenceName, self.Value);
            }

        }

        private void InicializarInterno()
        {
            _mapper = BsonMapper.Global;
            
            // Sequences 
            _mapper.Entity<SequenceValueWrapper>()
                .Id(m => m.Id)
                .Field(m => m.SequenceName, "SequenceName")
                .Field(m => m.Value, "Value");

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
            _liteDb = new LiteDatabase(_connString, GetMappers());

            return _liteDb;
        }

        private BsonMapper GetMappers()
        {
            return _mapper;
        }

        public Tuple<string, long> ObterSequence(string collectionName)
        {
            lock (this)
                using (var repositorio = GetLiteDbAccess())
                {
                    var tSeqRegistros = repositorio
                        .SingleOrDefault<SequenceValueWrapper>(item=> item.SequenceName == collectionName,
                            ColecaoSequences);
                    
                    if (tSeqRegistros == null)
                    {
                        tSeqRegistros = new SequenceValueWrapper(collectionName, 0);
                        repositorio.Insert(tSeqRegistros, ColecaoSequences);

                        repositorio.Database
                            .GetCollection<SequenceValueWrapper>(ColecaoSequences)
                            .EnsureIndex((doc) => doc.SequenceName, true);
                    }

                    return (Tuple<string,long>)tSeqRegistros;
                }
        }

        public Tuple<string, long> SincronizarSequence(long id, string collectionName)
        {
            lock (this)
                using (var repositorio = GetLiteDbAccess())
                {
                    var tSeqRegistros = repositorio
                        .Single<SequenceValueWrapper>(item => item.SequenceName == collectionName, ColecaoSequences);

                    tSeqRegistros.Value = id;
                    tSeqRegistros.SequenceName = collectionName;

                    repositorio.Update(tSeqRegistros, ColecaoSequences);

                    repositorio.Database.GetCollection<SequenceValueWrapper>(ColecaoSequences)
                        .EnsureIndex((doc) => doc.SequenceName, true);

                    return (Tuple<string, long>)tSeqRegistros;
                }
        }
    }
}