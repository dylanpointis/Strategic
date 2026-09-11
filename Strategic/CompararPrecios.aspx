<%@ Page Title="Comparar precios" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CompararPrecios.aspx.cs" Inherits="Strategic.CompararPrecios" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <main class="page-section">

        <div class="page-heading">
            <h1>Comparar precios</h1>
            <p>Comparación entre tus precios y los de las publicaciones de la competencia.</p>
        </div>

        <asp:Panel ID="pnlSinDatos" runat="server" CssClass="aviso-sincronizacion" Visible="false">
            <span class="aviso-icono">
                <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false">
                    <path d="M21 12a9 9 0 1 1-3-6.7"></path>
                    <path d="M21 4v5h-5"></path>
                </svg>
            </span>
            <div>
                <strong>No existen datos sincronizados</strong>
                <p>Realizá una sincronización para poder comparar tus precios con los de la competencia.</p>
            </div>
            <a class="btn btn-strategic" href="ImportarDatos.aspx">Importar datos</a>
        </asp:Panel>

        <asp:Panel ID="pnlSinMapeos" runat="server" CssClass="aviso-sincronizacion" Visible="false">
            <span class="aviso-icono">
                <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false">
                    <path d="M10 13a5 5 0 0 0 7 0l2-2a5 5 0 0 0-7-7l-1 1"></path>
                    <path d="M14 11a5 5 0 0 0-7 0l-2 2a5 5 0 0 0 7 7l1-1"></path>
                </svg>
            </span>
            <div>
                <strong>Tus productos no tienen competidores asociados</strong>
                <p>Asociá tus productos con publicaciones de la competencia para poder comparar los precios.</p>
            </div>
            <a class="btn btn-strategic" href="MapearProductosCompetencia.aspx">Mapear productos</a>
        </asp:Panel>

        <section class="strategic-card">
            <div class="strategic-card-header">
                <h2>Filtros de búsqueda</h2>
            </div>

            <div class="strategic-card-body">
                <div class="filtros-grid">
                    <div class="filtro">
                        <asp:Label ID="lblProducto" runat="server" Text="Producto" AssociatedControlID="ddlProducto" CssClass="form-label"></asp:Label>
                        <asp:DropDownList ID="ddlProducto" runat="server" CssClass="form-select"></asp:DropDownList>
                    </div>

                    <div class="filtro">
                        <asp:Label ID="lblCategoria" runat="server" Text="Categoría" AssociatedControlID="ddlCategoria" CssClass="form-label"></asp:Label>
                        <asp:DropDownList ID="ddlCategoria" runat="server" CssClass="form-select"></asp:DropDownList>
                    </div>

                    <div class="filtro">
                        <asp:Label ID="lblCompetidor" runat="server" Text="Competidor" AssociatedControlID="ddlCompetidor" CssClass="form-label"></asp:Label>
                        <asp:DropDownList ID="ddlCompetidor" runat="server" CssClass="form-select"></asp:DropDownList>
                    </div>
                </div>

                <div class="filtros-acciones">
                    <asp:Button ID="btnFiltrar" runat="server" Text="Filtrar" CssClass="btn btn-strategic" OnClick="btnFiltrar_Click" />
                    <asp:Button ID="btnLimpiar" runat="server" Text="Limpiar filtros" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnLimpiar_Click" />
                </div>

                <asp:Label ID="lblError" runat="server" CssClass="mensaje-error" EnableViewState="false"></asp:Label>
                <asp:Label ID="lblAvisoFiltros" runat="server" CssClass="mensaje-aviso" EnableViewState="false"></asp:Label>
            </div>
        </section>

        <section class="strategic-card">
            <div class="strategic-card-header">
                <h2>Posición frente a la competencia</h2>
            </div>

            <div class="strategic-card-body">
                <div class="indicadores-grid">
                    <div class="indicador">
                        <span class="indicador-titulo">Publicaciones monitoreadas</span>
                        <span class="indicador-valor"><asp:Label ID="lblPublicaciones" runat="server"></asp:Label></span>
                    </div>

                    <div class="indicador">
                        <span class="indicador-titulo">Más caros que la competencia</span>
                        <span class="indicador-valor"><asp:Label ID="lblMasCaros" runat="server"></asp:Label></span>
                    </div>

                    <div class="indicador">
                        <span class="indicador-titulo">Más baratos que la competencia</span>
                        <span class="indicador-valor"><asp:Label ID="lblMasBaratos" runat="server"></asp:Label></span>
                    </div>

                    <div class="indicador">
                        <span class="indicador-titulo">Diferencia promedio</span>
                        <span class="indicador-valor"><asp:Label ID="lblDiferenciaPromedio" runat="server"></asp:Label></span>
                    </div>
                </div>
            </div>
        </section>

        <section class="strategic-card">
            <div class="strategic-card-header">
                <h2>Comparación de precios</h2>
                <asp:Label ID="lblCantidad" runat="server" CssClass="strategic-card-meta"></asp:Label>
            </div>

            <div class="strategic-card-body sin-padding">
                <sc:GrillaGenerica ID="grillaComparacion" runat="server" OnAccionSeleccionada="grillaComparacion_AccionSeleccionada" />
            </div>
        </section>

        <asp:Panel ID="pnlDetalle" runat="server" Visible="false">

            <section class="strategic-card">
                <div class="strategic-card-header">
                    <h2>Historial de la publicación</h2>
                    <asp:Label ID="lblDetalleTitulo" runat="server" CssClass="strategic-card-meta"></asp:Label>
                </div>

                <div class="strategic-card-body">
                    <dl class="detalle-grid">
                        <div class="detalle-item">
                            <dt>Producto propio</dt>
                            <dd><asp:Label ID="lblDetalleProducto" runat="server"></asp:Label></dd>
                        </div>

                        <div class="detalle-item">
                            <dt>Competidor</dt>
                            <dd><asp:Label ID="lblDetalleCompetidor" runat="server"></asp:Label></dd>
                        </div>

                        <div class="detalle-item">
                            <dt>Marketplace</dt>
                            <dd><asp:Label ID="lblDetalleMarketplace" runat="server"></asp:Label></dd>
                        </div>

                        <div class="detalle-item">
                            <dt>Precio propio</dt>
                            <dd><asp:Label ID="lblDetallePrecioPropio" runat="server"></asp:Label></dd>
                        </div>

                        <div class="detalle-item">
                            <dt>Precio de la competencia</dt>
                            <dd><asp:Label ID="lblDetallePrecioCompetencia" runat="server"></asp:Label></dd>
                        </div>

                        <div class="detalle-item">
                            <dt>Diferencia</dt>
                            <dd><asp:Label ID="lblDetalleDiferencia" runat="server"></asp:Label></dd>
                        </div>

                        <div class="detalle-item">
                            <dt>Última consulta</dt>
                            <dd><asp:Label ID="lblDetalleUltimaConsulta" runat="server"></asp:Label></dd>
                        </div>

                        <div class="detalle-item">
                            <dt>Publicación</dt>
                            <dd>
                                <asp:HyperLink ID="lnkPublicacion" runat="server" CssClass="enlace-publicacion" Target="_blank" rel="noopener noreferrer" Visible="false"></asp:HyperLink>
                                <asp:Label ID="lblPublicacion" runat="server" Visible="false"></asp:Label>
                            </dd>
                        </div>
                    </dl>

                    <asp:Panel ID="pnlGraficoHistorial" runat="server" CssClass="grafico-contenedor" Visible="false">
                        <canvas id="graficoComparacion"></canvas>
                    </asp:Panel>

                    <asp:Panel ID="pnlSinHistorial" runat="server" CssClass="estado-vacio">
                        <span class="estado-vacio-icono">
                            <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false">
                                <path d="M4 19V5"></path>
                                <path d="M4 19h16"></path>
                                <path d="M7 15l4-5 3 3 4-6"></path>
                            </svg>
                        </span>
                        <p><asp:Label ID="lblSinHistorial" runat="server"></asp:Label></p>
                    </asp:Panel>

                    <div class="detalle-acciones">
                        <asp:Button ID="btnCerrarDetalle" runat="server" Text="Cerrar historial" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnCerrarDetalle_Click" />
                    </div>
                </div>
            </section>

            <section class="strategic-card">
                <div class="strategic-card-header">
                    <h2>Detalle del historial</h2>
                    <asp:Label ID="lblCantidadHistorial" runat="server" CssClass="strategic-card-meta"></asp:Label>
                </div>

                <div class="strategic-card-body sin-padding">
                    <sc:GrillaGenerica ID="grillaHistorial" runat="server" />
                </div>
            </section>

        </asp:Panel>

    </main>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ScriptsFinales" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.1/dist/chart.umd.min.js"></script>
    <script src="<%: ResolveUrl("~/Scripts/strategic-graficos.js") %>"></script>
    <script type="text/javascript">
        Strategic.grafico.lineas('graficoComparacion', <%= DatosHistorialJson %>, '$');
    </script>
</asp:Content>
