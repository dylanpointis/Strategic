<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SelectorCompetidor.ascx.cs" Inherits="Strategic.Componentes.SelectorCompetidor" %>

<asp:Label ID="lblEtiqueta" runat="server" AssociatedControlID="ddlCompetidor" CssClass="form-label"></asp:Label>
<asp:TextBox ID="txtBuscar" runat="server" CssClass="form-control form-control-sm selector-buscador" MaxLength="100" autocomplete="off"></asp:TextBox>
<div class="selector-desplegable">
    <asp:DropDownList ID="ddlCompetidor" runat="server" CssClass="form-select" OnSelectedIndexChanged="ddlCompetidor_SelectedIndexChanged"></asp:DropDownList>
    <asp:Label ID="lblCoincidencias" runat="server" CssClass="campo-ayuda selector-aviso"></asp:Label>
</div>
<asp:RequiredFieldValidator ID="valCompetidor" runat="server" ControlToValidate="ddlCompetidor"
    InitialValue="" Display="Dynamic" CssClass="campo-error" Enabled="false"></asp:RequiredFieldValidator>
