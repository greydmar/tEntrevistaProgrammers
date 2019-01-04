using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace testeProgrammers
{
    public class CadernoEnderecos : IEnumerable<Endereco>
    {
        private static int _id;
        private readonly Dictionary<Ponto, int> _mapa;
        private readonly List<Endereco> _caderno;

        public CadernoEnderecos()
        {
            _caderno = new List<Endereco>();
            _mapa = new Dictionary<Ponto, int>(Ponto.ComparadorSimples);
        }

        public static int NovoId()
        {
            return Interlocked.Increment(ref _id);
        }

        public void Set(Endereco endereco)
        {
            if (endereco==null)
                throw new ArgumentNullException(nameof(endereco));

            if (string.IsNullOrEmpty(endereco.NomePessoa) || string.IsNullOrWhiteSpace(endereco.NomePessoa))
                throw new ArgumentException("É necessário indicar o nome do amigo", nameof(endereco));

            if (endereco.IdPessoa <= 0 )
                throw new ArgumentException("É necessário inicializar o IdPessoa. Utilize {CadernoEnderecos.NovoId()}", nameof(endereco));

            if (endereco.Local.EhVazio)
                throw new ArgumentException("Localização não deve estar vazia", nameof(endereco));

            int indice;
            Endereco existente = null;

            if (_mapa.TryGetValue(endereco.Local, out indice)
                && (existente = Localizar(indice)).IdPessoa != endereco.IdPessoa)
                throw new InvalidOperationException("Localização já está atribuída a outro amigo. Tente remover antes de prosseguir");

            if (existente != null)
            {
                existente.NomePessoa = endereco.NomePessoa;
            }
            else
            {
                _caderno.Insert(_caderno.Count, endereco);
                _mapa.Add(endereco.Local, _caderno.Count-1);
            }
        }

        public Endereco Set(string nome, Ponto local)
        {
            var end = new Endereco()
            {
                IdPessoa = NovoId(), Local = local, NomePessoa = nome
            };

            Set(end);

            return end;
        }

        private Endereco Localizar(int indice)
        {
            return _caderno[indice];
        }

        public bool Remove(Endereco endereco)
        {
            //TODO: Implementar
            return false;
        }

        public int Count
        {
            get { return this._caderno.Count; }
        }

        public IEnumerable<Endereco> ListarMaisProximos(Endereco endereco, ushort nProximos = 3)
        {
            if (this.Count == 0)
                return new Endereco[0];

            if (this.Count < nProximos)
                return _caderno;

            var locais = CalcularDistancias(endereco.Local, nProximos);

            return _caderno.Where((end, index) => { return locais.Contains(index); }) ;
        }
        
        private IEnumerable<int> CalcularDistancias(Ponto ponto, int nItens)
        {
            var distancias = new Tuple<int, double>[this.Count-1];
            int[] ordenadas = new int[this.Count];

            int endBusca = _mapa[ponto];

            // Inicializa o vetor de distâncias
            for (int idx = 0; idx < this.Count; idx++)
            {
                Tuple<int, double> dPonto = null;

                dPonto = idx == endBusca
                    ? new Tuple<int, double>(endBusca, 0)
                    : new Tuple<int, double>(idx, Ponto.Distancia(_caderno[idx].Local, ponto));

                if (idx == endBusca)
                    continue;

                distancias[idx] = dPonto;
            }

            // inclui, ordenadamente, no vetor de distâncias
            return distancias
                .OrderByDescending(t => t.Item2)
                .Take(nItens)
                .Select(t => t.Item1);
        }


        public IEnumerator<Endereco> GetEnumerator()
        {
            return _caderno.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}