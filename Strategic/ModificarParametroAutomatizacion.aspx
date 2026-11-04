<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ModificarParametroAutomatizacion.aspx.cs" Inherits="Strategic.ModificarParametroAutomatizacion" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <main class="page-section">

        <div class="page-heading">
            <h1><%: Traductor.T("modificarParametro.titulo") %></h1>
            <p><%: Traductor.T("modificarParametro.subtitulo") %></p>
        </div>

        <asp:Panel ID="pnlFormulario" runat="server" CssClass="strategic-card">
            <div class="strategic-card-header">
                <h2><asp:Label ID="lblNombre" runat="server"></asp:Label></h2>
            </div>

            <div class="strategic-card-body">
                <p class="campo-ayuda"><asp:Label ID="lblDescripcion" runat="server"></asp:Label></p>

                <div class="formulario-grid">
                    <asp:Repeater ID="rptParametros" runat="server" OnItemDataBound="rptParametros_ItemDataBound">
                        <ItemTemplate>
                            <div class="campo">
                                <asp:HiddenField ID="hfClave" runat="server" Value='<%# Eval("Clave") %>' />
                                <asp:Label ID="lblParametro" runat="server" AssociatedControlID="txtValor" CssClass="form-label"></asp:Label>
                                <asp:TextBox ID="txtValor" runat="server" CssClass="form-control" MaxLength="4" Text='<%# Eval("Valor") %>'></asp:TextBox>
                                <asp:RequiredFieldValidator ID="valRequerido" runat="server" ControlToValidate="txtValor" Display="Dynamic" CssClass="campo-error" ValidationGroup="Modificar"></asp:RequiredFieldValidator>
                                <asp:RangeValidator ID="valRango" runat="server" ControlToValidate="txtValor" Type="Integer" Display="Dynamic" CssClass="campo-error" ValidationGroup="Modificar"></asp:RangeValidator>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>

                <asp:Panel ID="pnlConfirmacion" runat="server" CssClass="confirmacion" Visible="false">
                    <p><%: Traductor.T("modificarParametro.confirmar") %></p>
                    <asp:Button ID="btnConfirmar" runat="server" CssClass="btn btn-strategic" CausesValidation="false" OnClick="btnConfirmar_Click" />
                    <asp:Button ID="btnNoConfirmar" runat="server" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnNoConfirmar_Click" />
                </asp:Panel>

                <asp:Panel ID="pnlAcciones" runat="server" CssClass="formulario-acciones">
                    <asp:Button ID="btnGuardar" runat="server" CssClass="btn btn-strategic" ValidationGroup="Modificar" OnClick="btnGuardar_Click" />
                    <asp:Button ID="btnCancelar" runat="server" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnCancelar_Click" />
                </asp:Panel>

                <asp:Label ID="lblError" runat="server" CssClass="mensaje-error" EnableViewState="false"></asp:Label>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlSinAutomatizacion" runat="server" CssClass="strategic-card" Visible="false">
            <div class="strategic-card-body">
                <p><asp:Label ID="lblSinAutomatizacion" runat="server"></asp:Label></p>

                <div class="formulario-acciones">
                    <asp:Button ID="btnVolver" runat="server" CssClass="btn btn-strategic" CausesValidation="false" OnClick="btnCancelar_Click" />
                </div>
            </div>
        </asp:Panel>

    </main>
</asp:Content>
