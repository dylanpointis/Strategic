<%@ Page Title="Historial de precios" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="HistorialPrecios.aspx.cs" Inherits="Strategic.HistorialPrecios" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <main class="page-section">

        <div class="page-heading">
            <h1>Historial de precios</h1>
            <p>Evolución histórica del precio de cada producto.</p>
        </div>

        <section class="strategic-card">
            <div class="strategic-card-header">
                <h2>Filtros de búsqueda</h2>
            </div>

            <div class="strategic-card-body">
                <div class="filtros-grid">
                    <div class="filtro">
                        <asp:Label ID="lblProducto" runat="server" Text="Producto" AssociatedControlID="ddlProducto" CssClass="form-label"></asp:Label>
                        <asp:DropDownList ID="ddlProducto" runat="server" CssClass="form-select" AutoPostBack="true"
                            CausesValidation="false" OnSelectedIndexChanged="ddlProducto_SelectedIndexChanged"></asp:DropDownList>
                    </div>

                    <sc:FiltroFechas ID="filtroFechas" runat="server" MostrarBoton="false" />
                </div>

                <div class="filtros-acciones">
                    <asp:Button ID="btnFiltrar" runat="server" Text="Filtrar" CssClass="btn btn-strategic" OnClick="btnFiltrar_Click" />
                    <asp:Button ID="btnLimpiar" runat="server" Text="Limpiar filtros" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnLimpiar_Click" />
                </div>

                <asp:Label ID="lblError" runat="server" CssClass="mensaje-error" EnableViewState="false"></asp:Label>
            </div>
        </section>

        <section class="strategic-card">
            <div class="strategic-card-header">
                <h2>Evolución del precio</h2>
                <asp:Label ID="lblProductoElegido" runat="server" CssClass="strategic-card-meta" EnableViewState="false"></asp:Label>
            </div>

            <div class="strategic-card-body">
                <asp:Panel ID="pnlGrafico" runat="server" CssClass="grafico-contenedor" Visible="false">
                    <canvas id="graficoPrecios"></canvas>
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
                <h2>Detalle del historial</h2>
                <asp:Label ID="lblCantidad" runat="server" CssClass="strategic-card-meta"></asp:Label>
            </div>

            <div class="strategic-card-body sin-padding">
                <sc:GrillaGenerica ID="grillaPrecios" runat="server" />
            </div>
        </section>

    </main>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ScriptsFinales" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.1/dist/chart.umd.min.js"></script>
    <script src="<%: ResolveUrl("~/Scripts/strategic-graficos.js") %>"></script>
    <script type="text/javascript">
        Strategic.grafico.linea('graficoPrecios', <%= DatosGraficoJson %>, 'Precio', '$');
    </script>
</asp:Content>
