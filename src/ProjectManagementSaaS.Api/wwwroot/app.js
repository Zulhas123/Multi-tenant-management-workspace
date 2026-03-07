const state = {
    token: localStorage.getItem("pm_token") ?? "",
    refreshToken: localStorage.getItem("pm_refresh_token") ?? "",
    projects: [],
    users: [],
    logs: [],
    selectedProjectId: null
};

const authStatus = document.getElementById("authStatus");
const sessionBadge = document.getElementById("sessionBadge");
const responseOutput = document.getElementById("responseOutput");
const jwtToken = document.getElementById("jwtToken");
const organizationOutput = document.getElementById("organizationOutput");
const usersOutput = document.getElementById("usersOutput");
const logsOutput = document.getElementById("logsOutput");
const projectsOutput = document.getElementById("projectsOutput");
const projectSelect = document.getElementById("projectSelect");
const taskAssigneeSelect = document.getElementById("taskAssigneeSelect");
const projectCards = document.getElementById("projectCards");
const projectDetailsEmpty = document.getElementById("projectDetailsEmpty");
const projectDetailsContent = document.getElementById("projectDetailsContent");
const selectedProjectName = document.getElementById("selectedProjectName");
const selectedProjectDescription = document.getElementById("selectedProjectDescription");
const selectedProjectStatus = document.getElementById("selectedProjectStatus");
const selectedProjectStart = document.getElementById("selectedProjectStart");
const selectedProjectDue = document.getElementById("selectedProjectDue");
const selectedProjectTaskCount = document.getElementById("selectedProjectTaskCount");
const selectedProjectCompleted = document.getElementById("selectedProjectCompleted");
const projectTasksTableBody = document.getElementById("projectTasksTableBody");
const topWorkspaceName = document.getElementById("topWorkspaceName");
const topUserMeta = document.getElementById("topUserMeta");
const kpiProjects = document.getElementById("kpiProjects");
const kpiProjectsMeta = document.getElementById("kpiProjectsMeta");
const kpiTasks = document.getElementById("kpiTasks");
const kpiTasksMeta = document.getElementById("kpiTasksMeta");
const kpiCompletion = document.getElementById("kpiCompletion");
const kpiCompletionMeta = document.getElementById("kpiCompletionMeta");
const kpiMembers = document.getElementById("kpiMembers");
const kpiMembersMeta = document.getElementById("kpiMembersMeta");
const dashboardCompletionBadge = document.getElementById("dashboardCompletionBadge");
const completionDonut = document.getElementById("completionDonut");
const completionDonutValue = document.getElementById("completionDonutValue");
const dashboardOpenTasks = document.getElementById("dashboardOpenTasks");
const dashboardOpenTasksMeta = document.getElementById("dashboardOpenTasksMeta");
const taskStatusChart = document.getElementById("taskStatusChart");
const teamChart = document.getElementById("teamChart");
const navLinks = Array.from(document.querySelectorAll(".nav-link"));

function setAuthState(token, refreshToken = state.refreshToken) {
    state.token = token ?? "";
    state.refreshToken = refreshToken ?? "";
    if (state.token) {
        localStorage.setItem("pm_token", state.token);
    } else {
        localStorage.removeItem("pm_token");
    }
    if (state.refreshToken) {
        localStorage.setItem("pm_refresh_token", state.refreshToken);
    } else {
        localStorage.removeItem("pm_refresh_token");
    }

    jwtToken.value = state.token;
    authStatus.textContent = state.token ? "Signed in" : "Signed out";
    sessionBadge.textContent = state.token ? "Authenticated" : "Awaiting sign-in";
    topUserMeta.textContent = state.token ? "Authenticated session" : "Anonymous";

    if (!state.token) {
        state.projects = [];
        state.users = [];
        state.logs = [];
        state.selectedProjectId = null;
        topWorkspaceName.textContent = "Not connected";
        writeOutput(organizationOutput, "Sign in to load organization details.");
        writeOutput(usersOutput, "No data loaded.");
        writeOutput(logsOutput, "No data loaded.");
        writeOutput(projectsOutput, "No projects loaded.");
        projectSelect.innerHTML = '<option value="">Load projects first</option>';
        renderAssigneeOptions();
        renderDashboard();
        projectCards.innerHTML = '<p class="empty-state">No projects loaded.</p>';
        renderProjectDetails(null);
    }
}

function updateSessionSummary(authResponse) {
    if (!authResponse) {
        topUserMeta.textContent = "Anonymous";
        return;
    }

    topUserMeta.textContent = `${authResponse.fullName} / ${authResponse.role}`;
}

