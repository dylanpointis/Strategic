<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ConsultarAutomatizaciones.aspx.cs" Inherits="Strategic.ConsultarAutomatizaciones" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <main class="page-section">

        <div class="page-heading">
            <h1><%: Traductor.T("consultarAutomatizaciones.titulo") %></h1>
            <p><%: Traductor.T("consultarAutomatizaciones.subtitulo") %></p>
        </div>

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
                        </asp:DropDownList>
                    </div>

                    <div class="filtro">
                        <asp:Label ID="lblEstado" runat="server" AssociatedControlID="ddlEstado" CssClass="form-label"></asp:Label>
                        <asp:DropDownList ID="ddlEstado" runat="server" CssClass="form-select">
                            <asp:ListItem Value=""></asp:ListItem>
                            <asp:ListItem Value="Activa"></asp:ListItem>
                            <asp:ListItem Value="Pausada"></asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>

                <div class="filtros-acciones">
                    <asp:Button ID="btnFiltrar" runat="server" CssClass="btn btn-strategic" OnClick="btnFiltrar_Click" />
                    <asp:Button ID="btnLimpiar" runat="server" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnLimpiar_Click" />
                </div>

                <asp:Label ID="lblError" runat="server" CssClass="mensaje-error" EnableViewState="false"></asp:Label>
                <asp:Label ID="lblExito" runat="server" CssClass="mensaje-exito" EnableViewState="false"></asp:Label>
            </div>
        </section>

        <asp:Panel ID="pnlConfirmarEjecucion" runat="server" CssClass="strategic-card" Visible="false">
            <div class="strategic-card-header">
                <h2><%: Traductor.T("comun.confirmarAccion") %></h2>
            </div>

            <div class="strategic-card-body">
                <div class="confirmacion">
                    <p><%: Traductor.T("consultarAutomatizaciones.confirmarEjecucion") %></p>
                    <asp:Button ID="btnConfirmarEjecucion" runat="server" CssClass="btn btn-strategic" CausesValidation="false" OnClick="btnConfirmarEjecucion_Click" />
                    <asp:Button ID="btnCancelarEjecucion" runat="server" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnCancelarEjecucion_Click" />
                </div>
            </div>
        </asp:Panel>

        <section class="strategic-card">
            <div class="strategic-card-header">
                <h2><%: Traductor.T("consultarAutomatizaciones.listado") %></h2>
                <div class="strategic-card-acciones">
                    <asp:Label ID="lblCantidad" runat="server" CssClass="strategic-card-meta"></asp:Label>
                    <asp:Button ID="btnEjecutar" runat="server" CssClass="btn btn-strategic" CausesValidation="false" OnClick="btnEjecutar_Click" />
                </div>
            </div>

            <div class="strategic-card-body sin-padding">
                <sc:GrillaGenerica ID="grillaAutomatizaciones" runat="server" OnAccionSeleccionada="grillaAutomatizaciones_AccionSeleccionada" />
            </div>
        </section>

    </main>
</asp:Content>
