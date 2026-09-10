<%@ Page Title="Modificar usuario" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ModificarUsuario.aspx.cs" Inherits="Strategic.ModificarUsuario" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <main class="page-section">

        <div class="page-heading">
            <h1>Modificar usuario</h1>
            <p>Actualización de los datos de un usuario del sistema.</p>
        </div>

        <asp:Panel ID="pnlFormulario" runat="server" CssClass="strategic-card">
            <div class="strategic-card-header">
                <h2>Datos del usuario</h2>
                <asp:Label ID="lblEstadoActual" runat="server" CssClass="strategic-card-meta"></asp:Label>
            </div>

            <div class="strategic-card-body">
                <div class="formulario-grid">
                    <div class="campo">
                        <asp:Label ID="lblNombreUsuario" runat="server" Text="Nombre de usuario" CssClass="form-label"></asp:Label>
                        <div class="campo-fijo"><asp:Label ID="lblUsuarioEditado" runat="server"></asp:Label></div>
                        <p class="campo-ayuda">Identifica al usuario en la bitácora de eventos, no se puede cambiar.</p>
                    </div>

                    <div class="campo">
                        <asp:Label ID="lblRol" runat="server" Text="Rol" AssociatedControlID="ddlRol" CssClass="form-label"></asp:Label>
                        <asp:DropDownList ID="ddlRol" runat="server" CssClass="form-select"></asp:DropDownList>
                        <asp:RequiredFieldValidator ID="valRol" runat="server" ControlToValidate="ddlRol" InitialValue=""
                            ErrorMessage="Elegí un rol" Display="Dynamic" CssClass="campo-error" ValidationGroup="Modificar"></asp:RequiredFieldValidator>
                    </div>

                    <div class="campo">
                        <asp:Label ID="lblNombre" runat="server" Text="Nombre" AssociatedControlID="txtNombre" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control" MaxLength="50"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="valNombre" runat="server" ControlToValidate="txtNombre"
                            ErrorMessage="Ingresá el nombre" Display="Dynamic" CssClass="campo-error" ValidationGroup="Modificar"></asp:RequiredFieldValidator>
                    </div>

                    <div class="campo">
                        <asp:Label ID="lblApellido" runat="server" Text="Apellido" AssociatedControlID="txtApellido" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtApellido" runat="server" CssClass="form-control" MaxLength="50"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="valApellido" runat="server" ControlToValidate="txtApellido"
                            ErrorMessage="Ingresá el apellido" Display="Dynamic" CssClass="campo-error" ValidationGroup="Modificar"></asp:RequiredFieldValidator>
                    </div>

                    <div class="campo">
                        <asp:Label ID="lblEmail" runat="server" Text="Email" AssociatedControlID="txtEmail" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" CssClass="form-control" MaxLength="100"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="valEmail" runat="server" ControlToValidate="txtEmail"
                            ErrorMessage="Ingresá el email" Display="Dynamic" CssClass="campo-error" ValidationGroup="Modificar"></asp:RequiredFieldValidator>
                        <asp:RegularExpressionValidator ID="valFormatoEmail" runat="server" ControlToValidate="txtEmail"
                            ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]{2,}$" ErrorMessage="El email no tiene un formato válido"
                            Display="Dynamic" CssClass="campo-error" ValidationGroup="Modificar"></asp:RegularExpressionValidator>
                    </div>

                    <div class="campo campo-check">
                        <asp:CheckBox ID="chkBloqueado" runat="server" Text="Usuario bloqueado" />
                    </div>
                </div>

                <div class="formulario-acciones">
                    <asp:Button ID="btnGuardar" runat="server" Text="Guardar cambios" CssClass="btn btn-strategic" ValidationGroup="Modificar" OnClick="btnGuardar_Click" />
                    <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnCancelar_Click" />
                </div>

                <asp:Label ID="lblError" runat="server" CssClass="mensaje-error" EnableViewState="false"></asp:Label>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlSinUsuario" runat="server" CssClass="strategic-card" Visible="false">
            <div class="strategic-card-body">
                <div class="estado-vacio">
                    <span class="estado-vacio-icono">
                        <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false">
                            <circle cx="12" cy="8" r="4"></circle>
                            <path d="M4 21c0-4 3.5-7 8-7"></path>
                            <path d="M16 16l5 5"></path>
                            <path d="M21 16l-5 5"></path>
                        </svg>
                    </span>
                    <p><asp:Label ID="lblSinUsuario" runat="server"></asp:Label></p>
                </div>

                <div class="formulario-acciones">
                    <asp:Button ID="btnVolver" runat="server" Text="Volver al listado" CssClass="btn btn-strategic" CausesValidation="false" OnClick="btnCancelar_Click" />
                </div>
            </div>
        </asp:Panel>

    </main>
</asp:Content>
