<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ImportarDatos.aspx.cs" Inherits="Strategic.ImportarDatos" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <main class="page-section">

        <div class="page-heading">
            <h1><%: Traductor.T("importarDatos.titulo") %></h1>
            <p><%: Traductor.T("importarDatos.subtitulo") %></p>
        </div>

        <section class="strategic-card">
            <div class="strategic-card-header">
                <h2><%: Traductor.T("importarDatos.tipoAImportar") %></h2>
            </div>

            <div class="strategic-card-body">
                <div class="formulario-grid">
                    <div class="campo">
                        <asp:Label ID="lblTipoArchivo" runat="server" AssociatedControlID="ddlTipoArchivo" CssClass="form-label"></asp:Label>
                        <asp:DropDownList ID="ddlTipoArchivo" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlTipoArchivo_SelectedIndexChanged"></asp:DropDownList>
                    </div>
                </div>

                <asp:Panel ID="pnlSinConfiguracion" runat="server" Visible="false">
                    <asp:Label ID="lblSinConfiguracion" runat="server" CssClass="mensaje-aviso"></asp:Label>
                    <div class="formulario-acciones">
                        <asp:HyperLink ID="lnkConfigurar" runat="server" NavigateUrl="~/ConfigurarIntegracion.aspx" CssClass="btn btn-strategic-outline"></asp:HyperLink>
                    </div>
                </asp:Panel>

                <asp:Label ID="lblError" runat="server" CssClass="mensaje-error" EnableViewState="false"></asp:Label>
            </div>
        </section>

        <asp:Panel ID="pnlImportacion" runat="server" Visible="false">

            <section class="strategic-card">
                <div class="strategic-card-header">
                    <h2><%: Traductor.T("importarDatos.mapeoConfigurado") %></h2>
                    <asp:Label ID="lblFechaConfiguracion" runat="server" CssClass="strategic-card-meta"></asp:Label>
                </div>

                <div class="strategic-card-body sin-padding">
                    <sc:GrillaGenerica ID="grillaMapeo" runat="server" />
                </div>
            </section>

            <section class="strategic-card">
                <div class="strategic-card-header">
                    <h2><%: Traductor.T("importarDatos.archivo") %></h2>
                </div>

                <div class="strategic-card-body">
                    <div class="formulario-grid">
                        <div class="campo">
                            <asp:Label ID="lblArchivo" runat="server" AssociatedControlID="fuArchivo" CssClass="form-label"></asp:Label>
                            <asp:FileUpload ID="fuArchivo" runat="server" CssClass="form-control" accept=".xml" />
                        </div>
                    </div>

                    <div class="formulario-acciones">
                        <asp:Button ID="btnImportar" runat="server" CssClass="btn btn-strategic" CausesValidation="false" OnClick="btnImportar_Click" />
                    </div>
                </div>
            </section>

        </asp:Panel>

        <asp:Panel ID="pnlResultado" runat="server" CssClass="strategic-card" Visible="false">
            <div class="strategic-card-header">
                <h2><%: Traductor.T("importarDatos.resumen") %></h2>
                <asp:Label ID="lblEstadoResultado" runat="server" CssClass="strategic-card-meta"></asp:Label>
            </div>

            <div class="strategic-card-body">
                <asp:Label ID="lblMensajeResultado" runat="server"></asp:Label>

                <div class="indicadores-grid">
                    <div class="indicador">
                        <span class="indicador-titulo"><%: Traductor.T("integracion.procesados") %></span>
                        <span class="indicador-valor"><asp:Label ID="lblProcesados" runat="server"></asp:Label></span>
                    </div>

                    <div class="indicador">
                        <span class="indicador-titulo"><%: Traductor.T("integracion.importados") %></span>
                        <span class="indicador-valor"><asp:Label ID="lblImportados" runat="server"></asp:Label></span>
                    </div>

                    <div class="indicador">
                        <span class="indicador-titulo"><%: Traductor.T("integracion.actualizados") %></span>
                        <span class="indicador-valor"><asp:Label ID="lblActualizados" runat="server"></asp:Label></span>
                    </div>

                    <div class="indicador">
                        <span class="indicador-titulo"><%: Traductor.T("integracion.conError") %></span>
                        <span class="indicador-valor"><asp:Label ID="lblConError" runat="server"></asp:Label></span>
                    </div>
                </div>

                <asp:Panel ID="pnlErrores" runat="server" CssClass="lista-errores" Visible="false">
                    <h3 class="subtitulo-seccion"><%: Traductor.T("integracion.detalleErrores") %></h3>
                    <asp:BulletedList ID="blErrores" runat="server"></asp:BulletedList>
                </asp:Panel>

                <div class="formulario-acciones">
                    <asp:HyperLink ID="lnkHistorial" runat="server" NavigateUrl="~/HistorialSincronizaciones.aspx" CssClass="btn btn-strategic-outline"></asp:HyperLink>
                </div>
            </div>
        </asp:Panel>

    </main>
</asp:Content>
