import Cookies from "js-cookie"
import { Notification, Loading } from "element-ui"
import { ElLoadingComponent } from "element-ui/types/loading";
import { argKey, inputKey, outputKey } from "./parameters";
let loadingInstance: ElLoadingComponent | null = null;
export function showLoading(): void {
    loadingInstance = Loading.service({});
}
export function closeLoading(): void {
    (loadingInstance as ElLoadingComponent).close();
}


export function formatDoubleTimeSpan(seconds: number, includeMs = false): string {
    const h = Math.floor(seconds / 3600);
    const m = Math.floor((seconds / 60 % 60));
    const s = seconds - m * 60 - h * 3600;

    if (includeMs) {
        return h > 0 ? (String(h) + ":") : ""
            + String(m).padStart(2, '0') + ":"
            + String(Math.floor(s)).padStart(2, '0') + "."
            + String(s - Math.floor(s)).substr(2, 2)
    }
    else {
        return String(h).padStart(2, '0') + ":"
            + String(m).padStart(2, '0') + ":"
            + String(Math.floor(s)).padStart(2, '0');
    }
}
export function formatDateTime(time: Date | string, includeDate = true, includeTime = true, includeYear = true): string {
    if (typeof time == "string") {
        time = new Date(time);
    }
    time = time as Date;
    const strDate = includeYear ?
        time.getFullYear().toString().padStart(4, '0') + "-"
        + (time.getMonth() + 1).toString().padStart(2, '0') + "-"
        + time.getDate().toString().padStart(2, '0')
        :
        (time.getMonth() + 1).toString() + "-"
        + time.getDate().toString().padStart(2, '0');

    const strTime = time.getHours().toString().padStart(2, '0') + ":"
        + time.getMinutes().toString().padStart(2, '0') + ":"
        + time.getSeconds().toString().padStart(2, '0');
    if (includeDate && includeTime) {
        return strDate + " " + strTime
    }
    if (includeDate) {
        return strDate;
    }
    return strTime;
}


export function showError(r: any) {
    console.log(r);
    Notification.error({ title: "错误", message: r.response ? r.response.data : r });
}
export function showSuccess(msg: any): void {
    Notification.success({ title: "成功", message: msg });
}

export function jump(url: string): void {
    window.location.href = process.env.BASE_URL + "#/" + url;
}


export function loadArgs(argsComponent: any): any {
    if (argsComponent && localStorage.getItem(argKey) != null) {
        const args = JSON.parse(localStorage.getItem(argKey) as string);
        try {
            argsComponent.updateFromArgs(args);
            showSuccess("已加载参数");

        } catch (error) {
            showError("加载参数失败：" + error);
            console.log("错误参数为", args);

            throw error;
        } finally {
            localStorage.removeItem(argKey);
        }
    }
    const result: any = {}
    if (localStorage.getItem(outputKey) != null) {
        result.output = localStorage.getItem(outputKey);
    }
    if (localStorage.getItem(inputKey) != null) {
        result.inputs = JSON.parse(localStorage.getItem(inputKey) as string);
    }
    localStorage.removeItem(argKey);
    localStorage.removeItem(outputKey);
    localStorage.removeItem(inputKey);
    return result;
}
export function getTaskTypeDescription(type: number): string {
    const t = TaskType.GetByID(type);
    return t.Description;

}

export function jumpByArgs(args: any, input: Array<string>, output: string, type: number): void {
    localStorage.setItem(argKey, JSON.stringify(args));
    localStorage.setItem(inputKey, JSON.stringify(input));
    localStorage.setItem(outputKey, output);
    const t = TaskType.GetByID(type)

    jump("add/" + t.Route);
}


export class TaskType {
    public static Types = [
        new TaskType(0, "code","Code", "转码"),
        new TaskType(1, "combine","Combine", "合并音视频"),
        new TaskType(2, "compare","Compare", "视频对比"),
        new TaskType(3, "custom", "Custom","自定义"),
    ]
    public static GetByID(id: number): TaskType {
        const types = this.Types.filter(p => p.Id == id);
        if (types.length == 0) {
            throw new Error("未知类型：" + id);
        }
        return types[0];
    }
    public constructor(id: number, route: string,name:string, desc: string) {
        this.Id = id;
        this.Route = route;
        this.Name=name;
        this.Description = desc;
    }
    public Id: number;
    public Name: string;
    public Route: string;
    public Description: string
}