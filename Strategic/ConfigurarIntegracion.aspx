<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ConfigurarIntegracion.aspx.cs" Inherits="Strategic.ConfigurarIntegracion" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <main class="page-section">

        <div class="page-heading">
            <h1><%: Traductor.T("configurarIntegracion.titulo") %></h1>
            <p><%: Traductor.T("configurarIntegracion.subtitulo") %></p>
        </div>

        <asp:Label ID="lblExito" runat="server" CssClass="mensaje-exito" EnableViewState="false"></asp:Label>

        <section class="strategic-card">
            <div class="strategic-card-header">
                <h2><%: Traductor.T("configurarIntegracion.archivoMuestra") %></h2>
            </div>

            <div class="strategic-card-body">
                <div class="formulario-grid">
                    <div class="campo">
                        <asp:Label ID="lblTipoArchivo" runat="server" AssociatedControlID="ddlTipoArchivo" CssClass="form-label"></asp:Label>
                        <asp:DropDownList ID="ddlTipoArchivo" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlTipoArchivo_SelectedIndexChanged"></asp:DropDownList>
                    </div>

                    <div class="campo">
                        <asp:Label ID="lblArchivo" runat="server" AssociatedControlID="fuMuestra" CssClass="form-label"></asp:Label>
                        <asp:FileUpload ID="fuMuestra" runat="server" CssClass="form-control" accept=".xml" />
                        <span class="campo-ayuda"><%: Traductor.T("configurarIntegracion.ayudaArchivo") %></span>
                    </div>
                </div>

                <asp:Label ID="lblConfiguracionExistente" runat="server" CssClass="mensaje-aviso"></asp:Label>

                <div class="formulario-acciones">
                    <asp:Button ID="btnLeerArchivo" runat="server" CssClass="btn btn-strategic" CausesValidation="false" OnClick="btnLeerArchivo_Click" />
                </div>

                <asp:Label ID="lblError" runat="server" CssClass="mensaje-error" EnableViewState="false"></asp:Label>
            </div>
        </section>

        <asp:Panel ID="pnlMapeo" runat="server" CssClass="strategic-card" Visible="false">
            <div class="strategic-card-header">
                <h2><%: Traductor.T("configurarIntegracion.mapeo") %></h2>
                <asp:Label ID="lblColumnasLeidas" runat="server" CssClass="strategic-card-meta"></asp:Label>
            </div>

            <div class="strategic-card-body">
                <p class="campo-ayuda"><%: Traductor.T("configurarIntegracion.ayudaMapeo") %></p>

                <div class="tabla-scroll">
                    <table class="strategic-table tabla-mapeo">
                        <thead>
                            <tr>
                                <th><%: Traductor.T("configurarIntegracion.campoStrategic") %></th>
                                <th><%: Traductor.T("configurarIntegracion.columnaCliente") %></th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptMapeo" runat="server" OnItemDataBound="rptMapeo_ItemDataBound">
                                <ItemTemplate>
                                    <tr>
                                        <td>
                                            <asp:HiddenField ID="hfCampo" runat="server" Value='<%# Eval("Nombre") %>' />
                                            <asp:Label ID="lblCampo" runat="server"></asp:Label>
                                        </td>
                                        <td>
                                            <asp:DropDownList ID="ddlColumna" runat="server" CssClass="form-select"></asp:DropDownList>
                                        </td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                </div>

                <p class="campo-ayuda"><%: Traductor.T("configurarIntegracion.obligatorios") %></p>

                <div class="formulario-acciones">
                    <asp:Button ID="btnGuardar" runat="server" CssClass="btn btn-strategic" CausesValidation="false" OnClick="btnGuardar_Click" />
                    <asp:Button ID="btnCancelar" runat="server" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnCancelar_Click" />
                </div>

                <asp:Label ID="lblErrorMapeo" runat="server" CssClass="mensaje-error" EnableViewState="false"></asp:Label>
            </div>
        </asp:Panel>

    </main>
</asp:Content>
