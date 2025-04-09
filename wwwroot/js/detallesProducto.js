function handleDropdownClick(event) {
  console.log("Dropdown fue clickeado");
  const dropdownMenu = document.querySelector(".dropdown-menu");
  dropdownMenu.classList.toggle("show");
}

function decrementarCantidad() {
  const input = document.getElementById("cantidadInput");
  let value = parseInt(input.value) || 1;
  if (value > 1) {
    input.value = value - 1;
  }
}

function incrementarCantidad() {
  const input = document.getElementById("cantidadInput");
  let value = parseInt(input.value) || 1;
  const maxStock = parseInt(input.dataset.maxStock) || 1;
  if (value < maxStock) {
    input.value = value + 1;
  }
}

document
  .getElementById("formAgregarCarrito")
  .addEventListener("submit", async function (e) {
    e.preventDefault();

    const form = e.target;
    const formData = new FormData(form);
    const productoId = formData.get("productoId");
    const cantidad = formData.get("cantidad");

    try {
      const response = await fetch("/Carrito/AgregarAlCarrito", {
        method: "POST",
        body: formData,
      });

      const result = await response.json();

      mostrarMensaje(
        result.message || "Producto agregado al carrito.",
        result.success
      );
    } catch (error) {
      console.error("Error en la petición:", error);
      mostrarMensaje("Error al enviar la solicitud.", false);
    }
  });

function mostrarMensaje(mensaje, success) {
  let modal = document.getElementById("mensajeModal");
  let mensajeTexto = document.getElementById("mensajeTexto");

  if (!modal || !mensajeTexto) {
    // Si no existe el modal en el DOM, lo creamos
    modal = document.createElement("div");
    modal.id = "mensajeModal";
    modal.classList.add("temp-data-modal");

    modal.innerHTML = `
      <div class="temp-data-content">
        <div class="temp-data-icon">
          <i class="fas fa-info-circle"></i>
        </div>
        <div class="temp-data-message" id="mensajeTexto"></div>
        <button class="temp-data-close">&times;</button>
      </div>
    `;
    document.body.appendChild(modal);
    mensajeTexto = document.getElementById("mensajeTexto");
  }

  mensajeTexto.innerText = mensaje;
  modal.style.display = "block";

  // Cambiar color del icono según el resultado
  const icon = modal.querySelector(".temp-data-icon i");
  icon.style.color = success ? "#007bff" : "#dc3545";

  // Cerrar al hacer clic en el botón
  const closeBtn = modal.querySelector(".temp-data-close");
  closeBtn.onclick = () => (modal.style.display = "none");

  // Ocultar automáticamente después de unos segundos
  setTimeout(() => {
    modal.style.display = "none";
  }, 4000);
}
