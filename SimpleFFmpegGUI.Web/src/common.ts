import Cookies from "js-cookie"
import { Notification, Loading } from "element-ui"
import { ElLoadingComponent } from "element-ui/types/loading";
let loadingInstance: ElLoadingComponent | null = null;
export function showLoading(): void {
    loadingInstance = Loading.service({});
}
export function closeLoading(): void {
    (loadingInstance as ElLoadingComponent).close();
}
export function withToken(obj: any): any {
    console.log(Cookies.get("token"))
    const request = {
        UserID: Number.parseInt(Cookies.get("userID") ?? "0"),
        Token: Cookies.get("token")
    }
    Object.assign(request, obj);
    console.log("send request", request);
    return request;
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


export function loadArgs(codeArguments: any) {
    if (localStorage.getItem("codeArgs") != null) {
        const args = JSON.parse(localStorage.getItem("codeArgs") as string);
        try {
            codeArguments.updateFromArgs(args);
            showSuccess("已加载参数");
        } catch (error) {
            showError("加载参数失败");
            console.log("错误参数为",args);
            
            throw error;
        } finally {
            localStorage.removeItem("codeArgs");
        }
    }
}
export function getTaskTypeDescription(type: number): string {
    switch (type) {
        case 0:
            return "转码";
        case 1:
            return "合并视音频"
        default:
            return type.toString();
    }
}
export function stringType2Number(type:string)
{
    switch (type) {
        case "code":
            return 0;
        case "combine":
            return 1
        default:
            throw new Error("未知类型：" + type);
    }
}
export function jumpByArgs(args: any, type: number) {
    localStorage.setItem("codeArgs", JSON.stringify(args));
    switch (type) {
        case 0:
            jump("add/code");
            break;
        case 1:
            jump("add/combine");
            break;
        default:
            throw new Error("未知类型：" + type);
    }
}
