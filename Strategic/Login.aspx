<%@ Page Title="Inicio de sesion" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Strategic.Login" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
        <style>
        .login-logo {
            display: flex;
            justify-content: center;
            align-items: center;
            margin-bottom: 25px;
        }

        .strategic-logo {
            display: block;
            width: 200px;
            max-width: 100%;
            height: auto;
        }

        .login-heading {
            text-align: center;
        }
    </style>

    <main class="login-page">
        <section class="login-panel">

            <div class="login-logo">
                <img src="<%= ResolveUrl("~/Content/images/Strategic_LOGO_PNG.png") %>"
                     alt="Strategic"
                     class="strategic-logo" />
            </div>

            <div class="login-heading">
                <h1>Inicio de sesión</h1>
                <p>Ingresa con tu usuario para acceder al sistema.</p>
            </div>

            <div class="mb-3">
                <asp:Label ID="lblNombreUsuario" runat="server" Text="Usuario" AssociatedControlID="txtNombreUsuario" CssClass="form-label"></asp:Label>
                <asp:TextBox ID="txtNombreUsuario" runat="server" CssClass="form-control" MaxLength="50"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvNombreUsuario" runat="server" ControlToValidate="txtNombreUsuario" ErrorMessage="Debe escribir el usuario" CssClass="text-danger" Display="Dynamic"></asp:RequiredFieldValidator>
            </div>

            <div class="mb-3">
                <asp:Label ID="lblClave" runat="server" Text="Clave" AssociatedControlID="txtClave" CssClass="form-label"></asp:Label>
                <asp:TextBox ID="txtClave" runat="server" TextMode="Password" CssClass="form-control" MaxLength="100"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvClave" runat="server" ControlToValidate="txtClave" ErrorMessage="Debe escribir la clave" CssClass="text-danger" Display="Dynamic"></asp:RequiredFieldValidator>
            </div>

            <asp:Button ID="btnLogin" runat="server" Text="Ingresar" CssClass="btn btn-primary w-100" OnClick="btnLogin_Click" />
            <asp:Label ID="lblError" runat="server" CssClass="text-danger login-error" EnableViewState="false"></asp:Label>
        </section>
    </main>
</asp:Content>
