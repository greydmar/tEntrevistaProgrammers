using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
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
                    new MenuItem("_Novo", "Novo registro", CriarNovoRegistro),
                    new MenuItem("_Detalhar", "Exibir campos ", ExibirDetalhado),
                    new MenuItem("_Atualizar", "Atualizar", Atualizar),
                    new MenuItem("_Remover", "Remover selecionado", RemoverSelecionado),

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
        }

        private ListagemRegistros InicializarListagem()
        {
            var frame = new Rect();
            var lv = new ListagemRegistros(frame, " Registros disponíveis")
            {
                AllowsMarking = true,
                SourceList = ListarRegistrados,
                RenderTemplate = (registro) =>
                {
                    return new string[]
                    {
                        registro.Id.ToString("D3"), registro.Nome, registro.Telefone
                    };
                },
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

            var item = _controlador.LocalizarPorId(this._itemSelecionado);

            EditorRegistro.Exibir(ModoEdicao.Leitura, item, "Detalhamento", 70, 100);

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
        }

        private void CriarNovoRegistro()
        {
            var transiente = _controlador.CriarRegistro();

            var resultado = EditorRegistro.Exibir(ModoEdicao.Gravacao, transiente, "Novo Registro", 70, 20);

            if (resultado.Opcao == ModalResult.Confirmar)
            {
                _controlador.GravarRegistro(transiente);
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