function renderAssigneeOptions() {
    const selectedValue = taskAssigneeSelect.value;
    const members = state.users.filter(user => user.role === "Member");

    taskAssigneeSelect.innerHTML = '<option value="">Unassigned</option>';

    members.forEach(member => {
        const option = document.createElement("option");
        option.value = member.userId;
        option.textContent = `${member.fullName} (${member.email})`;
        taskAssigneeSelect.appendChild(option);
    });

    taskAssigneeSelect.disabled = members.length === 0;

    if (members.some(member => member.userId === selectedValue)) {
        taskAssigneeSelect.value = selectedValue;
    }
}

function createBarChartItem(label, value, total, toneClass, detail) {
    const safeTotal = total > 0 ? total : 1;
    const width = Math.max((value / safeTotal) * 100, value > 0 ? 6 : 0);

    return `
        <div class="bar-chart-item">
            <div class="bar-chart-header">
                <strong>${label}</strong>
                <span>${value}</span>
            </div>
            <div class="bar-chart-track">
                <div class="bar-chart-fill ${toneClass}" style="width: ${width}%"></div>
            </div>
            <p>${detail}</p>
        </div>
    `;
}

function renderDashboard() {
    const tasks = state.projects.flatMap(project => project.tasks ?? []);
    const completedTasks = tasks.filter(task => task.status === "Done").length;
    const openTasks = Math.max(tasks.length - completedTasks, 0);
    const activeProjects = state.projects.filter(project => project.status === "Active").length;
    const members = state.users.filter(user => user.role === "Member").length;
    const managers = state.users.filter(user => user.role === "Manager").length;
    const admins = state.users.filter(user => user.role === "Admin").length;
    const completionRate = tasks.length ? Math.round((completedTasks / tasks.length) * 100) : 0;

    const taskStatusCounts = {
        Todo: tasks.filter(task => task.status === "Todo").length,
        InProgress: tasks.filter(task => task.status === "InProgress").length,
        Done: completedTasks,
        Blocked: tasks.filter(task => task.status === "Blocked").length
    };

    kpiProjects.textContent = String(state.projects.length);
    kpiProjectsMeta.textContent = state.projects.length
        ? `${activeProjects} active project(s) in the current workspace.`
        : "Load projects to view portfolio coverage.";

    kpiTasks.textContent = String(tasks.length);
    kpiTasksMeta.textContent = tasks.length
        ? `${openTasks} open and ${completedTasks} completed task(s).`
        : "Task execution totals appear here.";

    kpiCompletion.textContent = `${completionRate}%`;
    kpiCompletionMeta.textContent = tasks.length
        ? `${completedTasks} of ${tasks.length} tasks are done.`
        : "Completed task ratio across all projects.";

    kpiMembers.textContent = String(members);
    kpiMembersMeta.textContent = state.users.length
        ? `${admins} admin, ${managers} manager, ${members} member seats loaded.`
        : "Members available for assignment.";

    dashboardCompletionBadge.textContent = tasks.length ? `${completionRate}% complete` : "No data";
    completionDonutValue.textContent = `${completionRate}%`;
    completionDonut.style.setProperty("--chart-angle", `${Math.round((completionRate / 100) * 360)}deg`);
    dashboardOpenTasks.textContent = `${openTasks} open task${openTasks === 1 ? "" : "s"}`;
    dashboardOpenTasksMeta.textContent = state.logs.length
        ? `${state.logs.length} activity log entr${state.logs.length === 1 ? "y" : "ies"} recorded for this workspace.`
        : "Create or load projects to populate the dashboard.";

    const maxTaskStatus = Math.max(...Object.values(taskStatusCounts), 0);
    taskStatusChart.innerHTML = tasks.length
        ? [
            createBarChartItem("To Do", taskStatusCounts.Todo, maxTaskStatus, "bar-tone-neutral", "Planned work not yet started."),
            createBarChartItem("In Progress", taskStatusCounts.InProgress, maxTaskStatus, "bar-tone-progress", "Active delivery currently underway."),
            createBarChartItem("Done", taskStatusCounts.Done, maxTaskStatus, "bar-tone-active", "Completed work items across all projects."),
            createBarChartItem("Blocked", taskStatusCounts.Blocked, maxTaskStatus, "bar-tone-blocked", "Items that need intervention or unblocking.")
        ].join("")
        : '<p class="empty-state">No task data loaded.</p>';

    const totalPeopleSignals = Math.max(state.users.length, state.logs.length, 1);
    teamChart.innerHTML = state.users.length || state.logs.length
        ? [
            createBarChartItem("Admins", admins, totalPeopleSignals, "bar-tone-neutral", "Workspace owners and platform administrators."),
            createBarChartItem("Managers", managers, totalPeopleSignals, "bar-tone-progress", "Delivery coordinators with planning access."),
            createBarChartItem("Members", members, totalPeopleSignals, "bar-tone-active", "Assignable contributors for execution."),
            createBarChartItem("Activity Logs", state.logs.length, totalPeopleSignals, "bar-tone-blocked", "Tracked audit events captured by the API.")
        ].join("")
        : '<p class="empty-state">Load workspace data to see team distribution.</p>';
}

