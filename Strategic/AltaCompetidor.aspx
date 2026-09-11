<%@ Page Title="Nuevo competidor" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AltaCompetidor.aspx.cs" Inherits="Strategic.AltaCompetidor" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <main class="page-section">

        <div class="page-heading">
            <h1>Nuevo competidor</h1>
            <p>Registro de un competidor a monitorear.</p>
        </div>

        <asp:Panel ID="pnlFormulario" runat="server" CssClass="strategic-card">
            <div class="strategic-card-header">
                <h2>Datos del competidor</h2>
                <asp:Label ID="lblEstadoActual" runat="server" Text="Nace activo" CssClass="strategic-card-meta"></asp:Label>
            </div>

            <div class="strategic-card-body">
                <div class="formulario-grid">
                    <div class="campo">
                        <asp:Label ID="lblNombre" runat="server" Text="Nombre" AssociatedControlID="txtNombre" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control" MaxLength="255"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="valNombre" runat="server" ControlToValidate="txtNombre"
                            ErrorMessage="Ingresá el nombre del competidor" Display="Dynamic" CssClass="campo-error" ValidationGroup="Alta"></asp:RequiredFieldValidator>
                    </div>

                    <div class="campo">
                        <asp:Label ID="lblMarketplace" runat="server" Text="Marketplace" AssociatedControlID="ddlMarketplace" CssClass="form-label"></asp:Label>
                        <asp:DropDownList ID="ddlMarketplace" runat="server" CssClass="form-control">
                            <asp:ListItem Text="" Value=""></asp:ListItem>
                            <asp:ListItem Text="MercadoLibre" Value="MercadoLibre"></asp:ListItem>
                            <asp:ListItem Text="Sitio Propio" Value="Sitio Propio"></asp:ListItem>
                            <asp:ListItem Text="Otro" Value="Otro"></asp:ListItem>
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator ID="valMarketplace" runat="server" ControlToValidate="ddlMarketplace"
                            ErrorMessage="Seleccioná el marketplace" Display="Dynamic" CssClass="campo-error" ValidationGroup="Alta"></asp:RequiredFieldValidator>
                    </div>

                    <div class="campo campo-ancho">
                        <asp:Label ID="lblDescripcion" runat="server" Text="Descripción" AssociatedControlID="txtDescripcion" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtDescripcion" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control" MaxLength="500"></asp:TextBox>
                    </div>
                </div>

                <div class="formulario-acciones">
                    <asp:Button ID="btnCrear" runat="server" Text="Crear competidor" CssClass="btn btn-strategic" ValidationGroup="Alta" OnClick="btnCrear_Click" />
                    <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnCancelar_Click" />
                </div>

                <asp:Label ID="lblError" runat="server" CssClass="mensaje-error" EnableViewState="false"></asp:Label>
            </div>
        </asp:Panel>

    </main>
</asp:Content>
