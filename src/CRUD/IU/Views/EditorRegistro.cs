using System.Net.Mail;
using NStack;
using testProgrammers.CRUD.Core;
using Terminal.Gui;

namespace testeProgrammers.CRUD.IU.Views
{
    internal class EditorRegistro: Dialog
    {
        internal const string CmdCancelar = "Cancelar";
        internal const string CmdGravar = "Gravar";

        private ustring _cmdEncerramento;
        private Registro _original;
        private Registro _atual;
        private bool _modificado;
        private ModoEdicao _modo;

        private TextField _txtNome;
        private TextField _txtTelefone;
        private TextField _txtEmail;

        private EditorRegistro(ustring title, int width, int height, params Button[] buttons) 
            : base(title, width, height, buttons)
        {
            _original = null;
            _atual = null;
            _modo = ModoEdicao.Leitura;
            _modificado = false;

            InicializarControles();

            foreach (var button in buttons)
            {
                var id = button.Id;
                button.Clicked = () =>
                {
                    _cmdEncerramento = id;
                    ProcessarComando();
                };
            }
        }

        private void ProcessarComando()
        {
            if (_modo == ModoEdicao.Gravacao && _cmdEncerramento == CmdGravar)
            {
                _atual = SincronizarModificacoes();
            }

            this.Running = false;
        }

        private void InicializarControles()
        {
            int xLeft = 2;

            int yTop = 2;

            this.Add(new Label(xLeft, yTop, "Nome: "));
            _txtNome = new TextField(12, yTop, 40, ustring.Empty) {Id = "txtNome"};
            this.Add(_txtNome);

            this.Add(new Label(xLeft, yTop+2, "Telefone: "));
            _txtTelefone = new TextField(12, yTop + 2, 40, ustring.Empty) { Id = "txtTelefone" };
            this.Add(_txtTelefone);

            this.Add(new Label(xLeft, yTop + 4, "E-mail: "));
            _txtEmail = new TextField(12, yTop + 4, 40, ustring.Empty) { Id = "txtEmail" };
            this.Add(_txtEmail);
        }

        public static ResultadoDialogo Exibir(ModoEdicao modo, Registro registro, string titulo, int width, int height)
        {
            var botoes = modo == ModoEdicao.Gravacao
                ? new Button[]
                {
                    new Button("Gravar", false) {Id = CmdGravar},
                    new Button("Cancelar", true) {Id = CmdCancelar},
                }
                : new Button[]
                {
                    new Button("Fechar", true) {Id = CmdCancelar}
                };

            var janela = new EditorRegistro(ustring.Make(titulo), width, height, botoes);
            janela.Renderizar(modo, registro);
            janela._txtNome.FocusFirst();

            Application.Run(janela);

            return janela.GerarResultado();
        }

        private ResultadoDialogo GerarResultado()
        {
            var registro = SincronizarModificacoes();

            return new ResultadoDialogo()
            {
                Opcao = _cmdEncerramento == CmdGravar ? ModalResult.Confirmar : ModalResult.Descartar,
                Modificado = _modificado,
                Registro = registro
            };
        }

        private void Renderizar(ModoEdicao modo, Registro registro)
        {
            this._modo = modo;
            this._modificado = false;
            this._original = registro;

            this._txtEmail.Text = registro.Email?.Address;
            this._txtNome.Text = registro.Nome ?? string.Empty;
            this._txtTelefone.Text = registro.Telefone ?? string.Empty;
        }

        private Registro SincronizarModificacoes()
        {
            if (_modo == ModoEdicao.Leitura)
                return _original;

            var local = new Registro()
            {
                Id = _original.Id
            };

            int cCount = 0;

            if (HaDiferencas(this._txtEmail.Text, this._original.Email.Address))
            {
                local.Email = new MailAddress(this._txtEmail.Text.ToString());
                cCount++;
            }
            if (HaDiferencas(this._txtNome.Text, this._original.Nome))
            {
                local.Nome = this._txtNome.Text.ToString();
                cCount++;
            }
            if (HaDiferencas(this._txtTelefone.Text, this._original.Telefone))
            {
                local.Telefone = this._txtTelefone.Text.ToString();
                cCount++;
            }

            _modificado = cCount > 0;

            if (cCount == 0)
                return _original;

            return local;
        }

        private static bool HaDiferencas(ustring textValue, string fieldValue)
        {
            if (textValue.IsEmpty && string.IsNullOrEmpty(fieldValue))
                return false;

            if (!textValue.IsEmpty && string.IsNullOrEmpty(fieldValue))
                return true;

            return !textValue.Equals(fieldValue);
        }
    }
}