<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="GrillaGenerica.ascx.cs" Inherits="Strategic.Componentes.GrillaGenerica" %>

<div class="grilla-generica">

    <div class="tabla-scroll">
        <asp:GridView ID="gvDatos" runat="server" AutoGenerateColumns="false" CssClass="strategic-table"
            GridLines="None" UseAccessibleHeader="true" AllowPaging="true" PageSize="10"
            OnRowCommand="gvDatos_RowCommand">
            <PagerSettings Visible="false" />
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

    <asp:Panel ID="pnlPaginado" runat="server" CssClass="grilla-paginado" Visible="false">

        <div class="grilla-paginado-info">
            <asp:Literal ID="litRango" runat="server"></asp:Literal>
        </div>

        <div class="grilla-paginado-controles">
            <asp:LinkButton ID="lnkPrimera" runat="server" CssClass="pagina-boton" Text="&#171;"
                ToolTip="Primera pagina" CausesValidation="false"
                CommandName="Primera" OnCommand="Paginado_Command"></asp:LinkButton>

            <asp:LinkButton ID="lnkAnterior" runat="server" CssClass="pagina-boton" Text="&#8249;"
                ToolTip="Pagina anterior" CausesValidation="false"
                CommandName="Anterior" OnCommand="Paginado_Command"></asp:LinkButton>

            <asp:Repeater ID="rptPaginas" runat="server">
                <ItemTemplate>
                    <asp:LinkButton ID="lnkPagina" runat="server" CausesValidation="false"
                        CssClass='<%# ClasePagina(Container.DataItem) %>'
                        Text='<%# TextoPagina(Container.DataItem) %>'
                        CommandName="Ir" CommandArgument='<%# Container.DataItem %>'
                        OnCommand="Paginado_Command"></asp:LinkButton>
                </ItemTemplate>
            </asp:Repeater>

            <asp:LinkButton ID="lnkSiguiente" runat="server" CssClass="pagina-boton" Text="&#8250;"
                ToolTip="Pagina siguiente" CausesValidation="false"
                CommandName="Siguiente" OnCommand="Paginado_Command"></asp:LinkButton>

            <asp:LinkButton ID="lnkUltima" runat="server" CssClass="pagina-boton" Text="&#187;"
                ToolTip="Ultima pagina" CausesValidation="false"
                CommandName="Ultima" OnCommand="Paginado_Command"></asp:LinkButton>
        </div>

        <asp:Panel ID="pnlFilasPorPagina" runat="server" CssClass="grilla-paginado-filas">
            <asp:Label ID="lblFilasPorPagina" runat="server" Text="Filas por pagina"
                AssociatedControlID="ddlFilasPorPagina" CssClass="grilla-paginado-etiqueta"></asp:Label>

            <asp:DropDownList ID="ddlFilasPorPagina" runat="server" CssClass="form-select" AutoPostBack="true"
                CausesValidation="false" OnSelectedIndexChanged="ddlFilasPorPagina_SelectedIndexChanged">
                <asp:ListItem Text="10" Value="10"></asp:ListItem>
                <asp:ListItem Text="25" Value="25"></asp:ListItem>
                <asp:ListItem Text="50" Value="50"></asp:ListItem>
                <asp:ListItem Text="100" Value="100"></asp:ListItem>
            </asp:DropDownList>
        </asp:Panel>

    </asp:Panel>

</div>
