<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="RenovarSuscripcion.aspx.cs" Inherits="Strategic.RenovarSuscripcion" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <main class="page-section">

        <div class="page-heading">
            <h1><%: Traductor.T("renovarSuscripcion.titulo") %></h1>
            <p><%: Traductor.T("renovarSuscripcion.subtitulo") %></p>
        </div>

        <asp:Label ID="lblAvisoVencida" runat="server" CssClass="mensaje-aviso"></asp:Label>
        <asp:Label ID="lblExito" runat="server" CssClass="mensaje-exito" EnableViewState="false"></asp:Label>

        <asp:Panel ID="pnlSuscripcion" runat="server">

            <section class="strategic-card">
                <div class="strategic-card-header">
                    <h2><%: Traductor.T("renovarSuscripcion.estadoActual") %></h2>
                    <asp:Label ID="lblEstadoActual" runat="server" CssClass="strategic-card-meta"></asp:Label>
                </div>

                <div class="strategic-card-body">
                    <dl class="detalle-grid">
                        <div class="detalle-item">
                            <dt><%: Traductor.T("suscripcion.plan") %></dt>
                            <dd><asp:Label ID="lblPlan" runat="server"></asp:Label></dd>
                        </div>

                        <div class="detalle-item">
                            <dt><%: Traductor.T("suscripcion.fechaInicio") %></dt>
                            <dd><asp:Label ID="lblFechaInicio" runat="server"></asp:Label></dd>
                        </div>

                        <div class="detalle-item">
                            <dt><%: Traductor.T("suscripcion.fechaVencimiento") %></dt>
                            <dd><asp:Label ID="lblFechaVencimiento" runat="server"></asp:Label></dd>
                        </div>

                        <div class="detalle-item">
                            <dt><%: Traductor.T("suscripcion.montoMensual") %></dt>
                            <dd><asp:Label ID="lblMonto" runat="server"></asp:Label></dd>
                        </div>
                    </dl>

                    <div class="formulario-acciones">
                        <asp:Button ID="btnRenovar" runat="server" CssClass="btn btn-strategic" CausesValidation="false" OnClick="btnRenovar_Click" />
                        <asp:Button ID="btnDarDeBaja" runat="server" CssClass="btn btn-strategic-danger" CausesValidation="false" OnClick="btnDarDeBaja_Click" />
                    </div>
                </div>
            </section>

            <asp:Panel ID="pnlRenovacion" runat="server" CssClass="strategic-card" Visible="false">
                <div class="strategic-card-header">
                    <h2><%: Traductor.T("renovarSuscripcion.resumen") %></h2>
                </div>

                <div class="strategic-card-body">
                    <dl class="detalle-grid">
                        <div class="detalle-item">
                            <dt><%: Traductor.T("suscripcion.plan") %></dt>
                            <dd><asp:Label ID="lblRenovacionPlan" runat="server"></asp:Label></dd>
                        </div>

                        <div class="detalle-item">
                            <dt><%: Traductor.T("suscripcion.montoAPagar") %></dt>
                            <dd><asp:Label ID="lblRenovacionMonto" runat="server"></asp:Label></dd>
                        </div>

                        <div class="detalle-item">
                            <dt><%: Traductor.T("suscripcion.nuevoVencimiento") %></dt>
                            <dd><asp:Label ID="lblRenovacionVencimiento" runat="server"></asp:Label></dd>
                        </div>
                    </dl>

                    <h3 class="subtitulo-seccion"><%: Traductor.T("renovarSuscripcion.datosPago") %></h3>

                    <div class="datos-pago">
                        <dl class="detalle-grid">
                            <div class="detalle-item">
                                <dt><%: Traductor.T("suscripcion.cbu") %></dt>
                                <dd><asp:Label ID="lblCbu" runat="server"></asp:Label></dd>
                            </div>

                            <div class="detalle-item">
                                <dt><%: Traductor.T("suscripcion.alias") %></dt>
                                <dd><asp:Label ID="lblAlias" runat="server"></asp:Label></dd>
                            </div>

                            <div class="detalle-item">
                                <dt><%: Traductor.T("suscripcion.titular") %></dt>
                                <dd><asp:Label ID="lblTitular" runat="server"></asp:Label></dd>
                            </div>
                        </dl>

                        <div class="qr-pago">
                            <div id="qrMercadoPago" class="qr-pago-imagen"></div>
                            <span class="qr-pago-texto"><%: Traductor.T("renovarSuscripcion.qrAyuda") %></span>
                            <span id="qrError" class="mensaje-error"></span>
                        </div>
                    </div>

                    <p class="campo-ayuda"><%: Traductor.T("renovarSuscripcion.instrucciones") %></p>

                    <div class="formulario-grid">
                        <div class="campo">
                            <asp:Label ID="lblNumeroOperacion" runat="server" AssociatedControlID="txtNumeroOperacion" CssClass="form-label"></asp:Label>
                            <asp:TextBox ID="txtNumeroOperacion" runat="server" CssClass="form-control" MaxLength="30" autocomplete="off"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="valNumeroOperacion" runat="server" ControlToValidate="txtNumeroOperacion" Display="Dynamic" CssClass="campo-error" ValidationGroup="Renovacion"></asp:RequiredFieldValidator>
                        </div>
                    </div>

                    <div class="formulario-acciones">
                        <asp:Button ID="btnConfirmar" runat="server" CssClass="btn btn-strategic" ValidationGroup="Renovacion" OnClick="btnConfirmar_Click" />
                        <asp:Button ID="btnCancelar" runat="server" CssClass="btn btn-strategic-outline" CausesValidation="false" OnClick="btnCancelar_Click" />
                    </div>

                    <asp:Label ID="lblErrorRenovacion" runat="server" CssClass="mensaje-error" EnableViewState="false"></asp:Label>
                </div>
            </asp:Panel>

            <section class="strategic-card">
                <div class="strategic-card-header">
                    <h2><%: Traductor.T("renovarSuscripcion.historial") %></h2>
                    <asp:Label ID="lblCantidad" runat="server" CssClass="strategic-card-meta"></asp:Label>
                </div>

                <div class="strategic-card-body sin-padding">
                    <sc:GrillaGenerica ID="grillaSuscripciones" runat="server" />
                </div>
            </section>

        </asp:Panel>

        <asp:Panel ID="pnlSinSuscripcion" runat="server" CssClass="strategic-card" Visible="false">
            <div class="strategic-card-body">
                <div class="estado-vacio">
                    <span class="estado-vacio-icono">
                        <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false">
                            <rect x="3" y="5" width="18" height="14" rx="2"></rect>
                            <path d="M3 10h18"></path>
                            <path d="M7 15h4"></path>
                        </svg>
                    </span>
                    <p><asp:Label ID="lblSinSuscripcion" runat="server"></asp:Label></p>
                </div>
            </div>
        </asp:Panel>

        <asp:Label ID="lblError" runat="server" CssClass="mensaje-error" EnableViewState="false"></asp:Label>

    </main>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ScriptsFinales" runat="server">
    <% if (pnlRenovacion.Visible) { %>
    <script src="https://cdn.jsdelivr.net/npm/qrcode-generator@1.4.4/qrcode.js"></script>
    <script type="text/javascript">
        (function () {
            var contenedor = document.getElementById('qrMercadoPago');

            // Alternativa 4.2 [#ERR051]: sin la libreria (o si falla) se paga con CBU o alias
            try {
                var qr = qrcode(0, 'M');
                qr.addData(<%= TextoQrJson %>);
                qr.make();
                contenedor.innerHTML = qr.createSvgTag(4, 8);
            } catch (e) {
                document.getElementById('qrError').textContent = <%= ErrorQrJson %>;
            }
        })();
    </script>
    <% } %>
</asp:Content>
