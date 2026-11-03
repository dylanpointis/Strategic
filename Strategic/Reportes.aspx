<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Reportes.aspx.cs" Inherits="Strategic.Reportes" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <main class="page-section">

        <div class="page-heading">
            <h1><%: Traductor.T("reportes.titulo") %></h1>
            <p><%: Traductor.T("reportes.subtitulo") %></p>
        </div>

        <%-- Alternativa 4.2: sin datos sincronizados no hay nada que reportar --%>
        <asp:Panel ID="pnlSinDatos" runat="server" CssClass="aviso-sincronizacion" Visible="false">
            <span class="aviso-icono">
                <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false">
                    <path d="M21 12a9 9 0 1 1-3-6.7"></path>
                    <path d="M21 4v5h-5"></path>
                </svg>
            </span>
            <div>
                <strong><%: Traductor.T("dashboard.sinDatos") %></strong>
                <p><%: Traductor.T("reportes.sinDatosAyuda") %></p>
            </div>
            <a class="btn btn-strategic" href="ImportarDatos.aspx"><%: Traductor.T("master.menu.importarDatos") %></a>
        </asp:Panel>

        <section class="strategic-card">
            <div class="strategic-card-header">
                <h2><%: Traductor.T("reportes.criterios") %></h2>
            </div>

            <div class="strategic-card-body">
                <div class="filtros-grid">
                    <div class="filtro">
                        <asp:Label ID="lblTipo" runat="server" AssociatedControlID="ddlTipo" CssClass="form-label"></asp:Label>
                        <asp:DropDownList ID="ddlTipo" runat="server" CssClass="form-select"></asp:DropDownList>
                    </div>

                    <sc:FiltroFechas ID="filtroFechas" runat="server" MostrarBoton="false" />

                    <div class="filtro">
                        <asp:Label ID="lblCategoria" runat="server" AssociatedControlID="ddlCategoria" CssClass="form-label"></asp:Label>
                        <asp:DropDownList ID="ddlCategoria" runat="server" CssClass="form-select"></asp:DropDownList>
                    </div>
                </div>

                <div class="filtros-acciones">
                    <asp:Button ID="btnGenerar" runat="server" CssClass="btn btn-strategic" OnClick="btnGenerar_Click" />
                    <asp:Button ID="btnLimpiar" runat="server" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnLimpiar_Click" />
                </div>

                <asp:Label ID="lblError" runat="server" CssClass="mensaje-error" EnableViewState="false"></asp:Label>
            </div>
        </section>

        <asp:Panel ID="pnlReporte" runat="server" CssClass="strategic-card" Visible="false">
            <div class="strategic-card-header">
                <h2><asp:Label ID="lblTituloReporte" runat="server"></asp:Label></h2>
                <asp:Label ID="lblPeriodo" runat="server" CssClass="strategic-card-meta"></asp:Label>
            </div>

            <div class="strategic-card-body">
                <div class="indicadores-grid">
                    <asp:Repeater ID="rptMetricas" runat="server" OnItemDataBound="rptMetricas_ItemDataBound">
                        <ItemTemplate>
                            <div class="indicador">
                                <span class="indicador-titulo"><asp:Label ID="lblNombre" runat="server"></asp:Label></span>
                                <span class="indicador-valor"><asp:Label ID="lblValor" runat="server"></asp:Label></span>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>

                <asp:Panel ID="pnlVentas" runat="server">
                    <h3 class="subtitulo-seccion"><%: Traductor.T("reportes.ventasPorCategoria") %></h3>
                    <sc:GrillaGenerica ID="grillaCategorias" runat="server" />

                    <h3 class="subtitulo-seccion"><%: Traductor.T("reportes.topProductos") %></h3>
                    <sc:GrillaGenerica ID="grillaTopProductos" runat="server" />
                </asp:Panel>

                <asp:Panel ID="pnlStock" runat="server">
                    <h3 class="subtitulo-seccion"><%: Traductor.T("reportes.detalleStock") %></h3>
                    <sc:GrillaGenerica ID="grillaStock" runat="server" />
                </asp:Panel>

                <asp:Panel ID="pnlCompetencia" runat="server">
                    <h3 class="subtitulo-seccion"><%: Traductor.T("reportes.detalleCompetencia") %></h3>
                    <sc:GrillaGenerica ID="grillaCompetencia" runat="server" />
                </asp:Panel>

                <%-- Pasos 5 y 6 del CU: el reporte se exporta en XML y se descarga --%>
                <div class="formulario-acciones">
                    <asp:Button ID="btnExportar" runat="server" CssClass="btn btn-strategic" CausesValidation="false" OnClick="btnExportar_Click" />
                </div>
            </div>
        </asp:Panel>

    </main>
</asp:Content>
