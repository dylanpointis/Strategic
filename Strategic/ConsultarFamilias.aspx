<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ConsultarFamilias.aspx.cs" Inherits="Strategic.ConsultarFamilias" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <main class="page-section">

        <div class="page-heading">
            <h1><%: Traductor.T("consultarFamilias.titulo") %></h1>
            <p><%: Traductor.T("consultarFamilias.subtitulo") %></p>
        </div>

        <section class="strategic-card">
            <div class="strategic-card-header">
                <h2><%: Traductor.T("comun.filtrosBusqueda") %></h2>
            </div>

            <div class="strategic-card-body">
                <div class="filtros-grid">
                    <div class="filtro">
                        <asp:Label ID="lblNombre" runat="server" AssociatedControlID="txtNombre" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control" MaxLength="80"></asp:TextBox>
                    </div>

                    <div class="filtro">
                        <asp:Label ID="lblEstado" runat="server" AssociatedControlID="ddlEstado" CssClass="form-label"></asp:Label>
                        <asp:DropDownList ID="ddlEstado" runat="server" CssClass="form-select">
                            <asp:ListItem Value="1" Selected="True"></asp:ListItem>
                            <asp:ListItem Value="0"></asp:ListItem>
                            <asp:ListItem Value=""></asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>

                <div class="filtros-acciones">
                    <asp:Button ID="btnFiltrar" runat="server" CssClass="btn btn-strategic" OnClick="btnFiltrar_Click" />
                    <asp:Button ID="btnLimpiar" runat="server" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnLimpiar_Click" />
                </div>

                <asp:Label ID="lblError" runat="server" CssClass="mensaje-error" EnableViewState="false"></asp:Label>
                <asp:Label ID="lblExito" runat="server" CssClass="mensaje-exito" EnableViewState="false"></asp:Label>
                <asp:Label ID="lblAvisoFiltros" runat="server" CssClass="mensaje-aviso" EnableViewState="false"></asp:Label>
            </div>
        </section>

        <section class="strategic-card">
            <div class="strategic-card-header">
                <h2><%: Traductor.T("consultarFamilias.listado") %></h2>
                <div class="strategic-card-acciones">
                    <asp:Label ID="lblCantidad" runat="server" CssClass="strategic-card-meta"></asp:Label>
                    <asp:Button ID="btnNuevo" runat="server" CssClass="btn btn-strategic" CausesValidation="false" OnClick="btnNuevo_Click" />
                </div>
            </div>

            <div class="strategic-card-body sin-padding">
                <sc:GrillaGenerica ID="grillaFamilias" runat="server" OnAccionSeleccionada="grillaFamilias_AccionSeleccionada" />
            </div>
        </section>

        <asp:Panel ID="pnlDetalle" runat="server" CssClass="strategic-card" Visible="false">
            <div class="strategic-card-header">
                <h2><%: Traductor.T("consultarFamilias.composicion") %></h2>
                <asp:Label ID="lblDetalleTitulo" runat="server" CssClass="strategic-card-meta"></asp:Label>
            </div>

            <div class="strategic-card-body">
                <dl class="detalle-grid">
                    <div class="detalle-item">
                        <dt><%: Traductor.T("familia.descripcion") %></dt>
                        <dd><asp:Label ID="lblDetalleDescripcion" runat="server"></asp:Label></dd>
                    </div>

                    <div class="detalle-item">
                        <dt><%: Traductor.T("consultarFamilias.elementosQueContiene") %></dt>
                        <dd><asp:Label ID="lblDetalleComponentes" runat="server"></asp:Label></dd>
                    </div>

                    <div class="detalle-item">
                        <dt><%: Traductor.T("consultarFamilias.pantallasQueHabilita") %></dt>
                        <dd><asp:Label ID="lblDetallePantallas" runat="server"></asp:Label></dd>
                    </div>

                    <div class="detalle-item">
                        <dt><%: Traductor.T("comun.estado") %></dt>
                        <dd><asp:Label ID="lblDetalleEstado" runat="server"></asp:Label></dd>
                    </div>
                </dl>

                <div id="divArbol" runat="server" class="arbol-permisos"></div>

                <div class="detalle-acciones">
                    <asp:Button ID="btnModificar" runat="server" CssClass="btn btn-strategic" CausesValidation="false" OnClick="btnModificar_Click" />
                    <asp:Button ID="btnCambiarEstado" runat="server" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnCambiarEstado_Click" />
                    <asp:Button ID="btnCerrarDetalle" runat="server" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnCerrarDetalle_Click" />
                </div>
            </div>
        </asp:Panel>

    </main>
</asp:Content>
