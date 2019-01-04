using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NStack;
using testProgrammers.CRUD.Core;
using Terminal.Gui;

namespace testeProgrammers.CRUD.IU.Views
{
    internal class ListagemRegistros: FrameView
    {
        private ListView _lvDisponiveis;
        private List<RegistroRenderizacao> _dataSource;
        private EventHandlerList _handlers;
        private static readonly object EventSelectedChanged = new object();

        public ListagemRegistros(Rect frame, ustring title) 
            : base(frame, title)
        {
            InicializarControles();
            _handlers = new EventHandlerList();
        }

        public ListagemRegistros(ustring title) 
            : base(title)
        {
            InicializarControles();
        }

        ~ListagemRegistros()
        {
            if (_handlers != null)
                _handlers.Dispose();
            _handlers = null;
        }

        private void InicializarControles()
        {
            _lvDisponiveis = new ListView();
            _lvDisponiveis.SelectedChanged+= LvDisponiveisOnSelectedChanged;
        }

        private int ObterIndiceLista(int registroId)
        {
            return _dataSource
                .First(registro => registro.Id == registroId)
                .SourceIndex;
        }

        private int ObterRegistroId(int indiceLista)
        {
            return _dataSource
                .First(registro => registro.SourceIndex == indiceLista)
                .Id;
        }

        private RegistroRenderizacao ObterRegistroRenderizado(int indiceLista)
        {
            return _dataSource
                .First(registro => registro.SourceIndex == indiceLista);
        }

        private void LvDisponiveisOnSelectedChanged()
        {
            var tmpIndex = _lvDisponiveis.SelectedItem;

            //_selecionado = ObterRegistroRenderizado(tmpIndex);

            var handler = _handlers[EventSelectedChanged] as Action;

            if (handler != null)
                handler();
        }

        public bool AllowsMarking
        {
            get => _lvDisponiveis.AllowsMarking;
            set => _lvDisponiveis.AllowsMarking = value;
        }

        public int TopItemId
        {
            get
            {
                var tmpIndex = _lvDisponiveis.TopItem;
                return ObterRegistroId(tmpIndex);
            }
            set
            {
                var tmpIndex = ObterIndiceLista(value);
                _lvDisponiveis.TopItem = tmpIndex;
            }
        }

        public int SelectedItemId
        {
            get
            {
                return ObterRegistroId(_lvDisponiveis.SelectedItem);
            }
            set
            {
                var tmpIndex = ObterIndiceLista(value);
                _lvDisponiveis.SelectedItem = tmpIndex;
            }
        }

        public Func<IList<Registro>> SourceList { get; set; }

        public Func<Registro, string[]> RenderTemplate { get; set; }

        public event Action SelectedChanged
        {
            add
            {
                _handlers.AddHandler(EventSelectedChanged, value);
            }
            remove
            {
                _handlers.AddHandler(EventSelectedChanged, value);
            }
        }

        private void SetSource(IList<Registro> lista)
        {
            this._dataSource = lista
                .Select((registro, index) => new RegistroRenderizacao(index, registro, RenderTemplate))
                .ToList();

            this._lvDisponiveis.SetSource(this._dataSource);
        }

        public void Atualizar()
        {
            var tmpSource = this.SourceList!=null? this.SourceList().ToList(): new List<Registro>();
            SetSource(tmpSource);
        }

        private class RegistroRenderizacao
        {
            private readonly int _sourceIndex;
            private readonly Registro _inner;
            private readonly Func<Registro, string[]> _tptRender;

            public RegistroRenderizacao(int index, Registro inner, Func<Registro, string[]> tptRender)
            {
                _sourceIndex = index;
                _inner = inner;
                _tptRender = tptRender;
            }

            public int Id
            {
                get
                {
                    return _inner.Id;
                }
            }

            public int SourceIndex
            {
                get { return _sourceIndex; }
            }

            public override string ToString()
            {
                return string.Join("|", _tptRender(this._inner));
            }
        }
    }
}