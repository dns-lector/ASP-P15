document.addEventListener('DOMContentLoaded', () => {
    loadCart();
});

function loadCart() {
    const container = document.getElementById("cart-container");
    if (!container) throw "#cart-container not found";
    container.innerHTML = "";

    const userId = container.getAttribute("data-user-id");

    fetch("/api/cart?id=" + userId)
    .then(r => r.json())
    .then(j => {
        let html = `<div class="row"><div class="col col-sm-12 col-lg-10 col-xl-8">`;
        let totalCnt = 0;
        if (j.data == null || j.data.cartProducts == null || j.data.cartProducts.length == 0) {
            html = "Кошик порожній";
        }
        else {
            let total = 0;
            for (let cartProduct of j.data.cartProducts) {
                html += `<div data-cart-id="${j.data.id}" class="row my-2 cart-product-item" data-cp-id="${cartProduct.id}">
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
                            ₴ <span data-role="cart-product-sum"
                                data-price="${cartProduct.product.price}">${cartProduct.cnt * cartProduct.product.price}</span>
                        </div>
                    </div>
                </div>`;
                total += cartProduct.cnt * cartProduct.product.price;
                totalCnt += cartProduct.cnt;
            }
            if (totalCnt > 0) {
                html += `<div class="d-flex align-items-center justify-content-center my-2">
                    <b>Разом: <span data-role="cart-total">${total}</span> грн</b>
                </div>`;
            }
        }
        html += `<div class="d-flex justify-content-between">
            <a href="/Shop" class="btn btn-info">До магазину</a>
            ${totalCnt > 0 ? '<button onclick="buyClick()" class="btn btn-success">Придбати</button>' : ''}
        </div>`;
        html += '</div></div>';

        container.innerHTML = html;
    });
}

function buyClick() {
    const block = document.querySelector("[data-cart-id]");
    const cartId = block.getAttribute("data-cart-id");
    const total = document.querySelector('[data-role="cart-total"]').innerText;
    if (confirm("Підтверджуєте покупку на суму ₴ " + total)) {
        console.log(cartId);
        fetch("/api/cart?cartId=" + cartId, {
            method: 'DELETE'
        }).then(r => r.json()).then(j => {
            console.log(j);
            if (j.data == 'Deleted') {
                loadCart();
            }
            else {
                alert('Виникла помилка');
            }
        });
    }
}
/*  [https://localhost/Shop/Cart]

    href="api/cart"  --> https://localhost/Shop/api/cart
    href="/api/cart"  --> https://localhost/api/cart
    href="//api/cart"  --> https://api/cart
    href="?x"  --> https://localhost/Shop/Cart?x
    href="#x"  --> https://localhost/Shop/Cart#x
*/

function updateTotal() {
    let total = 0;
    for (let s of document.querySelectorAll('[data-role="cart-product-sum"]')) {
        total += Number(s.innerHTML);
    }
    document.querySelector('[data-role="cart-total"]').innerHTML = total;
}

function decrementClick(e) {
    updateCart(e, -1);    
}
function incrementClick(e) {
    updateCart(e, 1);
}

function updateCart(e, increment) {
    const parentBlock = e.target.closest("[data-cp-id]");
    const cpId = parentBlock.getAttribute("data-cp-id");
    fetch(`/api/cart?increment=${increment}&cpId=${cpId}`, {
        method: 'PUT'
    }).then(r => r.json()).then(j => {
        if (j.meta.count == 0) {
            loadCart();
            return;
        }
        const cntBlock = parentBlock.querySelector('[data-role="cart-product-cnt"]');
        cntBlock.innerHTML = j.meta.count;
        const sumBlock = parentBlock.querySelector('[data-role="cart-product-sum"]');
        sumBlock.innerHTML = j.meta.count * sumBlock.getAttribute('data-price');
        updateTotal();
        // console.log(j);
    });
}

/* Д.З. Реалізувати на сторінці кошику відображення загальної кількості
товарів у кошику:
Разом: 123 грн --> Разом 4 товари на суму: 123 грн
Забезпечити оновлення цієї кількості при змінах у кошику (додавання
чи віднімання товарів)
*/