<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ReporteFacturacion.aspx.cs" Inherits="Strategic.ReporteFacturacion" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <main class="page-section">

        <div class="page-heading">
            <h1><%: Traductor.T("reporteFacturacion.titulo") %></h1>
            <p><%: Traductor.T("reporteFacturacion.subtitulo") %></p>
        </div>

        <section class="strategic-card">
            <div class="strategic-card-header">
                <h2><%: Traductor.T("comun.filtrosBusqueda") %></h2>
            </div>

            <div class="strategic-card-body">
                <div class="filtros-grid">
                    <sc:FiltroFechas ID="filtroFechas" runat="server" MostrarBoton="false" />

                    <div class="filtro">
                        <asp:Label ID="lblAgrupacion" runat="server" AssociatedControlID="ddlAgrupacion" CssClass="form-label"></asp:Label>
                        <asp:DropDownList ID="ddlAgrupacion" runat="server" CssClass="form-select"></asp:DropDownList>
                    </div>
                </div>

                <div class="filtros-acciones">
                    <asp:Button ID="btnFiltrar" runat="server" CssClass="btn btn-strategic" OnClick="btnFiltrar_Click" />
                    <asp:Button ID="btnLimpiar" runat="server" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnLimpiar_Click" />
                </div>

                <asp:Label ID="lblError" runat="server" CssClass="mensaje-error" EnableViewState="false"></asp:Label>
            </div>
        </section>

        <section class="strategic-card">
            <div class="strategic-card-header">
                <h2><%: Traductor.T("reporteFacturacion.resumen") %></h2>
            </div>

            <div class="strategic-card-body">
                <div class="indicadores-grid">
                    <div class="indicador">
                        <span class="indicador-titulo"><%: Traductor.T("reporteFacturacion.facturacionTotal") %></span>
                        <span class="indicador-valor"><asp:Label ID="lblFacturacionTotal" runat="server"></asp:Label></span>
                    </div>

                    <div class="indicador">
                        <span class="indicador-titulo"><%: Traductor.T("reporteFacturacion.pagosRegistrados") %></span>
                        <span class="indicador-valor"><asp:Label ID="lblCantidadPagos" runat="server"></asp:Label></span>
                    </div>

                    <div class="indicador">
                        <span class="indicador-titulo"><%: Traductor.T("reporteFacturacion.pagoPromedio") %></span>
                        <span class="indicador-valor"><asp:Label ID="lblPagoPromedio" runat="server"></asp:Label></span>
                    </div>

                    <div class="indicador">
                        <span class="indicador-titulo"><%: Traductor.T("reporteFacturacion.estadoSuscripcion") %></span>
                        <span class="indicador-valor"><asp:Label ID="lblEstadoSuscripcion" runat="server"></asp:Label></span>
                        <asp:Label ID="lblVencimiento" runat="server" CssClass="indicador-nota"></asp:Label>
                    </div>
                </div>
            </div>
        </section>

        <section class="strategic-card">
            <div class="strategic-card-header">
                <h2><asp:Label ID="lblTituloGrafico" runat="server"></asp:Label></h2>
            </div>

            <div class="strategic-card-body">
                <asp:Panel ID="pnlGrafico" runat="server" CssClass="grafico-contenedor" Visible="false">
                    <canvas id="graficoFacturacion"></canvas>
                </asp:Panel>

                <asp:Panel ID="pnlSinGrafico" runat="server" CssClass="estado-vacio">
                    <span class="estado-vacio-icono">
                        <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false">
                            <path d="M4 19V5"></path>
                            <path d="M4 19h16"></path>
                            <path d="M7 15l4-5 3 3 4-6"></path>
                        </svg>
                    </span>
                    <p><asp:Label ID="lblSinGrafico" runat="server"></asp:Label></p>
                </asp:Panel>
            </div>
        </section>

        <section class="strategic-card">
            <div class="strategic-card-header">
                <h2><%: Traductor.T("reporteFacturacion.listado") %></h2>
                <asp:Label ID="lblCantidad" runat="server" CssClass="strategic-card-meta"></asp:Label>
            </div>

            <div class="strategic-card-body sin-padding">
                <sc:GrillaGenerica ID="grillaPagos" runat="server" />
            </div>
        </section>

    </main>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ScriptsFinales" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.1/dist/chart.umd.min.js"></script>
    <script src="<%: ResolveUrl("~/Scripts/strategic-graficos.js") %>"></script>
    <script type="text/javascript">
        Strategic.grafico.barras('graficoFacturacion', <%= DatosGraficoJson %>, '<%: Traductor.T("reporteFacturacion.facturacion") %>', '$');
    </script>
</asp:Content>
