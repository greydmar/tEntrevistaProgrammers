using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using testProgrammers.CRUD.Core;
using Terminal.Gui;

namespace testeProgrammers.CRUD.IU.Views
{
    public class JanelaPrincipal: Terminal.Gui.Window
    {
        private readonly Controlador _controlador;
        private MenuBar _menuBar;
        private ListagemRegistros _listagem;
        private int _itemSelecionado;

        private JanelaPrincipal(Controlador controlador, Toplevel top)
            :base(new Rect(0,1, top.Frame.Width, top.Frame.Height-1), "Teste Programmers")
        {
            _controlador = controlador;
            top.Add(this);

            InicializarControles(top);
        }

        private void InicializarControles(Toplevel top)
        {
            _menuBar = new MenuBar(new MenuBarItem[]
            {
                new MenuBarItem("_Registros", new MenuItem[]
                {
                    new MenuItem("_Novo", "Novo registro", () => ExceptionSafeAction(CriarNovoRegistro)),
                    new MenuItem("_Detalhar", "Exibir campos ", () => ExceptionSafeAction(ExibirDetalhado)),
                    new MenuItem("_Atualizar", "Atualizar", () => ExceptionSafeAction(Atualizar)),
                    new MenuItem("_Remover", "Remover selecionado", () => ExceptionSafeAction(RemoverSelecionado)),
                }),
                new MenuBarItem("_Sair", new MenuItem[]
                {
                    new MenuItem("_Sair", "", () =>
                    {
                        if (ConfirmarSaida()) Application.RequestStop();
                    }),
                })
            });
            
            this.Add(_menuBar);
            _listagem = InicializarListagem();
            this.Add(_listagem);

            Atualizar();
            Application.Refresh();
        }

        private void ExceptionSafeAction(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                var msg = string.Concat("Operação falhou. ", "\n", ex.Message.Replace("\r\n","\n"));

                MessageBox.ErrorQuery(80, 08, "Falha", msg, new string[] {"OK"});
            }
        }

        private ListagemRegistros InicializarListagem()
        {
            var bounds = this.Bounds;
            var frame = new Rect(1, 2, bounds.Width-4, (bounds.Height - this._menuBar.Bounds.Height)-4);

            var lv = new ListagemRegistros(frame, " Registros disponíveis")
            {
                AllowsMarking = true,
                SourceList = ListarRegistrados,
                ConfigExibicao = new ListagemRegistros.ConfigExibicaoLista()
                {
                    Cabecalhos = new[]
                    {
                        new {Nome = "ID", Tamanho = 04, Alinhamento = TextAlignment.Left},
                        new {Nome = "NOME", Tamanho = 30, Alinhamento = TextAlignment.Left},
                        new {Nome = "TELEFONE", Tamanho = 20, Alinhamento = TextAlignment.Left},
                        new {Nome = "E-MAIL", Tamanho = 40, Alinhamento = TextAlignment.Left}
                    },
                    UsarElipsesTextosLongos = true,
                    Separador = "||",
                    RenderizacaoItem = (registro) =>
                    {
                        return new string[] {registro.Id.ToString("D3"), registro.Nome, registro.Telefone, registro.Email.Address};
                    }
                },
                CanFocus = true,
            };
            lv.SelectedChanged += SelectedChanged;

            return lv;
        }

        private void SelectedChanged()
        {
            _itemSelecionado = _listagem.SelectedItemId;
        }

        private IList<Registro> ListarRegistrados()
        {
            return _controlador.ListarTodos();
        }

        #region Ações Menu

        private bool HaItemItemSelecionado(bool exibirMensagem=true)
        {
            var result = (_itemSelecionado >= 0);

            if (!result && exibirMensagem)
            {
                MessageBox.Query(50, 7, "Aviso", "Selecione um item antes de prosseguir", "OK");
            }

            return result;
        }

        private void ExibirDetalhado()
        {
            if (!HaItemItemSelecionado())
                return;

            var registro = _controlador.LocalizarPorId(this._itemSelecionado);

            var resultado= EditorRegistro.Exibir(ModoEdicao.Gravacao, registro, "Detalhamento", 70, 20);

            if (resultado.Opcao == ModalResult.Confirmar)
            {
                _controlador.GravarRegistro(resultado.Registro);
            }

            Atualizar();
        }


        private void RemoverSelecionado()
        {
            if (!HaItemItemSelecionado())
                return;

            var result = MessageBox.Query(50, 7, "Confirmar", "Excluir Registro?", "Sim", "Não");
            if (result == 1)
                return;

            _controlador.RemoverPorId(this._itemSelecionado);

            this.Atualizar();
        }

        private void Atualizar()
        {
            this._itemSelecionado = -1;
            this._listagem.Atualizar();

            this.SetNeedsDisplay();
            //this.Redraw(this.Bounds);
        }

        private void CriarNovoRegistro()
        {
            var transiente = _controlador.CriarRegistro();

            var resultado = EditorRegistro.Exibir(ModoEdicao.Gravacao, transiente, "Novo Registro", 70, 20);

            if (resultado.Opcao == ModalResult.Confirmar)
            {
                _controlador.GravarRegistro(resultado.Registro);
            }

            Atualizar();
        }

        private bool ConfirmarSaida()
        {
            var result = MessageBox.Query(50, 7, "Confirmar", "Deseja sair?", "Sim", "Não");
            return result == 0;
        }

        #endregion

        public static JanelaPrincipal Criar(Controlador controlador, Toplevel top)
        {
            return new JanelaPrincipal(controlador, top);
        }
    }
}