function writeOutput(element, data) {
    element.textContent = typeof data === "string" ? data : JSON.stringify(data, null, 2);
}

function normalizeDate(value) {
    return value ? new Date(`${value}T00:00:00`).toISOString() : null;
}

function formatDate(value) {
    if (!value) {
        return "-";
    }

    return new Date(value).toLocaleDateString(undefined, {
        year: "numeric",
        month: "short",
        day: "numeric"
    });
}

function getStatusClass(status) {
    return `status-${String(status).toLowerCase().replace(/[^a-z]/g, "")}`;
}

function renderProjectCards() {
    if (!state.projects.length) {
        projectCards.innerHTML = '<p class="empty-state">No projects loaded.</p>';
        return;
    }

    projectCards.innerHTML = "";
    state.projects.forEach(project => {
        const button = document.createElement("button");
        button.type = "button";
        button.className = `project-card-button${project.projectId === state.selectedProjectId ? " active" : ""}`;
        button.innerHTML = `
            <strong>${project.name}</strong>
            <span>${project.status} • ${project.tasks.length} task(s)</span>
            <span>${project.description ?? "No description provided."}</span>
        `;
        button.addEventListener("click", () => {
            state.selectedProjectId = project.projectId;
            renderProjectCards();
            renderProjectDetails(project);
        });
        projectCards.appendChild(button);
    });
}

function renderProjectDetails(project) {
    if (!project) {
        projectDetailsEmpty.classList.remove("hidden");
        projectDetailsContent.classList.add("hidden");
        return;
    }

    projectDetailsEmpty.classList.add("hidden");
    projectDetailsContent.classList.remove("hidden");

    const completedTasks = project.tasks.filter(task => task.status === "Done").length;

    selectedProjectName.textContent = project.name;
    selectedProjectDescription.textContent = project.description ?? "No project description provided.";
    selectedProjectStatus.textContent = project.status;
    selectedProjectStart.textContent = formatDate(project.startDateUtc);
    selectedProjectDue.textContent = formatDate(project.dueDateUtc);
    selectedProjectTaskCount.textContent = String(project.tasks.length);
    selectedProjectCompleted.textContent = String(completedTasks);

    if (!project.tasks.length) {
        projectTasksTableBody.innerHTML = '<tr><td colspan="4">No tasks available for this project.</td></tr>';
        return;
    }

    projectTasksTableBody.innerHTML = "";
    project.tasks.forEach(task => {
        const row = document.createElement("tr");
        row.innerHTML = `
            <td class="task-title-cell">
                <strong>${task.title}</strong>
                <span>Task ID: ${task.taskId}</span>
            </td>
            <td><span class="task-status-chip ${getStatusClass(task.status)}">${task.status}</span></td>
            <td>${task.assigneeName ?? '<span class="muted-cell">Unassigned</span>'}</td>
            <td>${formatDate(task.dueDateUtc)}</td>
        `;
        projectTasksTableBody.appendChild(row);
    });
}

async function apiFetch(url, options = {}) {
    const headers = {
        "Content-Type": "application/json",
        ...(options.headers ?? {})
    };

    if (state.token) {
        headers.Authorization = `Bearer ${state.token}`;
    }

    const response = await fetch(url, { ...options, headers });
    const contentType = response.headers.get("content-type") ?? "";
    const payload = contentType.includes("application/json")
        ? await response.json()
        : await response.text();

    if (!response.ok) {
        throw new Error(typeof payload === "string" ? payload : payload.error ?? "Request failed.");
    }

    writeOutput(responseOutput, payload);
    return payload;
}

function bindForm(formId, handler) {
    document.getElementById(formId).addEventListener("submit", async event => {
        event.preventDefault();
        const formData = new FormData(event.target);

        try {
            await handler(formData, event.target);
        } catch (error) {
            writeOutput(responseOutput, error.message);
        }
    });
}

async function loadProjects() {
    const projects = await apiFetch("/api/v1/projects");
    state.projects = projects;

    projectSelect.innerHTML = "";
    if (!projects.length) {
        projectSelect.innerHTML = '<option value="">No projects available</option>';
    } else {
        projects.forEach(project => {
            const option = document.createElement("option");
            option.value = project.projectId;
            option.textContent = project.name;
            projectSelect.appendChild(option);
        });
    }

    if (!state.selectedProjectId || !projects.some(project => project.projectId === state.selectedProjectId)) {
        state.selectedProjectId = projects[0]?.projectId ?? null;
    }

    renderProjectCards();
    renderProjectDetails(projects.find(project => project.projectId === state.selectedProjectId) ?? null);
    renderDashboard();
    writeOutput(projectsOutput, projects);
}

