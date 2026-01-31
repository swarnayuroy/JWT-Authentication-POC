function showPassword(formType) {
    switch (formType) {
        case "signIn":
            const signInPasswordField = document.getElementById("signIn_password_input");
            const signInShowPasswordButton = document.getElementById("form_signIn_password_show");
            const signInHidePasswordButton = document.getElementById("form_signIn_password_hide");

            signInPasswordField.type = "text";
            signInShowPasswordButton.style.display = "none";
            signInHidePasswordButton.style.display = "block";
            break;
        case "registration":
            const registrationPasswordField = document.getElementById("registration_password_input");
            const registrationShowPasswordButton = document.getElementById("form_registration_password_show");
            const registrationHidePasswordButton = document.getElementById("form_registration_password_hide");

            registrationPasswordField.type = "text";
            registrationShowPasswordButton.style.display = "none";
            registrationHidePasswordButton.style.display = "block";
            break;
        default:
            console.error("Unknown formType in showPassword:", formType);
            break;
    }
}

function hidePassword(formType) {
    switch (formType) {
        case "signIn":
            const signInPasswordField = document.getElementById("signIn_password_input");
            const signInShowPasswordButton = document.getElementById("form_signIn_password_show");
            const signInHidePasswordButton = document.getElementById("form_signIn_password_hide");

            signInPasswordField.type = "password";
            signInShowPasswordButton.style.display = "block";
            signInHidePasswordButton.style.display = "none";
            break;
        case "registration":
            const registrationPasswordField = document.getElementById("registration_password_input");
            const registrationShowPasswordButton = document.getElementById("form_registration_password_show");
            const registrationHidePasswordButton = document.getElementById("form_registration_password_hide");

            registrationPasswordField.type = "password";
            registrationShowPasswordButton.style.display = "block";
            registrationHidePasswordButton.style.display = "none";
            break;
        default:
            console.error("Unknown formType in hidePassword:", formType);
            break;
    }
}
