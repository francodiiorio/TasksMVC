function addTask() {
    tareaListado.tasks.push(new elementTask({ id: 0, title: '' }));

    $("[name=title-task]").last().focus();
}

async function taskTitleHandle(task) {
    const title = task.title();
    if (!title) {
        tareaListado.tasks.pop();
        return;
    }

    const data = JSON.stringify(title);
    const response = await fetch(tasksUrl, {
        method: 'POST',
        body: data,
        headers: {
            'Content-Type': 'application/json'
        }
    });

    if (response.ok) {
        const json = await response.json();
        task.id(json.id);
    } else {
        ErrorApiHandle(response);
    }
}


async function getTasks() {
    tareaListado.charging(true);
    const response = await fetch(tasksUrl, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        }
    })
    if (!response.ok) {
        ErrorApiHandle(response);
        return;
    }

    const json = await response.json();
    tareaListado.tasks([]);

    json.forEach(value => {
        tareaListado.tasks.push(new elementTask(value));
    });
    tareaListado.charging(false);
}


async function taskOrderUpdate() {
    const ids = getIdTasks();
    await sendTasksIds(ids);

    const orderArray = tareaListado.tasks.sorted(function (a, b) {
        return ids.indexOf(a.id().toString()) - ids.indexOf(b.id().toString());
    });

    tareaListado.tasks([]);
    tareaListado.tasks(orderArray);
}

function getIdTasks() {
    const ids = $("[name=title-task]").map(function () {
        return $(this).attr("data-id")
    }).get();
    return ids;
}

async function sendTasksIds(ids) {
    var data = JSON.stringify(ids);
    await fetch(`${tasksUrl}/order`, {
        method: 'POST',
        body: data,
        headers: {
            'Content-Type': 'application/json'
        }
    });
}

async function clickTaskHandle(task) {
    if (task.isNew()) {
        return
    }
    const response = await fetch(`${tasksUrl}/${task.id()}`, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        }
    });

    if (!response.ok) {
        ErrorApiHandle(response);
        return;
    }
    const json = await response.json();

    taskEditVM.id = json.id;
    taskEditVM.title(json.title);
    taskEditVM.description(json.description);

    taskEditVM.steps([]);
    

    json.steps.forEach(step => {
        taskEditVM.steps.push(
            new stepVM({ ...step, editionMode: false })
        )
    })

    taskEditVM.fileAttachments([]);
    prepareFiles(json.fileAttachments);

    modalTaskEditBootstrap.show();
}

async function taskEditHandle() {
    const obj = {
        id: taskEditVM.id,
        title: taskEditVM.title(),
        description: taskEditVM.description()
    };

    if (!obj.title) {
        return;
    }

    await taskEditComplete(obj);

    const index = tareaListado.tasks().findIndex(t => t.id() === obj.id);
    const task = tareaListado.tasks()[index];
    task.title(obj.title);

}

async function taskEditComplete(task) {
    const data = JSON.stringify(task);
    const response = await fetch(`${tasksUrl}/${task.id}`, {
        method: 'PUT',
        body: data,
        headers: {
            'Content-Type': 'application/json'
        }
    });

    if (!response.ok) {
        ErrorApiHandle(response);
        throw "error";
    }
}

function tryDeleteTask(task) {
    console.log("hola")
    modalTaskEditBootstrap.hide();
    confirmAction({
        callBackAcept: () => {
            deleteTask(task);
        },
        callBackCancel: () => {
            modalTaskEditBootstrap.show();
        },
        title: `Desea borrar la tarea ${task.title()}?`
    })
}

async function deleteTask(task) {
    const idTask = task.id;
    const response = await fetch(`${tasksUrl}/${idTask}`, {
        method: 'DELETE',
        headers: {
            'Content-Type': 'application/json'
        }
    });
    if (response.ok) {
        const index = getTaskEditionId();
        tareaListado.tasks.splice(index, 1);
    }
}

function getTaskEditionId() {
    return tareaListado.tasks().findIndex(t => t.id() == taskEditVM.id);
}

$(function () {
    $("#reordenable").sortable({
        axis: 'y',
        stop: async function () {
            await taskOrderUpdate();
        }
    })
})

function getEditingTask() {
    const index = getTaskEditionId()
    return tareaListado.tasks()[index];
}