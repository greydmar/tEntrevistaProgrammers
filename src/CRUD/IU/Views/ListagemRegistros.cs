using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using NStack;
using testProgrammers.CRUD.Core;
using Terminal.Gui;

namespace testeProgrammers.CRUD.IU.Views
{
    internal class ListagemRegistros: FrameView
    {
        private ListView _lvDisponiveis;
        private List<RenderizadorRegistro> _dataSource;
        private EventHandlerList _handlers;
        private ConfigExibicaoLista _configExibicao;
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
            var contentView = this.Subviews.First();

            _lvDisponiveis = new ListView(contentView.Bounds, new List<RenderizadorRegistro>());
            _lvDisponiveis.SelectedChanged+= LvDisponiveisOnSelectedChanged;

            this.Add(_lvDisponiveis);
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

        private RenderizadorRegistro ObterRegistroRenderizado(int indiceLista)
        {
            return _dataSource
                .First(registro => registro.SourceIndex == indiceLista);
        }

        private void LvDisponiveisOnSelectedChanged()
        {
            var tmpIndex = _lvDisponiveis.SelectedItem;
            
            // Cabeçalho ou rodapé? 
            if (tmpIndex == 0 || tmpIndex == _lvDisponiveis.Source.Count - 1)
                return;

            if (tmpIndex == 1)
            {
                var item = ObterRegistroRenderizado(tmpIndex);
                if (item.TipoRegistro == TipoRegistro.Vazio)
                    return;
            }

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

        public ConfigExibicaoLista ConfigExibicao
        {
            get => _configExibicao;
            set
            {
                _configExibicao = value;
                if (value != null)
                {
                    SetSource(new List<Registro>());
                }
            }
        }

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

        private IList GetEmptyDataList()
        {
            return NormalizeDataList(new List<RenderizadorRegistro>());
        }

        private List<RenderizadorRegistro> NormalizeDataList(List<RenderizadorRegistro> dataList)
        {
            var count = dataList.Count;
            var config = RenderizadorConfig.GerarCabecalho(this.ConfigExibicao);
            dataList.Insert(0, new RenderizadorRegistro(0, null, config)
            {
                TipoRegistro = TipoRegistro.Cabecalho
            });

            if (count == 0)
            {
                config = RenderizadorConfig.GerarItemVazio(this.ConfigExibicao);
                dataList.Add(new RenderizadorRegistro(1, null, config)
                {
                    TipoRegistro = TipoRegistro.Vazio
                });
            }

            // 
            config = RenderizadorConfig.GerarRodape(this.ConfigExibicao);
            dataList.Add(new RenderizadorRegistro(dataList.Count, null, config) { TipoRegistro = TipoRegistro.Rodape });

            return dataList;
        }

        private void SetSource(IList<Registro> lista)
        {
            var config = RenderizadorConfig.GerarItem(this.ConfigExibicao);

            var dataList = lista
                .Select((registro, index) => new RenderizadorRegistro(index+1, registro, config)
                {
                    TipoRegistro = TipoRegistro.Dados
                }).ToList();

            this._dataSource = NormalizeDataList(dataList);

            this._lvDisponiveis.SetSource(this._dataSource);
        }

        public void Atualizar()
        {
            var tmpSource = this.SourceList!=null? this.SourceList().ToList(): new List<Registro>();
            SetSource(tmpSource);

            this.SetNeedsDisplay();
        }


        private enum TipoRegistro
        {
            Cabecalho, Dados, Vazio, Rodape
        }

        private class RenderizadorConfig
        {
            public string Separador = "||";

            public int[] TamanhoColunas;

            public TextAlignment[] Alinhamentos;

            public Func<Registro, string[]> LeitorCamposFn;
            public string ElipseTextosLongos { get; set; }

            private static T[] Ler<T>(object[] lista, string nomeCampo)
            {
                var pInfo = lista.GetType().GetElementType()
                    .GetProperty(nomeCampo, BindingFlags.Instance | BindingFlags.Public);

                return lista.Select(item => (T) pInfo.GetValue(item, null)).ToArray();
            }

            public static RenderizadorConfig GerarCabecalho(ConfigExibicaoLista configExibicao)
            {
                var rotulosCampos = Ler<string>(configExibicao.Cabecalhos, "Nome");
                return new RenderizadorConfig()
                {
                    ElipseTextosLongos = configExibicao.UsarElipsesTextosLongos ? "..." : string.Empty,
                    Separador = configExibicao.Separador,
                    TamanhoColunas = Ler<int>(configExibicao.Cabecalhos, "Tamanho"),
                    Alinhamentos = Ler<TextAlignment>(configExibicao.Cabecalhos, "Alinhamento"),
                    LeitorCamposFn = registro => { return rotulosCampos; }
                };
            }

            public static RenderizadorConfig GerarItemVazio(ConfigExibicaoLista configExibicao)
            {
                var tamanhos = Ler<int>(configExibicao.Cabecalhos, "Tamanho");
                return new RenderizadorConfig()
                {
                    ElipseTextosLongos = configExibicao.UsarElipsesTextosLongos ? "..." : string.Empty,
                    Separador = configExibicao.Separador,
                    TamanhoColunas = new int[]{ tamanhos.Sum() },
                    Alinhamentos = new TextAlignment[]{TextAlignment.Centered},
                    LeitorCamposFn = registro =>
                    {
                        return new string[]{ "NENHUM ITEM PARA EXIBIÇÃO" };
                    }
                };
            }

            public static RenderizadorConfig GerarRodape(ConfigExibicaoLista configExibicao)
            {
                return GerarCabecalho(configExibicao);
            }

            public static RenderizadorConfig GerarItem(ConfigExibicaoLista configExibicao)
            {
                return new RenderizadorConfig()
                {
                    ElipseTextosLongos = configExibicao.UsarElipsesTextosLongos ? "..." : string.Empty,
                    Separador = configExibicao.Separador,
                    TamanhoColunas = Ler<int>(configExibicao.Cabecalhos, "Tamanho"),
                    Alinhamentos = Ler<TextAlignment>(configExibicao.Cabecalhos, "Alinhamento"),
                    LeitorCamposFn = configExibicao.RenderizacaoItem
                };
            }
        }

        private class RenderizadorRegistro
        {
            private readonly int _sourceIndex;

            private readonly Registro _inner;
            private readonly RenderizadorConfig _config;

            public RenderizadorRegistro(int index, Registro inner, RenderizadorConfig config)
            {
                _sourceIndex = index;
                _inner = inner;
                _config = config;
            }

            public int Id
            {
                get
                {
                    return _inner.Id;
                }
            }

            public TipoRegistro TipoRegistro { get; set; }

            public int SourceIndex
            {
                get { return _sourceIndex; }
            }

            public override string ToString()
            {
                var valores = _config.LeitorCamposFn(this._inner);

                valores = AjustarTamanhos(valores);
                valores = AjustarAlinhamento(valores);

                return string.Join(_config.Separador, valores);
            }

            private string[] AjustarTamanhos(string[] valores)
            {
                var substituicao = _config.ElipseTextosLongos;

                for (int i = 0; i < valores.Length; i++)
                {
                    string valor = valores[i];
                    var tamanho = _config.TamanhoColunas[i];
                    
                    if (valor.Length <= tamanho)
                        continue;

                    valores[i] = string.Concat(valor.Substring(0, tamanho - substituicao.Length - 1), substituicao);
                }

                return valores;
            }

            private string[] AjustarAlinhamento(string[] valores)
            {
                for (int i = 0; i < valores.Length; i++)
                {
                    string valor = valores[i];
                    var alinhamento = _config.Alinhamentos[i];
                    var tamanho = _config.TamanhoColunas[i];

                    if (valor.Length == tamanho)
                        continue;

                    if (alinhamento == TextAlignment.Left)
                    {   
                        valor = valor.PadRight(tamanho);
                    }
                    else if (alinhamento == TextAlignment.Right)
                    {
                        valor = valor.PadLeft(tamanho);
                    }else if (alinhamento == TextAlignment.Centered)
                    {
                        int delta = tamanho / 2;
                        int resto = tamanho % 2;
                        valor = valor.PadLeft(delta - resto)
                            .PadRight(delta);
                    }
                    else
                    {
                        // Não implementado
                    }
                    valores[i] = valor;
                }
                return valores;
            }
        }

        public sealed class ConfigExibicaoLista
        {
            public object[] Cabecalhos { get; set; }
            public bool UsarElipsesTextosLongos { get; set; }
            public string Separador { get; set; }
            public Func<Registro, string[]> RenderizacaoItem { get; set; }
        }
    }
}