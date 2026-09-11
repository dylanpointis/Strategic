<%@ Page Title="Competidores" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ConsultarCompetidores.aspx.cs" Inherits="Strategic.ConsultarCompetidores" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <main class="page-section">

        <div class="page-heading">
            <h1>Competidores</h1>
            <p>Listado de competidores relevados en el sistema.</p>
        </div>

        <section class="strategic-card">
            <div class="strategic-card-header">
                <h2>Filtros de búsqueda</h2>
            </div>

            <div class="strategic-card-body">
                <div class="filtros-grid">
                    <div class="filtro">
                        <asp:Label ID="lblTexto" runat="server" Text="Nombre" AssociatedControlID="txtTexto" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtTexto" runat="server" CssClass="form-control" MaxLength="255" placeholder="Todos"></asp:TextBox>
                    </div>

                    <div class="filtro">
                        <asp:Label ID="lblMarketplace" runat="server" Text="Marketplace" AssociatedControlID="txtMarketplace" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtMarketplace" runat="server" CssClass="form-control" MaxLength="50" placeholder="Todos"></asp:TextBox>
                    </div>

                    <div class="filtro">
                        <asp:Label ID="lblEstado" runat="server" Text="Estado" AssociatedControlID="ddlEstado" CssClass="form-label"></asp:Label>
                        <asp:DropDownList ID="ddlEstado" runat="server" CssClass="form-select">
                            <asp:ListItem Text="Activos" Value="1" Selected="True"></asp:ListItem>
                            <asp:ListItem Text="Inactivos" Value="0"></asp:ListItem>
                            <asp:ListItem Text="Todos" Value=""></asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>

                <div class="filtros-acciones">
                    <asp:Button ID="btnFiltrar" runat="server" Text="Filtrar" CssClass="btn btn-strategic" OnClick="btnFiltrar_Click" />
                    <asp:Button ID="btnLimpiar" runat="server" Text="Limpiar filtros" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnLimpiar_Click" />
                </div>

                <asp:Label ID="lblError" runat="server" CssClass="mensaje-error" EnableViewState="false"></asp:Label>
                <asp:Label ID="lblExito" runat="server" CssClass="mensaje-exito" EnableViewState="false"></asp:Label>
                <asp:Label ID="lblAvisoFiltros" runat="server" CssClass="mensaje-aviso" EnableViewState="false"></asp:Label>
            </div>
        </section>

        <section class="strategic-card">
            <div class="strategic-card-header">
                <h2>Listado de competidores</h2>
                <div class="strategic-card-acciones">
                    <asp:Label ID="lblCantidad" runat="server" CssClass="strategic-card-meta" EnableViewState="false"></asp:Label>
                    <asp:Button ID="btnNuevo" runat="server" Text="Nuevo competidor" CssClass="btn btn-strategic" CausesValidation="false" OnClick="btnNuevo_Click" />
                </div>
            </div>

            <div class="strategic-card-body sin-padding">
                <sc:GrillaGenerica ID="grillaCompetidores" runat="server" OnAccionSeleccionada="grillaCompetidores_AccionSeleccionada" />
            </div>
        </section>

    </main>
</asp:Content>
