<%@ Page Title="Cerrar sesion" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Logout.aspx.cs" Inherits="Strategic.Logout" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        .logout-card {
            max-width: 500px;
            margin: 0 auto;
            padding: 35px 40px;
            text-align: center;
        }

        .login-logo {
            display: flex;
            justify-content: center;
            align-items: center;
            margin-bottom: 25px;
        }

        .strategic-logo {
            display: block;
            width: 180px;
            max-width: 100%;
            height: auto;
        }

        .login-heading {
            text-align: center;
        }

        .login-heading h1 {
            margin-bottom: 10px;
        }

        .login-heading p {
            margin-bottom: 30px;
        }

        .auth-actions {
            display: flex;
            justify-content: center;
            align-items: center;
            gap: 12px;
            width: 100%;
        }

        .auth-actions .btn {
            min-width: 160px;
            padding: 10px 20px;
        }

        @media (max-width: 480px) {
            .logout-card {
                padding: 30px 20px;
            }

            .auth-actions {
                flex-direction: column;
            }

            .auth-actions .btn {
                width: 100%;
                max-width: 280px;
            }
        }
    </style>

    <main class="login-page">
        <section class="auth-card logout-card">
            <div class="login-logo">
                <img src="<%= ResolveUrl("~/Content/images/Strategic_LOGO_PNG.png") %>" alt="Strategic" class="strategic-logo" />
            </div>

            <div class="login-heading">
                <h1>Cerrar sesión</h1>
                <p>¿Estás seguro de que querés cerrar la sesión actual?</p>
            </div>

            <div class="auth-actions">
                <asp:Button ID="btnConfirmarLogout" runat="server" Text="Sí, cerrar sesión" CssClass="btn btn-danger" OnClick="btnConfirmarLogout_Click" />
                <asp:Button ID="btnCancelarLogout" runat="server" Text="Cancelar" CssClass="btn btn-outline-secondary" CausesValidation="false" OnClick="btnCancelarLogout_Click" />
            </div>
        </section>
    </main>
</asp:Content>