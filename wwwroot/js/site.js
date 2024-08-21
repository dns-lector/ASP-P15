﻿document.addEventListener('submit', (e) => {
    const form = e.target;
    if (form.id == 'shop-group-form') {
        e.preventDefault();
        console.log('submit prevented');
        const formData = new FormData(form);
        fetch("/api/group", {
            method: 'POST',
            body: formData
        }).then(r => r.json()).then(j => {
            if (j.status == 'OK') {
                // alert('Додано успішно');
                window.location.reload();
            }
            else {
                alert(j.message);
            }
        });
    }
});


document.addEventListener('DOMContentLoaded', () => {
    const authButton = document.getElementById("auth-button");
    if (authButton) authButton.addEventListener('click', authClick);
    else console.error("auth-button not found");

    const logOutButton = document.getElementById("log-out-button");
    if (logOutButton) logOutButton.addEventListener('click', logOutClick);

    const profileEditButton = document.getElementById("profile-edit");
    if (profileEditButton) profileEditButton.addEventListener('click', profileEditClick);

    const profileDeleteButton = document.getElementById("profile-delete");
    if (profileDeleteButton) profileDeleteButton.addEventListener('click', profileDeleteClick);
});

function profileDeleteClick(e) {
    if (confirm("Підтверджуєте видалення облікового запису?")) {
        fetch(
            "/api/auth", {
            method: "UNLINK",
        })
        .then(r => r.json())
        .then(j => {
            if (j.status == "OK") {
                window.location = "/";
            }
            else {
                alert(j.message);
            }
        });
    }
}

function profileEditClick(e) {
    const btn = e.target;
    const isEditFinish = btn.classList.contains('bi-check2-square');  // <i class="bi bi-check2-square"></i>

    if (isEditFinish) {
        btn.classList.remove('bi-check2-square');
        btn.classList.add('bi-pencil-square');
    }
    else {
        btn.classList.add('bi-check2-square');
        btn.classList.remove('bi-pencil-square');
    }

    let changes = {};
    for (let elem of document.querySelectorAll('[profile-editable]')) {
        if (isEditFinish) {  // завершення редагування
            elem.removeAttribute('contenteditable');
            // треба визначити чи змінювався елемент
            if (elem.initialText != elem.innerText) {  // зміни є
                // дізнаємось назву поля, за яке він відповідає
                const fieldName = elem.getAttribute('profile-editable');
                // console.log(fieldName + ' -> ' + elem.innerText);
                changes[fieldName] = elem.innerText;
            }
        }
        else {  // початок редагування
            // переводимо елементи в режим редагування
            elem.setAttribute('contenteditable', 'true');
            // зберігаємо значення, що було на початок редагування
            elem.initialText = elem.innerText;  // initalText - нове поле, яке
            // JS дозволяє створювати під час роботи (ми самі його придумали)
        }        
    }
    if (isEditFinish) {
        if (Object.keys(changes).length > 0) {
            console.log(changes);
            fetch("/api/auth", {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(changes)
            }).then(r => r.json())
                .then(j => {
                    if (j.status == "OK") {
                        alert(j.message);
                    }
                    else {
                        // а) відновлення початкових даних, оскільки введені
                        // призвели до помилки
                        // for (let elem of document.querySelectorAll('[profile-editable]')) {
                        //     elem.innerText = elem.initialText;
                        // }
                        // б) не вимикаємо режим редагування, точніше включаємо знову
                        for (let elem of document.querySelectorAll('[profile-editable]')) {
                            elem.setAttribute('contenteditable', 'true');
                        }
                        btn.classList.add('bi-check2-square');
                        btn.classList.remove('bi-pencil-square');

                        alert(j.message);
                    }
                });
        }
        // else {
        //     console.log("No changes");
        // }
    }
}

function logOutClick() {
    fetch('/api/auth', {
        method: 'DELETE'
    }).then(r => location.reload());
}

function authClick() {
    const emailInput = document.querySelector('[name="auth-user-email"]');
    if (!emailInput) throw '[name="auth-user-email"] not found';

    const passwordInput = document.querySelector('[name="auth-user-password"]');
    if (!passwordInput) throw '[name="auth-user-password"] not found';

    const errorDiv = document.getElementById("auth-error");
    if (!errorDiv) throw '"auth-error" not found';
    errorDiv.show = err => {
        errorDiv.style.visibility = "visible";
        errorDiv.innerText = err;
    };
    errorDiv.hide = () => {
        errorDiv.style.visibility = "hidden";
        errorDiv.innerText = "";
    };

    const email = emailInput.value.trim();
    const password = passwordInput.value;

    if (email.length === 0) {
        errorDiv.show("Заповніть E-mail");
        return;
    }
    if (password.length === 0) {
        errorDiv.show("Заповніть пароль");
        return;
    }
    errorDiv.hide();
    console.log(email, password);
    fetch(`/api/auth?email=${email}&password=${password}`, {
        method: 'GET'
    }).then(r => r.json()).then(j => {
        console.log(j);
        if (j.code != 200) {
            errorDiv.show("Відмова. Перевірьте введені дані.");
        }
        else {
            window.location.reload();
        }
    });
}

/*
    fun() { ***** }

    --- await fun() -------
    ---  *****  -------

    --- fun().then(++++) -------
    --- | --------
        | ***** ++++ (C#)

    --- v --------   =============
                  | ***** ++++ (JS)
*/
