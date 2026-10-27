<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="HistorialSincronizaciones.aspx.cs" Inherits="Strategic.HistorialSincronizaciones" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <main class="page-section">

        <div class="page-heading">
            <h1><%: Traductor.T("historialSincronizaciones.titulo") %></h1>
            <p><%: Traductor.T("historialSincronizaciones.subtitulo") %></p>
        </div>

        <section class="strategic-card">
            <div class="strategic-card-header">
                <h2><%: Traductor.T("comun.filtrosBusqueda") %></h2>
            </div>

            <div class="strategic-card-body">
                <div class="filtros-grid">
                    <sc:FiltroFechas ID="filtroFechas" runat="server" MostrarBoton="false" />

                    <div class="filtro">
                        <asp:Label ID="lblTipo" runat="server" AssociatedControlID="ddlTipo" CssClass="form-label"></asp:Label>
                        <asp:DropDownList ID="ddlTipo" runat="server" CssClass="form-select"></asp:DropDownList>
                    </div>

                    <div class="filtro">
                        <asp:Label ID="lblEstado" runat="server" AssociatedControlID="ddlEstado" CssClass="form-label"></asp:Label>
                        <asp:DropDownList ID="ddlEstado" runat="server" CssClass="form-select"></asp:DropDownList>
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
                <h2><%: Traductor.T("historialSincronizaciones.listado") %></h2>
                <asp:Label ID="lblCantidad" runat="server" CssClass="strategic-card-meta"></asp:Label>
            </div>

            <div class="strategic-card-body sin-padding">
                <sc:GrillaGenerica ID="grillaHistorial" runat="server" OnAccionSeleccionada="grillaHistorial_AccionSeleccionada" />
            </div>
        </section>

        <asp:Panel ID="pnlDetalle" runat="server" CssClass="strategic-card" Visible="false">
            <div class="strategic-card-header">
                <h2><%: Traductor.T("historialSincronizaciones.detalle") %></h2>
                <asp:Label ID="lblDetalleEstado" runat="server" CssClass="strategic-card-meta"></asp:Label>
            </div>

            <div class="strategic-card-body">
                <dl class="detalle-grid">
                    <div class="detalle-item">
                        <dt><%: Traductor.T("comun.fecha") %></dt>
                        <dd><asp:Label ID="lblDetalleFecha" runat="server"></asp:Label></dd>
                    </div>

                    <div class="detalle-item">
                        <dt><%: Traductor.T("integracion.tipoArchivo") %></dt>
                        <dd><asp:Label ID="lblDetalleTipo" runat="server"></asp:Label></dd>
                    </div>

                    <div class="detalle-item">
                        <dt><%: Traductor.T("integracion.procesados") %></dt>
                        <dd><asp:Label ID="lblDetalleProcesados" runat="server"></asp:Label></dd>
                    </div>

                    <div class="detalle-item">
                        <dt><%: Traductor.T("integracion.importados") %></dt>
                        <dd><asp:Label ID="lblDetalleImportados" runat="server"></asp:Label></dd>
                    </div>

                    <div class="detalle-item">
                        <dt><%: Traductor.T("integracion.actualizados") %></dt>
                        <dd><asp:Label ID="lblDetalleActualizados" runat="server"></asp:Label></dd>
                    </div>

                    <div class="detalle-item">
                        <dt><%: Traductor.T("integracion.conError") %></dt>
                        <dd><asp:Label ID="lblDetalleConError" runat="server"></asp:Label></dd>
                    </div>
                </dl>

                <h3 class="subtitulo-seccion"><%: Traductor.T("integracion.detalleErrores") %></h3>
                <asp:BulletedList ID="blDetalle" runat="server" CssClass="lista-errores"></asp:BulletedList>
                <asp:Label ID="lblSinErrores" runat="server" CssClass="campo-ayuda"></asp:Label>

                <div class="formulario-acciones">
                    <asp:Button ID="btnCerrarDetalle" runat="server" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnCerrarDetalle_Click" />
                </div>
            </div>
        </asp:Panel>

    </main>
</asp:Content>
