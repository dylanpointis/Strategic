<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AltaUsuario.aspx.cs" Inherits="Strategic.AltaUsuario" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <main class="page-section">

        <div class="page-heading">
            <h1><%: Traductor.T("altaUsuario.titulo") %></h1>
            <p><%: Traductor.T("altaUsuario.subtitulo") %></p>
        </div>

        <asp:Panel ID="pnlFormulario" runat="server" CssClass="strategic-card">
            <div class="strategic-card-header">
                <h2><%: Traductor.T("usuario.datos") %></h2>
            </div>

            <div class="strategic-card-body">
                <div class="formulario-grid">
                    <div class="campo">
                        <asp:Label ID="lblNombreUsuario" runat="server" AssociatedControlID="txtNombreUsuario" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtNombreUsuario" runat="server" CssClass="form-control" MaxLength="50"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="valNombreUsuario" runat="server" ControlToValidate="txtNombreUsuario" Display="Dynamic" CssClass="campo-error" ValidationGroup="Alta"></asp:RequiredFieldValidator>
                        <asp:RegularExpressionValidator ID="valFormatoUsuario" runat="server" ControlToValidate="txtNombreUsuario"
                            ValidationExpression="^[A-Za-z0-9._-]+$"
                            Display="Dynamic" CssClass="campo-error" ValidationGroup="Alta"></asp:RegularExpressionValidator>
                    </div>

                    <div class="campo">
                        <asp:Label ID="lblRol" runat="server" AssociatedControlID="ddlRol" CssClass="form-label"></asp:Label>
                        <asp:DropDownList ID="ddlRol" runat="server" CssClass="form-select"></asp:DropDownList>
                        <asp:RequiredFieldValidator ID="valRol" runat="server" ControlToValidate="ddlRol" InitialValue="" Display="Dynamic" CssClass="campo-error" ValidationGroup="Alta"></asp:RequiredFieldValidator>
                    </div>

                    <div class="campo">
                        <asp:Label ID="lblNombre" runat="server" AssociatedControlID="txtNombre" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control" MaxLength="50"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="valNombre" runat="server" ControlToValidate="txtNombre" Display="Dynamic" CssClass="campo-error" ValidationGroup="Alta"></asp:RequiredFieldValidator>
                    </div>

                    <div class="campo">
                        <asp:Label ID="lblApellido" runat="server" AssociatedControlID="txtApellido" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtApellido" runat="server" CssClass="form-control" MaxLength="50"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="valApellido" runat="server" ControlToValidate="txtApellido" Display="Dynamic" CssClass="campo-error" ValidationGroup="Alta"></asp:RequiredFieldValidator>
                    </div>

                    <div class="campo campo-ancho">
                        <asp:Label ID="lblEmail" runat="server" AssociatedControlID="txtEmail" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" CssClass="form-control" MaxLength="100"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="valEmail" runat="server" ControlToValidate="txtEmail" Display="Dynamic" CssClass="campo-error" ValidationGroup="Alta"></asp:RequiredFieldValidator>
                        <asp:RegularExpressionValidator ID="valFormatoEmail" runat="server" ControlToValidate="txtEmail"
                            ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]{2,}$"
                            Display="Dynamic" CssClass="campo-error" ValidationGroup="Alta"></asp:RegularExpressionValidator>
                        <p class="campo-ayuda"><%: Traductor.T("altaUsuario.ayudaClave") %></p>
                    </div>
                </div>

                <div class="formulario-acciones">
                    <asp:Button ID="btnCrear" runat="server" CssClass="btn btn-strategic" ValidationGroup="Alta" OnClick="btnCrear_Click" />
                    <asp:Button ID="btnCancelar" runat="server" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnCancelar_Click" />
                </div>

                <asp:Label ID="lblError" runat="server" CssClass="mensaje-error" EnableViewState="false"></asp:Label>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlCreado" runat="server" CssClass="strategic-card" Visible="false">
            <div class="strategic-card-header">
                <h2><%: Traductor.T("altaUsuario.creado") %></h2>
            </div>

            <div class="strategic-card-body">
                <dl class="detalle-grid">
                    <div class="detalle-item">
                        <dt><%: Traductor.T("usuario.usuario") %></dt>
                        <dd><asp:Label ID="lblCreadoUsuario" runat="server"></asp:Label></dd>
                    </div>

                    <div class="detalle-item">
                        <dt><%: Traductor.T("usuario.nombreApellido") %></dt>
                        <dd><asp:Label ID="lblCreadoNombre" runat="server"></asp:Label></dd>
                    </div>

                    <div class="detalle-item">
                        <dt><%: Traductor.T("usuario.rol") %></dt>
                        <dd><asp:Label ID="lblCreadoRol" runat="server"></asp:Label></dd>
                    </div>

                    <div class="detalle-item">
                        <dt><%: Traductor.T("altaUsuario.claveInicial") %></dt>
                        <dd>
                            <span class="clave-generada"><asp:Label ID="lblCreadoClave" runat="server"></asp:Label></span>
                            <p class="campo-ayuda"><%: Traductor.T("altaUsuario.ayudaClaveInicial") %></p>
                        </dd>
                    </div>
                </dl>

                <div class="formulario-acciones">
                    <asp:Button ID="btnOtro" runat="server" CssClass="btn btn-strategic" CausesValidation="false" OnClick="btnOtro_Click" />
                    <asp:Button ID="btnVolver" runat="server" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnVolver_Click" />
                </div>
            </div>
        </asp:Panel>

    </main>
</asp:Content>
