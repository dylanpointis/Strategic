<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Predicciones.aspx.cs" Inherits="Strategic.Predicciones" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <main class="page-section">

        <div class="page-heading">
            <h1><%: Traductor.T("predicciones.titulo") %></h1>
            <p><%: Traductor.T("predicciones.subtitulo") %></p>
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
                <p><%: Traductor.T("predicciones.sinDatosAyuda") %></p>
            </div>
            <a class="btn btn-strategic" href="ImportarDatos.aspx"><%: Traductor.T("master.menu.importarDatos") %></a>
        </asp:Panel>

        <section class="strategic-card">
            <div class="strategic-card-header">
                <h2><%: Traductor.T("predicciones.quePredecir") %></h2>
            </div>

            <div class="strategic-card-body">
                <div class="filtros-grid">
                    <div class="filtro">
                        <asp:Label ID="lblTipo" runat="server" AssociatedControlID="ddlTipo" CssClass="form-label"></asp:Label>
                        <asp:DropDownList ID="ddlTipo" runat="server" CssClass="form-select"></asp:DropDownList>
                    </div>

                    <div class="filtro">
                        <asp:Label ID="lblProducto" runat="server" AssociatedControlID="ddlProducto" CssClass="form-label"></asp:Label>
                        <asp:DropDownList ID="ddlProducto" runat="server" CssClass="form-select"></asp:DropDownList>
                    </div>

                    <div class="filtro">
                        <asp:Label ID="lblHorizonte" runat="server" AssociatedControlID="ddlHorizonte" CssClass="form-label"></asp:Label>
                        <asp:DropDownList ID="ddlHorizonte" runat="server" CssClass="form-select">
                            <asp:ListItem Value="7"></asp:ListItem>
                            <asp:ListItem Value="15"></asp:ListItem>
                            <asp:ListItem Value="30" Selected="True"></asp:ListItem>
                            <asp:ListItem Value="60"></asp:ListItem>
                            <asp:ListItem Value="90"></asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>

                <div class="filtros-acciones">
                    <asp:Button ID="btnGenerar" runat="server" CssClass="btn btn-strategic" OnClick="btnGenerar_Click" />
                    <asp:Button ID="btnLimpiar" runat="server" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnLimpiar_Click" />
                </div>

                <asp:Label ID="lblError" runat="server" CssClass="mensaje-error" EnableViewState="false"></asp:Label>
            </div>
        </section>

        <asp:Panel ID="pnlResultado" runat="server" Visible="false">

            <section class="strategic-card">
                <div class="strategic-card-header">
                    <h2><%: Traductor.T("predicciones.resultado") %></h2>
                    <asp:Label ID="lblResumen" runat="server" CssClass="strategic-card-meta"></asp:Label>
                </div>

                <div class="strategic-card-body">
                    <div class="indicadores-grid">
                        <div class="indicador">
                            <span class="indicador-titulo"><%: Traductor.T("predicciones.valorActual") %></span>
                            <span class="indicador-valor"><asp:Label ID="lblValorActual" runat="server"></asp:Label></span>
                            <span class="indicador-nota"><asp:Label ID="lblFechaBase" runat="server"></asp:Label></span>
                        </div>

                        <div class="indicador">
                            <span class="indicador-titulo"><%: Traductor.T("predicciones.valorPredicho") %></span>
                            <span class="indicador-valor"><asp:Label ID="lblValorPredicho" runat="server"></asp:Label></span>
                            <span class="indicador-nota"><asp:Label ID="lblFechaFin" runat="server"></asp:Label></span>
                        </div>

                        <div class="indicador">
                            <span class="indicador-titulo"><%: Traductor.T("predicciones.variacion") %></span>
                            <span class="indicador-valor"><asp:Label ID="lblVariacion" runat="server"></asp:Label></span>
                            <span class="indicador-nota"><asp:Label ID="lblTendencia" runat="server"></asp:Label></span>
                        </div>

                        <div class="indicador">
                            <span class="indicador-titulo"><%: Traductor.T("predicciones.confianza") %></span>
                            <span class="indicador-valor"><asp:Label ID="lblConfianza" runat="server"></asp:Label></span>
                            <span class="indicador-nota"><asp:Label ID="lblR2" runat="server"></asp:Label></span>
                        </div>
                    </div>

                    <asp:Panel ID="pnlQuiebre" runat="server" CssClass="mensaje-aviso" Visible="false">
                        <asp:Label ID="lblQuiebre" runat="server"></asp:Label>
                    </asp:Panel>
                </div>
            </section>

            <section class="strategic-card">
                <div class="strategic-card-header">
                    <h2><%: Traductor.T("predicciones.historicoYProyeccion") %></h2>
                    <asp:Label ID="lblDetalleRecta" runat="server" CssClass="strategic-card-meta"></asp:Label>
                </div>

                <div class="strategic-card-body">
                    <div class="grafico-contenedor">
                        <canvas id="graficoPrediccion"></canvas>
                    </div>
                </div>
            </section>

            <section class="strategic-card">
                <div class="strategic-card-header">
                    <h2><%: Traductor.T("predicciones.detalleProyeccion") %></h2>
                    <asp:Label ID="lblCantidad" runat="server" CssClass="strategic-card-meta"></asp:Label>
                </div>

                <div class="strategic-card-body sin-padding">
                    <sc:GrillaGenerica ID="grillaProyeccion" runat="server" />
                </div>
            </section>

        </asp:Panel>

    </main>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ScriptsFinales" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.1/dist/chart.umd.min.js"></script>
    <script src="<%: ResolveUrl("~/Scripts/strategic-graficos.js") %>"></script>
    <script type="text/javascript">
        Strategic.grafico.lineas('graficoPrediccion', <%= DatosGraficoJson %>, '<%= PrefijoGrafico %>');
    </script>
</asp:Content>
