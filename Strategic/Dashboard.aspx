<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="Strategic.Dashboard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <main class="page-section">

        <div class="page-heading">
            <h1><%: Traductor.T("dashboard.titulo") %></h1>
            <p><%: Traductor.T("dashboard.subtitulo") %></p>
        </div>

        <asp:Panel ID="pnlSinDatos" runat="server" CssClass="aviso-sincronizacion" Visible="false">
            <span class="aviso-icono">
                <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false">
                    <path d="M21 12a9 9 0 1 1-3-6.7"></path>
                    <path d="M21 4v5h-5"></path>
                </svg>
            </span>
            <div>
                <strong><%: Traductor.T("dashboard.sinDatos") %></strong>
                <p><%: Traductor.T("dashboard.sinDatosAyuda") %></p>
            </div>
            <a class="btn btn-strategic" href="ImportarDatos.aspx"><%: Traductor.T("master.menu.importarDatos") %></a>
        </asp:Panel>

        <section class="strategic-card">
            <div class="strategic-card-header">
                <h2><%: Traductor.T("dashboard.filtros") %></h2>
                <asp:Label ID="lblPeriodo" runat="server" CssClass="strategic-card-meta" EnableViewState="false"></asp:Label>
            </div>

            <div class="strategic-card-body">
                <div class="filtros-grid">
                    <sc:FiltroFechas ID="filtroFechas" runat="server" MostrarBoton="false" />

                    <div class="filtro">
                        <asp:Label ID="lblCategoria" runat="server" AssociatedControlID="ddlCategoria" CssClass="form-label"></asp:Label>
                        <asp:DropDownList ID="ddlCategoria" runat="server" CssClass="form-select"></asp:DropDownList>
                    </div>
                </div>

                <div class="filtros-acciones">
                    <asp:Button ID="btnFiltrar" runat="server" CssClass="btn btn-strategic" OnClick="btnFiltrar_Click" />
                    <asp:Button ID="btnLimpiar" runat="server" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnLimpiar_Click" />
                </div>

                <asp:Label ID="lblError" runat="server" CssClass="mensaje-error" EnableViewState="false"></asp:Label>
                <asp:Label ID="lblAvisoFiltros" runat="server" CssClass="mensaje-aviso" EnableViewState="false"></asp:Label>
            </div>
        </section>

        <section class="strategic-card">
            <div class="strategic-card-header">
                <h2><%: Traductor.T("dashboard.indicadores") %></h2>
            </div>

            <div class="strategic-card-body">
                <div class="indicadores-grid">
                    <div class="indicador">
                        <span class="indicador-titulo"><%: Traductor.T("historialVentas.facturacionTotal") %></span>
                        <span class="indicador-valor"><asp:Label ID="lblFacturacionTotal" runat="server"></asp:Label></span>
                    </div>

                    <div class="indicador">
                        <span class="indicador-titulo"><%: Traductor.T("dashboard.cantidadVentas") %></span>
                        <span class="indicador-valor"><asp:Label ID="lblCantidadVentas" runat="server"></asp:Label></span>
                    </div>
                </div>
            </div>
        </section>

        <section class="strategic-card">
            <div class="strategic-card-header">
                <h2><%: Traductor.T("dashboard.vendidosPorCategoria") %></h2>
            </div>

            <div class="strategic-card-body">
                <asp:Panel ID="pnlGraficoCategoria" runat="server" CssClass="grafico-contenedor grafico-torta" Visible="false">
                    <canvas id="graficoCategoria"></canvas>
                </asp:Panel>

                <asp:Panel ID="pnlSinCategoria" runat="server" CssClass="estado-vacio">
                    <span class="estado-vacio-icono">
                        <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false">
                            <path d="M4 19V5"></path>
                            <path d="M4 19h16"></path>
                            <path d="M7 15l4-5 3 3 4-6"></path>
                        </svg>
                    </span>
                    <p><asp:Label ID="lblSinCategoria" runat="server"></asp:Label></p>
                </asp:Panel>
            </div>
        </section>

        <section class="strategic-card">
            <div class="strategic-card-header">
                <h2><%: Traductor.T("dashboard.masVendidos") %></h2>
                <asp:Label ID="lblTopMeta" runat="server" CssClass="strategic-card-meta" EnableViewState="false"></asp:Label>
            </div>

            <div class="strategic-card-body">
                <asp:Panel ID="pnlGraficoTop" runat="server" CssClass="grafico-contenedor grafico-alto" Visible="false">
                    <canvas id="graficoTop"></canvas>
                </asp:Panel>

                <asp:Panel ID="pnlSinTop" runat="server" CssClass="estado-vacio">
                    <span class="estado-vacio-icono">
                        <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false">
                            <path d="M4 19V5"></path>
                            <path d="M4 19h16"></path>
                            <path d="M7 15l4-5 3 3 4-6"></path>
                        </svg>
                    </span>
                    <p><asp:Label ID="lblSinTop" runat="server"></asp:Label></p>
                </asp:Panel>
            </div>
        </section>

        <section class="strategic-card">
            <div class="strategic-card-header">
                <h2><%: Traductor.T("dashboard.bajoStock") %></h2>
                <asp:Label ID="lblCantidadBajoStock" runat="server" CssClass="strategic-card-meta"></asp:Label>
            </div>

            <div class="strategic-card-body sin-padding">
                <sc:GrillaGenerica ID="grillaBajoStock" runat="server" />
            </div>
        </section>

    </main>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ScriptsFinales" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.1/dist/chart.umd.min.js"></script>
    <script src="<%: ResolveUrl("~/Scripts/strategic-graficos.js") %>"></script>
    <script type="text/javascript">
        Strategic.grafico.torta('graficoCategoria', <%= DatosCategoriaJson %>, '');
        Strategic.grafico.barrasHorizontales('graficoTop', <%= DatosTopJson %>, '<%: Traductor.T("historialVentas.unidades") %>', '');
    </script>
</asp:Content>
