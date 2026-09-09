<%@ Page Title="Productos sincronizados" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ProductosSincronizados.aspx.cs" Inherits="Strategic.ProductosSincronizados" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <main class="page-section">

        <div class="page-heading">
            <h1>Productos sincronizados</h1>
            <p>Catálogo de productos importados desde el sistema del cliente.</p>
        </div>

        <section class="strategic-card">
            <div class="strategic-card-header">
                <h2>Filtros de búsqueda</h2>
            </div>

            <div class="strategic-card-body">
                <div class="filtros-grid">
                    <div class="filtro">
                        <asp:Label ID="lblNombre" runat="server" Text="Nombre o código" AssociatedControlID="txtNombre" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control" MaxLength="255" placeholder="Todos"></asp:TextBox>
                    </div>

                    <div class="filtro">
                        <asp:Label ID="lblCategoria" runat="server" Text="Categoría" AssociatedControlID="ddlCategoria" CssClass="form-label"></asp:Label>
                        <asp:DropDownList ID="ddlCategoria" runat="server" CssClass="form-select"></asp:DropDownList>
                    </div>

                    <div class="filtro">
                        <asp:Label ID="lblEstado" runat="server" Text="Estado" AssociatedControlID="ddlEstado" CssClass="form-label"></asp:Label>
                        <asp:DropDownList ID="ddlEstado" runat="server" CssClass="form-select"></asp:DropDownList>
                    </div>

                    <div class="filtro">
                        <asp:Label ID="lblPrecioDesde" runat="server" Text="Precio desde" AssociatedControlID="txtPrecioDesde" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtPrecioDesde" runat="server" TextMode="Number" step="0.01" min="0" CssClass="form-control" placeholder="0,00"></asp:TextBox>
                    </div>

                    <div class="filtro">
                        <asp:Label ID="lblPrecioHasta" runat="server" Text="Precio hasta" AssociatedControlID="txtPrecioHasta" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtPrecioHasta" runat="server" TextMode="Number" step="0.01" min="0" CssClass="form-control" placeholder="Sin tope"></asp:TextBox>
                    </div>
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
                <h2>Listado de productos</h2>
                <asp:Label ID="lblCantidad" runat="server" CssClass="strategic-card-meta" EnableViewState="false"></asp:Label>
            </div>

            <div class="strategic-card-body sin-padding">
                <sc:GrillaGenerica ID="grillaProductos" runat="server" OnAccionSeleccionada="grillaProductos_AccionSeleccionada" />
            </div>
        </section>

        <asp:Panel ID="pnlDetalleProducto" runat="server" CssClass="strategic-card" Visible="false">
            <div class="strategic-card-header">
                <h2>Detalle del producto</h2>
            </div>

            <div class="strategic-card-body">
                <dl class="detalle-grid">
                    <div class="detalle-item">
                        <dt>Código</dt>
                        <dd><asp:Label ID="lblDetalleCodigo" runat="server"></asp:Label></dd>
                    </div>

                    <div class="detalle-item">
                        <dt>Nombre</dt>
                        <dd><asp:Label ID="lblDetalleNombre" runat="server"></asp:Label></dd>
                    </div>

                    <div class="detalle-item">
                        <dt>Marca</dt>
                        <dd><asp:Label ID="lblDetalleMarca" runat="server"></asp:Label></dd>
                    </div>

                    <div class="detalle-item">
                        <dt>Categoría</dt>
                        <dd><asp:Label ID="lblDetalleCategoria" runat="server"></asp:Label></dd>
                    </div>

                    <div class="detalle-item">
                        <dt>Estado</dt>
                        <dd><asp:Label ID="lblDetalleEstado" runat="server"></asp:Label></dd>
                    </div>

                    <div class="detalle-item">
                        <dt>Precio actual</dt>
                        <dd><asp:Label ID="lblDetallePrecio" runat="server"></asp:Label></dd>
                    </div>

                    <div class="detalle-item">
                        <dt>Stock disponible</dt>
                        <dd><asp:Label ID="lblDetalleStock" runat="server"></asp:Label></dd>
                    </div>

                    <div class="detalle-item">
                        <dt>Stock mínimo / máximo</dt>
                        <dd><asp:Label ID="lblDetalleStockMinimoMaximo" runat="server"></asp:Label></dd>
                    </div>

                    <div class="detalle-item">
                        <dt>Última sincronización</dt>
                        <dd><asp:Label ID="lblDetalleFechaSincronizacion" runat="server"></asp:Label></dd>
                    </div>
                </dl>

                <div class="detalle-acciones">
                    <asp:Button ID="btnCerrarDetalle" runat="server" Text="Cerrar detalle" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnCerrarDetalle_Click" />
                </div>
            </div>
        </asp:Panel>

    </main>
</asp:Content>
