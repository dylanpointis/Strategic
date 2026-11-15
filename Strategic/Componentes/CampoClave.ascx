<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CampoClave.ascx.cs" Inherits="Strategic.Componentes.CampoClave" %>

<%-- Campo de clave con boton para mostrarla, requisitos que se marcan mientras
     se escribe y, si se pide, el campo de confirmacion --%>
<div id="divCampoClave" runat="server" class="campo-clave">
    <asp:Label ID="lblEtiqueta" runat="server" AssociatedControlID="txtClave" CssClass="form-label"></asp:Label>
    <div class="campo-clave-entrada">
        <asp:TextBox ID="txtClave" runat="server" TextMode="Password" CssClass="form-control" MaxLength="100" data-campo-clave="principal"></asp:TextBox>
        <button type="button" class="campo-clave-ver" data-ver="principal"></button>
    </div>
    <asp:RequiredFieldValidator ID="valClave" runat="server" ControlToValidate="txtClave" Display="Dynamic" CssClass="campo-error"></asp:RequiredFieldValidator>
    <%-- Misma politica que BLLUsuario.ValidarClave: 8 caracteres, una mayuscula, un numero y un caracter especial.
         En el navegador ya la muestra la lista de requisitos, por eso valida solo en el servidor --%>
    <asp:RegularExpressionValidator ID="valFormato" runat="server" ControlToValidate="txtClave" Display="Dynamic" CssClass="campo-error" EnableClientScript="false"
        ValidationExpression="^(?=.*[A-ZÁÉÍÓÚÜÑ])(?=.*[0-9])(?=.*[^A-Za-z0-9ÁÉÍÓÚÜÑáéíóúüñ]).{8,}$"></asp:RegularExpressionValidator>

    <asp:Panel ID="pnlRequisitos" runat="server" CssClass="campo-clave-requisitos">
        <asp:Label ID="lblRequisitos" runat="server" CssClass="campo-clave-requisitos-titulo"></asp:Label>
        <ul>
            <li data-regla="largo"><asp:Label ID="lblReglaLargo" runat="server"></asp:Label></li>
            <li data-regla="mayuscula"><asp:Label ID="lblReglaMayuscula" runat="server"></asp:Label></li>
            <li data-regla="numero"><asp:Label ID="lblReglaNumero" runat="server"></asp:Label></li>
            <li data-regla="especial"><asp:Label ID="lblReglaEspecial" runat="server"></asp:Label></li>
        </ul>
    </asp:Panel>

    <asp:Panel ID="pnlConfirmacion" runat="server" CssClass="campo-clave-confirmacion">
        <asp:Label ID="lblConfirmacion" runat="server" AssociatedControlID="txtConfirmacion" CssClass="form-label"></asp:Label>
        <div class="campo-clave-entrada">
            <asp:TextBox ID="txtConfirmacion" runat="server" TextMode="Password" CssClass="form-control" MaxLength="100" autocomplete="new-password" data-campo-clave="confirmacion"></asp:TextBox>
            <button type="button" class="campo-clave-ver" data-ver="confirmacion"></button>
        </div>
        <asp:RequiredFieldValidator ID="valConfirmacion" runat="server" ControlToValidate="txtConfirmacion" Display="Dynamic" CssClass="campo-error"></asp:RequiredFieldValidator>
        <%-- En el navegador la coincidencia la avisa el script, por eso valida solo en el servidor --%>
        <asp:CompareValidator ID="valCoinciden" runat="server" ControlToValidate="txtConfirmacion" ControlToCompare="txtClave"
            Operator="Equal" Type="String" Display="Dynamic" CssClass="campo-error" EnableClientScript="false"></asp:CompareValidator>
        <span class="campo-clave-coincide" data-coincide=""></span>
    </asp:Panel>
</div>
