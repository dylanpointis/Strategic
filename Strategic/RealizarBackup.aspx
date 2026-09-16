<%@ Page Title="Realizar backup" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="RealizarBackup.aspx.cs" Inherits="Strategic.RealizarBackup" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <main class="page-section">

        <div class="page-heading">
            <h1>Realizar backup</h1>
            <p>Genera una copia de seguridad completa de la base de datos en un archivo .bak.</p>
        </div>

        <section class="strategic-card">
            <div class="strategic-card-header">
                <h2>Generar respaldo</h2>
            </div>

            <div class="strategic-card-body">
                <p class="campo-ayuda">
                    El archivo se guarda en la carpeta <asp:Label ID="lblCarpeta" runat="server"></asp:Label>
                    del servidor. Se conservan los últimos <asp:Label ID="lblMaximo" runat="server"></asp:Label> respaldos;
                    al generar uno nuevo se borran los más antiguos.
                </p>

                <div class="formulario-acciones">
                    <asp:Button ID="btnGenerar" runat="server" Text="Generar backup" CssClass="btn btn-strategic" CausesValidation="false" OnClick="btnGenerar_Click" />
                </div>

                <asp:Panel ID="pnlConfirmacion" runat="server" CssClass="confirmacion" Visible="false">
                    <p>¿Deseás generar un nuevo respaldo de la base de datos? La operación puede demorar unos segundos.</p>
                    <asp:Button ID="btnConfirmar" runat="server" Text="Confirmar" CssClass="btn btn-strategic" CausesValidation="false" OnClick="btnConfirmar_Click" />
                    <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnCancelar_Click" />
                </asp:Panel>

                <asp:Label ID="lblError" runat="server" CssClass="mensaje-error" EnableViewState="false"></asp:Label>
                <asp:Label ID="lblExito" runat="server" CssClass="mensaje-exito" EnableViewState="false"></asp:Label>
            </div>
        </section>

        <section class="strategic-card">
            <div class="strategic-card-header">
                <h2>Respaldos generados</h2>
                <div class="strategic-card-acciones">
                    <asp:Label ID="lblCantidad" runat="server" CssClass="strategic-card-meta"></asp:Label>
                </div>
            </div>

            <div class="strategic-card-body sin-padding">
                <sc:GrillaGenerica ID="grillaRespaldos" runat="server" OnAccionSeleccionada="grillaRespaldos_AccionSeleccionada" />
            </div>
        </section>

    </main>
</asp:Content>
