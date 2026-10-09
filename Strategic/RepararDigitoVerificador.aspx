<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="RepararDigitoVerificador.aspx.cs" Inherits="Strategic.RepararDigitoVerificador" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        .reparar-card {
            max-width: 640px;
            margin: 0 auto;
            padding: 35px 40px;
            text-align: center;
            margin-bottom: 2em;
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
            flex-wrap: wrap;
            gap: 12px;
            width: 100%;
        }

        .auth-actions .btn {
            min-width: 160px;
            padding: 10px 20px;
        }

        .reparar-card .mensaje-exito,
        .reparar-card .mensaje-error {
            text-align: left;
        }

        /* La pagina de login es un flex en fila: aca va en columna, el card arriba y la tabla abajo */
        .reparar-page {
            flex-direction: column;
            align-items: stretch;
        }

        .reparar-card {
            width: 100%;
        }

        .reparar-tabla {
            width: 100%;
            max-width: 900px;
            margin: 24px auto 0;
        }

        @media (max-width: 480px) {
            .reparar-card {
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

    <main class="login-page reparar-page">
        <section class="auth-card reparar-card">
            <div class="login-logo">
                <img src="<%= ResolveUrl("~/Content/images/Strategic_LOGO_PNG.png") %>" alt="Strategic" class="strategic-logo" />
            </div>

            <div class="login-heading">
                <h1><%: Traductor.T("repararDV.titulo") %></h1>
                <p><asp:Label ID="lblEstado" runat="server"></asp:Label></p>
            </div>

            <asp:Panel ID="pnlAcciones" runat="server" CssClass="auth-actions">
                <asp:Button ID="btnRecalcular" runat="server" CssClass="btn btn-danger" CausesValidation="false" OnClick="btnRecalcular_Click" />
                <asp:Button ID="btnRestaurar" runat="server" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnRestaurar_Click" />
                <asp:Button ID="btnVerificar" runat="server" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnVerificar_Click" />
            </asp:Panel>

            <asp:Panel ID="pnlConfirmacion" runat="server" CssClass="auth-actions" Visible="false">
                <asp:Button ID="btnConfirmar" runat="server" CssClass="btn btn-danger" CausesValidation="false" OnClick="btnConfirmar_Click" />
                <asp:Button ID="btnCancelar" runat="server" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnCancelar_Click" />
            </asp:Panel>

            <asp:Panel ID="pnlInicio" runat="server" CssClass="auth-actions" Visible="false">
                <asp:Button ID="btnInicio" runat="server" CssClass="btn btn-strategic" CausesValidation="false" OnClick="btnInicio_Click" />
            </asp:Panel>

            <asp:Label ID="lblExito" runat="server" CssClass="mensaje-exito" EnableViewState="false"></asp:Label>
            <asp:Label ID="lblError" runat="server" CssClass="mensaje-error" EnableViewState="false"></asp:Label>
        </section>

        <asp:Panel ID="pnlTabla" runat="server" CssClass="strategic-card reparar-tabla">
            <div class="strategic-card-header">
                <h2><%: Traductor.T("repararDV.inconsistenciasDetectadas") %></h2>
                <div class="strategic-card-acciones">
                    <asp:Label ID="lblCantidad" runat="server" CssClass="strategic-card-meta"></asp:Label>
                </div>
            </div>

            <div class="strategic-card-body sin-padding">
                <sc:GrillaGenerica ID="grillaInconsistencias" runat="server" />
            </div>
        </asp:Panel>
    </main>
</asp:Content>
