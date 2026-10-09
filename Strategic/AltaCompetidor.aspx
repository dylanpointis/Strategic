<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AltaCompetidor.aspx.cs" Inherits="Strategic.AltaCompetidor" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <main class="page-section">

        <div class="page-heading">
            <h1><%: Traductor.T("altaCompetidor.titulo") %></h1>
            <p><%: Traductor.T("altaCompetidor.subtitulo") %></p>
        </div>

        <asp:Panel ID="pnlFormulario" runat="server" CssClass="strategic-card">
            <div class="strategic-card-header">
                <h2><%: Traductor.T("competidor.datos") %></h2>
                <asp:Label ID="lblEstadoActual" runat="server" CssClass="strategic-card-meta"></asp:Label>
            </div>

            <div class="strategic-card-body">
                <div class="formulario-grid">
                    <div class="campo">
                        <asp:Label ID="lblNombre" runat="server" AssociatedControlID="txtNombre" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control" MaxLength="255"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="valNombre" runat="server" ControlToValidate="txtNombre" Display="Dynamic" CssClass="campo-error" ValidationGroup="Alta"></asp:RequiredFieldValidator>
                    </div>

                    <div class="campo">
                        <asp:Label ID="lblMarketplace" runat="server" AssociatedControlID="ddlMarketplace" CssClass="form-label"></asp:Label>
                        <asp:DropDownList ID="ddlMarketplace" runat="server" CssClass="form-control">
                            <asp:ListItem Text="" Value=""></asp:ListItem>
                            <asp:ListItem Text="MercadoLibre" Value="MercadoLibre"></asp:ListItem>
                            <asp:ListItem Text="Sitio Propio" Value="Sitio Propio"></asp:ListItem>
                            <asp:ListItem Text="Otro" Value="Otro"></asp:ListItem>
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator ID="valMarketplace" runat="server" ControlToValidate="ddlMarketplace" Display="Dynamic" CssClass="campo-error" ValidationGroup="Alta"></asp:RequiredFieldValidator>
                    </div>

                    <div class="campo campo-ancho">
                        <asp:Label ID="lblDescripcion" runat="server" AssociatedControlID="txtDescripcion" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtDescripcion" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control" MaxLength="500"></asp:TextBox>
                    </div>
                </div>

                <div class="formulario-acciones">
                    <asp:Button ID="btnCrear" runat="server" CssClass="btn btn-strategic" ValidationGroup="Alta" OnClick="btnCrear_Click" />
                    <asp:Button ID="btnCancelar" runat="server" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnCancelar_Click" />
                </div>

                <asp:Label ID="lblError" runat="server" CssClass="mensaje-error" EnableViewState="false"></asp:Label>
            </div>
        </asp:Panel>

    </main>
</asp:Content>
