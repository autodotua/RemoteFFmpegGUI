import Cookies from "js-cookie"
import { Notification } from "element-ui"
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

export function formatCSharpTimeSpan(time: any, includeMs = false): string {
    let str = String(time.days * 24 + time.hours).padStart(2, '0') + ":"
        + String(time.minutes).padStart(2, '0') + ":"
        + String(time.seconds).padStart(2, '0')
    if (includeMs) {
        str += "." + String(time.milliseconds).substr(0,2)
    } 
    return str;
}
export function formatDateTime(time: Date | string, includeDate = true, includeTime = true): string {
    if (typeof time == "string") {
        time = new Date(time);
    }
    time = time as Date;


    const strDate = time.getFullYear().toString().padStart(4, '0') + "-"
        + (time.getMonth() + 1).toString().padStart(2, '0') + "-"
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

export function getUrl(controller: string) {
    return `https://localhost:44305/${controller}`;
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