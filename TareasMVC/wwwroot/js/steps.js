function addStepHandle() {
    const index = taskEditVM.steps().findIndex(p => p.isNew());

    if (index !== -1) {
        return
    }

    taskEditVM.steps.push(new stepVM({ editionMode: true, complete: false }));
    $("[name=txtStepDescription]:visible").focus();
}

function cancelStepHandle(step) {
    if (step.isNew()) {
        taskEditVM.steps.pop();
    } else {
        step.editionMode(false);
        step.description(step.aboveDescription);
    }
}

async function saveStepHandle(step) {
    step.editionMode(false);
    const isNew = step.isNew();
    const taskId = taskEditVM.id;
    const data = getBodyStepRequest(step);
    const description = step.description();

    if (!description) {
        step.description(step.aboveDescription);

        if (isNew) {
            taskEditVM.steps.pop();
        }
        return;
    }

    if (isNew) {
        await insertStep(step, data, taskId)
    } else {
        updateStep(data, step.id());
    }
}

async function insertStep(step, data, taskId) {
    const response = await fetch(`${stepsUrl}/${taskId}`, {
        body: data,
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        }
    });

    if (response.ok) {
        const json = await response.json();
        step.id(json.id);

        const task = getEditingTask();
        task.totalSteps(task.totalSteps() + 1);

        if (task.complete()) {
            task.completeSteps(task.completeSteps() + 1);
        }

    } else {
        ErrorApiHandle(response);
    }
}

function getBodyStepRequest(step) {
    return JSON.stringify({
        description: step.description(),
        complete: step.complete()
    });
} 

function stepDescriptionClickHandle(step) {
    step.editionMode(true);
    step.aboveDescription = step.description();
    $("[name=txtStepDescription]:visible").focus();
}

async function updateStep(data, id) {
    const response = await fetch(`${stepsUrl}/${id}`, {
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

function checkboxStepHandle(step) {
    if (step.isNew()) {
        return true
    }

    const data = getBodyStepRequest(step);
    updateStep(data, step.id());

    const task = getEditingTask();
    let currentCompleteSteps = task.completeSteps();

    if (step.complete()) {
        currentCompleteSteps++;
    } else {
        currentCompleteSteps--;
    }

    task.completeSteps(currentCompleteSteps);

    return true
}

function deleteStepHandle(step) {
    modalTaskEditBootstrap.hide();
    confirmAction({
        callBackAcept: () => {
            deleteStep(step);
            modalTaskEditBootstrap.show();
        },
        callBackCancel: () => {
            modalTaskEditBootstrap.show();
        },
        title: `Desea borrar este paso?`
    })
}

async function deleteStep(step) {
    const response = await fetch(`${stepsUrl}/${step.id()}`, {
        method: 'DELETE'
    });

    if (!response.ok) {
        ErrorApiHandle(response)
        return
    }

    taskEditVM.steps.remove(function (item) { return item.id() == step.id() });

    const task = getEditingTask();
    task.totalSteps(task.totalSteps() - 1);

    if (step.complete()) {
        task.completeSteps(task.completeSteps() - 1);
    }
}

async function fileOrederUpdate() {
    const ids = getFilesId();
    await sendFilesBackend(ids);

    const orderArray = taskEditVM.steps.sorted(function (a, b) {
        return ids.indexOf(a.id().toString()) - ids.indexOf(b.id().toString());
    })

    taskEditVM.steps(orderArray);
}

function getFilesId() {
    const ids = $("[name=chbStep]").map(function () {
        return $(this).attr('data-id')
    }).get();
    return ids;
}

async function sendFilesBackend(ids) {
    var data = JSON.stringify(ids);
    await fetch(`${stepsUrl}/order/${taskEditVM.id}`, {
        method: 'POST',
        body: data,
        headers: {
            'Content-Type': 'application/json'
        }
    });
}

$(function () {
    $("#reorder-steps").sortable({
        axis: 'y',
        stop: async function () {
            await fileOrederUpdate();
        }
    })
})