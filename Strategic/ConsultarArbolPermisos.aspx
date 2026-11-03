<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ConsultarArbolPermisos.aspx.cs" Inherits="Strategic.ConsultarArbolPermisos" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <main class="page-section">

        <div class="page-heading">
            <h1><%: Traductor.T("consultarArbolPermisos.titulo") %></h1>
            <p><%: Traductor.T("consultarArbolPermisos.subtitulo") %></p>
        </div>

        <asp:Label ID="lblError" runat="server" CssClass="mensaje-error" EnableViewState="false"></asp:Label>
        <asp:Label ID="lblSinRoles" runat="server" CssClass="mensaje-aviso" EnableViewState="false"></asp:Label>

        <%-- Una tarjeta por rol, de solo lectura: el ABM esta en ConsultarRoles --%>
        <asp:Repeater ID="rptRoles" runat="server" OnItemDataBound="rptRoles_ItemDataBound">
            <ItemTemplate>
                <section class="strategic-card">
                    <div class="strategic-card-header">
                        <h2><asp:Label ID="lblNombre" runat="server"></asp:Label></h2>
                        <asp:Label ID="lblResumen" runat="server" CssClass="strategic-card-meta"></asp:Label>
                    </div>

                    <div class="strategic-card-body">
                        <h3 class="subtitulo-seccion"><asp:Label ID="lblTituloFamilias" runat="server"></asp:Label></h3>
                        <div id="divFamilias" runat="server" class="arbol-permisos"></div>
                        <asp:Label ID="lblSinFamilias" runat="server" CssClass="campo-ayuda"></asp:Label>

                        <h3 class="subtitulo-seccion"><asp:Label ID="lblTituloSueltos" runat="server"></asp:Label></h3>
                        <div id="divSueltos" runat="server" class="arbol-permisos"></div>
                        <asp:Label ID="lblSinSueltos" runat="server" CssClass="campo-ayuda"></asp:Label>
                    </div>
                </section>
            </ItemTemplate>
        </asp:Repeater>

    </main>
</asp:Content>
