class api {
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

class schedule {
    static data: any[];
    static chart: google.visualization.Timeline;
    static async loadTimeline() {
        let drawChart = async () => {

            // get API data
            var response = await api.schedule_get();
            schedule.data = await response.json();

            var container = document.getElementById('Schedule') as Element;
            container.addEventListener("dblclick", (e) => {
                var selected = schedule.getCurrentSelectedItem();
                alert(selected.name)
            });

            this.chart = new google.visualization.Timeline(container);
            var dataTable = new google.visualization.DataTable();

            dataTable.addColumn({ type: 'string', id: 'account' });
            dataTable.addColumn({ type: 'string', id: 'description' });
            dataTable.addColumn({ type: 'date', id: 'start' });
            dataTable.addColumn({ type: 'date', id: 'end' });
            var rows = [];
            for (let row of schedule.data) rows.push([row.name, row.description, new Date(0, 0, 0, row.start.hour, row.start.minute, 0), new Date(0, 0, 0, row.end.hour, row.end.minute, 0)]);

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
                var selected = schedule.getCurrentSelectedItem();
                console.log("selected: " + selected.name);
            })
        }

        google.charts.load("current", { packages: ["timeline"] });
        google.charts.setOnLoadCallback(drawChart);
    }

    private static getCurrentSelectedItem() {
        var selected = this.chart.getSelection()[0].row as number;
        return schedule.data[selected];
    }

    static initPage() {
        this.loadTimeline();
    }
}

function initPage(page: string) {
    switch (page) {
        case "Schedule":
            schedule.initPage();
            break;
        default:
    }
}
var chartInstance = null;