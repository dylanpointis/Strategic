<%@ Page Title="Cambiar clave" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CambiarClave.aspx.cs" Inherits="Strategic.CambiarClave" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        .auth-card {
            text-align: center;
        }

        .auth-card .mb-3,
        .auth-card .mb-4 {
            width: 280px;
            margin-left: auto;
            margin-right: auto;
            text-align: left;
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
            </div>

            <div class="mb-3">
                <asp:Label ID="lblClaveNueva" runat="server" Text="Nueva clave" AssociatedControlID="txtClaveNueva" CssClass="form-label"></asp:Label>
                <asp:TextBox ID="txtClaveNueva" runat="server" TextMode="Password" CssClass="form-control" MaxLength="100"></asp:TextBox>
            </div>

            <div class="mb-4">
                <asp:Label ID="lblConfirmarClave" runat="server" Text="Confirmar nueva clave" AssociatedControlID="txtConfirmarClave" CssClass="form-label"></asp:Label>
                <asp:TextBox ID="txtConfirmarClave" runat="server" TextMode="Password" CssClass="form-control" MaxLength="100"></asp:TextBox>
            </div>

            <div class="auth-actions">
                <asp:Button ID="btnGuardar" runat="server" Text="Guardar" CssClass="btn btn-primary" Enabled="false" />
                <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" CssClass="btn btn-outline-secondary" CausesValidation="false" OnClick="btnCancelar_Click" />
            </div>
        </section>
    </main>
</asp:Content>
