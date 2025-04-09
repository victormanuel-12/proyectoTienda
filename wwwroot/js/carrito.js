$(document).ready(function () {
  // Mostrar tempData modal si existe
  if ($("#tempDataModal").length > 0) {
    $("#tempDataModal").show();
    $(".temp-data-close").click(function () {
      $(this).closest(".temp-data-modal").hide();
    });

    // Ocultar automáticamente después de 4 segundos
    setTimeout(function () {
      $("#tempDataModal").fadeOut();
    }, 4000);
  }

  // Mostrar modal para confirmar eliminación
  $(".eliminar-item").click(function () {
    const id = $(this).data("id");
    const nombre = $(this).data("nombre");

    // Mostrar modal con el nombre y ID del producto
    $("#idProductoEliminar").val(id);
    $("#nombreProductoEliminar").text(nombre);
  });

  // Confirmar eliminación desde el modal
  $("#btnConfirmarEliminar").click(function () {
    const id =
      $("#nombreProductoEliminar").data("id") ||
      $(".eliminar-item:last").data("id");
    console.log(id);
    // Utilizar AJAX para enviar la solicitud al controlador
    $.ajax({
      url: '@Url.Action("Eliminar", "Carrito")',
      type: "POST",
      data: { idProductoEliminar: id },
      dataType: "json",
      success: function (result) {
        if (result.success) {
          // Eliminar el producto del DOM sin recargar la página
          $(`[data-item-id="${id}"]`).fadeOut(300, function () {
            $(this).remove();

            // Actualizar contadores y resumen después de eliminar
            actualizarResumenCarrito();
          });

          // Mostrar mensaje de éxito
          mostrarMensaje(
            result.message || "Producto eliminado correctamente.",
            true
          );

          // Cerrar el modal
          $("#modalConfirmarEliminacion").modal("hide");
        } else {
          // Mostrar mensaje de error
          mostrarMensaje(
            result.message || "Error al eliminar el producto.",
            false
          );
        }
      },
      error: function () {
        mostrarMensaje("Ocurrió un error al eliminar el producto.", false);
      },
    });
  });

  function actualizarResumenCarrito() {
    // Contar productos restantes
    let numProductos = $(".item-carrito").length;

    // Actualizar contador en título del carrito
    $(".card-header h2").text(`Carro (${numProductos} productos)`);

    // Si no quedan productos, mostrar carrito vacío
    if (numProductos === 0) {
      $(".carrito").html(`
                        <div class="p-5 text-center">
                            <i class="bi bi-cart-x" style="font-size: 3rem;"></i>
                            <h4 class="mt-3">Tu carrito está vacío</h4>
                            <p class="text-muted">Agrega productos para continuar con tu compra</p>
                            <a href="@Url.Action("Index", "Catalogo")" class="btn btn-primary mt-2">
                                Ir al catálogo
                            </a>
                        </div>
                    `);
      $(".resumen").html(`
                        
                    `);
    }

    // También se debería recalcular el total, pero eso requeriría una llamada adicional al servidor
    // o tener los precios disponibles en el cliente
  }
});

// Función para mostrar mensajes en el DOM
function mostrarMensaje(mensaje, success) {
  const modal = document.getElementById("mensajeModal");
  const mensajeTexto = document.getElementById("mensajeTexto");
  const icon = modal.querySelector(".temp-data-icon i");

  mensajeTexto.innerText = mensaje;
  icon.style.color = success ? "#007bff" : "#dc3545";

  modal.style.display = "block";

  // Cerrar al hacer clic en el botón
  const closeBtn = modal.querySelector(".temp-data-close");
  closeBtn.onclick = () => (modal.style.display = "none");

  // Ocultar automáticamente después de unos segundos
  setTimeout(() => {
    modal.style.display = "none";
  }, 4000);
}
