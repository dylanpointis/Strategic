<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ModificarUsuario.aspx.cs" Inherits="Strategic.ModificarUsuario" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <main class="page-section">

        <div class="page-heading">
            <h1><%: Traductor.T("modificarUsuario.titulo") %></h1>
            <p><%: Traductor.T("modificarUsuario.subtitulo") %></p>
        </div>

        <asp:Panel ID="pnlFormulario" runat="server" CssClass="strategic-card">
            <div class="strategic-card-header">
                <h2><%: Traductor.T("usuario.datos") %></h2>
                <asp:Label ID="lblEstadoActual" runat="server" CssClass="strategic-card-meta"></asp:Label>
            </div>

            <div class="strategic-card-body">
                <div class="formulario-grid">
                    <div class="campo">
                        <asp:Label ID="lblNombreUsuario" runat="server" CssClass="form-label"></asp:Label>
                        <div class="campo-fijo"><asp:Label ID="lblUsuarioEditado" runat="server"></asp:Label></div>
                        <p class="campo-ayuda"><%: Traductor.T("modificarUsuario.ayudaNombreUsuario") %></p>
                    </div>

                    <div class="campo">
                        <asp:Label ID="lblRol" runat="server" AssociatedControlID="ddlRol" CssClass="form-label"></asp:Label>
                        <asp:DropDownList ID="ddlRol" runat="server" CssClass="form-select"></asp:DropDownList>
                        <asp:RequiredFieldValidator ID="valRol" runat="server" ControlToValidate="ddlRol" InitialValue="" Display="Dynamic" CssClass="campo-error" ValidationGroup="Modificar"></asp:RequiredFieldValidator>
                    </div>

                    <div class="campo">
                        <asp:Label ID="lblNombre" runat="server" AssociatedControlID="txtNombre" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control" MaxLength="50"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="valNombre" runat="server" ControlToValidate="txtNombre" Display="Dynamic" CssClass="campo-error" ValidationGroup="Modificar"></asp:RequiredFieldValidator>
                    </div>

                    <div class="campo">
                        <asp:Label ID="lblApellido" runat="server" AssociatedControlID="txtApellido" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtApellido" runat="server" CssClass="form-control" MaxLength="50"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="valApellido" runat="server" ControlToValidate="txtApellido" Display="Dynamic" CssClass="campo-error" ValidationGroup="Modificar"></asp:RequiredFieldValidator>
                    </div>

                    <div class="campo">
                        <asp:Label ID="lblEmail" runat="server" AssociatedControlID="txtEmail" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" CssClass="form-control" MaxLength="100"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="valEmail" runat="server" ControlToValidate="txtEmail" Display="Dynamic" CssClass="campo-error" ValidationGroup="Modificar"></asp:RequiredFieldValidator>
                        <asp:RegularExpressionValidator ID="valFormatoEmail" runat="server" ControlToValidate="txtEmail"
                            ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]{2,}$"
                            Display="Dynamic" CssClass="campo-error" ValidationGroup="Modificar"></asp:RegularExpressionValidator>
                    </div>

                    <div class="campo campo-check">
                        <asp:CheckBox ID="chkBloqueado" runat="server" />
                    </div>
                </div>

                <div class="formulario-acciones">
                    <asp:Button ID="btnGuardar" runat="server" CssClass="btn btn-strategic" ValidationGroup="Modificar" OnClick="btnGuardar_Click" />
                    <asp:Button ID="btnCancelar" runat="server" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnCancelar_Click" />
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
                    <asp:Button ID="btnVolver" runat="server" CssClass="btn btn-strategic" CausesValidation="false" OnClick="btnCancelar_Click" />
                </div>
            </div>
        </asp:Panel>

    </main>
</asp:Content>
