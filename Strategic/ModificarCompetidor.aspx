<%@ Page Title="Modificar competidor" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ModificarCompetidor.aspx.cs" Inherits="Strategic.ModificarCompetidor" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <main class="page-section">

        <div class="page-heading">
            <h1>Modificar competidor</h1>
            <p>Actualización de los datos de un competidor.</p>
        </div>

        <asp:Panel ID="pnlFormulario" runat="server" CssClass="strategic-card">
            <div class="strategic-card-header">
                <h2>Datos del competidor</h2>
                <asp:Label ID="lblEstadoActual" runat="server" CssClass="strategic-card-meta"></asp:Label>
            </div>

            <div class="strategic-card-body">
                <div class="formulario-grid">
                    <div class="campo">
                        <asp:Label ID="lblNombre" runat="server" Text="Nombre" AssociatedControlID="txtNombre" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control" MaxLength="255"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="valNombre" runat="server" ControlToValidate="txtNombre"
                            ErrorMessage="Ingresá el nombre del competidor" Display="Dynamic" CssClass="campo-error" ValidationGroup="Modificar"></asp:RequiredFieldValidator>
                    </div>

                    <div class="campo">
                        <asp:Label ID="lblMarketplace" runat="server" Text="Marketplace" AssociatedControlID="txtMarketplace" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtMarketplace" runat="server" CssClass="form-control" MaxLength="50"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="valMarketplace" runat="server" ControlToValidate="txtMarketplace"
                            ErrorMessage="Ingresá el marketplace" Display="Dynamic" CssClass="campo-error" ValidationGroup="Modificar"></asp:RequiredFieldValidator>
                    </div>

                    <div class="campo campo-ancho">
                        <asp:Label ID="lblDescripcion" runat="server" Text="Descripción" AssociatedControlID="txtDescripcion" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtDescripcion" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control" MaxLength="500"></asp:TextBox>
                    </div>
                </div>

                <div class="formulario-acciones">
                    <asp:Button ID="btnGuardar" runat="server" Text="Guardar cambios" CssClass="btn btn-strategic" ValidationGroup="Modificar" OnClick="btnGuardar_Click" />
                    <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnCancelar_Click" />
                </div>

                <asp:Label ID="lblError" runat="server" CssClass="mensaje-error" EnableViewState="false"></asp:Label>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlSinCompetidor" runat="server" CssClass="strategic-card" Visible="false">
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
                    <p><asp:Label ID="lblSinCompetidor" runat="server"></asp:Label></p>
                </div>

                <div class="formulario-acciones">
                    <asp:Button ID="btnVolver" runat="server" Text="Volver al listado" CssClass="btn btn-strategic" CausesValidation="false" OnClick="btnCancelar_Click" />
                </div>
            </div>
        </asp:Panel>

    </main>
</asp:Content>
