import {useCallback, useContext, useSyncExternalStore} from "react";
import {LiveData} from "@/lib/live-data/LiveData.ts";
import {AppHost, AppHostContext, AppHostServices} from "@/lib/app-host/app-host.tsx";

export const useAppHost = () => {
    return useContext(AppHostContext);
}

export const useService = <A extends AppHost<any>, K extends keyof AppHostServices<A>>(host: A, name: K): AppHostServices<A>[K] => {
    const container = host.Container;

    return container.resolve(name);
};

export const useData = <T,>(data: LiveData<T>) =>{
    const dataRef = data;
    const subscribe = useCallback(dataRef.subscribe.bind(dataRef), [dataRef])

    return useSyncExternalStore<T>(subscribe, () => data.value);
}