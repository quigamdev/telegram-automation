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
    static async schedule_get() {
        return await fetch("/schedule/get");
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
}

class schedulePage {
    static data: any[];
    static chart: google.visualization.Timeline;
    static async loadTimeline() {
        let drawChart = async () => {

            // get API data
            var response = await api.schedule_get();
            schedulePage.data = await response.json();

            var container = document.getElementById('Schedule') as Element;
            container.addEventListener("dblclick", (e) => {
                var selected = schedulePage.getCurrentSelectedItem();
                alert(selected.name)
            });

            this.chart = new google.visualization.Timeline(container);
            var dataTable = new google.visualization.DataTable();

            dataTable.addColumn({ type: 'string', id: 'account' });
            dataTable.addColumn({ type: 'string', id: 'description' });
            dataTable.addColumn({ type: 'date', id: 'start' });
            dataTable.addColumn({ type: 'date', id: 'end' });
            var rows = [];

            for (let row of schedulePage.data) {
                let startDate = row.start ? new Date(0, 0, 0, row.start.hour, row.start.minute, 0) : "";
                let endDate = row.end ? new Date(0, 0, 0, row.end.hour, row.end.minute, 0) : "";
                rows.push([row.name, row.description, startDate, endDate]);
            }

            dataTable.addRows(rows);
            var options = {
                timeline: {
                    colorByRowLabel: false,
                },
                tooltip: {
                    isHtml: false
                }
            } as google.visualization.TimelineOptions;
            this.chart.draw(dataTable, options);
            google.visualization.events.addListener(this.chart, "select", (e: any) => {
                var selected = schedulePage.getCurrentSelectedItem();
                console.log("selected: " + selected.name);
            })
        }

        google.charts.load("current", { packages: ["timeline"] });
        google.charts.setOnLoadCallback(drawChart);
    }

    private static getCurrentSelectedItem() {
        var selected = this.chart.getSelection()[0].row as number;
        return schedulePage.data[selected];
    }
    static async add() {

        var name = (document.querySelector("#bot-name") as HTMLInputElement)?.value
        var start = (document.querySelector("#bot-start") as HTMLInputElement)?.value
        var end = (document.querySelector("#bot-end") as HTMLInputElement)?.value

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

    }
    static initPage() {
        this.loadTimeline();
    }
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


    static initPage() {
    }

}
class general {
    static showMessage(message: string): void {
        const elm = document.querySelector("#message");
        if (elm)
            elm.textContent = message;
    }
}

function initPage(page: string) {
    switch (page) {
        case "Schedule":
            schedulePage.initPage();
            break;
        case "Accounts":
            accountsPage.initPage();
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