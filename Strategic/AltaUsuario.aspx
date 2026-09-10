<%@ Page Title="Nuevo usuario" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AltaUsuario.aspx.cs" Inherits="Strategic.AltaUsuario" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <main class="page-section">

        <div class="page-heading">
            <h1>Nuevo usuario</h1>
            <p>Registro de un usuario del sistema.</p>
        </div>

        <asp:Panel ID="pnlFormulario" runat="server" CssClass="strategic-card">
            <div class="strategic-card-header">
                <h2>Datos del usuario</h2>
            </div>

            <div class="strategic-card-body">
                <div class="formulario-grid">
                    <div class="campo">
                        <asp:Label ID="lblNombreUsuario" runat="server" Text="Nombre de usuario" AssociatedControlID="txtNombreUsuario" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtNombreUsuario" runat="server" CssClass="form-control" MaxLength="50"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="valNombreUsuario" runat="server" ControlToValidate="txtNombreUsuario"
                            ErrorMessage="Ingresá el nombre de usuario" Display="Dynamic" CssClass="campo-error" ValidationGroup="Alta"></asp:RequiredFieldValidator>
                        <asp:RegularExpressionValidator ID="valFormatoUsuario" runat="server" ControlToValidate="txtNombreUsuario"
                            ValidationExpression="^[A-Za-z0-9._-]+$" ErrorMessage="Solo letras, números, punto, guion y guion bajo"
                            Display="Dynamic" CssClass="campo-error" ValidationGroup="Alta"></asp:RegularExpressionValidator>
                    </div>

                    <div class="campo">
                        <asp:Label ID="lblRol" runat="server" Text="Rol" AssociatedControlID="ddlRol" CssClass="form-label"></asp:Label>
                        <asp:DropDownList ID="ddlRol" runat="server" CssClass="form-select"></asp:DropDownList>
                        <asp:RequiredFieldValidator ID="valRol" runat="server" ControlToValidate="ddlRol" InitialValue=""
                            ErrorMessage="Elegí un rol" Display="Dynamic" CssClass="campo-error" ValidationGroup="Alta"></asp:RequiredFieldValidator>
                    </div>

                    <div class="campo">
                        <asp:Label ID="lblNombre" runat="server" Text="Nombre" AssociatedControlID="txtNombre" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control" MaxLength="50"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="valNombre" runat="server" ControlToValidate="txtNombre"
                            ErrorMessage="Ingresá el nombre" Display="Dynamic" CssClass="campo-error" ValidationGroup="Alta"></asp:RequiredFieldValidator>
                    </div>

                    <div class="campo">
                        <asp:Label ID="lblApellido" runat="server" Text="Apellido" AssociatedControlID="txtApellido" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtApellido" runat="server" CssClass="form-control" MaxLength="50"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="valApellido" runat="server" ControlToValidate="txtApellido"
                            ErrorMessage="Ingresá el apellido" Display="Dynamic" CssClass="campo-error" ValidationGroup="Alta"></asp:RequiredFieldValidator>
                    </div>

                    <div class="campo campo-ancho">
                        <asp:Label ID="lblEmail" runat="server" Text="Email" AssociatedControlID="txtEmail" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" CssClass="form-control" MaxLength="100"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="valEmail" runat="server" ControlToValidate="txtEmail"
                            ErrorMessage="Ingresá el email" Display="Dynamic" CssClass="campo-error" ValidationGroup="Alta"></asp:RequiredFieldValidator>
                        <asp:RegularExpressionValidator ID="valFormatoEmail" runat="server" ControlToValidate="txtEmail"
                            ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]{2,}$" ErrorMessage="El email no tiene un formato válido"
                            Display="Dynamic" CssClass="campo-error" ValidationGroup="Alta"></asp:RegularExpressionValidator>
                        <p class="campo-ayuda">
                            La contraseña inicial se genera sola con el formato <strong>nombre.apellido</strong> y se muestra al crear el usuario.
                        </p>
                    </div>
                </div>

                <div class="formulario-acciones">
                    <asp:Button ID="btnCrear" runat="server" Text="Crear usuario" CssClass="btn btn-strategic" ValidationGroup="Alta" OnClick="btnCrear_Click" />
                    <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnCancelar_Click" />
                </div>

                <asp:Label ID="lblError" runat="server" CssClass="mensaje-error" EnableViewState="false"></asp:Label>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlCreado" runat="server" CssClass="strategic-card" Visible="false">
            <div class="strategic-card-header">
                <h2>Usuario creado</h2>
            </div>

            <div class="strategic-card-body">
                <dl class="detalle-grid">
                    <div class="detalle-item">
                        <dt>Usuario</dt>
                        <dd><asp:Label ID="lblCreadoUsuario" runat="server"></asp:Label></dd>
                    </div>

                    <div class="detalle-item">
                        <dt>Nombre y apellido</dt>
                        <dd><asp:Label ID="lblCreadoNombre" runat="server"></asp:Label></dd>
                    </div>

                    <div class="detalle-item">
                        <dt>Rol</dt>
                        <dd><asp:Label ID="lblCreadoRol" runat="server"></asp:Label></dd>
                    </div>

                    <div class="detalle-item">
                        <dt>Contraseña inicial</dt>
                        <dd>
                            <span class="clave-generada"><asp:Label ID="lblCreadoClave" runat="server"></asp:Label></span>
                            <p class="campo-ayuda">Anotala y entregala al usuario: no se vuelve a mostrar.</p>
                        </dd>
                    </div>
                </dl>

                <div class="formulario-acciones">
                    <asp:Button ID="btnOtro" runat="server" Text="Crear otro usuario" CssClass="btn btn-strategic" CausesValidation="false" OnClick="btnOtro_Click" />
                    <asp:Button ID="btnVolver" runat="server" Text="Volver al listado" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnVolver_Click" />
                </div>
            </div>
        </asp:Panel>

    </main>
</asp:Content>
