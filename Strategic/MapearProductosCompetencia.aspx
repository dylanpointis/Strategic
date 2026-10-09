<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MapearProductosCompetencia.aspx.cs" Inherits="Strategic.MapearProductosCompetencia" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <main class="page-section">

        <div class="page-heading">
            <h1><%: Traductor.T("mapearProductos.titulo") %></h1>
            <p><%: Traductor.T("mapearProductos.subtitulo") %></p>
        </div>

        <asp:Panel ID="pnlFormulario" runat="server" CssClass="strategic-card">
            <div class="strategic-card-header">
                <h2><%: Traductor.T("mapearProductos.datos") %></h2>
            </div>

            <div class="strategic-card-body">
                <div class="formulario-grid">
                    <div class="campo">
                        <asp:Label ID="lblProducto" runat="server" AssociatedControlID="ddlProducto" CssClass="form-label"></asp:Label>
                        <asp:DropDownList ID="ddlProducto" runat="server" CssClass="form-control">
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator ID="valProducto" runat="server" ControlToValidate="ddlProducto"
                            InitialValue="" Display="Dynamic" CssClass="campo-error" ValidationGroup="Mapeo"></asp:RequiredFieldValidator>
                    </div>

                    <div class="campo">
                        <asp:Label ID="lblCompetidor" runat="server" AssociatedControlID="ddlCompetidor" CssClass="form-label"></asp:Label>
                        <asp:DropDownList ID="ddlCompetidor" runat="server" CssClass="form-control">
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator ID="valCompetidor" runat="server" ControlToValidate="ddlCompetidor"
                            InitialValue="" Display="Dynamic" CssClass="campo-error" ValidationGroup="Mapeo"></asp:RequiredFieldValidator>
                    </div>

                    <div class="campo campo-ancho">
                        <asp:Label ID="lblUrl" runat="server" AssociatedControlID="txtUrl" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtUrl" runat="server" CssClass="form-control" MaxLength="500" placeholder="https://www.mercadolibre.com.ar/..."></asp:TextBox>
                        <asp:RequiredFieldValidator ID="valUrl" runat="server" ControlToValidate="txtUrl" Display="Dynamic" CssClass="campo-error" ValidationGroup="Mapeo"></asp:RequiredFieldValidator>
                    </div>
                </div>

                <div class="formulario-acciones">
                    <asp:Button ID="btnMapear" runat="server" CssClass="btn btn-strategic" ValidationGroup="Mapeo" OnClick="btnMapear_Click" />
                    <asp:Button ID="btnLimpiar" runat="server" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnLimpiar_Click" />
                </div>

                <asp:Label ID="lblExito" runat="server" CssClass="mensaje-exito" EnableViewState="false"></asp:Label>
                <asp:Label ID="lblError" runat="server" CssClass="mensaje-error" EnableViewState="false"></asp:Label>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlSinDatos" runat="server" CssClass="strategic-card" Visible="false">
            <div class="strategic-card-body">
                <div class="estado-vacio">
                    <span class="estado-vacio-icono">
                        <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false">
                            <circle cx="12" cy="8" r="4"></circle>
                            <path d="M4 21c0-4 3.5-7 8-7"></path>
                            <path d="M16 16l5 5"></path>
                            <path d="M21 16l-5 5"></path>
                        </svg>
                    </span>
                    <p><asp:Label ID="lblSinDatos" runat="server"></asp:Label></p>
                </div>
            </div>
        </asp:Panel>

    </main>
</asp:Content>
