<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Recomendaciones.aspx.cs" Inherits="Strategic.Recomendaciones" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <main class="page-section">

        <div class="page-heading">
            <h1><%: Traductor.T("recomendaciones.titulo") %></h1>
            <p><%: Traductor.T("recomendaciones.subtitulo") %></p>
        </div>

        <asp:Label ID="lblErrorAnalisis" runat="server" CssClass="mensaje-error" EnableViewState="false"></asp:Label>

        <asp:Panel ID="pnlSinDatos" runat="server" CssClass="aviso-sincronizacion" Visible="false">
            <span class="aviso-icono">
                <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false">
                    <path d="M21 12a9 9 0 1 1-3-6.7"></path>
                    <path d="M21 4v5h-5"></path>
                </svg>
            </span>
            <div>
                <strong><asp:Label ID="lblSinDatos" runat="server"></asp:Label></strong>
                <p><%: Traductor.T("recomendaciones.sinDatosAyuda") %></p>
            </div>
            <asp:HyperLink ID="lnkImportarDatos" runat="server" CssClass="btn btn-strategic" NavigateUrl="~/ImportarDatos.aspx"></asp:HyperLink>
        </asp:Panel>

        <%-- Arranca oculto: se muestra cuando el analisis termina bien --%>
        <asp:Panel ID="pnlContenido" runat="server" Visible="false">

            <section class="strategic-card">
                <div class="strategic-card-header">
                    <h2><%: Traductor.T("recomendaciones.resumen") %></h2>
                </div>

                <div class="strategic-card-body">
                    <div class="indicadores-grid">
                        <div class="indicador">
                            <span class="indicador-titulo"><%: Traductor.T("recomendaciones.total") %></span>
                            <span class="indicador-valor"><asp:Label ID="lblTotal" runat="server"></asp:Label></span>
                        </div>

                        <div class="indicador">
                            <span class="indicador-titulo"><%: Traductor.T("recomendaciones.prioridadAlta") %></span>
                            <span class="indicador-valor"><asp:Label ID="lblAlta" runat="server"></asp:Label></span>
                        </div>

                        <div class="indicador">
                            <span class="indicador-titulo"><%: Traductor.T("recomendaciones.prioridadMedia") %></span>
                            <span class="indicador-valor"><asp:Label ID="lblMedia" runat="server"></asp:Label></span>
                        </div>

                        <div class="indicador">
                            <span class="indicador-titulo"><%: Traductor.T("recomendaciones.prioridadBaja") %></span>
                            <span class="indicador-valor"><asp:Label ID="lblBaja" runat="server"></asp:Label></span>
                        </div>
                    </div>
                </div>
            </section>

            <section class="strategic-card">
                <div class="strategic-card-header">
                    <h2><%: Traductor.T("comun.filtrosBusqueda") %></h2>
                </div>

                <div class="strategic-card-body">
                    <div class="filtros-grid">
                        <div class="filtro">
                            <asp:Label ID="lblTipo" runat="server" AssociatedControlID="ddlTipo" CssClass="form-label"></asp:Label>
                            <asp:DropDownList ID="ddlTipo" runat="server" CssClass="form-select">
                                <asp:ListItem Value=""></asp:ListItem>
                                <asp:ListItem Value="Stock"></asp:ListItem>
                                <asp:ListItem Value="Precio"></asp:ListItem>
                                <asp:ListItem Value="Ventas"></asp:ListItem>
                            </asp:DropDownList>
                        </div>

                        <div class="filtro">
                            <asp:Label ID="lblPrioridad" runat="server" AssociatedControlID="ddlPrioridad" CssClass="form-label"></asp:Label>
                            <asp:DropDownList ID="ddlPrioridad" runat="server" CssClass="form-select">
                                <asp:ListItem Value=""></asp:ListItem>
                                <asp:ListItem Value="Alta"></asp:ListItem>
                                <asp:ListItem Value="Media"></asp:ListItem>
                                <asp:ListItem Value="Baja"></asp:ListItem>
                            </asp:DropDownList>
                        </div>

                        <div class="filtro">
                            <asp:Label ID="lblProducto" runat="server" AssociatedControlID="ddlProducto" CssClass="form-label"></asp:Label>
                            <asp:DropDownList ID="ddlProducto" runat="server" CssClass="form-select"></asp:DropDownList>
                        </div>
                    </div>

                    <div class="filtros-acciones">
                        <asp:Button ID="btnFiltrar" runat="server" CssClass="btn btn-strategic" CausesValidation="false" OnClick="btnFiltrar_Click" />
                        <asp:Button ID="btnLimpiar" runat="server" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnLimpiar_Click" />
                    </div>

                    <asp:Label ID="lblError" runat="server" CssClass="mensaje-error" EnableViewState="false"></asp:Label>
                    <asp:Label ID="lblAvisoFiltros" runat="server" CssClass="mensaje-aviso" EnableViewState="false"></asp:Label>
                </div>
            </section>

            <section class="strategic-card">
                <div class="strategic-card-header">
                    <h2><%: Traductor.T("recomendaciones.listado") %></h2>
                    <asp:Label ID="lblCantidad" runat="server" CssClass="strategic-card-meta"></asp:Label>
                </div>

                <div class="strategic-card-body sin-padding">
                    <sc:GrillaGenerica ID="grillaRecomendaciones" runat="server" OnAccionSeleccionada="grillaRecomendaciones_AccionSeleccionada" />
                </div>
            </section>

            <asp:Panel ID="pnlDetalle" runat="server" Visible="false">

                <section class="strategic-card">
                    <div class="strategic-card-header">
                        <h2><asp:Label ID="lblDetalleTitulo" runat="server"></asp:Label></h2>
                        <asp:Label ID="lblDetalleEstado" runat="server" CssClass="strategic-card-meta"></asp:Label>
                    </div>

                    <div class="strategic-card-body">
                        <dl class="detalle-grid">
                            <div class="detalle-item">
                                <dt><%: Traductor.T("monitoreo.producto") %></dt>
                                <dd><asp:Label ID="lblDetalleProducto" runat="server"></asp:Label></dd>
                            </div>

                            <div class="detalle-item">
                                <dt><%: Traductor.T("recomendaciones.tipo") %></dt>
                                <dd><asp:Label ID="lblDetalleTipo" runat="server"></asp:Label></dd>
                            </div>

                            <div class="detalle-item">
                                <dt><%: Traductor.T("recomendaciones.prioridad") %></dt>
                                <dd><asp:Label ID="lblDetallePrioridad" runat="server"></asp:Label></dd>
                            </div>

                            <div class="detalle-item detalle-item-completo">
                                <dt><%: Traductor.T("recomendaciones.descripcion") %></dt>
                                <dd><asp:Label ID="lblDetalleDescripcion" runat="server"></asp:Label></dd>
                            </div>

                            <div class="detalle-item detalle-item-completo">
                                <dt><%: Traductor.T("recomendaciones.justificacion") %></dt>
                                <dd><asp:Label ID="lblDetalleJustificacion" runat="server"></asp:Label></dd>
                            </div>

                            <div class="detalle-item detalle-item-completo">
                                <dt><%: Traductor.T("recomendaciones.accionSugerida") %></dt>
                                <dd><asp:Label ID="lblDetalleAccion" runat="server"></asp:Label></dd>
                            </div>
                        </dl>

                        <asp:Label ID="lblErrorDetalle" runat="server" CssClass="mensaje-error" EnableViewState="false"></asp:Label>
                        <asp:Label ID="lblExito" runat="server" CssClass="mensaje-exito" EnableViewState="false"></asp:Label>

                        <div class="detalle-acciones">
                            <asp:Button ID="btnGuardar" runat="server" CssClass="btn btn-strategic" CausesValidation="false" OnClick="btnGuardar_Click" />
                            <asp:Button ID="btnCerrarDetalle" runat="server" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnCerrarDetalle_Click" />
                        </div>
                    </div>
                </section>

            </asp:Panel>

        </asp:Panel>

    </main>
</asp:Content>
