<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SelectorPermisos.ascx.cs" Inherits="Strategic.Componentes.SelectorPermisos" %>

<%-- Todo el selector va en un mismo recuadro: es un unico control --%>
<div class="componente-grupo">
    <asp:Label ID="lblTitulo" runat="server" CssClass="componente-titulo"></asp:Label>

    <div class="selector-permisos">

        <div class="selector-columna">
            <asp:Label ID="lblTituloDisponibles" runat="server" AssociatedControlID="lstDisponibles" CssClass="form-label"></asp:Label>
            <asp:ListBox ID="lstDisponibles" runat="server" CssClass="form-select selector-lista" Rows="12"></asp:ListBox>
            <asp:Label ID="lblAyudaDisponibles" runat="server" CssClass="campo-ayuda"></asp:Label>

            <%-- Contenido de cada familia; el script muestra el de la familia elegida --%>
            <div id="divDetalleFamilia" runat="server" class="selector-detalle" enableviewstate="false"></div>
        </div>

        <div class="selector-acciones">
            <asp:Button ID="btnAgregar" runat="server" CssClass="btn btn-strategic" CausesValidation="false" OnClick="btnAgregar_Click" />
            <asp:Button ID="btnQuitar" runat="server" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnQuitar_Click" />
        </div>

        <div class="selector-columna">
            <asp:Label ID="lblTituloSeleccionados" runat="server" AssociatedControlID="lstSeleccionados" CssClass="form-label"></asp:Label>
            <asp:ListBox ID="lstSeleccionados" runat="server" CssClass="form-select selector-lista" Rows="12"></asp:ListBox>
            <asp:Label ID="lblResumen" runat="server" CssClass="campo-ayuda"></asp:Label>
        </div>

    </div>

    <asp:Label ID="lblMensaje" runat="server" CssClass="mensaje-aviso" EnableViewState="false"></asp:Label>

</div>
