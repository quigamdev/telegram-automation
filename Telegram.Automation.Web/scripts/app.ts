class api {
    static headers = {
        'Content-Type': 'application/json',
    };
    static async schedule_add(newItem: scheduleItem) {
        return await fetch("/schedule/add", {
            method: "POST",
            body: JSON.stringify(newItem),
            headers: api.headers
        });
    }
    static async schedule_get(id: number) {
        return await fetch(`/schedule/get/${id}`);
    }
    static async accounts_start(id: string) {
        return await fetch(`/account/start/${id}`, {
            method: "POST"
        });
    }
    static async accounts_stop(id: string) {
        return await fetch(`/account/stop/${id}`, {
            method: "POST",
        });
    }
    static async schedule_createSchedule(data: { name: string }) {
        return await fetch(`/schedule/createSchedule?name=${name}`, {
            method: "POST",
            headers: this.headers, 
        });
    }
}

class schedulePage {
    static data: any[];
    static chart: google.visualization.Timeline;
    static dataTable: google.visualization.DataTable;
    static async loadTimeline(params: SchedulePageParams) {
        google.charts.load("current", { packages: ["timeline"] });
        google.charts.setOnLoadCallback(async () => {
            await this.initChart();
            await schedulePage.reloadData(params.id);
        });

    }
    static async initChart() {

        var container = document.getElementById('Schedule') as Element;
        container.addEventListener("dblclick", (e) => {
            var selected = schedulePage.getCurrentSelectedItem();
            alert(selected.name)
        });

        this.chart = new google.visualization.Timeline(container);
        this.dataTable = new google.visualization.DataTable();
        let dataTable = this.dataTable;

        dataTable.addColumn({ type: 'string', id: 'account' });
        dataTable.addColumn({ type: 'string', id: 'description' });
        dataTable.addColumn({ type: 'date', id: 'start' });
        dataTable.addColumn({ type: 'date', id: 'end' });

        google.visualization.events.addListener(this.chart, "select", (e: any) => {
            var selected = schedulePage.getCurrentSelectedItem();
            console.log("selected: " + selected.name);
        })
    }

    private static async reloadData(scheduleId: number) {
        var response = await api.schedule_get(scheduleId);
        schedulePage.data = await response.json();

        var rows = [];

        for (let row of schedulePage.data) {
            let startDate = row.start ? new Date(0, 0, 0, row.start.hour, row.start.minute, 0) : "";
            let endDate = row.end ? new Date(0, 0, 0, row.end.hour, row.end.minute, 0) : "";
            rows.push([row.name, "", startDate, endDate]);
        }
        this.dataTable.removeRows(0, this.dataTable.getNumberOfRows())
        this.dataTable.addRows(rows);

        var options = {
            timeline: {
                colorByRowLabel: true,
                showBarLabels: false
            },
            tooltip: {
                isHtml: true,
                trigger: "focus"
            }
        } as google.visualization.TimelineOptions;
        this.chart.draw(this.dataTable, options);
    }

    private static getCurrentSelectedItem() {
        var selected = this.chart.getSelection()[0].row as number;
        return schedulePage.data[selected];
    }
    static async add() {

        var name = (document.querySelector("#bot-name") as HTMLInputElement)?.value
        var start = (document.querySelector("#bot-start") as HTMLInputElement)?.value
        var end = (document.querySelector("#bot-end") as HTMLInputElement)?.value
        var scheduleId = 1; // TODO: add as parameter

        let newItem = {
            name,
            description: "",
            start: {
                hour: parseInt(start.split(":")[0]),
                minute: parseInt(start.split(":")[1])
            },
            end: {
                hour: parseInt(end.split(":")[0]),
                minute: parseInt(end.split(":")[1])
            }
        } as scheduleItem;
        await api.schedule_add(newItem);
        await this.reloadData(scheduleId);

    }
    static async initPage(data: any) {
        await this.loadTimeline(data as SchedulePageParams);
    }
}
interface SchedulePageParams {
    id: number
}

class accountsPage {
    static async handle_Start(elm: HTMLElement) {
        let td = elm.parentElement;
        let accountName = td?.dataset["name"] as string;
        if (!accountName) return;

        let response = await api.accounts_start(accountName);
        let data = await response.json();
        general.showMessage(data);

    }
    static async handle_Stop(elm: HTMLElement) {
        let td = elm.parentElement;
        var accountName = td?.dataset["name"] as string;
        if (!accountName) return;

        let response = await api.accounts_stop(accountName);
        let data = await response.json();
        general.showMessage(data);
    }


    static initPage(data: any) {
    }

    static async handle_CaptureAccounts() {
        await api.schedule_createSchedule({name: "Two Acc Schedule"}); // TODO: add as parameter
    }
}
class general {
    static showMessage(message: string): void {
        const elm = document.querySelector("#message");
        if (elm)
            elm.textContent = message;
    }
}

function initPage(page: string, data: any) {
    switch (page) {
        case "Schedule":
            schedulePage.initPage(data);
            break;
        case "Accounts":
            accountsPage.initPage(data);
            break;

        default:
    }
}
var chartInstance = null;

interface scheduleItem {
    name: string
    description: string
    start: scheduleTime
    end: scheduleTime
}
interface scheduleTime {
    hour: number
    minute: number
}