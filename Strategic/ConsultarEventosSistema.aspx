<%@ Page Title="Eventos del sistema" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ConsultarEventosSistema.aspx.cs" Inherits="Strategic.ConsultarEventosSistema" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <main class="page-section">

        <div class="page-heading">
            <h1>Eventos del sistema</h1>
            <p>Bitácora de eventos y acciones registradas en el sistema.</p>
        </div>

        <section class="strategic-card">
            <div class="strategic-card-header">
                <h2>Filtros de búsqueda</h2>
            </div>

            <div class="strategic-card-body">
                <div class="filtros-grid">
                    <div class="filtro">
                        <asp:Label ID="lblUsuario" runat="server" Text="Usuario" AssociatedControlID="txtUsuario" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control" MaxLength="50" placeholder="Todos"></asp:TextBox>
                    </div>

                    <div class="filtro">
                        <asp:Label ID="lblModulo" runat="server" Text="Módulo" AssociatedControlID="ddlModulo" CssClass="form-label"></asp:Label>
                        <asp:DropDownList ID="ddlModulo" runat="server" CssClass="form-select"></asp:DropDownList>
                    </div>

                    <div class="filtro">
                        <asp:Label ID="lblEvento" runat="server" Text="Evento" AssociatedControlID="txtEvento" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtEvento" runat="server" CssClass="form-control" MaxLength="100" placeholder="Todos"></asp:TextBox>
                    </div>

                    <div class="filtro">
                        <asp:Label ID="lblFechaDesde" runat="server" Text="Fecha desde" AssociatedControlID="txtFechaDesde" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtFechaDesde" runat="server" TextMode="Date" CssClass="form-control"></asp:TextBox>
                    </div>

                    <div class="filtro">
                        <asp:Label ID="lblFechaHasta" runat="server" Text="Fecha hasta" AssociatedControlID="txtFechaHasta" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtFechaHasta" runat="server" TextMode="Date" CssClass="form-control"></asp:TextBox>
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
                <h2>Listado de eventos</h2>
                <asp:Label ID="lblCantidad" runat="server" CssClass="strategic-card-meta" EnableViewState="false"></asp:Label>
            </div>

            <div class="strategic-card-body sin-padding">
                <div class="tabla-scroll">
                    <asp:GridView ID="gvEventos" runat="server" AutoGenerateColumns="false" CssClass="strategic-table"
                        GridLines="None" UseAccessibleHeader="true" DataKeyNames="CodEvento"
                        OnRowCommand="gvEventos_RowCommand">
                        <Columns>
                            <asp:TemplateField HeaderText="Código" ItemStyle-CssClass="celda-codigo">
                                <ItemTemplate><%# Eval("CodEvento") %></ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Fecha" HeaderText="Fecha" ItemStyle-CssClass="celda-fecha" />
                            <asp:BoundField DataField="Hora" HeaderText="Hora" ItemStyle-CssClass="celda-fecha" />
                            <asp:BoundField DataField="NombreUsuario" HeaderText="Usuario" ItemStyle-CssClass="celda-usuario" />
                            <asp:BoundField DataField="Modulo" HeaderText="Módulo" />
                            <asp:BoundField DataField="Descripcion" HeaderText="Evento" />
                            <asp:BoundField DataField="Criticidad" HeaderText="Criticidad" />
                            <asp:TemplateField HeaderText="Detalle">
                                <ItemTemplate>
                                    <asp:LinkButton ID="lnkVerUsuario" runat="server" CssClass="btn-fila"
                                        Text="Ver usuario" CausesValidation="false"
                                        CommandName="VerUsuario" CommandArgument='<%# Eval("NombreUsuario") %>'></asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>

                <asp:Panel ID="pnlSinResultados" runat="server" CssClass="estado-vacio" Visible="false">
                    <span class="estado-vacio-icono">
                        <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false">
                            <circle cx="11" cy="11" r="7"></circle>
                            <path d="M20 20l-3.5-3.5"></path>
                        </svg>
                    </span>
                    <p><asp:Literal ID="litSinResultados" runat="server"></asp:Literal></p>
                </asp:Panel>
            </div>
        </section>

        <asp:Panel ID="pnlDetalleUsuario" runat="server" CssClass="strategic-card" Visible="false">
            <div class="strategic-card-header">
                <h2>Usuario que ejecutó el evento</h2>
            </div>

            <div class="strategic-card-body">
                <dl class="detalle-grid">
                    <div class="detalle-item">
                        <dt>Usuario</dt>
                        <dd><asp:Literal ID="litNombreUsuario" runat="server"></asp:Literal></dd>
                    </div>

                    <div class="detalle-item">
                        <dt>Nombre y apellido</dt>
                        <dd><asp:Literal ID="litNombreApellido" runat="server"></asp:Literal></dd>
                    </div>

                    <div class="detalle-item">
                        <dt>Email</dt>
                        <dd><asp:Literal ID="litEmail" runat="server"></asp:Literal></dd>
                    </div>

                    <div class="detalle-item">
                        <dt>Rol</dt>
                        <dd><asp:Literal ID="litRol" runat="server"></asp:Literal></dd>
                    </div>

                    <div class="detalle-item">
                        <dt>Estado</dt>
                        <dd><asp:Literal ID="litEstado" runat="server"></asp:Literal></dd>
                    </div>
                </dl>

                <div class="detalle-acciones">
                    <asp:Button ID="btnCerrarDetalle" runat="server" Text="Cerrar detalle" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnCerrarDetalle_Click" />
                </div>
            </div>
        </asp:Panel>

    </main>
</asp:Content>
