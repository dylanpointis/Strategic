<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="RealizarBackup.aspx.cs" Inherits="Strategic.RealizarBackup" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <main class="page-section">

        <div class="page-heading">
            <h1><%: Traductor.T("backup.titulo") %></h1>
            <p><%: Traductor.T("backup.subtitulo") %></p>
        </div>

        <section class="strategic-card">
            <div class="strategic-card-header">
                <h2><%: Traductor.T("backup.generarRespaldo") %></h2>
            </div>

            <div class="strategic-card-body">
                <p class="campo-ayuda"><%: Traductor.T("backup.ayuda", CarpetaRespaldos, BLL.BLLRespaldo.MaximoRespaldos) %></p>

                <div class="formulario-acciones">
                    <asp:Button ID="btnGenerar" runat="server" CssClass="btn btn-strategic" CausesValidation="false" OnClick="btnGenerar_Click" />
                </div>

                <asp:Panel ID="pnlConfirmacion" runat="server" CssClass="confirmacion" Visible="false">
                    <p><%: Traductor.T("backup.confirmar") %></p>
                    <asp:Button ID="btnConfirmar" runat="server" CssClass="btn btn-strategic" CausesValidation="false" OnClick="btnConfirmar_Click" />
                    <asp:Button ID="btnCancelar" runat="server" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnCancelar_Click" />
                </asp:Panel>

                <asp:Label ID="lblError" runat="server" CssClass="mensaje-error" EnableViewState="false"></asp:Label>
                <asp:Label ID="lblExito" runat="server" CssClass="mensaje-exito" EnableViewState="false"></asp:Label>
            </div>
        </section>

        <section class="strategic-card">
            <div class="strategic-card-header">
                <h2><%: Traductor.T("backup.respaldosGenerados") %></h2>
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