async function loadWorkspace() {
    const [organization, users, logs] = await Promise.all([
        apiFetch("/api/v1/organization"),
        apiFetch("/api/v1/organization/users"),
        apiFetch("/api/v1/organization/activity-logs")
    ]);

    topWorkspaceName.textContent = organization.name ?? "Connected";
    state.users = users;
    state.logs = logs;
    renderAssigneeOptions();
    renderDashboard();
    topUserMeta.textContent = users[0]
        ? `${users[0].fullName} / ${users[0].role}`
        : "Authenticated user";

    writeOutput(organizationOutput, organization);
    writeOutput(usersOutput, users);
    writeOutput(logsOutput, logs);
}

bindForm("registerForm", async formData => {
    const payload = {
        organizationName: formData.get("organizationName"),
        organizationSlug: formData.get("organizationSlug"),
        adminFullName: formData.get("adminFullName"),
        adminEmail: formData.get("adminEmail"),
        password: formData.get("password")
    };

    const result = await apiFetch("/api/v1/auth/register", {
        method: "POST",
        body: JSON.stringify(payload)
    });

    setAuthState(result.token, result.refreshToken);
    updateSessionSummary(result);
    await loadWorkspace();
    await loadProjects();
});

bindForm("loginForm", async formData => {
    const result = await apiFetch("/api/v1/auth/login", {
        method: "POST",
        body: JSON.stringify({
            email: formData.get("email"),
            password: formData.get("password")
        })
    });

    setAuthState(result.token, result.refreshToken);
    updateSessionSummary(result);
    await loadWorkspace();
    await loadProjects();
});

bindForm("projectForm", async formData => {
    await apiFetch("/api/v1/projects", {
        method: "POST",
        body: JSON.stringify({
            name: formData.get("name"),
            description: formData.get("description"),
            status: Number(formData.get("status")),
            startDateUtc: normalizeDate(formData.get("startDateUtc")),
            dueDateUtc: normalizeDate(formData.get("dueDateUtc"))
        })
    });

    await loadProjects();
    await loadWorkspace();
});

bindForm("taskForm", async formData => {
    await apiFetch("/api/v1/tasks", {
        method: "POST",
        body: JSON.stringify({
            projectId: formData.get("projectId"),
            title: formData.get("title"),
            description: formData.get("description"),
            assignedToUserId: formData.get("assignedToUserId") || null,
            priority: Number(formData.get("priority")),
            dueDateUtc: normalizeDate(formData.get("dueDateUtc"))
        })
    });

    await loadProjects();
    await loadWorkspace();
});

document.getElementById("loadWorkspaceBtn").addEventListener("click", async () => {
    try {
        await loadWorkspace();
    } catch (error) {
        writeOutput(responseOutput, error.message);
    }
});

document.getElementById("loadProjectsBtn").addEventListener("click", async () => {
    try {
        await loadProjects();
    } catch (error) {
        writeOutput(responseOutput, error.message);
    }
});

document.getElementById("signOutBtn").addEventListener("click", async () => {
    try {
        if (state.token && state.refreshToken) {
            await apiFetch("/api/v1/auth/logout", {
                method: "POST",
                body: JSON.stringify({ refreshToken: state.refreshToken })
            });
        }
    } catch (error) {
        writeOutput(responseOutput, error.message);
    } finally {
        setAuthState("", "");
        updateSessionSummary(null);
    writeOutput(responseOutput, "Signed out.");
    }
});

async function tryRefreshSession() {
    if (state.token || !state.refreshToken) {
        return;
    }

    try {
        const result = await apiFetch("/api/v1/auth/refresh", {
            method: "POST",
            body: JSON.stringify({ refreshToken: state.refreshToken })
        });

        setAuthState(result.token, result.refreshToken);
        updateSessionSummary(result);
        await loadWorkspace();
        await loadProjects();
    } catch {
        setAuthState("", "");
        updateSessionSummary(null);
    }
}

window.addEventListener("scroll", () => {
    let activeId = "overview";

    for (const section of document.querySelectorAll(".content-section")) {
        const bounds = section.getBoundingClientRect();
        if (bounds.top <= 140 && bounds.bottom >= 140) {
            activeId = section.id;
            break;
        }
    }

    navLinks.forEach(link => {
        link.classList.toggle("active", link.getAttribute("href") === `#${activeId}`);
    });
});

setAuthState(state.token);
renderAssigneeOptions();
renderDashboard();
updateSessionSummary(null);
writeOutput(responseOutput, "Ready. Register a workspace or log in.");
tryRefreshSession();
