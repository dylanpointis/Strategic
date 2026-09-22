<%@ Page Title="Cambiar clave" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CambiarClave.aspx.cs" Inherits="Strategic.CambiarClave" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        .auth-card {
            text-align: center;
        }

        .auth-card .mb-3,
        .auth-card .mb-4,
        .auth-card .campo-ayuda,
        .auth-card .mensaje-error,
        .auth-card .mensaje-exito {
            width: 280px;
            margin-left: auto;
            margin-right: auto;
            text-align: left;
        }

        .auth-card .campo-ayuda {
            display: block;
            margin-bottom: 20px;
        }

        .auth-actions {
            display: flex;
            justify-content: center;
            gap: 10px;
        }
    </style>

    <main class="login-page">
        <section class="auth-card">
            <div class="login-heading">
                <h1>Cambiar clave</h1>
                <p>Actualiza la clave de acceso de tu usuario.</p>
            </div>

            <div class="mb-3">
                <asp:Label ID="lblClaveActual" runat="server" Text="Clave actual" AssociatedControlID="txtClaveActual" CssClass="form-label"></asp:Label>
                <asp:TextBox ID="txtClaveActual" runat="server" TextMode="Password" CssClass="form-control" MaxLength="100"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvClaveActual" runat="server" ControlToValidate="txtClaveActual" ErrorMessage="Debe escribir la clave actual" CssClass="text-danger" Display="Dynamic"></asp:RequiredFieldValidator>
            </div>

            <div class="mb-3">
                <asp:Label ID="lblClaveNueva" runat="server" Text="Nueva clave" AssociatedControlID="txtClaveNueva" CssClass="form-label"></asp:Label>
                <asp:TextBox ID="txtClaveNueva" runat="server" TextMode="Password" CssClass="form-control" MaxLength="100"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvClaveNueva" runat="server" ControlToValidate="txtClaveNueva" ErrorMessage="Debe escribir la nueva clave" CssClass="text-danger" Display="Dynamic"></asp:RequiredFieldValidator>
            </div>

            <div class="mb-3">
                <asp:Label ID="lblConfirmarClave" runat="server" Text="Confirmar nueva clave" AssociatedControlID="txtConfirmarClave" CssClass="form-label"></asp:Label>
                <asp:TextBox ID="txtConfirmarClave" runat="server" TextMode="Password" CssClass="form-control" MaxLength="100"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvConfirmarClave" runat="server" ControlToValidate="txtConfirmarClave" ErrorMessage="Debe confirmar la nueva clave" CssClass="text-danger" Display="Dynamic"></asp:RequiredFieldValidator>
            </div>

            <span class="campo-ayuda">La nueva clave debe tener al menos 8 caracteres, una mayúscula, un número y un carácter especial, y ser distinta de la anterior.</span>

            <div class="auth-actions">
                <asp:Button ID="btnGuardar" runat="server" Text="Guardar" CssClass="btn btn-strategic" OnClick="btnGuardar_Click" />
                <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnCancelar_Click" />
            </div>

            <asp:Label ID="lblError" runat="server" CssClass="mensaje-error" EnableViewState="false"></asp:Label>
            <asp:Label ID="lblExito" runat="server" CssClass="mensaje-exito" EnableViewState="false"></asp:Label>
        </section>
    </main>
</asp:Content>
