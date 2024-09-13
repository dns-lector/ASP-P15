document.addEventListener('DOMContentLoaded', () => {
    loadCart();
});

function loadCart() {
    const container = document.getElementById("cart-container");
    if (!container) throw "#cart-container not found";
    const userId = container.getAttribute("data-user-id");

    fetch("/api/cart?id=" + userId)
    .then(r => r.json())
    .then(j => {

        container.innerHTML = JSON.stringify(j);
    });
}
