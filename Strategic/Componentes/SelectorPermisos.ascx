<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SelectorPermisos.ascx.cs" Inherits="Strategic.Componentes.SelectorPermisos" %>

<div class="selector-permisos">

    <div class="selector-columna">
        <asp:Label ID="lblTituloDisponibles" runat="server" Text="Permisos y familias disponibles" AssociatedControlID="lstDisponibles" CssClass="form-label"></asp:Label>
        <asp:ListBox ID="lstDisponibles" runat="server" CssClass="form-select selector-lista" Rows="12"></asp:ListBox>
        <asp:Label ID="lblAyudaDisponibles" runat="server" CssClass="campo-ayuda"
            Text="Los permisos simples habilitan una pantalla; las familias agrupan permisos y otras familias."></asp:Label>
    </div>

    <div class="selector-acciones">
        <asp:Button ID="btnAgregar" runat="server" Text="Agregar &#8250;" CssClass="btn btn-strategic" CausesValidation="false" OnClick="btnAgregar_Click" />
        <asp:Button ID="btnQuitar" runat="server" Text="&#8249; Quitar" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnQuitar_Click" />
    </div>

    <div class="selector-columna">
        <asp:Label ID="lblTituloSeleccionados" runat="server" Text="Composición" AssociatedControlID="lstSeleccionados" CssClass="form-label"></asp:Label>
        <asp:ListBox ID="lstSeleccionados" runat="server" CssClass="form-select selector-lista" Rows="12"></asp:ListBox>
        <asp:Label ID="lblResumen" runat="server" CssClass="campo-ayuda"></asp:Label>
    </div>

</div>

<asp:Label ID="lblMensaje" runat="server" CssClass="mensaje-aviso" EnableViewState="false"></asp:Label>
