<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="HistorialAutomatizaciones.aspx.cs" Inherits="Strategic.HistorialAutomatizaciones" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <main class="page-section">

        <div class="page-heading">
            <h1><%: Traductor.T("historialAutomatizaciones.titulo") %></h1>
            <p><%: Traductor.T("historialAutomatizaciones.subtitulo") %></p>
        </div>

        <section class="strategic-card">
            <div class="strategic-card-header">
                <h2><%: Traductor.T("comun.filtrosBusqueda") %></h2>
            </div>

            <div class="strategic-card-body">
                <div class="filtros-grid">
                    <sc:FiltroFechas ID="filtroFechas" runat="server" MostrarBoton="false" />

                    <div class="filtro">
                        <asp:Label ID="lblAutomatizacion" runat="server" AssociatedControlID="ddlAutomatizacion" CssClass="form-label"></asp:Label>
                        <asp:DropDownList ID="ddlAutomatizacion" runat="server" CssClass="form-select">
                            <asp:ListItem Value=""></asp:ListItem>
                        </asp:DropDownList>
                    </div>

                    <div class="filtro">
                        <sc:SelectorProducto ID="selectorProducto" runat="server" TextoOpcionVacia="comun.todos" />
                    </div>

                    <div class="filtro">
                        <asp:Label ID="lblResultado" runat="server" AssociatedControlID="ddlResultado" CssClass="form-label"></asp:Label>
                        <asp:DropDownList ID="ddlResultado" runat="server" CssClass="form-select">
                            <asp:ListItem Value=""></asp:ListItem>
                            <asp:ListItem Value="Exitosa"></asp:ListItem>
                            <asp:ListItem Value="ConError"></asp:ListItem>
                        </asp:DropDownList>
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
                <h2><%: Traductor.T("historialAutomatizaciones.listado") %></h2>
                <asp:Label ID="lblCantidad" runat="server" CssClass="strategic-card-meta"></asp:Label>
            </div>

            <div class="strategic-card-body sin-padding">
                <sc:GrillaGenerica ID="grillaHistorial" runat="server" OnAccionSeleccionada="grillaHistorial_AccionSeleccionada" />
            </div>
        </section>

        <asp:Panel ID="pnlDetalle" runat="server" CssClass="strategic-card" Visible="false">
            <div class="strategic-card-header">
                <h2><%: Traductor.T("historialAutomatizaciones.detalle") %></h2>
                <asp:Label ID="lblDetalleResultado" runat="server" CssClass="strategic-card-meta"></asp:Label>
            </div>

            <div class="strategic-card-body">
                <dl class="detalle-grid">
                    <div class="detalle-item">
                        <dt><%: Traductor.T("comun.fecha") %></dt>
                        <dd><asp:Label ID="lblDetalleFecha" runat="server"></asp:Label></dd>
                    </div>

                    <div class="detalle-item">
                        <dt><%: Traductor.T("automatizacion.automatizacion") %></dt>
                        <dd><asp:Label ID="lblDetalleAutomatizacion" runat="server"></asp:Label></dd>
                    </div>

                    <div class="detalle-item">
                        <dt><%: Traductor.T("monitoreo.producto") %></dt>
                        <dd><asp:Label ID="lblDetalleProducto" runat="server"></asp:Label></dd>
                    </div>

                    <div class="detalle-item">
                        <dt><%: Traductor.T("automatizacion.accion") %></dt>
                        <dd><asp:Label ID="lblDetalleAccion" runat="server"></asp:Label></dd>
                    </div>

                    <div class="detalle-item">
                        <dt><%: Traductor.T("automatizacion.valorAnterior") %></dt>
                        <dd><asp:Label ID="lblDetalleValorAnterior" runat="server"></asp:Label></dd>
                    </div>

                    <div class="detalle-item">
                        <dt><%: Traductor.T("automatizacion.valorNuevo") %></dt>
                        <dd><asp:Label ID="lblDetalleValorNuevo" runat="server"></asp:Label></dd>
                    </div>
                </dl>

                <div class="formulario-acciones">
                    <asp:Button ID="btnCerrarDetalle" runat="server" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnCerrarDetalle_Click" />
                </div>
            </div>
        </asp:Panel>

    </main>
</asp:Content>
