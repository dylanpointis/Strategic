<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SelectorProducto.ascx.cs" Inherits="Strategic.Componentes.SelectorProducto" %>

<asp:Label ID="lblEtiqueta" runat="server" AssociatedControlID="ddlProducto" CssClass="form-label"></asp:Label>
<asp:TextBox ID="txtBuscar" runat="server" CssClass="form-control form-control-sm selector-buscador" MaxLength="100" autocomplete="off"></asp:TextBox>
<div class="selector-desplegable">
    <asp:DropDownList ID="ddlProducto" runat="server" CssClass="form-select" OnSelectedIndexChanged="ddlProducto_SelectedIndexChanged"></asp:DropDownList>
    <asp:Label ID="lblCoincidencias" runat="server" CssClass="campo-ayuda selector-aviso"></asp:Label>
</div>
<asp:RequiredFieldValidator ID="valProducto" runat="server" ControlToValidate="ddlProducto"
    InitialValue="" Display="Dynamic" CssClass="campo-error" Enabled="false"></asp:RequiredFieldValidator>
