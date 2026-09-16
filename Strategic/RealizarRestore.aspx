<%@ Page Title="Realizar restore" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="RealizarRestore.aspx.cs" Inherits="Strategic.RealizarRestore" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <main class="page-section">

        <div class="page-heading">
            <h1>Realizar restore</h1>
            <p>Restaura la base de datos completa a partir de un archivo .bak generado por el sistema.</p>
        </div>

        <section class="strategic-card">
            <div class="strategic-card-header">
                <h2>Restaurar base de datos</h2>
            </div>

            <div class="strategic-card-body">
                <asp:Panel ID="pnlSeleccion" runat="server">
                    <div class="formulario-grid">
                        <div class="campo campo-ancho">
                            <asp:Label ID="lblArchivo" runat="server" Text="Archivo de respaldo (.bak)" AssociatedControlID="fuArchivo" CssClass="form-label"></asp:Label>
                            <asp:FileUpload ID="fuArchivo" runat="server" CssClass="form-control" accept=".bak" />
                            <span class="campo-ayuda">Elegí un archivo .bak descargado desde la pantalla de backup. Todos los datos actuales se van a reemplazar por los del respaldo.</span>
                        </div>
                    </div>

                    <div class="formulario-acciones">
                        <asp:Button ID="btnRestaurar" runat="server" Text="Restaurar" CssClass="btn btn-strategic-danger" CausesValidation="false" OnClick="btnRestaurar_Click" />
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlConfirmacion" runat="server" CssClass="confirmacion" Visible="false">
                    <p><asp:Label ID="lblConfirmacion" runat="server"></asp:Label></p>
                    <asp:Button ID="btnConfirmar" runat="server" Text="Confirmar restore" CssClass="btn btn-strategic-danger" CausesValidation="false" OnClick="btnConfirmar_Click" />
                    <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnCancelar_Click" />
                </asp:Panel>

                <asp:Label ID="lblError" runat="server" CssClass="mensaje-error" EnableViewState="false"></asp:Label>
                <asp:Label ID="lblExito" runat="server" CssClass="mensaje-exito" EnableViewState="false"></asp:Label>
            </div>
        </section>

    </main>
</asp:Content>
