async function ErrorApiHandle(response) {
    let errorMsj = '';

    if (response.status === 400) {
        errorMsj = await response.text();
    } else if (response.status === 404) {
        errorMsj = resourceNotFound;
    } else {
        errorMsj = unexpectedError;
    }

    showErrorMsj(errorMsj);

}

function showErrorMsj(msj) {
    Swal.fire({
        icon: 'error',
        title: 'Error',
        text: msj
    });
}


function confirmAction({ callBackAcept, callBackCancel, title }) {
    Swal.fire({
        title: title || 'Deseas confirmar la accion',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Si',
        focusConfirm: true
    }).then((result) => {
        if (result.isConfirmed) {
            callBackAcept();
        } else if (callBackCancel) {
            callBackCancel();
        }
    })
}

function downloadFile(url, name) {
    var link = document.createElement('a');
    link.download = name;
    link.target = "_blank";
    link.href = url;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    delete link;
}