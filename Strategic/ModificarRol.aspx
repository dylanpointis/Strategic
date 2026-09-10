<%@ Page Title="Modificar rol" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ModificarRol.aspx.cs" Inherits="Strategic.ModificarRol" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <main class="page-section">

        <div class="page-heading">
            <h1>Modificar rol</h1>
            <p>Cambiá el nombre del rol o los permisos y familias que tiene asignados.</p>
        </div>

        <asp:Panel ID="pnlFormulario" runat="server" CssClass="strategic-card">
            <div class="strategic-card-header">
                <h2>Datos del rol</h2>
                <asp:Label ID="lblEstadoActual" runat="server" CssClass="strategic-card-meta"></asp:Label>
            </div>

            <div class="strategic-card-body">
                <div class="formulario-grid">
                    <div class="campo campo-ancho">
                        <asp:Label ID="lblNombre" runat="server" Text="Nombre del rol" AssociatedControlID="txtNombre" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control" MaxLength="50"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="valNombre" runat="server" ControlToValidate="txtNombre"
                            ErrorMessage="Ingresá el nombre del rol" Display="Dynamic" CssClass="campo-error" ValidationGroup="Rol"></asp:RequiredFieldValidator>
                    </div>
                </div>

                <sc:SelectorPermisos ID="selectorPermisos" runat="server" />

                <div class="formulario-acciones">
                    <asp:Button ID="btnGuardar" runat="server" Text="Guardar cambios" CssClass="btn btn-strategic" ValidationGroup="Rol" OnClick="btnGuardar_Click" />
                    <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnCancelar_Click" />
                </div>

                <asp:Label ID="lblError" runat="server" CssClass="mensaje-error" EnableViewState="false"></asp:Label>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlSinRegistro" runat="server" CssClass="strategic-card" Visible="false">
            <div class="strategic-card-body">
                <div class="estado-vacio">
                    <span class="estado-vacio-icono">
                        <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false">
                            <path d="M12 3l8 4v6c0 4-3.5 7-8 8-4.5-1-8-4-8-8V7z"></path>
                        </svg>
                    </span>
                    <p><asp:Label ID="lblSinRegistro" runat="server"></asp:Label></p>
                </div>

                <div class="formulario-acciones">
                    <asp:Button ID="btnVolver" runat="server" Text="Volver al listado" CssClass="btn btn-strategic" CausesValidation="false" OnClick="btnCancelar_Click" />
                </div>
            </div>
        </asp:Panel>

    </main>
</asp:Content>
