<%@ Page Title="Acceso denegado" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AccesoDenegado.aspx.cs" Inherits="Strategic.AccesoDenegado" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <main class="page-section">

        <div class="page-heading">
            <h1>Acceso denegado</h1>
            <p>Tu rol no habilita esta pantalla.</p>
        </div>

        <section class="strategic-card">
            <div class="strategic-card-body">
                <div class="estado-vacio">
                    <span class="estado-vacio-icono">
                        <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false">
                            <rect x="5" y="11" width="14" height="10" rx="2"></rect>
                            <path d="M8 11V7a4 4 0 0 1 8 0v4"></path>
                        </svg>
                    </span>
                    <p><asp:Label ID="lblMensaje" runat="server"></asp:Label></p>
                </div>

                <div class="formulario-acciones">
                    <asp:Button ID="btnInicio" runat="server" Text="Ir al inicio" CssClass="btn btn-strategic" CausesValidation="false" OnClick="btnInicio_Click" />
                </div>
            </div>
        </section>

    </main>
</asp:Content>
