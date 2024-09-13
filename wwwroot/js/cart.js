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
        let html = "";
        if (j.data == null || j.data.cartProducts == null) {
            html = "Кошик порожній";
        }
        else {
            html = '<div class="row"><div class="col col-8">';
            for (let cartProduct of j.data.cartProducts) {
                html += `<div class="row my-2 cart-product-item" data-product-id="${cartProduct.id}">
                    <div class="col col-2">
                        <img src="/Home/Download/Shop_${cartProduct.product.image}" alt="Picture"/>
                    </div>
                    <div class="col col-8 cart-product-info">
                        <p>${cartProduct.product.name}</p>
                        <p class="text-muted">${cartProduct.product.description}</p>
                    </div>
                    <div class="col col-2 cart-product-calc">
                        <div class="d-flex justify-content-between  align-items-center">
                            <button onclick="decrementClick(event)">-</button>
                            <b data-role="cart-product-cnt">${cartProduct.cnt}</b>
                            <button onclick="incrementClick(event)">+</button>
                        </div>
                        <div class="text-center mx-auto cart-product-sum" >
                            ₴ <span data-role="cart-product-sum">${cartProduct.cnt * cartProduct.product.price}</span>
                        </div>
                    </div>
                </div>`;
            }
            html += '</div></div>';
        }
        container.innerHTML = html;
    });
}

function decrementClick(e) {
    const parentBlock = e.target.closest("[data-product-id]");
    const productId = parentBlock.getAttribute("data-product-id");
    console.log(productId + "decrement");
}
function incrementClick(e) {

}
/* Д.З. Реалізувати на сторінці кошику "клікабельність" зображення
кожного товару з переходом на його сторінку.
*/