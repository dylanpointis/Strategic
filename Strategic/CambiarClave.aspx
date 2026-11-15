<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CambiarClave.aspx.cs" Inherits="Strategic.CambiarClave" %>

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
                <h1><%: Traductor.T("cambiarClave.titulo") %></h1>
                <p><%: Traductor.T("cambiarClave.subtitulo") %></p>
            </div>

            <div class="mb-3">
                <sc:CampoClave ID="campoClaveActual" runat="server" Etiqueta="cambiarClave.claveActual" MensajeObligatorio="cambiarClave.claveActualRequerida" />
            </div>

            <%-- La nueva clave muestra los requisitos mientras se escribe y pide confirmarla --%>
            <div class="mb-4">
                <sc:CampoClave ID="campoClaveNueva" runat="server" Etiqueta="cambiarClave.claveNueva" MensajeObligatorio="cambiarClave.claveNuevaRequerida"
                    MostrarRequisitos="true" PedirConfirmacion="true" Autocompletar="new-password" />
            </div>

            <div class="auth-actions">
                <asp:Button ID="btnGuardar" runat="server" CssClass="btn btn-strategic" OnClick="btnGuardar_Click" />
                <asp:Button ID="btnCancelar" runat="server" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnCancelar_Click" />
            </div>

            <asp:Label ID="lblError" runat="server" CssClass="mensaje-error" EnableViewState="false"></asp:Label>
            <asp:Label ID="lblExito" runat="server" CssClass="mensaje-exito" EnableViewState="false"></asp:Label>
        </section>
    </main>
</asp:Content>
