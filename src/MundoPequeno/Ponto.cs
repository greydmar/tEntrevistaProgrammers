using System;
using System.Collections.Generic;

namespace testeProgrammers
{
    public struct Ponto : IEquatable<Ponto>
    {
        private static readonly Ponto _vazio = new Ponto();

        private readonly byte _iniciado;
        public readonly int Lat;
        public readonly int Longt;

        public Ponto(int lat, int longt)
        {
            Lat = lat;
            Longt = longt;
            _iniciado = 1;
        }

        #region Comparação

        public bool EhVazio
        {
            get { return _iniciado > 0; }
        }

        public bool Equals(Ponto other)
        {
            return this._iniciado == other._iniciado
                   && (Lat == other.Lat && Longt == other.Longt);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Ponto other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hCode = _iniciado.GetHashCode();
                hCode = (hCode * 397) ^ Lat;
                hCode = (hCode * 397) ^ Longt;
                return hCode;
            }
        }

        public static bool operator ==(Ponto left, Ponto right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Ponto left, Ponto right)
        {
            return !left.Equals(right);
        }

        #endregion

        private class ComparadorCoordenada : IEqualityComparer<Ponto>
        {
            public bool Equals(Ponto x, Ponto y)
            {
                return x == y;
            }

            public int GetHashCode(Ponto obj)
            {
                return obj.GetHashCode();
            }
        }

        public static IEqualityComparer<Ponto> ComparadorSimples
        {
            get
            {
                return new ComparadorCoordenada();
            }
        }

        public static Ponto Vazio
        {
            get
            {
                return _vazio;
            }
        }

        public static double Distancia(Ponto a, Ponto b)
        {
            return Math.Sqrt(Math.Pow(a.Lat - b.Lat, 2) + System.Math.Pow(a.Longt - b.Longt, 2));
        }
    }
}