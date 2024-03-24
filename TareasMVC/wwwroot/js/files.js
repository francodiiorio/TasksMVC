let inputFileTask = document.getElementById('taskFile');

function addFileAttachmentHandle() {
    inputFileTask.click();
}

async function taskFileSelectionHandle(event) {
    const files = event.target.files;
    const filesArray = Array.from(files);

    const taskId = taskEditVM.id;
    const formData = new FormData();
    for (var i = 0; i < filesArray.length; i++) {
        formData.append("files", filesArray[i]);
    }
    const response = await fetch(`${filesUrl}/${taskId}`, {
        body: formData,
        method: 'POST'
    });

    if (!response.ok) {
        ErrorApiHandle(response);
        return;
    }

    const json = await response.json();
    prepareFiles(json);

    inputFileTask.value = null;
}

function prepareFiles(files) {
    files.forEach(file => {
        let creationDate = file.date;
        if (file.date.indexOf('Z') === -1) {
            creationDate += 'Z';
        }

        const creationDateDT = new Date(creationDate);
        file.published = creationDateDT.toLocaleString();

        taskEditVM.fileAttachments.push(new fileAttachmentVM({ ...file, editionMode: false }));
    });
}

let previousTitleFile;
function fileTitleHandle(file) {
    file.editionMode(true);
    previousTitleFile = file.title();
    $("[name='txtTitleFile']:visible").focus();
}

async function focusoutFileTitleHandle(file) {
    file.editionMode(false);
    const taskId = file.id;

    if (!file.title()) {
        file.title(previousTitleFile);
    }

    if (file.title() === previousTitleFile) {
        return;
    }

    const data = JSON.stringify(file.title());
    const response = await fetch(`${filesUrl}/${taskId}`, {
        body: data,
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json'
        }
    });

    if (!response.ok) {
        ErrorApiHandle(response);
    }
}

function fileDeleteHandle(file) {
    modalTaskEditBootstrap.hide();

    confirmAction({
        callBackAcept: () => {
            deleteFile(file);
            modalTaskEditBootstrap.show();
        },
        callBackCancel: () => {
            modalTaskEditBootstrap.show();
        },
        title: 'Desea borrar el archivo?'
    });        
}

async function deleteFile(file) {
    const response = await fetch(`${filesUrl}/${file.id}`, {
        method: 'DELETE'
    });

    if (!response.ok) {
        ErrorApiHandle(response);
        return
    }

    taskEditVM.fileAttachments.remove(function (item) { return item.id == file.id });
}

function fileDownloadHandle(file) {
    downloadFile(file.url, file.title());
}

async function fileOrederUpdate() {
    const ids = getFilesId();
    await sendFilesBackend(ids);

    const orderArray = taskEditVM.fileAttachments.sorted(function (a, b) {
        return ids.indexOf(a.id.toString()) - ids.indexOf(b.id.toString());
    })

    taskEditVM.fileAttachments(orderArray);
}

function getFilesId() {
    const ids = $("[name=txtTitleFile]").map(function () {
        return $(this).attr('data-id')
    }).get();
    return ids;
}

async function sendFilesBackend(ids) {
    var data = JSON.stringify(ids);
    const response = await fetch(`${filesUrl}/order/${taskEditVM.id}`, {
        method: 'POST',
        body: data,
        headers: {
            'Content-Type': 'application/json'
        }
    });

    if (!response.ok) {
        ErrorApiHandle(response);
    }
}

$(function () {
    $("#reorder-files").sortable({
        axis: 'y',
        stop: async function () {
            await fileOrederUpdate();
        }
    })
})