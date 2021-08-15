import Vue from "vue";
import { AxiosResponse } from "axios";


function getUrl(controller: string): string {
        return `/api/${controller}`;//发布
        // return `https://localhost:44305/${controller}`;//调试
}

export function postResetTask(id: number): Promise<AxiosResponse<any>> {
        return Vue.axios
                .post(getUrl("Task/Reset?id=" + id));
}

export function postResetTasks(ids: number[]): Promise<AxiosResponse<any>> {
        return Vue.axios
                .post(getUrl("Task/Reset/List"), ids);
}

export function postCancelTasks(ids: number[]): Promise<AxiosResponse<any>> {
        return Vue.axios
                .post(getUrl("Task/Cancel/List"), ids);
}

export function postDeleteTasks(ids: number[]): Promise<AxiosResponse<any>> {
        return Vue.axios
                .post(getUrl("Task/Delete/List"), ids);
}
export function postCancelTask(id: number): Promise<AxiosResponse<any>> {
        return Vue.axios
                .post(getUrl("Task/Cancel?id=" + id));
}
export function postCancelQueue(): Promise<AxiosResponse<any>> {
        return Vue.axios
                .post(getUrl("Queue/Cancel"));
}
export function postStartQueue(): Promise<AxiosResponse<any>> {
        return Vue.axios
                .post(getUrl("Queue/Start"));
}
export function postAddCodeTask(item: any): Promise<AxiosResponse<any>> {
        return Vue.axios
                .post(getUrl("Task/Add/Code"), item);
}
export function getQueueStatus(): Promise<AxiosResponse<any>> {
        return Vue.axios
                .get(getUrl("Queue/Status"))
}
export function getTaskList(status: number | null, skip: number | null, take: number | null): Promise<AxiosResponse<any>> {
        let url = "Task/List?";
        if (status) {
                url = url + `status=${status}&`
        }
        if (skip) {
                url = url + `skip=${skip}&`
        }
        if (take) {
                url = url + `take=${take}&`
        }
        return Vue.axios
                .get(getUrl(url))
}

export function getMediaInfo(name: string): Promise<AxiosResponse<any>> {
        return Vue.axios
                .get(getUrl("MediaInfo") + "?name=" + name)
}

export function getMediaNames(): Promise<AxiosResponse<any>> {
        return Vue.axios
                .get(getUrl("MediaDir"))
}

export function getPresets(): Promise<AxiosResponse<any>> {
        return Vue.axios
                .get(getUrl("Preset/List"))
}
export function postAddOrUpdatePreset(item: any): Promise<AxiosResponse<any>> {
        return Vue.axios
                .post(getUrl("Preset/Add"), item);
}


export function getFtpStatus(): Promise<AxiosResponse<any>> {
        return Vue.axios
                .get(getUrl("Ftp/Status"))
}

export function postFtp(input: boolean, on: boolean): Promise<AxiosResponse<any>> {
        return Vue.axios
                .post(getUrl("Ftp/" + (input ? "Input" : "Output") + "/" + (on ? "On" : "Off")))
}

export function postDeletePreset(id:number): Promise<AxiosResponse<any>> {
        return Vue.axios
                .post(getUrl("Preset/Delete?id="+id))
}

export function getLogs(type:string|null,from:string|null,to:string|null,skip:number,take:number): Promise<AxiosResponse<any>> {
        let url=`Log/List?skip=${skip}&take=${take}`;
        if(type)
        {
                url+=`&type=${type}`;
        }
        if(from)
        {
                url+=`&from=${from}`;
        }
        if(to)
        {
                url+=`&to=${to}`;
        }
        return Vue.axios
                .get(getUrl(url))
}
