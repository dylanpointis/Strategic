<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ConsultarEventosSistema.aspx.cs" Inherits="Strategic.ConsultarEventosSistema" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <main class="page-section">

        <div class="page-heading">
            <h1><%: Traductor.T("eventos.titulo") %></h1>
            <p><%: Traductor.T("eventos.subtitulo") %></p>
        </div>

        <section class="strategic-card">
            <div class="strategic-card-header">
                <h2><%: Traductor.T("comun.filtrosBusqueda") %></h2>
            </div>

            <div class="strategic-card-body">
                <div class="filtros-grid">
                    <div class="filtro">
                        <asp:Label ID="lblUsuario" runat="server" AssociatedControlID="txtUsuario" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control" MaxLength="50"></asp:TextBox>
                    </div>

                    <div class="filtro">
                        <asp:Label ID="lblModulo" runat="server" AssociatedControlID="ddlModulo" CssClass="form-label"></asp:Label>
                        <asp:DropDownList ID="ddlModulo" runat="server" CssClass="form-select"></asp:DropDownList>
                    </div>

                    <div class="filtro">
                        <asp:Label ID="lblEvento" runat="server" AssociatedControlID="txtEvento" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtEvento" runat="server" CssClass="form-control" MaxLength="100"></asp:TextBox>
                    </div>

                    <sc:FiltroFechas ID="filtroFechas" runat="server" MostrarBoton="false" />
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
                <h2><%: Traductor.T("eventos.listado") %></h2>
                <asp:Label ID="lblCantidad" runat="server" CssClass="strategic-card-meta"></asp:Label>
            </div>

            <div class="strategic-card-body sin-padding">
                <sc:GrillaGenerica ID="grillaEventos" runat="server" OnAccionSeleccionada="grillaEventos_AccionSeleccionada" />
            </div>
        </section>

        <asp:Panel ID="pnlDetalleUsuario" runat="server" CssClass="strategic-card" Visible="false">
            <div class="strategic-card-header">
                <h2><%: Traductor.T("eventos.usuarioQueEjecuto") %></h2>
            </div>

            <div class="strategic-card-body">
                <dl class="detalle-grid">
                    <div class="detalle-item">
                        <dt><%: Traductor.T("usuario.usuario") %></dt>
                        <dd><asp:Label ID="lblNombreUsuario" runat="server"></asp:Label></dd>
                    </div>

                    <div class="detalle-item">
                        <dt><%: Traductor.T("usuario.nombreApellido") %></dt>
                        <dd><asp:Label ID="lblNombreApellido" runat="server"></asp:Label></dd>
                    </div>

                    <div class="detalle-item">
                        <dt><%: Traductor.T("usuario.email") %></dt>
                        <dd><asp:Label ID="lblEmail" runat="server"></asp:Label></dd>
                    </div>

                    <div class="detalle-item">
                        <dt><%: Traductor.T("usuario.rol") %></dt>
                        <dd><asp:Label ID="lblRol" runat="server"></asp:Label></dd>
                    </div>

                    <div class="detalle-item">
                        <dt><%: Traductor.T("comun.estado") %></dt>
                        <dd><asp:Label ID="lblEstado" runat="server"></asp:Label></dd>
                    </div>
                </dl>

                <div class="detalle-acciones">
                    <asp:Button ID="btnCerrarDetalle" runat="server" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnCerrarDetalle_Click" />
                </div>
            </div>
        </asp:Panel>

    </main>
</asp:Content>
