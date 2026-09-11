<%@ Page Title="Monitoreo de publicaciones" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MonitoreoPublicacionesCompetidoras.aspx.cs" Inherits="Strategic.MonitoreoPublicacionesCompetidoras" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <main class="page-section">

        <div class="page-heading">
            <h1>Monitoreo de publicaciones</h1>
            <p>Estado del monitoreo automático de las publicaciones de la competencia mapeadas.</p>
        </div>

        <asp:Panel ID="pnlSinDatos" runat="server" CssClass="strategic-card" Visible="false">
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
                    <p>No existen publicaciones mapeadas.</p>
                    <a href="MapearProductosCompetencia.aspx" class="btn btn-strategic">Mapear productos</a>
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlContenido" runat="server">

            <section class="strategic-card">
                <div class="strategic-card-header">
                    <h2>Filtros de búsqueda</h2>
                </div>

                <div class="strategic-card-body">
                    <div class="filtros-grid">
                        <div class="filtro">
                            <asp:Label ID="lblProducto" runat="server" Text="Producto" AssociatedControlID="ddlProducto" CssClass="form-label"></asp:Label>
                            <asp:DropDownList ID="ddlProducto" runat="server" CssClass="form-select">
                            </asp:DropDownList>
                        </div>

                        <div class="filtro">
                            <asp:Label ID="lblCompetidor" runat="server" Text="Competidor" AssociatedControlID="ddlCompetidor" CssClass="form-label"></asp:Label>
                            <asp:DropDownList ID="ddlCompetidor" runat="server" CssClass="form-select">
                            </asp:DropDownList>
                        </div>

                        <div class="filtro">
                            <asp:Label ID="lblEstado" runat="server" Text="Estado" AssociatedControlID="ddlEstado" CssClass="form-label"></asp:Label>
                            <asp:DropDownList ID="ddlEstado" runat="server" CssClass="form-select">
                                <asp:ListItem Text="Todos" Value=""></asp:ListItem>
                                <asp:ListItem Text="Activa" Value="Activa"></asp:ListItem>
                                <asp:ListItem Text="Pausada" Value="Pausada"></asp:ListItem>
                                <asp:ListItem Text="Finalizada" Value="Finalizada"></asp:ListItem>
                                <asp:ListItem Text="No encontrada" Value="No encontrada"></asp:ListItem>
                                <asp:ListItem Text="Error" Value="Error"></asp:ListItem>
                            </asp:DropDownList>
                        </div>
                    </div>

                    <div class="filtros-acciones">
                        <asp:Button ID="btnFiltrar" runat="server" Text="Filtrar" CssClass="btn btn-strategic" CausesValidation="false" OnClick="btnFiltrar_Click" />
                        <asp:Button ID="btnLimpiar" runat="server" Text="Limpiar filtros" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnLimpiar_Click" />
                    </div>

                    <asp:Label ID="lblError" runat="server" CssClass="mensaje-error" EnableViewState="false"></asp:Label>
                    <asp:Label ID="lblExito" runat="server" CssClass="mensaje-exito" EnableViewState="false"></asp:Label>
                    <asp:Label ID="lblAvisoFiltros" runat="server" CssClass="mensaje-aviso" EnableViewState="false"></asp:Label>
                </div>
            </section>

            <section class="strategic-card">
                <div class="strategic-card-header">
                    <h2>Publicaciones mapeadas</h2>
                    <div class="strategic-card-acciones">
                        <asp:Label ID="lblCantidad" runat="server" CssClass="strategic-card-meta" EnableViewState="false"></asp:Label>
                    </div>
                </div>

                <div class="strategic-card-body sin-padding">
                    <sc:GrillaGenerica ID="grillaPublicaciones" runat="server" OnAccionSeleccionada="grillaPublicaciones_AccionSeleccionada" />
                </div>
            </section>

        </asp:Panel>

    </main>
</asp:Content>
