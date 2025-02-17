function successAlert(title, text, timer) {
    if (!timer) timer = 1500;
    Swal.fire({
        icon: 'success',
        title: title,
        text: text,
        timer: timer,
        timerProgressBar: true,
        showConfirmButton: false
    });
}

function errorAlert(title, text, timer) {
    Swal.fire({
        icon: 'error',
        title: title,
        text: text,
        timer: timer,
        timerProgressBar: true,
        showConfirmButton: false
    });
}