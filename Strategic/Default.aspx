<%@ Page Title="Inicio" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Strategic._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <%-- Pagina de bienvenida para quien entra sin sesion. El contenido es fijo
         (no sale de la base), por eso no participa del digito verificador.
         No hay registro: las cuentas las crea el WebMaster para cada cliente. --%>

    <div class="bienvenida">

        <%-- Portada --%>
        <section class="bv-hero">
            <div class="bv-hero-texto">
                <h1>
                    <%: Traductor.T("bienvenida.hero.titulo") %>
                    <span class="bv-degradado"><%: Traductor.T("bienvenida.hero.tituloDestacado") %></span>
                </h1>
                <p class="bv-hero-bajada"><%: Traductor.T("bienvenida.hero.texto") %></p>
                <div class="bv-acciones">
                    <a class="bv-boton bv-boton-marca" href="#contacto"><%: Traductor.T("bienvenida.solicitarCotizacion") %></a>
                    <a class="bv-boton bv-boton-claro" href="<%: ResolveUrl("~/Login.aspx") %>"><%: Traductor.T("bienvenida.yaSoyCliente") %></a>
                </div>
            </div>
        </section>

        <%-- Quienes somos --%>
        <section class="bv-seccion bv-nosotros">
            <div class="bv-nosotros-texto">
                <span class="bv-etiqueta"><%: Traductor.T("bienvenida.nosotros.etiqueta") %></span>
                <h2><%: Traductor.T("bienvenida.nosotros.titulo") %></h2>
                <p><%: Traductor.T("bienvenida.nosotros.texto1") %></p>
                <p><%: Traductor.T("bienvenida.nosotros.texto2") %></p>
                <p><%: Traductor.T("bienvenida.nosotros.texto3") %></p>
            </div>
            <ul class="bv-pilares">
                <li>
                    <strong><%: Traductor.T("bienvenida.pilar.medida.titulo") %></strong>
                    <span><%: Traductor.T("bienvenida.pilar.medida.texto") %></span>
                </li>
                <li>
                    <strong><%: Traductor.T("bienvenida.pilar.integrado.titulo") %></strong>
                    <span><%: Traductor.T("bienvenida.pilar.integrado.texto") %></span>
                </li>
                <li>
                    <strong><%: Traductor.T("bienvenida.pilar.acompanamiento.titulo") %></strong>
                    <span><%: Traductor.T("bienvenida.pilar.acompanamiento.texto") %></span>
                </li>
                <li>
                    <strong><%: Traductor.T("bienvenida.pilar.empresas.titulo") %></strong>
                    <span><%: Traductor.T("bienvenida.pilar.empresas.texto") %></span>
                </li>
            </ul>
        </section>

        <%-- Mision y vision --%>
        <section class="bv-seccion bv-proposito">
            <div>
                <span class="bv-etiqueta"><%: Traductor.T("bienvenida.mision.titulo") %></span>
                <p><%: Traductor.T("bienvenida.mision.texto") %></p>
            </div>
            <div>
                <span class="bv-etiqueta"><%: Traductor.T("bienvenida.vision.titulo") %></span>
                <p><%: Traductor.T("bienvenida.vision.texto") %></p>
            </div>
        </section>

        <%-- Que hacemos --%>
        <section class="bv-seccion">
            <div class="bv-encabezado">
                <span class="bv-etiqueta"><%: Traductor.T("bienvenida.servicios.etiqueta") %></span>
                <h2><%: Traductor.T("bienvenida.servicios.titulo") %></h2>
            </div>
            <div class="bv-servicios">
                <article class="bv-servicio">
                    <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M3 3v18h18" /><path d="M8 17v-6" /><path d="M13 17V7" /><path d="M18 17v-4" /></svg>
                    <h3><%: Traductor.T("bienvenida.servicio.dashboard.titulo") %></h3>
                    <p><%: Traductor.T("bienvenida.servicio.dashboard.texto") %></p>
                </article>
                <article class="bv-servicio">
                    <svg viewBox="0 0 24 24" aria-hidden="true"><polyline points="3 17 9 11 13 15 21 7" /><polyline points="15 7 21 7 21 13" /></svg>
                    <h3><%: Traductor.T("bienvenida.servicio.predicciones.titulo") %></h3>
                    <p><%: Traductor.T("bienvenida.servicio.predicciones.texto") %></p>
                </article>
                <article class="bv-servicio">
                    <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M9 18h6" /><path d="M10 22h4" /><path d="M12 2a7 7 0 0 0-4 12.7c.6.5 1 1.3 1 2.1v.2h6v-.2c0-.8.4-1.6 1-2.1A7 7 0 0 0 12 2z" /></svg>
                    <h3><%: Traductor.T("bienvenida.servicio.recomendaciones.titulo") %></h3>
                    <p><%: Traductor.T("bienvenida.servicio.recomendaciones.texto") %></p>
                </article>
                <article class="bv-servicio">
                    <svg viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="12" r="9" /><circle cx="12" cy="12" r="5" /><circle cx="12" cy="12" r="1" /></svg>
                    <h3><%: Traductor.T("bienvenida.servicio.competencia.titulo") %></h3>
                    <p><%: Traductor.T("bienvenida.servicio.competencia.texto") %></p>
                </article>
                <article class="bv-servicio">
                    <svg viewBox="0 0 24 24" aria-hidden="true"><polygon points="13 2 3 14 12 14 11 22 21 10 12 10 13 2" /></svg>
                    <h3><%: Traductor.T("bienvenida.servicio.automatizaciones.titulo") %></h3>
                    <p><%: Traductor.T("bienvenida.servicio.automatizaciones.texto") %></p>
                </article>
                <article class="bv-servicio">
                    <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M10 13a5 5 0 0 0 7.5.5l3-3a5 5 0 0 0-7-7l-1.7 1.7" /><path d="M14 11a5 5 0 0 0-7.5-.5l-3 3a5 5 0 0 0 7 7l1.7-1.7" /></svg>
                    <h3><%: Traductor.T("bienvenida.servicio.integracion.titulo") %></h3>
                    <p><%: Traductor.T("bienvenida.servicio.integracion.texto") %></p>
                </article>
            </div>
        </section>

        <%-- Como trabajamos --%>
        <section class="bv-seccion">
            <div class="bv-encabezado">
                <span class="bv-etiqueta"><%: Traductor.T("bienvenida.proceso.etiqueta") %></span>
                <h2><%: Traductor.T("bienvenida.proceso.titulo") %></h2>
                <p><%: Traductor.T("bienvenida.proceso.texto") %></p>
            </div>
            <ol class="bv-pasos">
                <li>
                    <span class="bv-paso-numero">1</span>
                    <h3><%: Traductor.T("bienvenida.paso.relevamiento.titulo") %></h3>
                    <p><%: Traductor.T("bienvenida.paso.relevamiento.texto") %></p>
                </li>
                <li>
                    <span class="bv-paso-numero">2</span>
                    <h3><%: Traductor.T("bienvenida.paso.cotizacion.titulo") %></h3>
                    <p><%: Traductor.T("bienvenida.paso.cotizacion.texto") %></p>
                </li>
                <li>
                    <span class="bv-paso-numero">3</span>
                    <h3><%: Traductor.T("bienvenida.paso.desarrollo.titulo") %></h3>
                    <p><%: Traductor.T("bienvenida.paso.desarrollo.texto") %></p>
                </li>
                <li>
                    <span class="bv-paso-numero">4</span>
                    <h3><%: Traductor.T("bienvenida.paso.puestaEnMarcha.titulo") %></h3>
                    <p><%: Traductor.T("bienvenida.paso.puestaEnMarcha.texto") %></p>
                </li>
            </ol>
        </section>

        <%-- Planes: precios de referencia, se contratan por cotizacion --%>
        <section class="bv-seccion" id="planes">
            <div class="bv-encabezado">
                <span class="bv-etiqueta"><%: Traductor.T("bienvenida.planes.etiqueta") %></span>
                <h2><%: Traductor.T("bienvenida.planes.titulo") %></h2>
                <p><%: Traductor.T("bienvenida.planes.texto") %></p>
            </div>
            <div class="bv-planes">
                <article class="bv-plan">
                    <h3><%: Traductor.T("bienvenida.plan.basico") %></h3>
                    <p class="bv-plan-descripcion"><%: Traductor.T("bienvenida.plan.basico.descripcion") %></p>
                    <div class="bv-plan-precio">
                        <strong><%: Precio(160) %></strong><span><%: Traductor.T("bienvenida.plan.porMes") %></span>
                    </div>
                    <div class="bv-plan-implementacion">
                        <span><%: Traductor.T("bienvenida.plan.implementacion") %></span>
                        <strong><%: Traductor.T("bienvenida.plan.desde", Precio(1500)) %></strong>
                    </div>
                    <ul class="bv-plan-items">
                        <li><%: Traductor.T("bienvenida.plan.basico.item1") %></li>
                        <li><%: Traductor.T("bienvenida.plan.basico.item2") %></li>
                        <li><%: Traductor.T("bienvenida.plan.basico.item3") %></li>
                        <li><%: Traductor.T("bienvenida.plan.basico.item4") %></li>
                    </ul>
                    <a class="bv-boton bv-boton-claro" href="#contacto"><%: Traductor.T("bienvenida.solicitarCotizacion") %></a>
                </article>
                <article class="bv-plan bv-plan-destacado">
                    <span class="bv-plan-insignia"><%: Traductor.T("bienvenida.plan.recomendado") %></span>
                    <h3><%: Traductor.T("bienvenida.plan.pro") %></h3>
                    <p class="bv-plan-descripcion"><%: Traductor.T("bienvenida.plan.pro.descripcion") %></p>
                    <div class="bv-plan-precio">
                        <strong><%: Precio(240) %></strong><span><%: Traductor.T("bienvenida.plan.porMes") %></span>
                    </div>
                    <div class="bv-plan-implementacion">
                        <span><%: Traductor.T("bienvenida.plan.implementacion") %></span>
                        <strong><%: Traductor.T("bienvenida.plan.desde", Precio(3200)) %></strong>
                    </div>
                    <ul class="bv-plan-items">
                        <li class="bv-plan-incluye"><%: Traductor.T("bienvenida.plan.pro.incluye") %></li>
                        <li><%: Traductor.T("bienvenida.plan.pro.item1") %></li>
                        <li><%: Traductor.T("bienvenida.plan.pro.item2") %></li>
                        <li><%: Traductor.T("bienvenida.plan.pro.item3") %></li>
                        <li><%: Traductor.T("bienvenida.plan.pro.item4") %></li>
                    </ul>
                    <a class="bv-boton bv-boton-marca" href="#contacto"><%: Traductor.T("bienvenida.solicitarCotizacion") %></a>
                </article>
                <article class="bv-plan">
                    <h3><%: Traductor.T("bienvenida.plan.max") %></h3>
                    <p class="bv-plan-descripcion"><%: Traductor.T("bienvenida.plan.max.descripcion") %></p>
                    <div class="bv-plan-precio">
                        <strong><%: Precio(480) %></strong><span><%: Traductor.T("bienvenida.plan.porMes") %></span>
                    </div>
                    <div class="bv-plan-implementacion">
                        <span><%: Traductor.T("bienvenida.plan.implementacion") %></span>
                        <strong><%: Traductor.T("bienvenida.plan.desde", Precio(6500)) %></strong>
                    </div>
                    <ul class="bv-plan-items">
                        <li class="bv-plan-incluye"><%: Traductor.T("bienvenida.plan.max.incluye") %></li>
                        <li><%: Traductor.T("bienvenida.plan.max.item1") %></li>
                        <li><%: Traductor.T("bienvenida.plan.max.item2") %></li>
                        <li><%: Traductor.T("bienvenida.plan.max.item3") %></li>
                        <li><%: Traductor.T("bienvenida.plan.max.item4") %></li>
                    </ul>
                    <a class="bv-boton bv-boton-claro" href="#contacto"><%: Traductor.T("bienvenida.solicitarCotizacion") %></a>
                </article>
            </div>
            <p class="bv-planes-nota"><%: Traductor.T("bienvenida.planes.nota") %></p>
        </section>

        <%-- Contacto --%>
        <section class="bv-contacto" id="contacto">
            <h2><%: Traductor.T("bienvenida.contacto.titulo") %></h2>
            <p><%: Traductor.T("bienvenida.contacto.texto") %></p>
            <%-- Sin funcionalidad por ahora: el envio de la consulta se implementa mas adelante --%>
            <button type="button" class="bv-boton bv-boton-blanco"><%: Traductor.T("bienvenida.contacto.boton") %></button>
            <small><%: Traductor.T("bienvenida.contacto.cuentas") %></small>
        </section>

    </div>

</asp:Content>
